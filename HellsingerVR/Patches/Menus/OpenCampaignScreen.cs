using HarmonyLib;
using System.Reflection;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using System.Linq;

namespace HellsingerVR.Patches.Menus
{

	[HarmonyPatch(typeof(TitleState), nameof(TitleState.OpenCampaignScreen))]
	internal class OpenCampaignScreen
	{
		private static void Postfix()
		{
			// Move UI to world space
			HellsingerVR.MoveLevelSelectToWorld();
		}
	}

	[HarmonyPatch(typeof(StageSelectCampaignView), nameof(StageSelectCampaignView.SetVisibility))]
	internal class OpenCampaignScreen2
	{
		private static void Postfix(StageSelectCampaignView __instance)
		{
			// Move UI to world space
			HellsingerVR.MoveLevelSelectToWorld();
		}
	}

	/*
	[HarmonyPatch(typeof(TitleScreenView))]
	internal class DebugPatch
	{
		static IEnumerable<MethodBase> TargetMethods()
		{
			return typeof(TitleScreenView).GetMethods().Where(method => method.IsPublic
			&& !method.IsConstructor && !method.IsSpecialName && !method.IsGenericMethod
			&& !method.Name.ToLower().Contains("set") && !method.Name.ToLower().Contains("get")
			&& !method.Name.Contains("ToString")
			&& (method.DeclaringType == method.ReflectedType && !method.IsAbstract)
			&& !method.Name.Contains("IGameState")
			&& !method.Name.Contains("Engagement"));
		}

		static void Postfix(MethodBase __originalMethod)
		{
			// use dynamic code to handle all method calls
			var parameters = __originalMethod.GetParameters();
			Debug.Log($"Method {__originalMethod.FullDescription()}:");
			for (var i = 0; i < parameters.Length; i++)
				Debug.Log($"{parameters[i].Name} of type {parameters[i].ParameterType}");
		}
	}
	*/
}
