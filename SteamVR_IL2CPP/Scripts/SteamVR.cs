﻿using Il2CppInterop.Runtime.Injection;
using Newtonsoft.Json;
using Standalone;
using SteamVR_Standalone_IL2CPP.Standalone;
using SteamVR_Standalone_IL2CPP.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR;
using Valve.VR.InteractionSystem;
using Debug = UnityEngine.Debug;
using Mathf = SteamVR_Standalone_IL2CPP.Util.Mathf;

namespace Valve.VR
{

	public class SteamVR : IDisposable
	{
		public static BepInEx.Logging.ManualLogSource Log;

		public static bool active
		{
			get
			{
				return SteamVR._instance != null;
			}
		}




		public static bool enabled
		{
			get
			{
				return SteamVR._enabled;
			}
			set
			{
				SteamVR._enabled = value;
				if (SteamVR._enabled)
				{
					SteamVR.Initialize(false);
					return;
				}
			}
		}

		public static SteamVR instance
		{
			get
			{
				if (!SteamVR.enabled)
				{
					return null;
				}
				if (SteamVR._instance == null)
				{
					SteamVR._instance = SteamVR.CreateInstance();
					if (SteamVR._instance == null)
					{
						SteamVR._enabled = false;
					}
				}
				return SteamVR._instance;
			}
		}

		static bool HasRegisteredCallbacks = false;

		public static void Initialize(bool forceUnityVRMode = false)
		{
			if (forceUnityVRMode)
			{
				SteamVR_Behaviour.instance.InitializeSteamVR(true);
				return;
			}
			if (SteamVR._instance == null)
			{
				SteamVR._instance = SteamVR.CreateInstance();
				if (SteamVR._instance == null)
				{
					Debug.Log("Couldn't create instance");
					SteamVR._enabled = false;
				}
			}
			if (SteamVR._enabled)
			{
				SteamVR_Behaviour.Initialize(forceUnityVRMode);

				if (!HasRegisteredCallbacks)
				{
					RenderPipelineManager.add_beginContextRendering(new Action<ScriptableRenderContext, Il2CppSystem.Collections.Generic.List<Camera>>(OnBeginContextRendering));
					RenderPipelineManager.add_beginCameraRendering(new Action<ScriptableRenderContext, Camera>(OnBeginCameraRendering));
					HasRegisteredCallbacks = true;
				}
			}
		}

		static int LastFrameRendered = -1;

		private static void OnBeginContextRendering(ScriptableRenderContext context, Il2CppSystem.Collections.Generic.List<Camera> cameras)
		{
			if (!SteamVR_Render.instance)
			{
				return;
			}

			// Unity has decided it should call this function twice per frame...
			if (LastFrameRendered == Time.frameCount)
			{
				return;
			}
			LastFrameRendered = Time.frameCount;

			System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
			sw.Start();

			var compositor = OpenVR.Compositor;
			if (compositor != null)
			{
				if (!compositor.CanRenderScene() || SteamVR_Render.instance.cameras[0] == null)
					return;

				compositor.SetTrackingSpace(SteamVR.settings.trackingSpace);
				SteamVR_Utils.QueueEventOnRenderThread(SteamVR.OpenVRMagic.k_nRenderEventID_WaitGetPoses);

				// Hack to flush render event that was queued in Update (this ensures WaitGetPoses has returned before we grab the new values).
				SteamVR.OpenVRMagic.EventWriteString("[UnityMain] GetNativeTexturePtr - Begin");
				SteamVR_Camera.GetSceneTexture(EVREye.Eye_Left, SteamVR_Render.instance.cameras[0].camera.allowHDR).GetNativeTexturePtr();
				SteamVR.OpenVRMagic.EventWriteString("[UnityMain] GetNativeTexturePtr - End");

				compositor.GetLastPoses(SteamVR_Render.instance.poses, SteamVR_Render.instance.gamePoses);
				SteamVR_Events.NewPoses.Send(SteamVR_Render.instance.poses);
				SteamVR_Events.NewPosesApplied.Send();
			}

			sw.Stop();
			Debug.Log($"{Time.frameCount} Called WaitGetPoses in {(1000.0f * sw.ElapsedTicks) / System.Diagnostics.Stopwatch.Frequency}ms");
		}

		private static void OnBeginCameraRendering(ScriptableRenderContext context, Camera camera)
		{
			System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
			sw.Start();
			foreach (SteamVR_Camera vrCam in SteamVR_Render.instance.cameras)
			{
				if (vrCam && vrCam.camera == camera)
				{
					vrCam.PreRender();
					break;
				}
			}
			sw.Stop();
			Debug.Log($"Prerendered eyes in {(1000.0f * sw.ElapsedTicks) / System.Diagnostics.Stopwatch.Frequency}ms");
		}

		public static bool usingNativeSupport
		{
			get
			{
				return XRDevice.GetNativePtr() != IntPtr.Zero;
			}
		}

		public static SteamVR_Settings settings { get; private set; }


		private static void ReportGeneralErrors()
		{
			string text = "<b>[SteamVR_Standalone]</b> Initialization failed. ";
			if (!XRSettings.enabled)
			{
				text += "VR may be disabled in player settings. Go to player settings in the editor and check the 'Virtual Reality Supported' checkbox'. ";
			}
			if (XRSettings.supportedDevices != null && XRSettings.supportedDevices.Length != 0)
			{
				if (!XRSettings.supportedDevices.Contains("OpenVR"))
				{
					text += "OpenVR is not in your list of supported virtual reality SDKs. Add it to the list in player settings. ";
				}
				else if (!XRSettings.supportedDevices.First<string>().Contains("OpenVR"))
				{
					text += "OpenVR is not first in your list of supported virtual reality SDKs. <b>This is okay, but if you have an Oculus device plugged in, and Oculus above OpenVR in this list, it will try and use the Oculus SDK instead of OpenVR.</b> ";
				}
			}
			else
			{
				text += "You have no SDKs in your Player Settings list of supported virtual reality SDKs. Add OpenVR to it. ";
			}
			text += "To force OpenVR initialization call SteamVR_Standalone.Initialize(true). ";
			UnityEngine.Debug.LogWarning(text);
			Log.LogWarning(text);
		}


		private static SteamVR CreateInstance()
		{
			SteamVR.initializedState = SteamVR.InitializedStates.Initializing;
			try
			{
				EVRInitError evrinitError = EVRInitError.None;
				OpenVR.GetGenericInterface("IVRCompositor_022", ref evrinitError);
				OpenVR.Init(ref evrinitError, EVRApplicationType.VRApplication_Scene, "");
				CVRSystem system = OpenVR.System;
				string manifestFile = SteamVR.GetManifestFile();
				EVRApplicationError evrapplicationError = OpenVR.Applications.AddApplicationManifest(manifestFile, true);
				if (evrapplicationError != EVRApplicationError.None)
				{
					UnityEngine.Debug.LogError("<b>[SteamVR_Standalone]</b> Error adding vr manifest file: " + evrapplicationError.ToString());
					Log.LogError("<b>[SteamVR_Standalone]</b> Error adding vr manifest file: " + evrapplicationError.ToString());
				}
				int id = Process.GetCurrentProcess().Id;
				OpenVR.Applications.IdentifyApplication((uint)id, SteamVR_Settings.instance.editorAppKey);
				UnityEngine.Debug.Log("Is HMD here? " + OpenVR.IsHmdPresent().ToString());
				Log.LogInfo("Is HMD here? " + OpenVR.IsHmdPresent().ToString());
				if (evrinitError != EVRInitError.None)
				{
					SteamVR.initializedState = SteamVR.InitializedStates.InitializeFailure;
					SteamVR.ReportError(evrinitError);
					SteamVR.ReportGeneralErrors();
					SteamVR_Events.Initializing.Send(false);
					return null;
				}
				OpenVR.GetGenericInterface("IVROverlay_021", ref evrinitError);
				if (evrinitError != EVRInitError.None)
				{
					SteamVR.initializedState = SteamVR.InitializedStates.InitializeFailure;
					SteamVR.ReportError(evrinitError);
					SteamVR_Events.Initializing.Send(false);
					return null;
				}
				OpenVR.GetGenericInterface("IVRInput_007", ref evrinitError);
				if (evrinitError != EVRInitError.None)
				{
					SteamVR.initializedState = SteamVR.InitializedStates.InitializeFailure;
					SteamVR.ReportError(evrinitError);
					SteamVR_Events.Initializing.Send(false);
					return null;
				}
				SteamVR.settings = SteamVR_Settings.instance;
				if (Application.isEditor)
				{
					SteamVR.IdentifyEditorApplication(true);
				}
				SteamVR_Input.IdentifyActionsFile(true);
				if (SteamVR_Settings.instance.inputUpdateMode != SteamVR_UpdateModes.Nothing || SteamVR_Settings.instance.poseUpdateMode != SteamVR_UpdateModes.Nothing)
				{
					SteamVR_Input.Initialize(false);
				}
			}
			catch (Exception arg)
			{
				UnityEngine.Debug.LogError("<b>[SteamVR_Standalone]</b> " + arg);
				Log.LogError("<b>[SteamVR_Standalone]</b> " + arg);
				SteamVR_Events.Initializing.Send(false);
				return null;
			}
			SteamVR._enabled = true;
			SteamVR.initializedState = SteamVR.InitializedStates.InitializeSuccess;
			SteamVR_Events.Initializing.Send(true);
			return new SteamVR();
		}


		private static void ReportError(EVRInitError error)
		{
			if (error <= EVRInitError.Init_VRClientDLLNotFound)
			{
				if (error == EVRInitError.None)
				{
					return;
				}
				if (error == EVRInitError.Init_VRClientDLLNotFound)
				{
					UnityEngine.Debug.LogWarning("<b>[SteamVR_Standalone]</b> Drivers not found!  They can be installed via Steam under Library > Tools.  Visit http://steampowered.com to install Steam.");
					Log.LogWarning("<b>[SteamVR_Standalone]</b> Drivers not found!  They can be installed via Steam under Library > Tools.  Visit http://steampowered.com to install Steam.");
					return;
				}
			}
			else
			{
				if (error == EVRInitError.Driver_RuntimeOutOfDate)
				{
					UnityEngine.Debug.LogWarning("<b>[SteamVR_Standalone]</b> Initialization Failed!  Make sure device's runtime is up to date.");
					Log.LogWarning("<b>[SteamVR_Standalone]</b> Initialization Failed!  Make sure device's runtime is up to date.");
					return;
				}
				if (error == EVRInitError.VendorSpecific_UnableToConnectToOculusRuntime)
				{
					UnityEngine.Debug.LogWarning("<b>[SteamVR_Standalone]</b> Initialization Failed!  Make sure device is on, Oculus runtime is installed, and OVRService_*.exe is running.");
					Log.LogWarning("<b>[SteamVR_Standalone]</b> Initialization Failed!  Make sure device is on, Oculus runtime is installed, and OVRService_*.exe is running.");
					return;
				}
			}
			UnityEngine.Debug.LogWarning("<b>[SteamVR_Standalone]</b> " + OpenVR.GetStringForHmdError(error));
			Log.LogWarning("<b>[SteamVR_Standalone]</b> " + OpenVR.GetStringForHmdError(error));
		}




		public CVRSystem hmd { get; private set; }




		public CVRCompositor compositor { get; private set; }




		public CVROverlay overlay { get; private set; }




		public static bool initializing { get; private set; }




		public static bool calibrating { get; private set; }




		public static bool outOfRange { get; private set; }




		public float sceneWidth { get; private set; }




		public float sceneHeight { get; private set; }




		public float aspect { get; private set; }




		public float fieldOfView { get; private set; }




		public Vector2 tanHalfFov { get; private set; }




		public VRTextureBounds_t[] textureBounds { get; private set; }




		public SteamVR_Utils.RigidTransform[] eyes { get; private set; }



		public string hmd_TrackingSystemName
		{
			get
			{
				return this.GetStringProperty(ETrackedDeviceProperty.Prop_TrackingSystemName_String, 0u);
			}
		}



		public string hmd_ModelNumber
		{
			get
			{
				return this.GetStringProperty(ETrackedDeviceProperty.Prop_ModelNumber_String, 0u);
			}
		}



		public string hmd_SerialNumber
		{
			get
			{
				return this.GetStringProperty(ETrackedDeviceProperty.Prop_SerialNumber_String, 0u);
			}
		}



		public float hmd_SecondsFromVsyncToPhotons
		{
			get
			{
				return this.GetFloatProperty(ETrackedDeviceProperty.Prop_SecondsFromVsyncToPhotons_Float, 0u);
			}
		}



		public float hmd_DisplayFrequency
		{
			get
			{
				return this.GetFloatProperty(ETrackedDeviceProperty.Prop_DisplayFrequency_Float, 0u);
			}
		}


		public EDeviceActivityLevel GetHeadsetActivityLevel()
		{
			return OpenVR.System.GetTrackedDeviceActivityLevel(0u);
		}


		public string GetTrackedDeviceString(uint deviceId)
		{
			ETrackedPropertyError etrackedPropertyError = ETrackedPropertyError.TrackedProp_Success;
			uint stringTrackedDeviceProperty = this.hmd.GetStringTrackedDeviceProperty(deviceId, ETrackedDeviceProperty.Prop_AttachedDeviceId_String, null, 0u, ref etrackedPropertyError);
			if (stringTrackedDeviceProperty > 1u)
			{
				StringBuilder stringBuilder = new StringBuilder((int)stringTrackedDeviceProperty);
				this.hmd.GetStringTrackedDeviceProperty(deviceId, ETrackedDeviceProperty.Prop_AttachedDeviceId_String, stringBuilder, stringTrackedDeviceProperty, ref etrackedPropertyError);
				return stringBuilder.ToString();
			}
			return null;
		}


		public string GetStringProperty(ETrackedDeviceProperty prop, uint deviceId = 0u)
		{
			ETrackedPropertyError etrackedPropertyError = ETrackedPropertyError.TrackedProp_Success;
			uint stringTrackedDeviceProperty = this.hmd.GetStringTrackedDeviceProperty(deviceId, prop, null, 0u, ref etrackedPropertyError);
			if (stringTrackedDeviceProperty > 1u)
			{
				StringBuilder stringBuilder = new StringBuilder((int)stringTrackedDeviceProperty);
				this.hmd.GetStringTrackedDeviceProperty(deviceId, prop, stringBuilder, stringTrackedDeviceProperty, ref etrackedPropertyError);
				return stringBuilder.ToString();
			}
			if (etrackedPropertyError == ETrackedPropertyError.TrackedProp_Success)
			{
				return "<unknown>";
			}
			return etrackedPropertyError.ToString();
		}


		public float GetFloatProperty(ETrackedDeviceProperty prop, uint deviceId = 0u)
		{
			ETrackedPropertyError etrackedPropertyError = ETrackedPropertyError.TrackedProp_Success;
			return this.hmd.GetFloatTrackedDeviceProperty(deviceId, prop, ref etrackedPropertyError);
		}


		public static bool InitializeTemporarySession(bool initInput = false)
		{
			if (Application.isEditor)
			{
				EVRInitError evrinitError = EVRInitError.None;
				OpenVR.GetGenericInterface("IVRCompositor_022", ref evrinitError);
				bool flag = evrinitError > EVRInitError.None;
				if (flag)
				{
					EVRInitError evrinitError2 = EVRInitError.None;
					OpenVR.Init(ref evrinitError2, EVRApplicationType.VRApplication_Overlay, "");
					if (evrinitError2 != EVRInitError.None)
					{
						UnityEngine.Debug.LogError("<b>[SteamVR_Standalone]</b> Error during OpenVR Init: " + evrinitError2.ToString());
						return false;
					}
					SteamVR.IdentifyEditorApplication(false);
					SteamVR_Input.IdentifyActionsFile(false);
					SteamVR.runningTemporarySession = true;
				}
				if (initInput)
				{
					SteamVR_Input.Initialize(true);
				}
				return flag;
			}
			return false;
		}


		public static void ExitTemporarySession()
		{
			if (SteamVR.runningTemporarySession)
			{
				OpenVR.Shutdown();
				SteamVR.runningTemporarySession = false;
			}
		}


		public static string GenerateAppKey()
		{
			string arg = SteamVR.GenerateCleanProductName();
			return string.Format("application.generated.unity.{0}.exe", arg);
		}


		public static string GenerateCleanProductName()
		{
			string text = Application.productName;
			if (string.IsNullOrEmpty(text))
			{
				text = "unnamed_product";
			}
			else
			{
				text = Regex.Replace(Application.productName, "[^\\w\\._]", "");
				text = text.ToLower();
			}
			return text;
		}


		private static string GetManifestFile()
		{
			string text = Application.dataPath;
			int num = text.LastIndexOf('/');
			text = text.Remove(num, text.Length - num);
			string text2 = Path.Combine(text, "unityProject.vrmanifest");
			FileInfo fileInfo = new FileInfo(SteamVR_Input.GetActionsFilePath(true));
			if (File.Exists(text2))
			{
				SteamVR_Input_ManifestFile steamVR_Input_ManifestFile = JsonConvert.DeserializeObject<SteamVR_Input_ManifestFile>(File.ReadAllText(text2));
				if (steamVR_Input_ManifestFile != null && steamVR_Input_ManifestFile.applications != null && steamVR_Input_ManifestFile.applications.Count > 0 && steamVR_Input_ManifestFile.applications[0].app_key != SteamVR_Settings.instance.editorAppKey)
				{
					UnityEngine.Debug.Log("<b>[SteamVR_Standalone]</b> Deleting existing VRManifest because it has a different app key.");
					FileInfo fileInfo2 = new FileInfo(text2);
					if (fileInfo2.IsReadOnly)
					{
						fileInfo2.IsReadOnly = false;
					}
					fileInfo2.Delete();
				}
				if (steamVR_Input_ManifestFile != null && steamVR_Input_ManifestFile.applications != null && steamVR_Input_ManifestFile.applications.Count > 0 && steamVR_Input_ManifestFile.applications[0].action_manifest_path != fileInfo.FullName)
				{
					UnityEngine.Debug.Log("<b>[SteamVR_Standalone]</b> Deleting existing VRManifest because it has a different action manifest path:\nExisting:" + steamVR_Input_ManifestFile.applications[0].action_manifest_path + "\nNew: " + fileInfo.FullName);
					FileInfo fileInfo3 = new FileInfo(text2);
					if (fileInfo3.IsReadOnly)
					{
						fileInfo3.IsReadOnly = false;
					}
					fileInfo3.Delete();
				}
			}
			if (!File.Exists(text2))
			{
				SteamVR_Input_ManifestFile steamVR_Input_ManifestFile2 = new SteamVR_Input_ManifestFile();
				steamVR_Input_ManifestFile2.source = "Unity";
				SteamVR_Input_ManifestFile_Application steamVR_Input_ManifestFile_Application = new SteamVR_Input_ManifestFile_Application();
				steamVR_Input_ManifestFile_Application.app_key = SteamVR_Settings.instance.editorAppKey;
				steamVR_Input_ManifestFile_Application.action_manifest_path = fileInfo.FullName;
				steamVR_Input_ManifestFile_Application.launch_type = "url";
				steamVR_Input_ManifestFile_Application.url = "steam://launch/";
				steamVR_Input_ManifestFile_Application.strings.Add("en_us", new SteamVR_Input_ManifestFile_ApplicationString {
					name = "Metal: Hellsinger"
				});
				steamVR_Input_ManifestFile2.applications = new List<SteamVR_Input_ManifestFile_Application>();
				steamVR_Input_ManifestFile2.applications.Add(steamVR_Input_ManifestFile_Application);
				string contents = JsonConvert.SerializeObject(steamVR_Input_ManifestFile2, Formatting.Indented, new JsonSerializerSettings {
					NullValueHandling = NullValueHandling.Ignore
				});
				File.WriteAllText(text2, contents);
			}
			return text2;
		}


		private static void IdentifyEditorApplication(bool showLogs = true)
		{
			if (string.IsNullOrEmpty(SteamVR_Settings.instance.editorAppKey))
			{
				UnityEngine.Debug.LogError("<b>[SteamVR_Standalone]</b> Critical Error identifying application. EditorAppKey is null or empty. Input may not work.");
				return;
			}
			string manifestFile = SteamVR.GetManifestFile();
			EVRApplicationError evrapplicationError = OpenVR.Applications.AddApplicationManifest(manifestFile, true);
			if (evrapplicationError != EVRApplicationError.None)
			{
				UnityEngine.Debug.LogError("<b>[SteamVR_Standalone]</b> Error adding vr manifest file: " + evrapplicationError.ToString());
			}
			else if (showLogs)
			{
				UnityEngine.Debug.Log("<b>[SteamVR_Standalone]</b> Successfully added VR manifest to SteamVR_Standalone");
			}
			int id = Process.GetCurrentProcess().Id;
			EVRApplicationError evrapplicationError2 = OpenVR.Applications.IdentifyApplication((uint)id, SteamVR_Settings.instance.editorAppKey);
			if (evrapplicationError2 != EVRApplicationError.None)
			{
				UnityEngine.Debug.LogError("<b>[SteamVR_Standalone]</b> Error identifying application: " + evrapplicationError2.ToString());
				return;
			}
			if (showLogs)
			{
				UnityEngine.Debug.Log(string.Format("<b>[SteamVR_Standalone]</b> Successfully identified process as editor project to SteamVR_Standalone ({0})", SteamVR_Settings.instance.editorAppKey));
			}
		}


		private void OnInitializing(bool initializing)
		{
			SteamVR.initializing = initializing;
		}


		private void OnCalibrating(bool calibrating)
		{
			SteamVR.calibrating = calibrating;
		}


		private void OnOutOfRange(bool outOfRange)
		{
			SteamVR.outOfRange = outOfRange;
		}


		private void OnDeviceConnected(int i, bool connected)
		{
			SteamVR.connected[i] = connected;
		}


		private void OnNewPoses(TrackedDevicePose_t[] poses)
		{
			this.eyes[0] = new SteamVR_Utils.RigidTransform(this.hmd.GetEyeToHeadTransform(EVREye.Eye_Left));
			this.eyes[1] = new SteamVR_Utils.RigidTransform(this.hmd.GetEyeToHeadTransform(EVREye.Eye_Right));
			for (int i = 0; i < poses.Length; i++)
			{
				bool bDeviceIsConnected = poses[i].bDeviceIsConnected;
				if (bDeviceIsConnected != SteamVR.connected[i])
				{
					SteamVR_Events.DeviceConnected.Send(i, bDeviceIsConnected);
				}
			}
			if (poses.Length > 0L)
			{
				ETrackingResult eTrackingResult = poses[0].eTrackingResult;
				bool flag = eTrackingResult == ETrackingResult.Uninitialized;
				if (flag != SteamVR.initializing)
				{
					SteamVR_Events.Initializing.Send(flag);
				}
				bool flag2 = eTrackingResult == ETrackingResult.Calibrating_InProgress || eTrackingResult == ETrackingResult.Calibrating_OutOfRange;
				if (flag2 != SteamVR.calibrating)
				{
					SteamVR_Events.Calibrating.Send(flag2);
				}
				bool flag3 = eTrackingResult == ETrackingResult.Running_OutOfRange || eTrackingResult == ETrackingResult.Calibrating_OutOfRange;
				if (flag3 != SteamVR.outOfRange)
				{
					SteamVR_Events.OutOfRange.Send(flag3);
				}
			}
		}

		private static bool classesRegistered = false;

		/// <summary>
		/// Inject types into il2cpp pre-emptively to prevent errors using this method
		/// </summary>
		public static void PreRegisterIL2CPPClasses()
		{
			if (classesRegistered)
			{
				return;
			}
			classesRegistered = true;
			ClassInjector.RegisterTypeInIl2Cpp<MelonCoroutineCallbacks>();
			ClassInjector.RegisterTypeInIl2Cpp<SteamVR_ActivateActionSetOnLoad>();
			ClassInjector.RegisterTypeInIl2Cpp<SteamVR_Behaviour>();
			ClassInjector.RegisterTypeInIl2Cpp<SteamVR_Behaviour_Boolean>();
			ClassInjector.RegisterTypeInIl2Cpp<SteamVR_Behaviour_Single>();
			ClassInjector.RegisterTypeInIl2Cpp<SteamVR_Behaviour_Skeleton>();
			ClassInjector.RegisterTypeInIl2Cpp<SteamVR_Behaviour_Vector2>();
			ClassInjector.RegisterTypeInIl2Cpp<SteamVR_Behaviour_Vector3>();
			ClassInjector.RegisterTypeInIl2Cpp<SteamVR_Behaviour_Pose>();
			ClassInjector.RegisterTypeInIl2Cpp<SteamVR_Camera>();
			ClassInjector.RegisterTypeInIl2Cpp<SteamVR_CameraFlip>();
			ClassInjector.RegisterTypeInIl2Cpp<SteamVR_CameraMask>();
			ClassInjector.RegisterTypeInIl2Cpp<SteamVR_Ears>();
			ClassInjector.RegisterTypeInIl2Cpp<SteamVR_ExternalCamera>();
			ClassInjector.RegisterTypeInIl2Cpp<SteamVR_Frustum>();
			ClassInjector.RegisterTypeInIl2Cpp<SteamVR_Fade>();
			ClassInjector.RegisterTypeInIl2Cpp<SteamVR_GameView>();
			ClassInjector.RegisterTypeInIl2Cpp<SteamVR_IK>();
			ClassInjector.RegisterTypeInIl2Cpp<SteamVR_Overlay>();
			ClassInjector.RegisterTypeInIl2Cpp<SteamVR_PlayArea>();
			ClassInjector.RegisterTypeInIl2Cpp<SteamVR_RenderModel>();
			ClassInjector.RegisterTypeInIl2Cpp<SteamVR_Render>();
			ClassInjector.RegisterTypeInIl2Cpp<SteamVR_Skeleton_Poser>();
			ClassInjector.RegisterTypeInIl2Cpp<SteamVR_SphericalProjection>();
			ClassInjector.RegisterTypeInIl2Cpp<SteamVR_Skybox>();
			ClassInjector.RegisterTypeInIl2Cpp<SteamVR_TrackingReferenceManager>();
			ClassInjector.RegisterTypeInIl2Cpp<SteamVR_TrackedObject>();
			ClassInjector.RegisterTypeInIl2Cpp<VelocityEstimator>();
		}

		private static bool m_setup;
		private SteamVR()
		{
			// Sometimes during game exit SteamVR will attempt to re-create itself, for some reason.
			if (m_setup)
			{
				return;
			}
			m_setup = true;
			ExternalPluginFunctionExtractor.GetLoadPluginFunction();
			PreRegisterIL2CPPClasses();
			Log.LogInfo("[SteamVR_Standalone] Preregistered Il2CPP classes");
			UnityHooks.Init();


			this.hmd = OpenVR.System;
			UnityEngine.Debug.Log("<b>[SteamVR_Standalone]</b> Initialized. Connected to " + this.hmd_TrackingSystemName + ":" + this.hmd_SerialNumber);
			Log.LogInfo("<b>[SteamVR_Standalone]</b> Initialized. Connected to " + this.hmd_TrackingSystemName + ":" + this.hmd_SerialNumber);
			this.compositor = OpenVR.Compositor;
			this.overlay = OpenVR.Overlay;

			Log.LogInfo("[SteamVR_Standalone] Preregistered Il2CPP classes");
			uint num = 0u;
			uint num2 = 0u;
			this.hmd.GetRecommendedRenderTargetSize(ref num, ref num2);
			this.sceneWidth = num;
			this.sceneHeight = num2;
			float num3 = 0f;
			float num4 = 0f;
			float num5 = 0f;
			float num6 = 0f;
			this.hmd.GetProjectionRaw(EVREye.Eye_Left, ref num3, ref num4, ref num5, ref num6);
			float num7 = 0f;
			float num8 = 0f;
			float num9 = 0f;
			float num10 = 0f;
			this.hmd.GetProjectionRaw(EVREye.Eye_Right, ref num7, ref num8, ref num9, ref num10);
			this.tanHalfFov = new Vector2(Mathf.Max(new float[]
			{
				-num3,
				num4,
				-num7,
				num8
			}), Mathf.Max(new float[]
			{
				-num5,
				num6,
				-num9,
				num10
			}));
			this.textureBounds = new VRTextureBounds_t[2];
			this.textureBounds[0].uMin = 0.5f + (0.5f * num3 / this.tanHalfFov.x);
			this.textureBounds[0].uMax = 0.5f + (0.5f * num4 / this.tanHalfFov.x);
			this.textureBounds[0].vMin = 0.5f - (0.5f * num6 / this.tanHalfFov.y);
			this.textureBounds[0].vMax = 0.5f - (0.5f * num5 / this.tanHalfFov.y);
			this.textureBounds[1].uMin = 0.5f + (0.5f * num7 / this.tanHalfFov.x);
			this.textureBounds[1].uMax = 0.5f + (0.5f * num8 / this.tanHalfFov.x);
			this.textureBounds[1].vMin = 0.5f - (0.5f * num10 / this.tanHalfFov.y);
			this.textureBounds[1].vMax = 0.5f - (0.5f * num9 / this.tanHalfFov.y);
			SteamVR.OpenVRMagic.SetSubmitParams(this.textureBounds[0], this.textureBounds[1], EVRSubmitFlags.Submit_Default);
			this.sceneWidth /= Mathf.Max(this.textureBounds[0].uMax - this.textureBounds[0].uMin, this.textureBounds[1].uMax - this.textureBounds[1].uMin);
			this.sceneHeight /= Mathf.Max(this.textureBounds[0].vMax - this.textureBounds[0].vMin, this.textureBounds[1].vMax - this.textureBounds[1].vMin);
			this.aspect = this.tanHalfFov.x / this.tanHalfFov.y;
			this.fieldOfView = 2f * Mathf.Atan(this.tanHalfFov.y) * 57.29578f;
			this.eyes = new SteamVR_Utils.RigidTransform[]
			{
				new SteamVR_Utils.RigidTransform(this.hmd.GetEyeToHeadTransform(EVREye.Eye_Left)),
				new SteamVR_Utils.RigidTransform(this.hmd.GetEyeToHeadTransform(EVREye.Eye_Right))
			};
			GraphicsDeviceType graphicsDeviceType = SystemInfo.graphicsDeviceType;
			if (graphicsDeviceType <= GraphicsDeviceType.OpenGLES3)
			{
				if (graphicsDeviceType != GraphicsDeviceType.OpenGLES2 && graphicsDeviceType != GraphicsDeviceType.OpenGLES3)
				{
					goto IL_3F8;
				}
			}
			else if (graphicsDeviceType != GraphicsDeviceType.OpenGLCore)
			{
				if (graphicsDeviceType == GraphicsDeviceType.Vulkan)
				{
					this.textureType = ETextureType.Vulkan;
					goto IL_3FF;
				}
				goto IL_3F8;
			}
			this.textureType = ETextureType.OpenGL;
			goto IL_3FF;
		IL_3F8:
			this.textureType = ETextureType.DirectX;
		IL_3FF:

			SteamVR_Events.Initializing.Listen(this.OnInitializing);
			SteamVR_Events.Calibrating.Listen(this.OnCalibrating);
			SteamVR_Events.OutOfRange.Listen(this.OnOutOfRange);
			SteamVR_Events.DeviceConnected.Listen(this.OnDeviceConnected);
			SteamVR_Events.NewPoses.Listen(this.OnNewPoses);

			Log.LogInfo("[SteamVR_Standalone] Finished constructing SteamVR");
		}


		~SteamVR()
		{
			this.Dispose(false);
		}


		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}


		private void Dispose(bool disposing)
		{
			Debug.Log("DESTROYED!");
			SteamVR_Events.Initializing.Remove(this.OnInitializing);
			SteamVR_Events.Calibrating.Remove(this.OnCalibrating);
			SteamVR_Events.OutOfRange.Remove(this.OnOutOfRange);
			SteamVR_Events.DeviceConnected.Remove(this.OnDeviceConnected);
			SteamVR_Events.NewPoses.Remove(this.OnNewPoses);
			SteamVR._instance = null;
		}


		public static void SafeDispose()
		{
			if (SteamVR._instance != null)
			{
				SteamVR._instance.Dispose();
			}
		}


		private static bool _enabled = true;


		private static SteamVR _instance;


		public static SteamVR.InitializedStates initializedState = SteamVR.InitializedStates.None;


		public static bool[] connected = new bool[64];


		public ETextureType textureType;


		private static bool runningTemporarySession = false;


		public const string defaultUnityAppKeyTemplate = "application.generated.unity.{0}.exe";


		public const string defaultAppKeyTemplate = "application.generated.{0}";


		public enum InitializedStates
		{

			None,

			Initializing,

			InitializeSuccess,

			InitializeFailure
		}


		public class OpenVRMagic
		{

			public static string DLLName = "openvr_api";

			[DllImport("openvr_api", EntryPoint = "UnityHooks_GetRenderEventFunc")]
			public static extern IntPtr GetRenderEventFunc();


			[DllImport("openvr_api", EntryPoint = "UnityHooks_SetSubmitParams")]
			public static extern void SetSubmitParams(VRTextureBounds_t boundsL, VRTextureBounds_t boundsR, EVRSubmitFlags nSubmitFlags);


			[DllImport("openvr_api", EntryPoint = "UnityHooks_SetColorSpace")]
			public static extern void SetColorSpace(EColorSpace eColorSpace);


			[DllImport("openvr_api", EntryPoint = "UnityHooks_EventWriteString")]
			public static extern void EventWriteString([MarshalAs(UnmanagedType.LPWStr)][In] string sEvent);


			public const int k_nRenderEventID_WaitGetPoses = 201510020;


			public const int k_nRenderEventID_SubmitL = 201510021;


			public const int k_nRenderEventID_SubmitR = 201510022;


			public const int k_nRenderEventID_Flush = 201510023;


			public const int k_nRenderEventID_PostPresentHandoff = 201510024;
		}


		/// <summary>
		/// Most of this code by @Knah https://github.com/knah/VRCMods/blob/master/TrueShaderAntiCrash/TrueShaderAntiCrashMod.cs
		/// Thank him for supporting il2cpp modding ^^
		/// </summary>

		public static class ExternalPluginFunctionExtractor
		{
			// 2019.4.1f1 0x786D00
			// 2019.4.21f1 : 0x0792350

			/// <summary>
			/// Use this if you're using a different engine version. Decompile UnityPlayer.dll with IDA PRO and get the pdb files for it 
			/// </summary>
			public static void SetFindAndLoadPluginFunctionOffset(int offset)
			{
				FindAndLoadUnityPluginOffset = offset;
			}

			public static int FindAndLoadUnityPluginOffset = 0x4E4520;//0x4E4520;//0x2DF5BB;


			public static void GetLoadPluginFunction()
			{
				Debug.Log("Loading external plugin load function");
				var process = Process.GetCurrentProcess();
				foreach (ProcessModule module in process.Modules)
				{
					Debug.Log(module.FileName);
					if (!module.FileName.Contains("UnityPlayer")) continue;

					var loadLibraryAddress = module.BaseAddress + FindAndLoadUnityPluginOffset;
					var dg = Marshal.GetDelegateForFunctionPointer<FindAndLoadUnityPlugin>(loadLibraryAddress);

					var strPtr = Marshal.StringToHGlobalAnsi(OpenVRMagic.DLLName);

					dg(strPtr, out var loaded, true);
					//dg(OpenVRMagic.DLLName, out var loaded, true);

					if (loaded == IntPtr.Zero)
					{
						Debug.LogError("Module load failed");
						return;
					}

					InitUnityHookRenderEventFuncCallback(loaded);

					Marshal.FreeHGlobal(strPtr);

					break;

				}
			}

			public static void InitUnityHookRenderEventFuncCallback(IntPtr hModule)
			{
				ourGetRenderEventFunc = Marshal.GetDelegateForFunctionPointer<CallbackPointer>(GetProcAddress(hModule, "UnityHooks_GetRenderEventFunc"));
			}

			[UnmanagedFunctionPointer(CallingConvention.Winapi)]
			private delegate IntPtr FindAndLoadUnityPlugin(IntPtr name, out IntPtr loadedModule, bool logError);

			[DllImport("kernel32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
			private static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			private delegate IntPtr CallbackPointer();

			private static CallbackPointer ourGetRenderEventFunc;

			[DllImport("openvr_api", EntryPoint = "UnityHooks_GetRenderEventFunc")]
			public static extern IntPtr GetRenderEventFunc();

			//public static IntPtr GetRenderEventFunc() => ourGetRenderEventFunc();

		}
	}
}
