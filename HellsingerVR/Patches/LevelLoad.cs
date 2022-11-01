using HarmonyLib;
using Outsiders.Messages;
using System;
using System.Collections.Generic;
using System.Text;

namespace HellsingerVR.Patches
{
	[HarmonyPatch(typeof(Main), nameof(Main.OnLevelChangeRequest))]
	internal class LevelLoad
	{
		private static void Postfix(Main __instance, CoreRequestLoadLevelMessage loadLevelMsg)
		{
			HellsingerVR.OnLevelLoad(loadLevelMsg);
		}
	}
}
