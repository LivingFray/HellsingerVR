using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace HellsingerVR.Patches
{

	[HarmonyPatch(typeof(EnemyDamageIndicator), nameof(EnemyDamageIndicator.RefreshPosition))]

	internal class MoveEnemyDamageIndicator
	{
		static void Postfix(EnemyDamageIndicator __instance)
		{
			try
			{
				if (__instance.m_mainCamera != null
					&& __instance.m_damageInfo != null
					&& HellsingerVR.rig != null
					&& HellsingerVR.rig.head != null)
				{
					__instance.m_mainCamera = HellsingerVR.rig.camera;
					__instance.transform.position = __instance.m_damageInfo.WorldPosition;
					__instance.transform.LookAt(HellsingerVR.rig.head);
					__instance.transform.rotation = Quaternion.Euler(0.0f, 180.0f, 0.0f) * __instance.transform.rotation;
				}
			}
			catch (Exception ex)
			{
				HellsingerVR._instance.Log.LogError($"Exception in patch of void EnemyDamageIndicator::RefreshPosition():\n{ex}");
			}
		}
	}
}
