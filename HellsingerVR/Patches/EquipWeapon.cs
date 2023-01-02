using HarmonyLib;

namespace HellsingerVR.Patches
{

	[HarmonyPatch(typeof(FirstPersonController), nameof(FirstPersonController.EquipWeapon))]
	internal class EquipWeapon
	{
		private static void Postfix(PlayerWeaponType type)
		{
			if (HellsingerVR.rig && HellsingerVR.rig.viewModelManager)
			{
				HellsingerVR.rig.viewModelManager.OnChangeWeapon(type);
			}
		}
	}
}
