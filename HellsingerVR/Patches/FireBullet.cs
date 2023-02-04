using HarmonyLib;
using HellsingerVR.Components;
using UnityEngine;

namespace HellsingerVR.Patches
{

	[HarmonyPatch(typeof(BulletSystem), nameof(FireBullet))]
	internal class FireBullet
	{
		private static int layermask = Helper.RangedTargetPositionLayerMask;// ~LayerMask.GetMask("Ignore Raycast");

		private static void Prefix(BulletSystem __instance, ref BulletFireData fireData)
		{
			if (fireData.WeaponConfig.WeaponType == PlayerWeaponType.Turret)
			{
				return;
			}

			// Bullets don't actually originate at the eyes, but rather an offset that is likely the muzzle position
			// This needs to be accounted for when adjusting the bullets to point the correct direction while maintaining spread

			// Calculate spread-free bullet direction (vector from start to the intersection of a ray from the player camera)
			RaycastHit raycastHit;

			bool Hit = Physics.Raycast(HellsingerVR.rig.CameraTransform.position, HellsingerVR.rig.CameraTransform.rotation * Vector3.forward, out raycastHit, 100.0f, layermask, QueryTriggerInteraction.Ignore);

			Vector3 TargetPosition = Hit ? raycastHit.point : HellsingerVR.rig.CameraTransform.position + (HellsingerVR.rig.CameraTransform.rotation * Vector3.forward * 100.0f);

			Vector3 SpreadFreeDirection = TargetPosition - fireData.Position;

			// Get hand position
			(Vector3 location, Quaternion rotation) = VRInputManager.GetHandTransform(VRInputManager.LastHandToShootWasLeft);

			// Set bullet start to hand position + muzzle offset * hand rotation
			// TODO: Muzzle offset is broken, probably multiplying quaternions in the wrong order or something
			fireData.Position = location;// + rotation * VRViewModelManager.GetMuzzleOffset(fireData.WeaponConfig.WeaponType);

			// Calculate spread rotation (difference between spread-free direction and actual direction)
			Quaternion Spread = Quaternion.FromToRotation(SpreadFreeDirection, fireData.Direction);

			// Set bullet direction to hand direction * spread rotation
			fireData.Direction = rotation * Spread * Vector3.forward;
		}
	}
}
