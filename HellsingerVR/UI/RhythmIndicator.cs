using HellsingerVR.Components;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Valve.VR;

namespace HellsingerVR.UI
{
	public class RhythmIndicator
	{
		GameObject gameObject;

		static int layermask = LayerMask.NameToLayer("Ignore Raycast");

		public enum Position {Head, Sights, Target};

		// TODO: Put into a proper config
		public Position position = Position.Target;

		// TODO: put into a proper config
		public float head_distance = 2.5f;

		public float target_distance = 5.0f;
		public float target_offset = 0.25f;

		Vector3 SightOffset = new Vector3(0.05f, 0.25f, 0.0f);

		Transform LeftContainer;
		Transform RightContainer;
		Transform LowAmmoIndicator;
		Transform NoAmmoIndicator;
		Transform BeatGradingContainer;

		public void Init()
		{
			gameObject = GameObject.Find("RhythmIndicator");
			if (gameObject == null)
			{
				Debug.LogError("Could not find rhythm indicator");
				return;
			}

			Transform reticle = gameObject.transform.parent.Find("Reticle");
			if (reticle)
			{
				reticle.gameObject.SetActive(false);
			}

			LeftContainer = gameObject.transform.Find("LeftContainer");
			RightContainer = gameObject.transform.Find("RightContainer");
			LowAmmoIndicator = gameObject.transform.parent.Find("LowAmmoIndicator");
			NoAmmoIndicator = gameObject.transform.parent.Find("NoAmmoIndicator");
			BeatGradingContainer = gameObject.transform.parent.Find("BeatGradingContainer");
		}

		public void Update()
		{
			if (!gameObject) return;

			switch (position)
			{
				case Position.Head:
					Update_Head();
					break;
				case Position.Sights:
					Update_Sights();
					break;
				case Position.Target:
					Update_Target();
					break;
			}
		}

		void Update_Head()
		{
			Transform transform = HellsingerVR.rig.head;

			if (Input.GetKeyDown(KeyCode.LeftBracket))
			{
				head_distance -= 0.2f;
				Debug.Log(""+head_distance);
			}
			if (Input.GetKeyDown(KeyCode.RightBracket))
			{
				head_distance += 0.2f;
				Debug.Log(""+head_distance);
			}

			if (Input.GetKeyDown(KeyCode.RightArrow))
			{
				position = Position.Sights;
			}

			gameObject.transform.position = transform.position + transform.forward * head_distance;
			gameObject.transform.LookAt(transform);

			gameObject.transform.rotation *= Quaternion.Euler(0.0f, 180.0f, 0.0f);

			FixOtherUIElements();
		}

		void Update_Sights()
		{
			if (Input.GetKeyDown(KeyCode.RightArrow))
			{
				position = Position.Target;
			}

			// TODO: Handedness setting
			bool bFromLeftHand = false;

			Vector3 location = SteamVR_Input.GetAction<SteamVR_Action_Pose>("Pose").GetLocalPosition(bFromLeftHand ? SteamVR_Input_Sources.LeftHand : SteamVR_Input_Sources.RightHand);
			Quaternion rotation = SteamVR_Input.GetAction<SteamVR_Action_Pose>("Pose").GetLocalRotation(bFromLeftHand ? SteamVR_Input_Sources.LeftHand : SteamVR_Input_Sources.RightHand);

			location = HellsingerVR.rig.transform.TransformPoint(location);

			rotation = HellsingerVR.rig.transform.rotation * rotation * HellsingerVR.HandOffset;

			// TODO: Per gun offset
			// TODO: Scale

			gameObject.transform.position = location + (rotation * SightOffset);
			gameObject.transform.rotation = rotation;

			FixOtherUIElements();
		}

		void Update_Target()
		{
			if (Input.GetKeyDown(KeyCode.RightArrow))
			{
				position = Position.Head;
			}

			// TODO: Handedness setting
			bool bFromLeftHand = false;

			Vector3 location = SteamVR_Input.GetAction<SteamVR_Action_Pose>("Pose").GetLocalPosition(bFromLeftHand ? SteamVR_Input_Sources.LeftHand : SteamVR_Input_Sources.RightHand);
			Quaternion rotation = SteamVR_Input.GetAction<SteamVR_Action_Pose>("Pose").GetLocalRotation(bFromLeftHand ? SteamVR_Input_Sources.LeftHand : SteamVR_Input_Sources.RightHand);

			location = HellsingerVR.rig.transform.TransformPoint(location);

			rotation = HellsingerVR.rig.transform.rotation * rotation * HellsingerVR.HandOffset;

			RaycastHit raycastHit;

			Physics.Raycast(location, rotation * Vector3.forward, out raycastHit, target_distance, layermask);

			Vector3 OutPoint = raycastHit.point;
			Vector3 OutNormal = raycastHit.normal;

			if (raycastHit.distance < float.Epsilon)
			{
				OutPoint = location + rotation * Vector3.forward * target_distance;
				OutNormal = rotation * Vector3.back;
			}

			Vector3 direction = (OutPoint - location).normalized;

			gameObject.transform.position = OutPoint - direction * target_offset;

			gameObject.transform.LookAt(OutPoint - OutNormal);

			FixOtherUIElements();
		}

		void FixOtherUIElements()
		{
			Quaternion rot = Quaternion.Inverse(gameObject.transform.localRotation);

			if (!LeftContainer || !RightContainer)
			{
				LeftContainer = gameObject.transform.Find("LeftContainer");
				RightContainer = gameObject.transform.Find("RightContainer");
			}

			if (!LeftContainer || !RightContainer)
			{
				return;
			}

			LeftContainer.localRotation = rot;
			RightContainer.localRotation = rot;

			// This better not backfire, but overlay needs same rotation as beats for tween to work
			// (Could probably fix it by creating a separate overlay for it)
			HellsingerVR.overlay.transform.rotation = gameObject.transform.rotation;

			LowAmmoIndicator.position = gameObject.transform.position + gameObject.transform.up * 0.25f;
			LowAmmoIndicator.rotation = gameObject.transform.rotation;

			NoAmmoIndicator.position = LowAmmoIndicator.position;
			NoAmmoIndicator.rotation = LowAmmoIndicator.rotation;

			BeatGradingContainer.position = gameObject.transform.position - gameObject.transform.up * 0.25f;
			BeatGradingContainer.rotation = gameObject.transform.rotation;
		}
	}
}