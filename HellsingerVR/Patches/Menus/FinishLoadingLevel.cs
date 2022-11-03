using HarmonyLib;

namespace HellsingerVR.Patches.Menus
{
	[HarmonyPatch(typeof(LoadingView), nameof(LoadingView.OnDestroy))]
	internal class FinishLoadingLevel
	{
		private static void Postfix()
		{
			// Move UI to world space
			HellsingerVR.IsLoading = false;
		}
	}
}
