using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace HellsingerVR.Patches.Menus
{
	public enum ResurrectType {Resurrect, Retry, Giveup};

	public class ResHolder
	{
		public static ResurrectType resType;
	}

	[HarmonyPatch(typeof(ResurrectionView), nameof(ResurrectionView.OnGainedFocus))]
	internal class OpenResurrectionScreen
	{
		private static void Postfix()
		{
			HellsingerVR.IsPaused = true;
			// Move UI to world space
			HellsingerVR.MoveOverlayToWorld();
		}
	}

	[HarmonyPatch(typeof(ResurrectionView), nameof(ResurrectionView.OnResurrect))]
	internal class ResurrectResurrectionScreen
	{
		private static void Postfix()
		{
			ResHolder.resType = ResurrectType.Resurrect;
		}
	}

	[HarmonyPatch(typeof(ResurrectionView), nameof(ResurrectionView.OnRetry))]
	internal class RetryResurrectionScreen
	{
		private static void Postfix()
		{
			ResHolder.resType = ResurrectType.Retry;
		}
	}

	[HarmonyPatch(typeof(ResurrectionView), nameof(ResurrectionView.OnGiveUp))]
	internal class GiveupResurrectionScreen
	{
		private static void Postfix()
		{
			ResHolder.resType = ResurrectType.Giveup;
		}
	}

	[HarmonyPatch(typeof(ResurrectionView), nameof(ResurrectionView.OnClosed))]
	internal class CloseResurrectionScreen
	{
		private static void Postfix()
		{
			HellsingerVR.IsPaused = false;
			if (ResHolder.resType != ResurrectType.Resurrect)
			{
				HellsingerVR.IsLoading = true;
				if (HellsingerVR.rig)
				{
					HellsingerVR.rig.EnterTitleScreen();
				}
			}
		}
	}
}
