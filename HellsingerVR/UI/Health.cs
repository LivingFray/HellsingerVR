using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Valve.VR;

namespace HellsingerVR.UI
{
	public class Health
	{

		Transform HealthBar;
		Transform UltimateIndicator;
		Transform BonusCounter;

		Quaternion RotationOffset = Quaternion.Euler(0.0f, 90.0f, -145.0f);

		float R_x = 0.0f;
		float R_y = 0.0f;
		float R_z = 0.0f;

		float HealthOffsetX = 0.0f;
		float HealthOffsetY = 0.0f;

		float BonusOffsetY = 0.0f;

		public void Init()
		{
			GameObject gameObject = GameObject.Find("UltimateIndicator");

			if (gameObject == null)
			{
				Debug.LogError("Could not find ultimate indicator");
				return;
			}

			UltimateIndicator = gameObject.transform;

			gameObject = GameObject.Find("HealthBar");

			if (gameObject == null)
			{
				Debug.LogError("Could not find health bar");
				return;
			}

			HealthBar = gameObject.transform;

			gameObject = GameObject.Find("BonusCounter");

			if (gameObject == null)
			{
				Debug.LogError("Could not find score badge container");
				return;
			}

			BonusCounter = gameObject.transform;

			HealthOffsetX = HealthBar.localPosition.x - UltimateIndicator.localPosition.x;
			HealthOffsetY = HealthBar.localPosition.y - UltimateIndicator.localPosition.y;

			BonusOffsetY = gameObject.GetComponent<RectTransform>().GetHeight();

			Vector3 LocalScale = new Vector3(0.25f, 0.25f, 0.25f);

			HealthBar.localScale = LocalScale;
			UltimateIndicator.localScale = LocalScale;
			BonusCounter.localScale = LocalScale;

			HealthOffsetX *= HealthBar.lossyScale.x;
			HealthOffsetY *= HealthBar.lossyScale.y;
			BonusOffsetY *= BonusCounter.lossyScale.y;

			Debug.Log($"Offsets: {HealthOffsetX} {HealthOffsetY} {BonusOffsetY} Scaled by: {HealthBar.lossyScale.x} {HealthBar.lossyScale.y} {BonusCounter.lossyScale.y}");
		}

		public void Update()
		{
			// TODO: Handedness setting
			bool bFromLeftHand = true;

			Vector3 location = SteamVR_Input.GetAction<SteamVR_Action_Pose>("Pose").GetLocalPosition(bFromLeftHand ? SteamVR_Input_Sources.LeftHand : SteamVR_Input_Sources.RightHand);
			Quaternion rotation = SteamVR_Input.GetAction<SteamVR_Action_Pose>("Pose").GetLocalRotation(bFromLeftHand ? SteamVR_Input_Sources.LeftHand : SteamVR_Input_Sources.RightHand);

			location = HellsingerVR.rig.transform.TransformPoint(location);

			rotation = HellsingerVR.rig.transform.rotation * rotation * RotationOffset;

			// TODO: Offset from wrist upwards
			UltimateIndicator.position = location;
			UltimateIndicator.rotation = rotation;

			Vector3 up = rotation * Vector3.up;
			Vector3 right = rotation * Vector3.right;

			HealthBar.position = location + right * HealthOffsetX + up * HealthOffsetY;
			HealthBar.rotation = rotation;

			BonusCounter.position = location + up * BonusOffsetY;
			BonusCounter.rotation = rotation;

			/*
			if (Input.GetKeyDown(KeyCode.U))
			{
				R_x += 5.0f;
				RotationOffset = Quaternion.Euler(R_x, R_y, R_z);
				Debug.Log($"({R_x}, {R_y}, {R_z})");
			}
			if (Input.GetKeyDown(KeyCode.J))
			{
				R_x -= 5.0f;
				RotationOffset = Quaternion.Euler(R_x, R_y, R_z);
				Debug.Log($"({R_x}, {R_y}, {R_z})");
			}
			if (Input.GetKeyDown(KeyCode.I))
			{
				R_y += 5.0f;
				RotationOffset = Quaternion.Euler(R_x, R_y, R_z);
				Debug.Log($"({R_x}, {R_y}, {R_z})");
			}
			if (Input.GetKeyDown(KeyCode.K))
			{
				R_y -= 5.0f;
				RotationOffset = Quaternion.Euler(R_x, R_y, R_z);
				Debug.Log($"({R_x}, {R_y}, {R_z})");
			}
			if (Input.GetKeyDown(KeyCode.O))
			{
				R_z += 5.0f;
				RotationOffset = Quaternion.Euler(R_x, R_y, R_z);
				Debug.Log($"({R_x}, {R_y}, {R_z})");
			}
			if (Input.GetKeyDown(KeyCode.L))
			{
				R_z -= 5.0f;
				RotationOffset = Quaternion.Euler(R_x, R_y, R_z);
				Debug.Log($"({R_x}, {R_y}, {R_z})");
			}
			*/
		}
	}
}
