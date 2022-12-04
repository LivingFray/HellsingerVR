using HellsingerVR.Components;
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
		Transform BossHealthContainer;

		Vector3 PositionOffset = new Vector3(0.0f, 0.0f, -0.1f);
		Quaternion RotationOffset = Quaternion.Euler(0.0f, -90.0f, 0.0f);

		float R_x = 0.0f;
		float R_y = -90.0f;
		float R_z = 0.0f;

		bool HasCalculatedOffsets = false;
		float HealthOffsetX = 0.0f;
		float HealthOffsetY = 0.0f;

		float BonusOffsetY = 0.0f;

		float BossOffsetY = 0.0f;

		Vector3 LocalScale = new Vector3(0.25f, 0.25f, 0.25f);

		public void Init()
		{
			Main main = Main.GetInstance();
			Transform SharedHUD = main.transform.Find("UIRoot/Overlay/Layer-HUD/HUD(Clone)/Shared");
			Transform StageHUD = main.transform.Find("UIRoot/Overlay/Layer-HUD/HUD(Clone)/StageHud");

			UltimateIndicator = SharedHUD.Find("UltimateIndicator");
			HealthBar = SharedHUD.Find("HealthBar");
			BonusCounter = StageHUD.Find("BonusCounter");
			BossHealthContainer = StageHUD.Find("BossHealthContainer");

			if (!HasCalculatedOffsets)
			{
				HealthOffsetX = HealthBar.localPosition.x - UltimateIndicator.localPosition.x;
				HealthOffsetY = HealthBar.localPosition.y - UltimateIndicator.localPosition.y;

				BonusOffsetY = BonusCounter.GetComponent<RectTransform>().GetHeight();
			}

			HealthBar.localScale = LocalScale;
			UltimateIndicator.localScale = LocalScale;
			BonusCounter.localScale = LocalScale;
			
			if (!HasCalculatedOffsets)
			{
				HealthOffsetX *= HealthBar.lossyScale.x;
				HealthOffsetY *= HealthBar.lossyScale.y;
				BonusOffsetY *= BonusCounter.lossyScale.y;
				BossOffsetY = BonusOffsetY * 3.0f;
				HasCalculatedOffsets = true;
			}

		}

		public void Update()
		{
			if (HellsingerVR.rig?.head == null)
			{
				return;
			}

			// TODO: Handedness setting
			bool bFromLeftHand = true;

			(Vector3 location, Quaternion rotation) = VRInputManager.GetHandTransform(bFromLeftHand);

			rotation *= RotationOffset;

			location += rotation * PositionOffset;

			Vector3 ToHead = HellsingerVR.rig.head.position - location;

			bool IsVisible = Vector3.Dot(ToHead, rotation * Vector3.forward) < 0.0f;

			if (IsVisible)
			{

				// TODO: Offset from wrist upwards
				UltimateIndicator.position = location;
				UltimateIndicator.rotation = rotation;

				Vector3 up = rotation * Vector3.up;
				Vector3 right = rotation * Vector3.right;

				HealthBar.position = location + right * HealthOffsetX + up * HealthOffsetY;
				HealthBar.rotation = rotation;

				BonusCounter.position = location + up * BonusOffsetY;
				BonusCounter.rotation = rotation;

				BossHealthContainer.position = location + up * BossOffsetY;
				BossHealthContainer.rotation = rotation;

				HealthBar.localScale = LocalScale;
				UltimateIndicator.localScale = LocalScale;
				BonusCounter.localScale = LocalScale;
				BossHealthContainer.localScale = LocalScale;

			}

			HealthBar.gameObject.active = IsVisible;
			UltimateIndicator.gameObject.active = IsVisible;
			BonusCounter.gameObject.active = IsVisible;
			BossHealthContainer.gameObject.active = IsVisible;

			/*
			if (Input.GetKeyDown(KeyCode.U))
			{
				R_x += 5.0f;
				RotationOffset = Quaternion.Euler(R_x, R_y, R_z);
				HellsingerVR._instance.Log.LogInfo($"({R_x}, {R_y}, {R_z})");
			}
			if (Input.GetKeyDown(KeyCode.J))
			{
				R_x -= 5.0f;
				RotationOffset = Quaternion.Euler(R_x, R_y, R_z);
				HellsingerVR._instance.Log.LogInfo($"({R_x}, {R_y}, {R_z})");
			}
			if (Input.GetKeyDown(KeyCode.I))
			{
				R_y += 5.0f;
				RotationOffset = Quaternion.Euler(R_x, R_y, R_z);
				HellsingerVR._instance.Log.LogInfo($"({R_x}, {R_y}, {R_z})");
			}
			if (Input.GetKeyDown(KeyCode.K))
			{
				R_y -= 5.0f;
				RotationOffset = Quaternion.Euler(R_x, R_y, R_z);
				HellsingerVR._instance.Log.LogInfo($"({R_x}, {R_y}, {R_z})");
			}
			if (Input.GetKeyDown(KeyCode.O))
			{
				R_z += 5.0f;
				RotationOffset = Quaternion.Euler(R_x, R_y, R_z);
				HellsingerVR._instance.Log.LogInfo($"({R_x}, {R_y}, {R_z})");
			}
			if (Input.GetKeyDown(KeyCode.L))
			{
				R_z -= 5.0f;
				RotationOffset = Quaternion.Euler(R_x, R_y, R_z);
				HellsingerVR._instance.Log.LogInfo($"({R_x}, {R_y}, {R_z})");
			}
			//*/
		}
	}
}
