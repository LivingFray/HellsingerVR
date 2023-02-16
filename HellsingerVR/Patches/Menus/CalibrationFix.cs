using HarmonyLib;
using HellsingerVR.UI;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace HellsingerVR.Patches.Menus
{

	[HarmonyPatch(typeof(AudioLatencyCompensationView), nameof(AudioLatencyCompensationView.SetButtonsVisible))]
	internal class Patch_AudioLatencyCompensationView_SetButtonsVisible
	{
		private static void Postfix(AudioLatencyCompensationView __instance)
		{
			Transform ButtonContainer = __instance.transform.Find("ButtonContainer");

			CalibrationRotationFixer fixer = __instance.gameObject.GetComponent<CalibrationRotationFixer>();
			if (!fixer)
			{
				fixer = __instance.gameObject.AddComponent<CalibrationRotationFixer>();
			}
			fixer.UIElement = ButtonContainer;
		}
	}

}
