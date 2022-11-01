using HarmonyLib;
using Outsiders.Messages;
using System;
using System.Collections.Generic;
using System.Text;

namespace HellsingerVR.Patches
{
	[HarmonyPatch(typeof(CutscenePlayer), nameof(CutscenePlayer.Play))]
	internal class CutsceneStateEnter
	{
		private static void Postfix()
		{
			if (HellsingerVR.rig != null)
			{
				HellsingerVR.rig.EnterCutscene();
			}
			// TEMP
			HellsingerVR.MoveOverlayToWorld();
		}
	}
}
