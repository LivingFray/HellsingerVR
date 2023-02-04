using SteamVR_Standalone_IL2CPP.Util;
using System;
using System.Collections;
using System.IO;
using System.Text;
using UnityEngine;

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

		private void AddInternal(SteamVR_Camera vrcam)
		{
			cameras[(int)vrcam.eye] = vrcam;
		}

		private void RemoveInternal(SteamVR_Camera vrcam)
		{
			cameras[(int)vrcam.eye] = null;
		}

		private SteamVR_Camera TopInternal()
		{
			return cameras[0];
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

		[Il2CppInterop.Runtime.Attributes.HideFromIl2Cpp()]
		private IEnumerator RenderLoop()
		{
			while (Application.isPlaying)
			{
				yield return waitForEndOfFrame;

				if (cameras[1] == null)
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
				}


				var overlay = SteamVR_Overlay.instance;
				if (overlay != null)
					overlay.UpdateOverlay();

				if (preRenderBothEyesCallback != null)
				{
					preRenderBothEyesCallback.Invoke();
				}

				var vr = SteamVR.instance;

				System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
				sw.Start();

				RenderEye(vr, EVREye.Eye_Left);
				RenderEye(vr, EVREye.Eye_Right);

				sw.Stop();
				Debug.Log($"Submitted both eyes in {(1000.0f * sw.ElapsedTicks) / System.Diagnostics.Stopwatch.Frequency}ms");

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

		[Il2CppInterop.Runtime.Attributes.HideFromIl2Cpp()]
		private void RenderEye(SteamVR vr, EVREye eye)
		{
			int i = (int)eye;
			SteamVR_Render.eye = eye;

			SteamVR_Camera c = cameras[i];

			Camera camera = c.camera;

			var temp = RenderTexture.GetTemporary(camera.targetTexture.descriptor);
			Graphics.Blit(camera.targetTexture, temp, new Vector2(1, -1), new Vector2(0, 1));

			var outTex = new Texture_t();
			outTex.handle = temp.GetNativeTexturePtr();
			outTex.eType = SteamVR.instance.textureType;
			outTex.eColorSpace = EColorSpace.Auto;

			OpenVR.Compositor.Submit(eye, ref outTex, ref SteamVR.instance.textureBounds[i], EVRSubmitFlags.Submit_Default);

			RenderTexture.ReleaseTemporary(temp);

			if (eye == EVREye.Eye_Right)
			{
				float Aspect = (float)Screen.width / Screen.height;
				float VRAspect = (float)camera.targetTexture.width / camera.targetTexture.height;

				Vector2 Scale = new Vector2(1.0f, VRAspect / Aspect);
				Vector2 Offset = new Vector2(0.0f, 0.5f * (1.0f - Scale.y));

				Graphics.Blit(camera.targetTexture, null, Scale, Offset);
			}
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
			SteamVR_Events.InputFocus.Listen(this.OnInputFocus);
			SteamVR_Events.System(EVREventType.VREvent_RequestScreenshot).Listen(this.OnRequestScreenshot);
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
			SteamVR_Events.System(EVREventType.VREvent_RequestScreenshot).Remove(this.OnRequestScreenshot);
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

		private void Update()
		{
			if (cameras[1] == null)
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

		private void FixedUpdate()
		{
			// We want to call this as soon after Present as possible.
			SteamVR_Utils.QueueEventOnRenderThread(SteamVR.OpenVRMagic.k_nRenderEventID_PostPresentHandoff);
		}

		private void OnCameraPreCull(Camera cam)
		{
			// Only update poses on the first camera per frame.
			if (Time.frameCount != lastFrameCount)
			{
				lastFrameCount = Time.frameCount;
				UpdatePoses();
			}
		}

		private static int lastFrameCount = -1;

		private void Awake()
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


		public SteamVR_Camera[] cameras = new SteamVR_Camera[2];


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


		public SteamVR_CameraMask cameraMask;
	}
}
