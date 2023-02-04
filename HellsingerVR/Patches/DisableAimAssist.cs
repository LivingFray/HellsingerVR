using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace HellsingerVR.Patches
{

	[HarmonyPatch(typeof(InputReader), nameof(InputReader.IsAimAssistEnabled))]
	internal class Patch_InputReader_IsAimAssistEnabled
	{
		private static void Postfix(InputReader __instance, ref bool __result)
		{
			__result = false;
		}
	}

}
