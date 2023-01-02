//======= Copyright (c) Valve Corporation, All rights reserved. ===============

using UnityEngine;

namespace Valve.VR
{
	public class SteamVR_Settings
	{
		private static SteamVR_Settings _instance;
		public static SteamVR_Settings instance
		{
			get
			{
				LoadInstance();

				return _instance;
			}
		}

		public bool pauseGameWhenDashboardVisible = false;
		public bool lockPhysicsUpdateRateToRenderFrequency = false;
		public ETrackingUniverseOrigin trackingSpace
		{
			get
			{
				return trackingSpaceOrigin;
			}
			set
			{
				trackingSpaceOrigin = value;
				if (SteamVR_Behaviour.isPlaying)
					SteamVR_Action_Pose.SetTrackingUniverseOrigin(trackingSpaceOrigin);
			}
		}


		private ETrackingUniverseOrigin trackingSpaceOrigin = ETrackingUniverseOrigin.TrackingUniverseStanding;


		public string actionsFilePath = "actions.json";


		public string steamVRInputPath = "SteamVR_Input";

		public SteamVR_UpdateModes inputUpdateMode = SteamVR_UpdateModes.OnUpdate;
		public SteamVR_UpdateModes poseUpdateMode = SteamVR_UpdateModes.OnLateUpdate;

		public bool activateFirstActionSetOnStart = true;


		public string editorAppKey;

		public bool autoEnableVR = true;


		public bool legacyMixedRealityCamera = true;

		public SteamVR_Action_Pose mixedRealityCameraPose = SteamVR_Input.GetPoseAction("ExternalCamera");

		public SteamVR_Input_Sources mixedRealityCameraInputSource = SteamVR_Input_Sources.Camera;

		public bool mixedRealityActionSetAutoEnable = true;

		public GameObject previewHandLeft;

		public GameObject previewHandRight;


		private const string previewLeftDefaultAssetName = "vr_glove_left_model_slim";
		private const string previewRightDefaultAssetName = "vr_glove_right_model_slim";


		public bool IsInputUpdateMode(SteamVR_UpdateModes tocheck)
		{
			return (inputUpdateMode & tocheck) == tocheck;
		}
		public bool IsPoseUpdateMode(SteamVR_UpdateModes tocheck)
		{
			return (poseUpdateMode & tocheck) == tocheck;
		}

		public static void VerifyScriptableObject()
		{
			LoadInstance();
		}

		private static void LoadInstance()
		{
			if (_instance == null)
			{
				//_instance = Resources.Load<SteamVR_Settings>("SteamVR_Settings");

				if (_instance == null)
				{
					_instance = new SteamVR_Settings();
				}

				SetDefaultsIfNeeded();
			}
		}

		private const string defaultSettingsAssetName = "SteamVR_Settings";

		private static void SetDefaultsIfNeeded()
		{
			if (string.IsNullOrEmpty(_instance.editorAppKey))
			{
				_instance.editorAppKey = SteamVR.GenerateAppKey();
				Debug.Log("<b>[SteamVR_Standalone Setup]</b> Generated you an editor app key of: " + _instance.editorAppKey + ". This lets the editor tell SteamVR_Standalone what project this is. Has no effect on builds. This can be changed in Assets/SteamVR_Standalone/Resources/SteamVR_Settings");

			}


		}

		private static GameObject FindDefaultPreviewHand(string assetName)
		{

			return null;

		}
	}
}