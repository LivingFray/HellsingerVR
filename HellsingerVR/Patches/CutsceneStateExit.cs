using HarmonyLib;

namespace HellsingerVR.Patches
{
	[HarmonyPatch(typeof(CutscenePlayer), nameof(CutscenePlayer.Finished))]
	internal class CutsceneStateExit
	{
		private static void Postfix()
		{
			if (HellsingerVR.rig != null)
			{
				HellsingerVR.rig.ExitCutscene();
			}
		}
	}
}
