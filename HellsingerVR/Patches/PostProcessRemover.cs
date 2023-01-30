using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using Valve.VR;

namespace HellsingerVR.Patches
{
	[HarmonyPatch(typeof(Volume), nameof(Volume.Update))]
	internal class Patch_Volume_Update
	{
		private static void Postfix(Volume __instance)
		{
			if (__instance.sharedProfile == null)
			{
				return;
			}

			int RemoveLevel = HellsingerVR._instance.PostProcessingLevel.Value;

			if (RemoveLevel >= 99)
			{
				foreach (VolumeComponent comp in __instance.sharedProfile.components)
				{
					comp.active = false;
				}
			}
			else if (RemoveLevel >= 1)
			{
				foreach (VolumeComponent comp in __instance.sharedProfile.components)
				{
					if (comp.TryCast<DepthOfField>() || comp.TryCast<MotionBlur>() || comp.TryCast<LensDistortion>()
				|| comp.TryCast<ChromaticAberration>() || comp.TryCast<Bloom>() || comp.TryCast<PathTracing>()
				|| comp.TryCast<AmbientOcclusion>() || comp.TryCast<VolumetricFog>() || comp.TryCast<VolumetricClouds>()
				|| comp.TryCast<GlobalIllumination>() || comp.TryCast<ScreenSpaceReflection>() || comp.TryCast<ScreenSpaceRefraction>()
				|| comp.TryCast<SubSurfaceScattering>())
					{
						comp.active = false;
					}
				}
			}
			else if (RemoveLevel == 0)
			{
				foreach (VolumeComponent comp in __instance.sharedProfile.components)
				{
					if (comp.TryCast<DepthOfField>() || comp.TryCast<MotionBlur>() || comp.TryCast<LensDistortion>())
					{
						comp.active = false;
					}
				}
			}
		}
	}
}
