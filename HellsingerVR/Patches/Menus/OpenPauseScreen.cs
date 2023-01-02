using HarmonyLib;

namespace HellsingerVR.Patches.Menus
{
	[HarmonyPatch(typeof(PauseScreenView), nameof(PauseScreenView.OnOpen))]
	internal class OpenPauseScreen
	{
		private static void Postfix()
		{
			HellsingerVR.IsPaused = true;
			// Move UI to world space
			HellsingerVR.MoveOverlayToWorld(true);
		}
	}

	[HarmonyPatch(typeof(PauseScreenView), nameof(PauseScreenView.OnClosed))]
	internal class ClosePauseScreen
	{
		private static void Postfix()
		{
			HellsingerVR.IsPaused = false;
		}
	}
}
