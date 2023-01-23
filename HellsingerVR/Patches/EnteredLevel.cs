using HarmonyLib;
using Outsiders.Messages;
using System;
using System.Collections.Generic;
using System.Text;

namespace HellsingerVR.Patches
{
	[HarmonyPatch(typeof(ChallengeTracker), nameof(ChallengeTracker.StartChallenge))]
	internal class EnteredLevel
	{
		private static void Postfix(ChallengeTracker __instance)
		{
			HellsingerVR._instance.Log.LogInfo("Started");
			HellsingerVR.rig.OnLevelBegin();
		}
	}
}
