using Assets.SteamVR_Standalone.Standalone;
using System;
using UnityEngine;

namespace Valve.VR
{

	public class SteamVR_CameraFlip : MonoBehaviour
	{
		public SteamVR_CameraFlip(IntPtr value)
: base(value) { }
		private void OnEnable()
		{
			if (blitMaterial == null)
			{
				blitMaterial = new Material(VRShaders.GetShader(VRShaders.VRShader.blitFlip));
			}
		}


		private void OnRenderImage(RenderTexture src, RenderTexture dest)
		{
			Graphics.Blit(src, dest, blitMaterial);
		}


		public static Material blitMaterial;
	}
}
