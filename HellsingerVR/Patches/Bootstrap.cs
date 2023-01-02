using HarmonyLib;

namespace HellsingerVR.Patches
{
	[HarmonyPatch(typeof(GameManager), nameof(GameManager.Init))]
	internal class Bootstrap
	{
		private static void Postfix(GameManager __instance)
		{
			HellsingerVR.InstantiateVRRig();
		}
	}
}
