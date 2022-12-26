using BepInEx;
using BepInEx.Unity.IL2CPP;
using Valve.VR;
using UnityEngine;
using HarmonyLib;
using System.Reflection;
using Il2CppInterop.Runtime.Injection;
using Outsiders.Messages;
using System.IO;
using HellsingerVR.Components;
using UnityEngine.Rendering.HighDefinition;
using Il2CppInterop.Runtime;
using BepInEx.Configuration;

namespace HellsingerVR
{
	[BepInPlugin("LivingFray.HellsingerVR", "HellsingerVR", "0.2.0")]
	public class HellsingerVR : BasePlugin
	{
		static GameObject vrRig;

		public static VRRig rig;

		public static HellsingerVR _instance;

		public static bool IsLoading = false;

		public static bool IsPaused = false;

		public static bool IsPreLogin = false;

		public static Vector3 TitleScreenPosition = new Vector3(-227.62f, 9.64f, 48.17f + 3.0f);

		public static Canvas overlay;

		static RectTransform overlayRect;

		public static Quaternion HandOffset = Quaternion.Euler(0.0f, 0.0f, 0.0f);

		// === Config
		// General
		public ConfigEntry<bool> IsVREnabled;
		public ConfigEntry<bool> IsLeftHanded;
		// Locomotion
		public ConfigEntry<float> SnapTurningAngle;
		public ConfigEntry<string> MovementType;
		// UI
		public ConfigEntry<string> ReticleLocation;
		public ConfigEntry<bool> ShowHealthOnHand;
		public ConfigEntry<bool> ShowUltimateOnHand;
		public ConfigEntry<bool> ShowFuryOnHand;
		public ConfigEntry<bool> ShowWeaponsOnHand;
		public ConfigEntry<bool> ShowBossOnHand;


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

			IsPreLogin = true;
		}

		public static void InstantiateVRRig()
		{
			_instance.Log.LogInfo($"InstantiateVRRig");

			SteamVR_Actions._default.Activate(SteamVR_Input_Sources.Any, 0, false);
			SteamVR_Actions._menu.Activate(SteamVR_Input_Sources.Any, 0, false);

			if (vrRig == null)
			{
				/* The slightly confusing hierarchy:
				 * [Origin] - Playspace origin and such
				 *      [Head] - Can contain a camera, is supposed to be used as the game view, but is currently broken
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
				eyes.AddComponent<Camera>();
				eyes.AddComponent<SteamVR_Camera>();

				eyes.GetComponent<Camera>().enabled = false;
				eyes.GetComponent<Camera>().nearClipPlane = 0.01f;

				//TODO: Do ears

				vrRig.AddComponent<VRRig>();
				vrRig.AddComponent<VRInputManager>();
				vrRig.AddComponent<VRViewModelManager>();

				rig = vrRig.GetComponent<VRRig>();
				rig.head = head.transform;
				rig.vrCamera = eyes.GetComponent<SteamVR_Camera>();
				rig.camera = eyes.GetComponent<Camera>();

				rig.camera.backgroundColor = new Color(0.0f, 0.0f, 0.0f);
				rig.camera.clearFlags = CameraClearFlags.Color;

				rig.viewModelManager = vrRig.GetComponent<VRViewModelManager>();
				rig.viewModelManager.enabled = false;

				_instance.Log.LogInfo($"Finished creating VRRig");
				Object.DontDestroyOnLoad(vrRig);
			}
		}

		private void SetupConfig()
		{
			// General
			IsVREnabled = Config.Bind("General", "Enabled", true, "Set to false to disable this mod without uninstalling");
			IsLeftHanded = Config.Bind("General", "LeftHanded", false, "Set to true to use the left hand as the dominant hand. This does not affect controller bindings, which can be configured in SteamVR");
			// Locomotion
			SnapTurningAngle = Config.Bind("Locomotion", "SnapTurnAmount", 0.0f, "Snap turning angle. Set to 0 or less to use smooth turning");
			MovementType = Config.Bind("Locomotion", "MovementType", "head", "Movement direction, valid options are \"head\", \"hand\", \"offhand\". Defaults to \"head\"");
			// UI
			ReticleLocation = Config.Bind("UI", "ReticleLocation", "target", "Location of the reticle/beat indicator in the world, valid options are \"target\" (location in world the dominant hand is pointing to), \"head\" (floats a fixed distance in front of the camera), \"sights\" (placed above the weapon in the dominant hand akin to ironsights");
			ShowHealthOnHand = Config.Bind("UI", "ShowHealthOnHand", true, "Set to false to show the health bar floating in front of the camera instead of attached to the non dominant hand");
			ShowUltimateOnHand = Config.Bind("UI", "ShowUltimateOnHand", true, "Set to false to show the ultimate bar floating in front of the camera instead of attached to the non dominant hand");
			ShowFuryOnHand = Config.Bind("UI", "ShowFuryOnHand", true, "Set to false to show the fury meter floating in front of the camera instead of attached to the non dominant hand");
			ShowWeaponsOnHand = Config.Bind("UI", "ShowUltimateOnHand", true, "Set to false to show the equipped weapons and ammo UI floating in front of the camera instead of attached to the non dominant hand");
			ShowBossOnHand = Config.Bind("UI", "ShowBossOnHand", true, "Set to false to show the boss bar floating in front of the camera instead of attached to the non dominant hand");
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
			}

			RemoveDOF();

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

		public static void RemoveDOF()
		{
			DepthOfField[] dof = Resources.FindObjectsOfTypeAll<DepthOfField>();
			foreach (DepthOfField d in dof)
			{
				if (d.active)
				{
					d.active = false;
				}
			}
		}

		public static void MoveTitleToWorld()
		{
			HellsingerVR._instance.Log.LogInfo("Moving title screen to world space");
			Canvas c = UnityEngine.Object.FindObjectOfType<Canvas>();
			if (c)
			{
				c.renderMode = RenderMode.WorldSpace;
				c.transform.position = TitleScreenPosition + Vector3.back * 3.5f;
				c.transform.rotation = Quaternion.Euler(0.0f, 180.0f, 0.0f);
				RectTransform rect = c.GetComponent<RectTransform>();
				c.transform.localScale = Vector3.one * (2.0f / rect.rect.height);
			}
			else
			{
				HellsingerVR._instance.Log.LogInfo("No canvas!");
			}

			RemoveDOF();

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
			c.transform.position = TitleScreenPosition + Vector3.back * 3.5f;
			c.transform.rotation = Quaternion.Euler(0.0f, 180.0f, 0.0f);
			RectTransform rect = c.GetComponent<RectTransform>();
			c.transform.localScale = Vector3.one * (2.0f / rect.rect.height);
		}

		public static void MoveOverlayToWorld()
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

			overlay.renderMode = RenderMode.WorldSpace;
			overlay.transform.position = rig.head.transform.position + rig.head.transform.forward * 3.5f;
			overlay.transform.LookAt(rig.head);
			overlay.transform.rotation = Quaternion.Euler(0.0f, overlay.transform.rotation.eulerAngles.y + 180.0f, 0.0f);
			overlay.transform.localScale = Vector3.one * (2.0f / overlayRect.rect.height);
		}
	}
}
