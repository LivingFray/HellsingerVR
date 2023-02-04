using HarmonyLib;
using Outsiders.GUI;
using System;
using System.Collections.Generic;
using System.Text;

namespace HellsingerVR.Patches
{

	[HarmonyPatch(typeof(InstructionPromptView), nameof(InstructionPromptView.OnInputReceived))]
	internal class Patch_InstructionPromptView_OnInputReceived
	{
		private static void Prefix(InstructionPromptView __instance, ref UIInputType inputFlags)
		{
			inputFlags |= UIInputType.Submit;
		}
	}

}
