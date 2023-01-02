using HarmonyLib;

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
