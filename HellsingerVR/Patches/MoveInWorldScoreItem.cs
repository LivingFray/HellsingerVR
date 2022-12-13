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
				if (__instance != null
					&& HellsingerVR.rig != null
					&& HellsingerVR.rig.head != null)
				{
					//Vector3 Offset = (HellsingerVR.rig.head.transform.position - __instance.m_worldPosition).normalized * 0.5f;
					__instance.transform.position = __instance.m_worldPosition;
					__instance.transform.LookAt(HellsingerVR.rig.head);
					__instance.transform.rotation = Quaternion.Euler(0.0f, 180.0f, 0.0f) * __instance.transform.rotation;
					//__instance.transform.parent.localScale = new Vector3(5.0f, 5.0f, 5.0f);
				}
			}
			catch (Exception ex)
			{
				HellsingerVR._instance.Log.LogError($"Exception in patch of void InWorldScoreItem::RefreshPosition():\n{ex}");
			}
		}
	}
}
