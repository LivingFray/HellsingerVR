using HarmonyLib;

namespace HellsingerVR.Patches.Menus
{
	[HarmonyPatch(typeof(TitleScreenView), nameof(TitleScreenView.OnGainedFocus))]
	internal class OpenMainMenu
	{
		private static void Postfix()
		{
			// Move UI to world space
			HellsingerVR.MoveTitleToWorld();
		}
	}
}
