using HarmonyLib;

namespace HellsingerVR.Patches
{
	[HarmonyPatch(typeof(Main), nameof(Main.OnApplicationFocus))]
	internal class OnApplicationFocus
	{
		private static void Prefix(ref bool focusStatus)
		{
			focusStatus = true;
		}
	}
}
