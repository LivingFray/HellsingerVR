using HellsingerVR.Components;
using UnityEngine;

namespace HellsingerVR.UI
{
	public class Health
	{
		private Transform HealthBar;
		private Transform UltimateIndicator;
		private Transform WeaponsContainer;
		private Transform BonusCounter;
		private Transform BossHealthContainer;
		private Transform EmptyUltimateBgContainer;
		private Vector3 PositionOffset = new Vector3(0.0f, 0.0f, -0.1f);
		private Quaternion RotationOffset = Quaternion.Euler(0.0f, -90.0f, 0.0f);
		private bool HasCalculatedOffsets = false;
		private float HealthOffsetX = 0.0f;
		private float HealthOffsetY = 0.0f;
		private float WeaponsOffsetX = 0.0f;
		private float WeaponsOffsetY = 0.0f;
		private float BonusOffsetY = 0.0f;
		private float BossOffsetY = 0.0f;
		private Vector3 LocalScale = new Vector3(0.25f, 0.25f, 0.25f);
		private bool DoHealth;
		private bool DoUltimate;
		private bool DoWeapons;
		private bool DoBonus;
		private bool DoBoss;


		public void Init()
		{
			if (HellsingerVR._instance.IsLeftHanded.Value)
			{
				RotationOffset = Quaternion.Euler(0.0f, 90.0f, 0.0f);
			}

			Main main = Main.GetInstance();
			Transform SharedHUD = main.transform.Find("UIRoot/Overlay/Layer-HUD/HUD(Clone)/Shared");
			Transform StageHUD = main.transform.Find("UIRoot/Overlay/Layer-HUD/HUD(Clone)/StageHud");

			HealthBar = SharedHUD.Find("HealthBar");
			UltimateIndicator = SharedHUD.Find("UltimateIndicator");
			WeaponsContainer = SharedHUD.Find("WeaponsContainer");
			BonusCounter = StageHUD.Find("BonusCounter");
			BossHealthContainer = StageHUD.Find("BossHealthContainer");
			EmptyUltimateBgContainer = SharedHUD.Find("EmptyUltimateBgContainer");

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
					HealthBar.position = location + (right * HealthOffsetX) + (up * HealthOffsetY);
					HealthBar.rotation = rotation;
				}

				if (DoUltimate)
				{
					UltimateIndicator.position = location;
					UltimateIndicator.rotation = rotation;


					if (EmptyUltimateBgContainer != null)
					{
						EmptyUltimateBgContainer.position = location;
						EmptyUltimateBgContainer.rotation = rotation;
					}
				}

				if (DoWeapons)
				{
					WeaponsContainer.position = location + (right * WeaponsOffsetX) + (up * WeaponsOffsetY);
					WeaponsContainer.rotation = rotation;
				}

				if (DoBonus)
				{
					BonusCounter.position = location + (up * BonusOffsetY);
					BonusCounter.rotation = rotation;
				}

				if (DoBoss)
				{
					BossHealthContainer.position = location + (up * BossOffsetY);
					BossHealthContainer.rotation = rotation;
				}

				if (DoHealth) HealthBar.localScale = LocalScale;
				if (DoUltimate) UltimateIndicator.localScale = LocalScale;
				if (DoUltimate && EmptyUltimateBgContainer != null) EmptyUltimateBgContainer.localScale = LocalScale;
				if (DoWeapons) WeaponsContainer.localScale = LocalScale;
				if (DoBonus) BonusCounter.localScale = LocalScale;
				if (DoBoss) BossHealthContainer.localScale = LocalScale;

			}

			if (DoHealth) HealthBar.gameObject.active = IsVisible;
			if (DoUltimate) UltimateIndicator.gameObject.active = IsVisible;
			if (DoUltimate && EmptyUltimateBgContainer != null) EmptyUltimateBgContainer.gameObject.active = IsVisible;
			if (DoWeapons) WeaponsContainer.gameObject.active = IsVisible;
			if (DoBonus) BonusCounter.gameObject.active = IsVisible;
			if (DoBoss) BossHealthContainer.gameObject.active = IsVisible;
		}
	}
}
