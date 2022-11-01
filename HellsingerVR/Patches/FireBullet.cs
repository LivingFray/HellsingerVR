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
		private static void Prefix(ref BulletFireData fireData)
		{
			Debug.Log($"FIRED BULLET: {fireData.Owner} {fireData.Position} {fireData.Direction}");

			// TODO: Account for settings
			bool bFromLeftHand = false;

			// Are crows two handers?
			if (fireData.WeaponConfig.WeaponType == PlayerWeaponType.Pistols)
			{
				// Somehow get which gun? Just go fuck it and let the guns share ammo and be fired by either
				bFromLeftHand = VRInputManager.LastHandToShootWasLeft;
			}

			Vector3 location = SteamVR_Input.GetAction<SteamVR_Action_Pose>("Pose").GetLocalPosition(bFromLeftHand ? SteamVR_Input_Sources.LeftHand : SteamVR_Input_Sources.RightHand);
			Quaternion rotation = SteamVR_Input.GetAction<SteamVR_Action_Pose>("Pose").GetLocalRotation(bFromLeftHand ? SteamVR_Input_Sources.LeftHand : SteamVR_Input_Sources.RightHand);

			// nb: get this from skele somehow
			rotation *= HellsingerVR.HandOffset;

			Debug.Log($"{location} {rotation}");

			location = HellsingerVR.rig.transform.TransformPoint(location);
			rotation = HellsingerVR.rig.transform.rotation * rotation;

			// Calculate what original spread must have been and apply to repositioned bullets

			Quaternion initialRotation = HellsingerVR.rig.PlayerTransform.rotation;
			Quaternion vrRotation = rotation;

			fireData.Direction = vrRotation * Quaternion.Inverse(initialRotation) * fireData.Direction;
			fireData.Position = location;
		}
	}
}
