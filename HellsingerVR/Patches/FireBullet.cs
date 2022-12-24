using HarmonyLib;
using HellsingerVR.Components;
using Outsiders.Messages;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Valve.VR;

namespace HellsingerVR.Patches
{

    [HarmonyPatch(typeof(BulletSystem), nameof(FireBullet))]
	internal class FireBullet
	{
		private static void Prefix(BulletSystem __instance, ref BulletFireData fireData)
		{
			//HellsingerVR._instance.Log.LogInfo($"FIRED BULLET: {fireData.Owner} {fireData.Position} {fireData.Direction}");

			bool bFromLeftHand = VRInputManager.LastHandToShootWasLeft;

			(Vector3 location, Quaternion rotation) = VRInputManager.GetHandTransform(bFromLeftHand);

			// Calculate what original spread must have been and apply to repositioned bullets

			Quaternion initialRotation = HellsingerVR.rig.CameraTransform.rotation;
			Quaternion vrRotation = rotation;

			fireData.Direction = vrRotation * Quaternion.Inverse(initialRotation) * fireData.Direction;
			fireData.Position = location + vrRotation * VRViewModelManager.GetMuzzleOffset(fireData.WeaponConfig.WeaponType);
		}
	}
}
