using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace HellsingerVR.Patches
{
	[HarmonyPatch(typeof(GameManager), nameof(GameManager.Init))]
	internal class Bootstrap
	{
		private static void Postfix(GameManager __instance)
		{
			HellsingerVR.InstantiateVRRig();
		}
	}
}
