using Il2CppSystem.Diagnostics;
using SteamVR_Standalone_IL2CPP.Util;
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;

namespace Valve.VR
{

	public class SteamVR_Render : MonoBehaviour
	{
		public SteamVR_Render(IntPtr value)
: base(value) { }


		public static EVREye eye { get; private set; }

		public static float unfocusedRenderResolution = 1f;



		public static SteamVR_Render instance
		{
			get
			{
				return SteamVR_Behaviour.instance.steamvr_render;
			}
		}


		private void OnApplicationQuit()
		{
			SteamVR_Render.isQuitting = true;
			SteamVR.SafeDispose();
		}


		public static void Add(SteamVR_Camera vrcam)
		{
			if (!SteamVR_Render.isQuitting)
			{
				SteamVR_Render.instance.AddInternal(vrcam);
			}
		}


		public static void Remove(SteamVR_Camera vrcam)
		{
			if (!SteamVR_Render.isQuitting && SteamVR_Render.instance != null)
			{
				SteamVR_Render.instance.RemoveInternal(vrcam);
			}
		}


		public static SteamVR_Camera Top()
		{
			if (!SteamVR_Render.isQuitting)
			{
				return SteamVR_Render.instance.TopInternal();
			}
			return null;
		}


		void AddInternal(SteamVR_Camera vrcam)
		{
			var camera = vrcam.GetComponent<Camera>();
			var length = cameras.Length;
			var sorted = new SteamVR_Camera[length + 1];
			int insert = 0;
			for (int i = 0; i < length; i++)
			{
				var c = cameras[i].GetComponent<Camera>();
				if (i == insert && c.depth > camera.depth)
					sorted[insert++] = vrcam;

				sorted[insert++] = cameras[i];
			}
			if (insert == length)
				sorted[insert] = vrcam;

			cameras = sorted;
			enabled = true;
		}

		void RemoveInternal(SteamVR_Camera vrcam)
		{
			var length = cameras.Length;
			int count = 0;
			for (int i = 0; i < length; i++)
			{
				var c = cameras[i];
				if (c == vrcam)
					++count;
			}
			if (count == 0)
				return;

			var sorted = new SteamVR_Camera[length - count];
			int insert = 0;
			for (int i = 0; i < length; i++)
			{
				var c = cameras[i];
				if (c != vrcam)
					sorted[insert++] = c;
			}

			cameras = sorted;
		}

		SteamVR_Camera TopInternal()
		{
			if (cameras.Length > 0)
				return cameras[cameras.Length - 1];

			return null;
		}

		public static bool pauseRendering
		{
			get
			{
				return SteamVR_Render._pauseRendering;
			}
			set
			{
				SteamVR_Render._pauseRendering = value;
				CVRCompositor compositor = OpenVR.Compositor;
				if (compositor != null)
				{
					compositor.SuspendRendering(value);
				}
			}
		}

		public static event Action<EVREye, SteamVR_CameraMask> eyePreRenderCallback;
		public static event Action<EVREye> eyePostRenderCallback;
		public static event Action preRenderBothEyesCallback;
		public static event Action postBothEyesRenderedCallback;

		private IEnumerator RenderLoop()
		{
			while (Application.isPlaying)
			{
				yield return waitForEndOfFrame;

				if (cameras.Length == 0)
				{
					continue;
				}

				if (pauseRendering)
					continue;

				var compositor = OpenVR.Compositor;
				if (compositor != null)
				{
					if (!compositor.CanRenderScene())
						continue;

					compositor.SetTrackingSpace(SteamVR.settings.trackingSpace);
					SteamVR_Utils.QueueEventOnRenderThread(SteamVR.OpenVRMagic.k_nRenderEventID_WaitGetPoses);

					// Hack to flush render event that was queued in Update (this ensures WaitGetPoses has returned before we grab the new values).
					SteamVR.OpenVRMagic.EventWriteString("[UnityMain] GetNativeTexturePtr - Begin");
					SteamVR_Camera.GetSceneTexture(cameras[0].GetComponent<Camera>().allowHDR).GetNativeTexturePtr();
					SteamVR.OpenVRMagic.EventWriteString("[UnityMain] GetNativeTexturePtr - End");

					compositor.GetLastPoses(poses, gamePoses);
					SteamVR_Events.NewPoses.Send(poses);
					SteamVR_Events.NewPosesApplied.Send();
				}

				var overlay = SteamVR_Overlay.instance;
				if (overlay != null)
					overlay.UpdateOverlay();

				RenderExternalCamera();

				if (preRenderBothEyesCallback != null)
				{
					preRenderBothEyesCallback.Invoke();
				}

				var vr = SteamVR.instance;
				RenderEye(vr, EVREye.Eye_Left);
				RenderEye(vr, EVREye.Eye_Right);

				// Move cameras back to head position so they can be tracked reliably
				foreach (var c in cameras)
				{
					c.transform.localPosition = Vector3.zero;
					c.transform.localRotation = Quaternion.identity;
				}

				if (cameraMask != null)
					cameraMask.Clear();

				if (postBothEyesRenderedCallback != null)
				{
					postBothEyesRenderedCallback.Invoke();
				}
			}
		}

		void RenderEye(SteamVR vr, EVREye eye)
		{
			int i = (int)eye;
			SteamVR_Render.eye = eye;

			if (cameraMask != null)
				cameraMask.Set(vr, eye);

			foreach (var c in cameras)
			{

				c.transform.localPosition = vr.eyes[i].pos;
				c.transform.localRotation = vr.eyes[i].rot;

				// Update position to keep from getting culled
				cameraMask.transform.position = c.transform.position;

				var camera = c.camera;

				int cullingMask = camera.cullingMask;
				if (eye == EVREye.Eye_Left)
				{
					camera.cullingMask &= ~rightMask;
					camera.cullingMask |= leftMask;
				}
				else
				{
					camera.cullingMask &= ~leftMask;
					camera.cullingMask |= rightMask;
				}
				eyePreRenderCallback?.Invoke(eye, cameraMask);
				var tex = camera.targetTexture;
				camera.targetTexture = SteamVR_Camera.GetSceneTexture(camera.allowHDR);
				camera.cullingMask = c.TrueMask;
				camera.Render();
				camera.cullingMask = 0;

				var temp = RenderTexture.GetTemporary(camera.targetTexture.descriptor);
				Graphics.Blit(camera.targetTexture, temp, new Vector2(1, -1), new Vector2(0, 1));

				var outTex = new Texture_t();
				outTex.handle = temp.GetNativeTexturePtr();
				outTex.eType = SteamVR.instance.textureType;
				outTex.eColorSpace = EColorSpace.Auto;

				OpenVR.Compositor.Submit(eye, ref outTex, ref SteamVR.instance.textureBounds[i], EVRSubmitFlags.Submit_Default);
				
				RenderTexture.ReleaseTemporary(temp);

				camera.targetTexture = tex;

				camera.cullingMask = cullingMask;
			}
			eyePostRenderCallback?.Invoke(eye);
		}


		private bool CheckExternalCamera()
		{
			bool? flag = this.doesPathExist;
			bool flag2 = false;
			if (flag.GetValueOrDefault() == flag2 & flag != null)
			{
				return false;
			}
			if (this.doesPathExist == null)
			{
				this.doesPathExist = new bool?(File.Exists(this.externalCameraConfigPath));
			}
			if (this.externalCamera == null)
			{
				flag = this.doesPathExist;
				flag2 = true;
				if (flag.GetValueOrDefault() == flag2 & flag != null)
				{
					GameObject gameObject = Resources.Load<GameObject>("SteamVR_ExternalCamera");
					if (gameObject == null)
					{
						this.doesPathExist = new bool?(false);
						return false;
					}
					if (SteamVR_Settings.instance.legacyMixedRealityCamera)
					{
						if (!SteamVR_ExternalCamera_LegacyManager.hasCamera)
						{
							return false;
						}
						GameObject gameObject2 = UnityEngine.Object.Instantiate<GameObject>(gameObject);
						gameObject2.gameObject.name = "External Camera";
						this.externalCamera = gameObject2.transform.GetChild(0).GetComponent<SteamVR_ExternalCamera>();
						this.externalCamera.configPath = this.externalCameraConfigPath;
						this.externalCamera.ReadConfig();
						this.externalCamera.SetupDeviceIndex(SteamVR_ExternalCamera_LegacyManager.cameraIndex);
					}
					else
					{
						SteamVR_Action_Pose mixedRealityCameraPose = SteamVR_Settings.instance.mixedRealityCameraPose;
						SteamVR_Input_Sources mixedRealityCameraInputSource = SteamVR_Settings.instance.mixedRealityCameraInputSource;
						if (mixedRealityCameraPose != null && SteamVR_Settings.instance.mixedRealityActionSetAutoEnable && mixedRealityCameraPose.actionSet != null && !mixedRealityCameraPose.actionSet.IsActive(mixedRealityCameraInputSource))
						{
							mixedRealityCameraPose.actionSet.Activate(mixedRealityCameraInputSource, 0, false);
						}
						if (mixedRealityCameraPose == null)
						{
							this.doesPathExist = new bool?(false);
							return false;
						}
						if (mixedRealityCameraPose != null && mixedRealityCameraPose[mixedRealityCameraInputSource].active && mixedRealityCameraPose[mixedRealityCameraInputSource].deviceIsConnected)
						{
							GameObject gameObject3 = UnityEngine.Object.Instantiate<GameObject>(gameObject);
							gameObject3.gameObject.name = "External Camera";
							this.externalCamera = gameObject3.transform.GetChild(0).GetComponent<SteamVR_ExternalCamera>();
							this.externalCamera.configPath = this.externalCameraConfigPath;
							this.externalCamera.ReadConfig();
							this.externalCamera.SetupPose(mixedRealityCameraPose, mixedRealityCameraInputSource);
						}
					}
				}
			}
			return this.externalCamera != null;
		}


		private void RenderExternalCamera()
		{
			if (this.externalCamera == null)
			{
				return;
			}
			if (!this.externalCamera.gameObject.activeInHierarchy)
			{
				return;
			}
			int num = (int)SteamVR_Standalone_IL2CPP.Util.Mathf.Max(this.externalCamera.config.frameSkip, 0f);
			if (Time.frameCount % (num + 1) != 0)
			{
				return;
			}
			this.externalCamera.AttachToCamera(this.TopInternal());
			this.externalCamera.RenderNear();
			this.externalCamera.RenderFar();
		}


		private void OnInputFocus(bool hasFocus)
		{
			if (!SteamVR.active)
			{
				return;
			}
			if (hasFocus)
			{
				if (SteamVR.settings.pauseGameWhenDashboardVisible)
				{
					Time.timeScale = this.timeScale;
				}
				SteamVR_Camera.sceneResolutionScale = this.sceneResolutionScale;
				return;
			}
			if (SteamVR.settings.pauseGameWhenDashboardVisible)
			{
				this.timeScale = Time.timeScale;
				Time.timeScale = 0f;
			}
			this.sceneResolutionScale = SteamVR_Camera.sceneResolutionScale;
			SteamVR_Camera.sceneResolutionScale = unfocusedRenderResolution;
		}


		private string GetScreenshotFilename(uint screenshotHandle, EVRScreenshotPropertyFilenames screenshotPropertyFilename)
		{
			EVRScreenshotError evrscreenshotError = EVRScreenshotError.None;
			uint screenshotPropertyFilename2 = OpenVR.Screenshots.GetScreenshotPropertyFilename(screenshotHandle, screenshotPropertyFilename, null, 0u, ref evrscreenshotError);
			if (evrscreenshotError != EVRScreenshotError.None && evrscreenshotError != EVRScreenshotError.BufferTooSmall)
			{
				return null;
			}
			if (screenshotPropertyFilename2 <= 1u)
			{
				return null;
			}
			StringBuilder stringBuilder = new StringBuilder((int)screenshotPropertyFilename2);
			OpenVR.Screenshots.GetScreenshotPropertyFilename(screenshotHandle, screenshotPropertyFilename, stringBuilder, screenshotPropertyFilename2, ref evrscreenshotError);
			if (evrscreenshotError != EVRScreenshotError.None)
			{
				return null;
			}
			return stringBuilder.ToString();
		}


		private void OnRequestScreenshot(VREvent_t vrEvent)
		{
			uint handle = vrEvent.data.screenshot.handle;
			EVRScreenshotType type = (EVRScreenshotType)vrEvent.data.screenshot.type;
			if (type == EVRScreenshotType.StereoPanorama)
			{
				string screenshotFilename = this.GetScreenshotFilename(handle, EVRScreenshotPropertyFilenames.Preview);
				string screenshotFilename2 = this.GetScreenshotFilename(handle, EVRScreenshotPropertyFilenames.VR);
				if (screenshotFilename == null || screenshotFilename2 == null)
				{
					return;
				}
				SteamVR_Utils.TakeStereoScreenshot(handle, new GameObject("screenshotPosition") {
					transform =
					{
						position = SteamVR_Render.Top().transform.position,
						rotation = SteamVR_Render.Top().transform.rotation,
						localScale = SteamVR_Render.Top().transform.lossyScale
					}
				}, 32, 0.064f, ref screenshotFilename, ref screenshotFilename2);
				OpenVR.Screenshots.SubmitScreenshot(handle, type, screenshotFilename, screenshotFilename2);
			}
		}


		private void OnEnable()
		{
			Debug.Log("Start render loop!");
			MelonCoroutines.Start(RenderLoop());
			SteamVR_Events.InputFocus.Listen((this.OnInputFocus));
			SteamVR_Events.System(EVREventType.VREvent_RequestScreenshot).Listen((this.OnRequestScreenshot));
			if (SteamVR_Settings.instance.legacyMixedRealityCamera)
			{
				SteamVR_ExternalCamera_LegacyManager.SubscribeToNewPoses();
			}

			UnityHooks.OnBeforeRender += OnBeforeRender;
			UnityHooks.OnBeforeCull += OnCameraPreCull;
			if (SteamVR.initializedState == SteamVR.InitializedStates.InitializeSuccess)
			{
				OpenVR.Screenshots.HookScreenshot(this.screenshotTypes);
				return;
			}
			SteamVR_Events.Initialized.Listen(this.OnSteamVRInitialized);
		}


		private void OnSteamVRInitialized(bool success)
		{
			if (success)
			{
				OpenVR.Screenshots.HookScreenshot(this.screenshotTypes);
			}
		}


		private void OnDisable()
		{
			SteamVR_Events.InputFocus.Remove(this.OnInputFocus);
			SteamVR_Events.System(EVREventType.VREvent_RequestScreenshot).Remove((this.OnRequestScreenshot));
			UnityHooks.OnBeforeRender -= OnBeforeRender;
			UnityHooks.OnBeforeCull -= OnCameraPreCull;
			if (SteamVR.initializedState != SteamVR.InitializedStates.InitializeSuccess)
			{
				SteamVR_Events.Initialized.Remove(OnSteamVRInitialized);
			}
		}


		public void UpdatePoses()
		{
			var compositor = OpenVR.Compositor;
			if (compositor != null)
			{
				compositor.GetLastPoses(poses, gamePoses);
				SteamVR_Events.NewPoses.Send(poses);
				SteamVR_Events.NewPosesApplied.Send();
			}
		}


		private void OnBeforeRender()
		{
			if (!SteamVR.active)
			{
				return;
			}
			this.UpdatePoses();
		}


		void Update()
		{
			if (cameras.Length == 0)
			{
				return;
			}
			this.UpdatePoses();
			// If our FixedUpdate rate doesn't match our render framerate, then catch the handoff here.
			SteamVR_Utils.QueueEventOnRenderThread(SteamVR.OpenVRMagic.k_nRenderEventID_PostPresentHandoff);

			// Force controller update in case no one else called this frame to ensure prevState gets updated.

			// Dispatch any OpenVR events.
			var system = OpenVR.System;
			if (system != null)
			{
				var vrEvent = new VREvent_t();
				var size = (uint)System.Runtime.InteropServices.Marshal.SizeOf(typeof(VREvent_t));
				for (int i = 0; i < 64; i++)
				{
					if (!system.PollNextEvent(ref vrEvent, size))
						break;

					switch ((EVREventType)vrEvent.eventType)
					{
						case EVREventType.VREvent_InputFocusCaptured: // another app has taken focus (likely dashboard)
							if (vrEvent.data.process.oldPid == 0)
							{
								SteamVR_Events.InputFocus.Send(false);
							}
							break;
						case EVREventType.VREvent_InputFocusReleased: // that app has released input focus
							if (vrEvent.data.process.pid == 0)
							{
								SteamVR_Events.InputFocus.Send(true);
							}
							break;
						case EVREventType.VREvent_ShowRenderModels:
							SteamVR_Events.HideRenderModels.Send(false);
							break;
						case EVREventType.VREvent_HideRenderModels:
							SteamVR_Events.HideRenderModels.Send(true);
							break;
						default:
							SteamVR_Events.System((EVREventType)vrEvent.eventType).Send(vrEvent);
							break;
					}
				}
			}

			// Ensure various settings to minimize latency.
			Application.targetFrameRate = -1;
			Application.runInBackground = true; // don't require companion window focus
			QualitySettings.maxQueuedFrames = -1;
			QualitySettings.vSyncCount = 0; // this applies to the companion window
		}

		void FixedUpdate()
		{
			// We want to call this as soon after Present as possible.
			SteamVR_Utils.QueueEventOnRenderThread(SteamVR.OpenVRMagic.k_nRenderEventID_PostPresentHandoff);
		}

		void OnCameraPreCull(Camera cam)
		{
			// Only update poses on the first camera per frame.
			if (Time.frameCount != lastFrameCount)
			{
				lastFrameCount = Time.frameCount;
				UpdatePoses();
			}
		}
		static int lastFrameCount = -1;


		void Awake()
		{
			var go = new GameObject("cameraMask");
			go.transform.parent = transform;
			cameraMask = go.AddComponent<SteamVR_CameraMask>();

			if (externalCamera == null && System.IO.File.Exists(externalCameraConfigPath))
			{
				var prefab = Resources.Load<GameObject>("SteamVR_ExternalCamera");
				var instance = Instantiate(prefab);
				instance.gameObject.name = "External Camera";

				externalCamera = instance.transform.GetChild(0).GetComponent<SteamVR_ExternalCamera>();
				externalCamera.configPath = externalCameraConfigPath;
				externalCamera.ReadConfig();
			}
		}


		public SteamVR_ExternalCamera externalCamera;


		public string externalCameraConfigPath = "externalcamera.cfg";


		private static bool isQuitting;


		public SteamVR_Camera[] cameras = new SteamVR_Camera[0];


		public TrackedDevicePose_t[] poses = new TrackedDevicePose_t[64];


		public TrackedDevicePose_t[] gamePoses = new TrackedDevicePose_t[0];


		private static bool _pauseRendering;


		private WaitForEndOfFrame waitForEndOfFrame = new WaitForEndOfFrame();


		private bool? doesPathExist;


		private float sceneResolutionScale = 1f;


		private float timeScale = 1f;


		private EVRScreenshotType[] screenshotTypes = new EVRScreenshotType[]
		{
			EVRScreenshotType.StereoPanorama
		};

		public LayerMask leftMask;


		public LayerMask rightMask;


		private SteamVR_CameraMask cameraMask;
	}
}
