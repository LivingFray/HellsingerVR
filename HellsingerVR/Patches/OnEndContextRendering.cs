using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Rendering;
using Valve.VR;

namespace HellsingerVR.Patches
{

	[HarmonyPatch(typeof(RenderPipeline), nameof(RenderPipeline.EndContextRendering))]
	internal class Patch_RenderPipeline_EndContextRendering
	{
		private static void Postfix(ScriptableRenderContext context, List<Camera> cameras)
		{
			SteamVR_Render.instance.RenderLoop();
		}
	}

}
