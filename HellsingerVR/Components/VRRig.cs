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

		public Transform PlayerTransform;
		public Transform CameraTransform;

		float LastLocalHeadYaw;

		// Printing the character extents to log yields: (0.60, 0.80, 0.60)
		const float Height = 1.6f;

		Vector3 InitialPosition;
		Quaternion InitialRotation;

		bool InCutscene = false;

		bool _InLevel = false;

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

			if (Camera.main)
			{
				Camera.main.enabled = true;
				Camera.main.cullingMask = 0;
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

			HellsingerVR.MoveOverlayToWorld();

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

			if (Camera.main)
			{
				Camera.main.cullingMask = 0;
				Camera.main.enabled = false;
			}

			HellsingerVR.RemoveDOF();
		}

		public void Update()
		{
			if (InLevel)
			{
				UpdateLevel();
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
				HellsingerVR.MoveOverlayToWorld();
			}

			if (!InCutscene)
			{
				UpdateTransform();

				if (rhythmIndicator != null) rhythmIndicator.Update();
				if (health != null) health.Update();
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
