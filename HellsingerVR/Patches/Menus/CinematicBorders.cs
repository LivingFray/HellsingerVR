using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace HellsingerVR.Patches.Menus
{
	[HarmonyPatch(typeof(ScreenCinematicBorders), nameof(ScreenCinematicBorders.Show))]
	internal class CinematicsBorders
	{
		private static void Prefix(ref float duration)
		{
			duration = 0.0f;
		}
		private static void Postfix(ScreenCinematicBorders __instance)
		{
			__instance.Hide(0.0f);
		}
	}
}
