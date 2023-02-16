using HellsingerVR.Components;
using UnityEngine;

namespace HellsingerVR.UI
{
	public class RhythmIndicator
	{
		private Transform RhythmIndicatorTrans;
		private static int layermask = ~LayerMask.GetMask("Ignore Raycast");

		public enum Position { Head, Sights, Target };

		public enum Scaling { None, Partial, Full };

		public Position position = Position.Target;

		public Scaling scaling = Scaling.Partial;

		public bool FaceCamera = false;

		public float head_distance = 2.5f;

		public float target_distance = 25.0f;
		public float target_offset = 0.25f;

		public float head_scale = 1.0f;
		public float sights_scale = 0.5f;
		public float target_scale = 2.0f;
		private Vector3 SightOffset = new Vector3(0.05f, 0.25f, 0.0f);
		private Transform LowAmmoIndicator;
		private Transform NoAmmoIndicator;
		private Transform BeatGradingContainer;
		private Transform Reticle;

		public void Init()
		{
			Main main = Main.GetInstance();
			Transform SharedHUD = main.transform.Find("UIRoot/Overlay/Layer-HUD/HUD(Clone)/Shared");

			RhythmIndicatorTrans = SharedHUD.Find("RhythmIndicator");

			Reticle = SharedHUD.Find("Reticle");

			LowAmmoIndicator = SharedHUD.Find("LowAmmoIndicator");
			NoAmmoIndicator = SharedHUD.Find("NoAmmoIndicator");
			BeatGradingContainer = SharedHUD.Find("BeatGradingContainer");

			head_distance = HellsingerVR._instance.GameUIDistance.Value;

			string locFromPos = HellsingerVR._instance.ReticleLocation.Value.ToLower();

			switch (locFromPos)
			{
				case "sights":
					position = Position.Sights;
					break;
				case "head":
					position = Position.Head;
					break;
				default:
					position = Position.Target;
					break;
			}

			string scalingCfg = HellsingerVR._instance.ReticleScaling.Value.ToLower();

			switch (scalingCfg)
			{
				case "none":
					scaling = Scaling.None;
					break;
				case "full":
					scaling = Scaling.Full;
					break;
				default:
					scaling = Scaling.Partial;
					break;
			}

			FaceCamera = HellsingerVR._instance.ReticleFacesCamera.Value;

			if (HellsingerVR._instance.DisableMotionControls.Value)
			{
				position = Position.Head;
			}
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

		private void Update_Head()
		{
			Transform transform = HellsingerVR.rig.head;

			if (Input.GetKeyDown(KeyCode.LeftBracket))
			{
				head_distance -= 0.2f;
				HellsingerVR._instance.Log.LogInfo("" + head_distance);
			}
			if (Input.GetKeyDown(KeyCode.RightBracket))
			{
				head_distance += 0.2f;
				HellsingerVR._instance.Log.LogInfo("" + head_distance);
			}

			if (Input.GetKeyDown(KeyCode.RightArrow))
			{
				position = Position.Sights;
			}

			RhythmIndicatorTrans.position = transform.position + (transform.forward * head_distance);
			RhythmIndicatorTrans.LookAt(transform);
			RhythmIndicatorTrans.rotation *= Quaternion.Euler(0.0f, 180.0f, 0.0f);

			FixOtherUIElements(head_scale);
		}

		private void Update_Sights()
		{
			if (Input.GetKeyDown(KeyCode.RightArrow))
			{
				position = Position.Target;
			}

			bool bFromLeftHand = HellsingerVR._instance.IsLeftHanded.Value;
			(Vector3 location, Quaternion rotation) = VRInputManager.GetHandTransform(bFromLeftHand);

			// TODO: Per gun offset
			// TODO: Scale

			RhythmIndicatorTrans.position = location + (rotation * SightOffset);
			RhythmIndicatorTrans.rotation = rotation;

			FixOtherUIElements(sights_scale);
		}

		private void Update_Target()
		{
			if (Input.GetKeyDown(KeyCode.RightArrow))
			{
				position = Position.Head;
			}

			bool bFromLeftHand = HellsingerVR._instance.IsLeftHanded.Value;
			(Vector3 location, Quaternion rotation) = VRInputManager.GetHandTransform(bFromLeftHand);

			RaycastHit raycastHit;

			bool Hit = Physics.Raycast(location, rotation * Vector3.forward, out raycastHit, target_distance, layermask, QueryTriggerInteraction.Ignore);

			Vector3 OutPoint = raycastHit.point;
			Vector3 OutNormal = raycastHit.normal;

			if (!Hit)
			{
				OutPoint = location + (rotation * Vector3.forward * target_distance);
				OutNormal = rotation * Vector3.back;
			}

			if (FaceCamera)
			{
				OutNormal = rotation * Vector3.back;
			}

			Vector3 direction = (OutPoint - location).normalized;

			RhythmIndicatorTrans.position = OutPoint - (direction * target_offset);

			RhythmIndicatorTrans.LookAt(OutPoint - OutNormal);

			float Distance = (OutPoint - location).magnitude / head_distance;

			float scaleModifier = 1.0f;

			switch (scaling)
			{
				case Scaling.None:
					scaleModifier = 1.0f;
					break;
				case Scaling.Partial:
					scaleModifier = Mathf.Sqrt(Distance);
					break;
				case Scaling.Full:
					scaleModifier = Distance;
					break;
			}

			FixOtherUIElements(target_scale * scaleModifier);
		}

		private void FixOtherUIElements(float scale)
		{
			RhythmIndicatorTrans.localScale = Vector3.one * scale;
			LowAmmoIndicator.localScale = RhythmIndicatorTrans.localScale;
			NoAmmoIndicator.localScale = RhythmIndicatorTrans.localScale;
			BeatGradingContainer.localScale = RhythmIndicatorTrans.localScale;
			Reticle.localScale = RhythmIndicatorTrans.localScale;

			Reticle.position = RhythmIndicatorTrans.position;
			Reticle.rotation = RhythmIndicatorTrans.rotation;

			LowAmmoIndicator.position = RhythmIndicatorTrans.position + (RhythmIndicatorTrans.up * 0.25f * scale);
			LowAmmoIndicator.rotation = RhythmIndicatorTrans.rotation;

			NoAmmoIndicator.position = LowAmmoIndicator.position;
			NoAmmoIndicator.rotation = LowAmmoIndicator.rotation;

			BeatGradingContainer.position = RhythmIndicatorTrans.position - (RhythmIndicatorTrans.up * 0.25f * scale);
			BeatGradingContainer.rotation = RhythmIndicatorTrans.rotation;
		}
	}
}