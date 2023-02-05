using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using HellsingerVR.Components;
using Il2CppInterop.Runtime.Injection;
using Outsiders.Messages;
using System.Reflection;
using UnityEngine;
using Valve.VR; 

namespace HellsingerVR
{
	[BepInPlugin("LivingFray.HellsingerVR", "HellsingerVR", "0.7.1")]
	public class HellsingerVR : BasePlugin
	{
		private static GameObject vrRig;

		public static VRRig rig;

		public static HellsingerVR _instance;

		public static bool IsLoading = false;

		public static bool IsPaused = false;

		public static bool IsPreLogin = false;

		public static Vector3 TitleScreenPosition = new Vector3(-227.62f, 9.64f, 48.17f);

		public static Canvas overlay;
		private static RectTransform overlayRect;

		public static Quaternion HandOffset = Quaternion.Euler(0.0f, 0.0f, 0.0f);

		// === Config
		// General
		public ConfigEntry<bool> IsVREnabled;
		public ConfigEntry<bool> IsLeftHanded;
		public ConfigEntry<float> BeatVibrationStrength;
		public ConfigEntry<float> BeatVibrationFrequency;
		public ConfigEntry<float> BeatVibrationLength;
		// Locomotion
		public ConfigEntry<float> SnapTurningAngle;
		public ConfigEntry<string> MovementType;
		// UI
		public ConfigEntry<bool> MoveUIVertically;
		public ConfigEntry<bool> ReticleFacesCamera;
		public ConfigEntry<string> ReticleScaling;
		public ConfigEntry<float> MenuUIDistance;
		public ConfigEntry<float> GameUIDistance;
		public ConfigEntry<string> ReticleLocation;
		public ConfigEntry<bool> ShowHealthOnHand;
		public ConfigEntry<bool> ShowUltimateOnHand;
		public ConfigEntry<bool> ShowFuryOnHand;
		public ConfigEntry<bool> ShowWeaponsOnHand;
		public ConfigEntry<bool> ShowBossOnHand;
		// Performance
		public ConfigEntry<int> PostProcessingLevel;


		public override void Load()
		{
			_instance = this;
			// Plugin startup logic
			Log.LogInfo("HellsingerVR loaded");

			SetupConfig();

			if (!IsVREnabled.Value)
			{
				Log.LogInfo("VR has been disabled, aborted loading plugin");
				return;
			}

			TitleScreenPosition += new Vector3(0.0f, 0.0f, MenuUIDistance.Value);

			ClassInjector.RegisterTypeInIl2Cpp<VRRig>();
			ClassInjector.RegisterTypeInIl2Cpp<VRInputManager>();
			ClassInjector.RegisterTypeInIl2Cpp<VRViewModelManager>();

			SteamVR.Log = Log;

			SteamVR.Initialize(false);

			SteamVR_Camera.useHeadTracking = false;

			Log.LogInfo($"VR status: {SteamVR.enabled}");

			Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());

			QualitySettings.vSyncCount = 0;

			Application.runInBackground = true;

			// Built in dynamic resolution scaler targets the monitor refresh rate
			DynamicResolutionScaler.Enabled = false;

			IsPreLogin = true;
		}

		public static void InstantiateVRRig()
		{
			_instance.Log.LogInfo($"InstantiateVRRig");

			SteamVR_Actions._default.Activate(SteamVR_Input_Sources.Any, 0, false);
			SteamVR_Actions._menu.Activate(SteamVR_Input_Sources.Any, 0, false);

			if (vrRig == null)
			{
				/* Hierarchy:
				 * [Origin] - Playspace origin and such
				 *      [Head] - Can contain a camera, is supposed to be used as the game view, cheaper to just blit the right eye to the screen
				 *          [Eyes] - Holds the steamvr camera and a camera for vr view
				 *          [Ears] - Holds steamvr_ears 
				 * 
				 */

				_instance.Log.LogInfo($"Creating [VR Rig]");
				vrRig = new GameObject("[VR Rig]");

				_instance.Log.LogInfo($"Creating [Head]");
				GameObject head = new GameObject("[Head]");
				//head.AddComponent<Camera>();
				head.AddComponent<SteamVR_TrackedObject>();
				head.transform.parent = vrRig.transform;

				head.GetComponent<SteamVR_TrackedObject>().index = SteamVR_TrackedObject.EIndex.Hmd;

				_instance.Log.LogInfo($"Creating [Eyes]");
				GameObject eyes = new GameObject("[Eyes]");
				eyes.transform.parent = head.transform;
				GameObject leftEye = new GameObject("[Left Eye]");
				leftEye.transform.parent = eyes.transform;
				leftEye.AddComponent<Camera>();
				SteamVR_Camera leftCam = leftEye.AddComponent<SteamVR_Camera>();
				leftCam.eye = EVREye.Eye_Left;
				leftCam.Activate();

				GameObject rightEye = new GameObject("[Right Eye]");
				rightEye.transform.parent = eyes.transform;
				rightEye.AddComponent<Camera>();
				SteamVR_Camera rightCam = rightEye.AddComponent<SteamVR_Camera>();
				rightCam.eye = EVREye.Eye_Right;
				rightCam.Activate();

				//TODO: Do ears

				vrRig.AddComponent<VRRig>();
				vrRig.AddComponent<VRInputManager>();
				vrRig.AddComponent<VRViewModelManager>();

				rig = vrRig.GetComponent<VRRig>();
				rig.head = head.transform;
				rig.leftEye = leftEye.GetComponent<Camera>();
				rig.rightEye = rightEye.GetComponent<Camera>();

				rig.viewModelManager = vrRig.GetComponent<VRViewModelManager>();
				rig.viewModelManager.enabled = false;

				_instance.Log.LogInfo($"Finished creating VRRig");
				Object.DontDestroyOnLoad(vrRig);

				LoginHack.FocusGame();
			}
		}

		private void SetupConfig()
		{
			// General
			IsVREnabled = Config.Bind("General", "Enabled", true, "Set to false to disable this mod without uninstalling");
			IsLeftHanded = Config.Bind("General", "LeftHanded", false, "Set to true to use the left hand as the dominant hand. This does not affect controller bindings, which can be configured in SteamVR");
			BeatVibrationStrength = Config.Bind("General", "BeatVibrationStrength", 0.25f, "Strength of the vibration force (0-1) played on the beat");
			BeatVibrationFrequency = Config.Bind("General", "BeatVibrationFrequency", 100.0f, "Frequency (0-320hz) to vibrate the motors at on the beat");
			BeatVibrationLength = Config.Bind("General", "BeatVibrationLength", 50.0f, "Time in milliseconds to vibrate for on the beat");
			// Locomotion
			SnapTurningAngle = Config.Bind("Locomotion", "SnapTurnAmount", 0.0f, "Snap turning angle. Set to 0 or less to use smooth turning");
			MovementType = Config.Bind("Locomotion", "MovementType", "head", "Movement direction, valid options are \"head\", \"hand\", \"offhand\". Defaults to \"head\"");
			// UI
			MoveUIVertically = Config.Bind("UI", "MoveUIVertically", false, "Should the game UI move up and down to be in front of the HMD");
			MenuUIDistance = Config.Bind("UI", "MenuUIDistance", 2.5f, "Distance between the HMD and the menu UIs in meters");
			GameUIDistance = Config.Bind("UI", "GameUIDistance", 2.5f, "Distance between the HMD and the game UIs in meters");
			ReticleLocation = Config.Bind("UI", "ReticleLocation", "target", "Location of the reticle/beat indicator in the world, valid options are \"target\" (location in world the dominant hand is pointing to), \"head\" (floats a fixed distance in front of the camera), \"sights\" (placed above the weapon in the dominant hand akin to ironsights");
			ReticleFacesCamera = Config.Bind("UI", "ReticleFacesCamera", false, "When reticle location is set to target should the reticle always face the camera, or be flat against the surface it hits");
			ReticleScaling = Config.Bind("UI", "ReticleScaling", "partial", "How should the reticle's scale change with distance. Options are \"none\" (reticle will always be the same scale) \"partial\" (reticle will grow with distance but still appear smaller at a distance) or \"full\" (reticle will grow with distance such that it always occupies the same percentage of the screen)");
			ShowHealthOnHand = Config.Bind("UI", "ShowHealthOnHand", true, "Set to false to show the health bar floating in front of the camera instead of attached to the non dominant hand");
			ShowUltimateOnHand = Config.Bind("UI", "ShowUltimateOnHand", true, "Set to false to show the ultimate bar floating in front of the camera instead of attached to the non dominant hand");
			ShowFuryOnHand = Config.Bind("UI", "ShowFuryOnHand", true, "Set to false to show the fury meter floating in front of the camera instead of attached to the non dominant hand");
			ShowWeaponsOnHand = Config.Bind("UI", "ShowUltimateOnHand", true, "Set to false to show the equipped weapons and ammo UI floating in front of the camera instead of attached to the non dominant hand");
			ShowBossOnHand = Config.Bind("UI", "ShowBossOnHand", true, "Set to false to show the boss bar floating in front of the camera instead of attached to the non dominant hand");
			// Performance
			PostProcessingLevel = Config.Bind("Performance", "PostProcessingLevel", 1, "How aggressively post processing effects are removed in the name of performance. (0 = Minimal, 1 = Moderate, 99 = Extreme). Other values will be rounded down to the previous valid option. Only use extreme as a last resort, it will remove every PP effect it can, including the skybox");
		}

		public static void OnLevelLoad(CoreRequestLoadLevelMessage loadLevelMsg)
		{
			if (_instance != null)
			{
				_instance.Log.LogInfo($"Level load: {loadLevelMsg.Level} (show loading: {loadLevelMsg.ShowLoadingScreen})");
			}

			if (Camera.main)
			{
				Camera.main.cullingMask = 0;
				Camera.main.clearFlags = CameraClearFlags.Nothing;
				//Camera.main.allowMSAA = false;
				//HDCamera HDMain = HDCamera.GetOrCreate(Camera.main);
				//HDMain.antialiasing = HDAdditionalCameraData.AntialiasingMode.None;
				//HDMain.SetPostProcessScreenSize(0, 0);
				rig.leftEye.allowHDR = Camera.main.allowHDR;
				rig.rightEye.allowHDR = Camera.main.allowHDR;
			}

			switch (loadLevelMsg.Level)
			{
				case "TitleScreen":
					rig.EnterTitleScreen();
					break;
				case "EndGameScreen":
					rig.EnterTitleScreen();
					break;
				default:
					rig.EnterLevel();
					IsLoading = loadLevelMsg.ShowLoadingScreen;
					break;
			}
		}

		public static void DisableUIDepth()
		{
			Canvas canvas = Object.FindObjectOfType<Canvas>();

			CanvasRenderer[] canvasRenderers = canvas.GetComponentsInChildren<CanvasRenderer>();

			foreach (CanvasRenderer renderer in canvasRenderers)
			{
				renderer.GetMaterial().SetInt("unity_GUIZTestMode", (int)UnityEngine.Rendering.CompareFunction.Always);
			}
		}

		public static void MoveTitleToWorld()
		{
			HellsingerVR._instance.Log.LogInfo("Moving title screen to world space");
			Canvas c = UnityEngine.Object.FindObjectOfType<Canvas>();
			if (c)
			{
				c.renderMode = RenderMode.WorldSpace;
				c.transform.position = TitleScreenPosition + (Vector3.back * _instance.MenuUIDistance.Value);
				c.transform.rotation = Quaternion.Euler(0.0f, 180.0f, 0.0f);
				RectTransform rect = c.GetComponent<RectTransform>();
				c.transform.localScale = Vector3.one * (2.0f / rect.rect.height);
			}
			else
			{
				HellsingerVR._instance.Log.LogInfo("No canvas!");
			}

			if (Camera.main)
			{
				Camera.main.cullingMask = 0;
				Camera.main.clearFlags = CameraClearFlags.Nothing;
			}
		}

		public static void MoveLevelSelectToWorld()
		{
			Canvas c = UnityEngine.Object.FindObjectOfType<Canvas>();
			if (c == null)
			{
				return;
			}
			_instance.Log.LogInfo("Moving level select to world space");
			c.renderMode = RenderMode.WorldSpace;
			c.transform.position = TitleScreenPosition + (Vector3.back * _instance.MenuUIDistance.Value);
			c.transform.rotation = Quaternion.Euler(0.0f, 180.0f, 0.0f);
			RectTransform rect = c.GetComponent<RectTransform>();
			c.transform.localScale = Vector3.one * (2.0f / rect.rect.height);
		}

		public static void MoveOverlayToWorld(bool bIsMenuUI = false)
		{
			if (overlay == null || overlayRect == null)
			{
				GameObject overlayGO = GameObject.Find("Overlay");

				if (overlayGO != null)
				{
					overlay = overlayGO.GetComponent<Canvas>();
					overlayRect = overlay.GetComponent<RectTransform>();
				}

				if (overlay == null || overlayRect == null)
				{
					return;
				}
			}

			Vector3 Forward = rig.head.transform.forward;

			if (!_instance.MoveUIVertically.Value)
			{
				Forward = Forward.SetY(0.0f);
				Forward.Normalize();
			}

			overlay.renderMode = RenderMode.WorldSpace;
			overlay.transform.position = rig.head.transform.position + (Forward * (bIsMenuUI ? _instance.MenuUIDistance.Value : _instance.GameUIDistance.Value));
			overlay.transform.LookAt(rig.head);
			overlay.transform.rotation = Quaternion.Euler(0.0f, overlay.transform.rotation.eulerAngles.y + 180.0f, 0.0f);
			overlay.transform.localScale = Vector3.one * (2.0f / overlayRect.rect.height);
		}
	}
}
