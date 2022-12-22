using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace HellsingerVR.Patches
{

	[HarmonyPatch(typeof(InWorldScoreItem), nameof(InWorldScoreItem.RefreshPosition))]

	internal class MoveInWorldScoreItem
	{
		static void Postfix(InWorldScoreItem __instance)
		{
			try
			{
				if (__instance.m_mainCamera != null
					&& HellsingerVR.rig != null
					&& HellsingerVR.rig.head != null)
				{
					HellsingerVR._instance.Log.LogInfo($"Score item using camera {__instance.m_mainCamera.name}");
					HellsingerVR._instance.Log.LogInfo($"Preupdate score item position {__instance.transform.position.ToString()}");
					__instance.m_mainCamera = HellsingerVR.rig.camera;
					__instance.transform.position = __instance.m_worldPosition;
					__instance.transform.LookAt(HellsingerVR.rig.head);
					__instance.transform.rotation = Quaternion.Euler(0.0f, 180.0f, 0.0f) * __instance.transform.rotation;
					//__instance.transform.parent.localScale = new Vector3(5.0f, 5.0f, 5.0f);
					HellsingerVR._instance.Log.LogInfo($"Updating score item position {__instance.m_worldPosition.ToString()}");
				}
			}
			catch (Exception ex)
			{
				HellsingerVR._instance.Log.LogError($"Exception in patch of void InWorldScoreItem::RefreshPosition():\n{ex}");
			}
		}
	}
}
