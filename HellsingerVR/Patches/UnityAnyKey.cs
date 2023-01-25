using HarmonyLib;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Text;
using Valve.VR;

namespace HellsingerVR.Patches
{
	[HarmonyPatch(typeof(Input), "get_anyKeyDown")]
	internal class UnityAnyKey
	{
		private static void Postfix(ref bool __result)
		{
			__result |= SteamVR_Input.GetBooleanAction("menu", "Select", true).state;
		}
	}
}
