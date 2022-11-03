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

namespace HellsingerVR
{
	[BepInPlugin("LivingFray.HellsingerVR", "HellsingerVR", "0.1.0")]
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

		static Transform overlayTrans;

		public static Quaternion HandOffset = Quaternion.Euler(0.0f, 0.0f, 0.0f);

		public override void Load()
		{
			_instance = this;
			// Plugin startup logic
			Log.LogInfo("HellsingerVR loaded");

			ClassInjector.RegisterTypeInIl2Cpp<VRRig>();
			ClassInjector.RegisterTypeInIl2Cpp<VRInputManager>();

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
				head.AddComponent<Camera>();
				head.AddComponent<SteamVR_TrackedObject>();
				head.transform.parent = vrRig.transform;

				head.GetComponent<SteamVR_TrackedObject>().index = SteamVR_TrackedObject.EIndex.Hmd;
				head.GetComponent<Camera>().backgroundColor = new Color(0.0f, 0.0f, 0.0f);
				head.GetComponent<Camera>().clearFlags = CameraClearFlags.Color;

				_instance.Log.LogInfo($"Creating [Eyes]");
				GameObject eyes = new GameObject("[Eyes]");
				eyes.transform.parent = head.transform;
				eyes.AddComponent<Camera>();
				eyes.AddComponent<SteamVR_Camera>();

				eyes.GetComponent<Camera>().enabled = false;

				//TODO: Do ears

				vrRig.AddComponent<VRRig>();
				vrRig.AddComponent<VRInputManager>();

				rig = vrRig.GetComponent<VRRig>();

				rig.head = head.transform;
				// Watch out for scene changes straight up deleting the rig lol

				_instance.Log.LogInfo($"Finished creating VRRig");
				Object.DontDestroyOnLoad(vrRig);
			}
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
			Debug.Log("Moving title screen to world space");
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
				Debug.Log("No canvas!");
			}

			RemoveDOF();

			/*
			Camera.main.orthographic = true;
			Camera.main.orthographicSize = 1.0f;
			Camera.main.nearClipPlane = 1.0f;
			Camera.main.farClipPlane = 1.0f;
			Camera.main.clearFlags = CameraClearFlags.Nothing;
			Camera.main.cullingMask = 0;
			Camera.main.eventMask = 0;
			Camera.main.useOcclusionCulling = false;
			*/
			//Camera.main.rect = new Rect(0.0f, 0.0f, 100.0f, 100.0f);
			//Camera.main.cullingMask = 0;

			if (Camera.main)
			{
				Camera.main.enabled = false;
			}
		}

		public static void MoveLevelSelectToWorld()
		{
			Canvas c = UnityEngine.Object.FindObjectOfType<Canvas>();
			if (c == null)
			{
				return;
			}
			Debug.Log("Moving level select to world space");
			Debug.Log(c.name + " of " + c.gameObject.name);
			c.renderMode = RenderMode.WorldSpace;
			c.transform.position = TitleScreenPosition + Vector3.back * 3.5f;
			c.transform.rotation = Quaternion.Euler(0.0f, 180.0f, 0.0f);
			RectTransform rect = c.GetComponent<RectTransform>();
			c.transform.localScale = Vector3.one * (2.0f / rect.rect.height);
		}

		public static void MoveOverlayToWorld()
		{
			if (overlay == null)
			{
				GameObject overlayGO = GameObject.Find("Overlay");
				
				if (overlayGO != null)
				{
					overlay = overlayGO.GetComponent<Canvas>();
				}
				
				if (overlay == null)
				{
					return;
				}
			}

			if (overlay.renderMode != RenderMode.WorldSpace)
			{
				overlayTrans = overlay.transform;
			}

			overlay.renderMode = RenderMode.WorldSpace;
			overlay.transform.position = rig.head.transform.position + rig.head.transform.forward * 3.5f;
			overlay.transform.LookAt(rig.head);
			overlay.transform.rotation = Quaternion.Euler(0.0f, overlay.transform.rotation.eulerAngles.y + 180.0f, 0.0f);
			RectTransform rect = overlay.GetComponent<RectTransform>();
			overlay.transform.localScale = Vector3.one * (2.0f / rect.rect.height);
		}

		// Might not go with this in the end?
		public static void MoveOverlayToScreen()
		{
			if (overlay)
			{
				overlay.renderMode = RenderMode.ScreenSpaceOverlay;
				overlay.transform.position = overlayTrans.position;
				overlay.transform.rotation = overlayTrans.rotation;
				overlay.transform.localScale = overlayTrans.localScale;
			}
		}
	}
}
