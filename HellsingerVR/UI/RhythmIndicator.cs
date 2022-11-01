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

		public enum Position {Head, Sights, Target};

		// TODO: Put into a proper config
		public Position position = Position.Head;

		// TODO: put into a proper config
		public float head_distance = 2.5f;

		Vector3 SightOffset = new Vector3(0.05f, 0.25f, 0.0f);

		public void Init()
		{
			gameObject = GameObject.Find("RhythmIndicator");
			if (gameObject == null)
			{
				Debug.LogError("Could not find rhythm indicator");
				return;
			}

			// Each rhythm beat part needs its local transform adjusting to line up properly at the new scale
			if (gameObject.transform.FindChild("VR_Rescaled"))
			{
				return;
			}

			// Mark the object so we don't reapply the scaling later
			(new GameObject("VR_Rescaled")).transform.parent = gameObject.transform;
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

			FixBeats();
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

			rotation *= HellsingerVR.HandOffset;

			// TODO: Per gun offset

			gameObject.transform.position = location + (rotation * SightOffset);
			gameObject.transform.rotation = rotation;

			FixBeats();
		}

		void Update_Target()
		{
			if (Input.GetKeyDown(KeyCode.RightArrow))
			{
				position = Position.Head;
			}
		}

		void FixBeats()
		{
			Quaternion rot = Quaternion.Inverse(gameObject.transform.localRotation);

			gameObject.transform.FindChild("LeftContainer").localRotation = rot;
			gameObject.transform.FindChild("RightContainer").localRotation = rot;

			// This better not backfire, but overlay needs same rotation as beats for tween to work
			// (Could probably fix it by creating a separate overlay for it)
			HellsingerVR.overlay.transform.rotation = gameObject.transform.rotation;
		}
	}
}