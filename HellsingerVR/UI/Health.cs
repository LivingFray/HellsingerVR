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
		Transform WeaponsContainer;
		Transform BonusCounter;
		Transform BossHealthContainer;

		Vector3 PositionOffset = new Vector3(0.0f, 0.0f, -0.1f);
		Quaternion RotationOffset = Quaternion.Euler(0.0f, -90.0f, 0.0f);

		bool HasCalculatedOffsets = false;
		float HealthOffsetX = 0.0f;
		float HealthOffsetY = 0.0f;

		float WeaponsOffsetX = 0.0f;
		float WeaponsOffsetY = 0.0f;

		float BonusOffsetY = 0.0f;

		float BossOffsetY = 0.0f;

		Vector3 LocalScale = new Vector3(0.25f, 0.25f, 0.25f);

		bool DoHealth;
		bool DoUltimate;
		bool DoWeapons;
		bool DoBonus;
		bool DoBoss;


		public void Init()
		{
			Main main = Main.GetInstance();
			Transform SharedHUD = main.transform.Find("UIRoot/Overlay/Layer-HUD/HUD(Clone)/Shared");
			Transform StageHUD = main.transform.Find("UIRoot/Overlay/Layer-HUD/HUD(Clone)/StageHud");

			HealthBar = SharedHUD.Find("HealthBar");
			UltimateIndicator = SharedHUD.Find("UltimateIndicator");
			WeaponsContainer = SharedHUD.Find("WeaponsContainer"); // Has offset of (83, -489.2, 0)
			BonusCounter = StageHUD.Find("BonusCounter");
			BossHealthContainer = StageHUD.Find("BossHealthContainer");

			DoHealth = HellsingerVR._instance.ShowHealthOnHand.Value;
			DoUltimate = HellsingerVR._instance.ShowUltimateOnHand.Value;
			DoWeapons = HellsingerVR._instance.ShowWeaponsOnHand.Value;
			DoBonus = HellsingerVR._instance.ShowFuryOnHand.Value;
			DoBoss = HellsingerVR._instance.ShowBossOnHand.Value;

			if (!HasCalculatedOffsets)
			{
				HealthOffsetX = HealthBar.localPosition.x - UltimateIndicator.localPosition.x;
				HealthOffsetY = HealthBar.localPosition.y - UltimateIndicator.localPosition.y;

				WeaponsOffsetX = WeaponsContainer.localPosition.x - UltimateIndicator.localPosition.x;
				WeaponsOffsetY = WeaponsContainer.localPosition.y - UltimateIndicator.localPosition.y;

				BonusOffsetY = BonusCounter.GetComponent<RectTransform>().GetHeight();
			}

			if (DoHealth) HealthBar.localScale = LocalScale;
			if (DoUltimate) UltimateIndicator.localScale = LocalScale;
			if (DoWeapons) WeaponsContainer.localScale = LocalScale;
			if (DoBonus) BonusCounter.localScale = LocalScale;
			
			if (!HasCalculatedOffsets)
			{
				HealthOffsetX *= HealthBar.lossyScale.x;
				HealthOffsetY *= HealthBar.lossyScale.y;
				WeaponsOffsetX *= WeaponsContainer.lossyScale.x;
				WeaponsOffsetY *= WeaponsContainer.lossyScale.y;
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

			bool bFromLeftHand = !HellsingerVR._instance.IsLeftHanded.Value;
			(Vector3 location, Quaternion rotation) = VRInputManager.GetHandTransform(bFromLeftHand);

			rotation *= RotationOffset;

			location += rotation * PositionOffset;

			Vector3 ToHead = HellsingerVR.rig.head.position - location;

			bool IsVisible = Vector3.Dot(ToHead, rotation * Vector3.forward) < 0.0f;

			if (IsVisible)
			{
				Vector3 up = rotation * Vector3.up;
				Vector3 right = rotation * Vector3.right;

				if (DoHealth)
				{
					HealthBar.position = location + right * HealthOffsetX + up * HealthOffsetY;
					HealthBar.rotation = rotation;
				}

				if (DoUltimate)
				{
					UltimateIndicator.position = location;
					UltimateIndicator.rotation = rotation;
				}

				if (DoWeapons)
				{
					WeaponsContainer.position = location + right * WeaponsOffsetX + up * WeaponsOffsetY;
					WeaponsContainer.rotation = rotation;
				}

				if (DoBonus)
				{
					BonusCounter.position = location + up * BonusOffsetY;
					BonusCounter.rotation = rotation;
				}

				if (DoBoss)
				{
					BossHealthContainer.position = location + up * BossOffsetY;
					BossHealthContainer.rotation = rotation;
				}

				if (DoHealth) HealthBar.localScale = LocalScale;
				if (DoUltimate) UltimateIndicator.localScale = LocalScale;
				if (DoWeapons) WeaponsContainer.localScale = LocalScale;
				if (DoBonus) BonusCounter.localScale = LocalScale;
				if (DoBoss) BossHealthContainer.localScale = LocalScale;

			}

			if (DoHealth) HealthBar.gameObject.active = IsVisible;
			if (DoUltimate) UltimateIndicator.gameObject.active = IsVisible;
			if (DoWeapons) WeaponsContainer.gameObject.active = IsVisible;
			if (DoBonus) BonusCounter.gameObject.active = IsVisible;
			if (DoBoss) BossHealthContainer.gameObject.active = IsVisible;
		}
	}
}
