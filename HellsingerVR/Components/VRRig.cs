using Il2CppInterop.Runtime;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Unity.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.XR;
using UnityEngine.SceneManagement;
using Valve.VR;

namespace HellsingerVR.Components
{
	public class VRRig : MonoBehaviour
	{
		public Transform head;
		public Transform leftHand;
		public Transform rightHand;
		public SteamVR_Camera vrCamera;
		public Camera camera;

		public Transform PlayerTransform;
		public Transform CameraTransform;

		float LastLocalHeadYaw;

		// Printing the character extents to log yields: (0.60, 0.80, 0.60)
		const float Height = 1.6f;

		Vector3 InitialPosition;
		Quaternion InitialRotation;

		bool InCutscene = false;

		bool _InLevel = false;

		int CameraMask = 0;

		int uiMask = LayerMask.GetMask("UI");

		public UI.RhythmIndicator rhythmIndicator;
		public UI.Health health;

		public VRViewModelManager viewModelManager;

		public bool InLevel
		{
			get { return _InLevel; }
		}

		public Vector3 GetHeadXZ()
		{
			return new Vector3(head.transform.localPosition.x, 0.0f, head.transform.localPosition.z);
		}

		public void Start()
		{
			CameraMask = vrCamera.TrueMask;
			EnterTitleScreen();
		}

		public void EnterTitleScreen()
		{
			rhythmIndicator = null;
			health = null;

			viewModelManager.enabled = false;

			_InLevel = false;

			transform.position = HellsingerVR.TitleScreenPosition - Vector3.up * Height;

			transform.rotation = Quaternion.Euler(0.0f, -90.0f - head.transform.localRotation.eulerAngles.y, 0.0f);

			PlayerTransform = null;
			CameraTransform = null;
		}

		public void EnterLevel()
		{
			_InLevel = true;
			InCutscene = false;

			viewModelManager.enabled = true;

			HellsingerVR._instance.Log.LogInfo("Entered Level");
		}

		public void EnterCutscene()
		{
			InitialPosition = -head.localPosition;
			InitialRotation = Quaternion.Euler(0.0f, -head.localRotation.eulerAngles.y, 0.0f);

			InCutscene = true;

			FirstPersonController fpController = FindObjectOfType<FirstPersonController>();
			if (fpController)
			{
				PlayerTransform = fpController.m_player.PlayerTransform;
				CameraTransform = fpController.m_cameraTransform;
			}

			HellsingerVR._instance.Log.LogInfo("Entered cutscene");
		}

		public void ExitCutscene()
		{
			HellsingerVR._instance.Log.LogInfo("Left cutscene");
			InCutscene = false;

			LastLocalHeadYaw = head.localRotation.eulerAngles.y;

			transform.position = PlayerTransform.position;
			transform.rotation = PlayerTransform.rotation * Quaternion.Euler(0.0f, -head.localRotation.eulerAngles.y, 0.0f);

			if (rhythmIndicator == null)
			{
				rhythmIndicator = new UI.RhythmIndicator();
			}

			if (health == null)
			{
				health = new UI.Health();
			}

			rhythmIndicator.Init();
			health.Init();

			viewModelManager.HideArms();

			HellsingerVR.RemoveDOF();
			HellsingerVR.MoveOverlayToWorld();
		}

		public void Update()
		{
			if (Input.GetKeyDown(KeyCode.Tab))
			{
				HellsingerVR._instance.Log.LogInfo($"IsPaused: {HellsingerVR.IsPaused}");
				HellsingerVR._instance.Log.LogInfo($"IsLoading: {HellsingerVR.IsLoading}");
				HellsingerVR._instance.Log.LogInfo($"InCutscene: {InCutscene}");
				HellsingerVR._instance.Log.LogInfo($"InLevel: {InLevel}");
				HellsingerVR._instance.Log.LogInfo($"PlayerPosition: {transform.position.ToString()}");
				HellsingerVR._instance.Log.LogInfo($"UIPosition: {GameObject.Find("Overlay").transform.position.ToString()}");

			}

			// Only need to hide the world while in a level
			// Title screen, level select, etc are supposed to render their geo
			if ((HellsingerVR.IsPaused || HellsingerVR.IsLoading) && InLevel)
			{
				vrCamera.TrueMask = uiMask;
			}
			else
			{
				vrCamera.TrueMask = CameraMask;
			}

			if (InLevel)
			{
				UpdateLevel();
			}
		}

		public void LateUpdate()
		{
			// Do UI in LateUpdate so game state is most up to date
			if (InLevel && !InCutscene)
			{
				UpdateTransform();

				if (!HellsingerVR.IsPaused && !HellsingerVR.IsLoading)
				{
					HellsingerVR.MoveOverlayToWorld();
				}

				if (rhythmIndicator != null) rhythmIndicator.Update();
				if (health != null) health.Update();

			}
			else if (InCutscene && CameraTransform != null)
			{
				HellsingerVR.MoveOverlayToWorld();
			}
		}

		public void UpdateLevel()
		{
			if (InCutscene && CameraTransform != null)
			{
				// Pesky DOF
				//HellsingerVR.RemoveDOF();
				transform.position = CameraTransform.position + InitialPosition;
				transform.rotation = Quaternion.Euler(0.0f, CameraTransform.rotation.eulerAngles.y, 0.0f) * InitialRotation;
			}
		}

		public void UpdateTransform()
		{
			if (!PlayerTransform)
			{
				return;
			}

			float DeltaRotation = head.localRotation.eulerAngles.y - LastLocalHeadYaw;
			LastLocalHeadYaw = head.localRotation.eulerAngles.y;

			Vector3 euler = CameraTransform.eulerAngles;

			float NewYaw = euler.y + DeltaRotation;

			CameraTransform.rotation = Quaternion.Euler(head.rotation.eulerAngles.x, NewYaw, 0.0f);

			transform.position = PlayerTransform.position;
			transform.rotation = Quaternion.Euler(0.0f, NewYaw - LastLocalHeadYaw, 0.0f);
		}
	}
}
