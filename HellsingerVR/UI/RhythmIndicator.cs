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
		Transform RhythmIndicatorTrans;

		static int layermask = ~LayerMask.GetMask("Ignore Raycast");

		public enum Position {Head, Sights, Target};

		// TODO: Put into a proper config
		public Position position = Position.Target;

		// TODO: put into a proper config
		public float head_distance = 2.5f;

		public float target_distance = 10.0f;
		public float target_offset = 0.25f;

		public float head_scale = 1.0f;
		public float sights_scale = 0.5f;
		public float target_scale = 2.0f;

		Vector3 SightOffset = new Vector3(0.05f, 0.25f, 0.0f);

		Transform LowAmmoIndicator;
		Transform NoAmmoIndicator;
		Transform BeatGradingContainer;
		Transform Reticle;

		public void Init()
		{
			Main main = Main.GetInstance();
			Transform SharedHUD = main.transform.Find("UIRoot/Overlay/Layer-HUD/HUD(Clone)/Shared");

			RhythmIndicatorTrans = SharedHUD.Find("RhythmIndicator");

			Reticle = SharedHUD.Find("Reticle");

			LowAmmoIndicator = SharedHUD.Find("LowAmmoIndicator");
			NoAmmoIndicator = SharedHUD.Find("NoAmmoIndicator");
			BeatGradingContainer = SharedHUD.Find("BeatGradingContainer");
		}

		public void Update()
		{
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
				HellsingerVR._instance.Log.LogInfo(""+head_distance);
			}
			if (Input.GetKeyDown(KeyCode.RightBracket))
			{
				head_distance += 0.2f;
				HellsingerVR._instance.Log.LogInfo(""+head_distance);
			}

			if (Input.GetKeyDown(KeyCode.RightArrow))
			{
				position = Position.Sights;
			}

			RhythmIndicatorTrans.position = transform.position + transform.forward * head_distance;
			RhythmIndicatorTrans.LookAt(transform);
			RhythmIndicatorTrans.rotation *= Quaternion.Euler(0.0f, 180.0f, 0.0f);

			FixOtherUIElements(head_scale);
		}

		void Update_Sights()
		{
			if (Input.GetKeyDown(KeyCode.RightArrow))
			{
				position = Position.Target;
			}

			// TODO: Handedness setting
			bool bFromLeftHand = false;

			(Vector3 location, Quaternion rotation) = VRInputManager.GetHandTransform(bFromLeftHand);

			// TODO: Per gun offset
			// TODO: Scale

			RhythmIndicatorTrans.position = location + (rotation * SightOffset);
			RhythmIndicatorTrans.rotation = rotation;

			FixOtherUIElements(sights_scale);
		}

		void Update_Target()
		{
			if (Input.GetKeyDown(KeyCode.RightArrow))
			{
				position = Position.Head;
			}

			// TODO: Handedness setting
			bool bFromLeftHand = false;

			(Vector3 location, Quaternion rotation) = VRInputManager.GetHandTransform(bFromLeftHand);

			RaycastHit raycastHit;

			bool Hit = Physics.Raycast(location, rotation * Vector3.forward, out raycastHit, target_distance, layermask, QueryTriggerInteraction.Ignore);

			Vector3 OutPoint = raycastHit.point;
			Vector3 OutNormal = raycastHit.normal;

			if (!Hit)
			{
				OutPoint = location + rotation * Vector3.forward * target_distance;
				OutNormal = rotation * Vector3.back;
			}

			Vector3 direction = (OutPoint - location).normalized;

			RhythmIndicatorTrans.position = OutPoint - direction * target_offset;

			RhythmIndicatorTrans.LookAt(OutPoint - OutNormal);

			FixOtherUIElements(target_scale);
		}

		void FixOtherUIElements(float scale)
		{
			RhythmIndicatorTrans.localScale = Vector3.one * scale;
			LowAmmoIndicator.localScale = RhythmIndicatorTrans.localScale;
			NoAmmoIndicator.localScale = RhythmIndicatorTrans.localScale;
			BeatGradingContainer.localScale = RhythmIndicatorTrans.localScale;
			Reticle.localScale = RhythmIndicatorTrans.localScale;

			Reticle.position = RhythmIndicatorTrans.position;
			Reticle.rotation = RhythmIndicatorTrans.rotation;

			LowAmmoIndicator.position = RhythmIndicatorTrans.position + RhythmIndicatorTrans.up * 0.25f * scale;
			LowAmmoIndicator.rotation = RhythmIndicatorTrans.rotation;

			NoAmmoIndicator.position = LowAmmoIndicator.position;
			NoAmmoIndicator.rotation = LowAmmoIndicator.rotation;

			BeatGradingContainer.position = RhythmIndicatorTrans.position - RhythmIndicatorTrans.up * 0.25f * scale;
			BeatGradingContainer.rotation = RhythmIndicatorTrans.rotation;
		}
	}
}