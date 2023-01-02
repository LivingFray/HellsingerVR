using HarmonyLib;
using System;
using UnityEngine;

namespace HellsingerVR.Patches
{

	[HarmonyPatch(typeof(InWorldScoreItem), nameof(InWorldScoreItem.RefreshPosition))]

	internal class MoveInWorldScoreItem
	{
		private static void Postfix(InWorldScoreItem __instance)
		{
			try
			{
				if (__instance.m_mainCamera != null
					&& HellsingerVR.rig != null
					&& HellsingerVR.rig.head != null)
				{
					__instance.m_mainCamera = HellsingerVR.rig.camera;
					__instance.transform.position = __instance.m_worldPosition;
					__instance.transform.LookAt(HellsingerVR.rig.head);
					__instance.transform.rotation = Quaternion.Euler(0.0f, 180.0f, 0.0f) * __instance.transform.rotation;
				}
			}
			catch (Exception ex)
			{
				HellsingerVR._instance.Log.LogError($"Exception in patch of void InWorldScoreItem::RefreshPosition():\n{ex}");
			}
		}
	}
}
