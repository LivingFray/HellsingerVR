using HarmonyLib;
using Outsiders.Messages;

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
