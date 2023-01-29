using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Rendering.HighDefinition.Compositor;
using Valve.VR;

namespace HellsingerVR.Patches
{
	

	[HarmonyPatch(typeof(VolumeComponent), nameof(VolumeComponent.OnEnable))]
	internal class PostProcessRemover
	{
		private static void Postfix(VolumeComponent __instance)
		{
			int RemoveLevel = HellsingerVR._instance.PostProcessingLevel.Value;

			if (RemoveLevel >= 99)
			{
				__instance.active = false;
			}
			else if (RemoveLevel >= 1)
			{
				if (__instance.TryCast<DepthOfField>() || __instance.TryCast<MotionBlur>() || __instance.TryCast<LensDistortion>()
				|| __instance.TryCast<ChromaticAberration>() || __instance.TryCast<Bloom>() || __instance.TryCast<PathTracing>()
				|| __instance.TryCast<AmbientOcclusion>() || __instance.TryCast<VolumetricFog>() || __instance.TryCast<VolumetricClouds>()
				|| __instance.TryCast<GlobalIllumination>() || __instance.TryCast<ScreenSpaceReflection>() || __instance.TryCast<ScreenSpaceRefraction>()
				|| __instance.TryCast<SubSurfaceScattering>())
				{
					__instance.active = false;
				}
			}
			else if (RemoveLevel == 0)
			{
				if (__instance.TryCast<DepthOfField>() || __instance.TryCast<MotionBlur>() || __instance.TryCast<LensDistortion>())
				{
					__instance.active = false;
				}
			}
			
		}
	}
}
