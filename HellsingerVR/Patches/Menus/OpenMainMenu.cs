using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace HellsingerVR.Patches.Menus
{
	[HarmonyPatch(typeof(TitleScreenView), nameof(TitleScreenView.OnGainedFocus))]
	internal class OpenMainMenu
	{
		private static void Postfix()
		{
			// Move UI to world space
			HellsingerVR.MoveTitleToWorld();
		}
	}
}
