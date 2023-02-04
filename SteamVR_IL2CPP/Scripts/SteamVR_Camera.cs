//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: Adds SteamVR render support to existing camera objects
//
//=============================================================================

using Assets.SteamVR_Standalone.Standalone;
using Standalone;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace Valve.VR
{

	public class SteamVR_Camera : MonoBehaviour
	{
		public SteamVR_Camera(IntPtr value)
	: base(value) { }
		private Transform _head;
		public Transform head { get { return _head; } }
		public Transform offset { get { return _head; } } // legacy
		public Transform origin { get { return _head.parent; } }

		public Camera camera { get; private set; }

		private Transform _ears;
		public Transform ears { get { return _ears; } }

		public static int TrueMask;

		public EVREye eye;

		public Ray GetRay()
		{
			return new Ray(_head.position, _head.forward);
		}

		public bool wireframe = false;

		private SteamVR_CameraFlip flip;

		#region Materials

		static public Material blitMaterial;

		public static Action<int, int> OnResolutionChanged;

		// Using a single shared offscreen buffer to render the scene.  This needs to be larger
		// than the backbuffer to account for distortion correction.  The default resolution
		// gives us 1:1 sized pixels in the center of view, but quality can be adjusted up or
		// down using the following scale value to balance performance.
		static public float sceneResolutionScale = 1.0f;
		static public float sceneResolutionScaleMultiplier = 1f;

		static private RenderTexture[] sceneTextures = new RenderTexture[2];

		public static Resolution GetSceneResolution()
		{
			var vr = SteamVR.instance;
			Resolution r = new Resolution();
			int w = (int)(vr.sceneWidth * sceneResolutionScale * sceneResolutionScaleMultiplier);
			int h = (int)(vr.sceneHeight * sceneResolutionScale * sceneResolutionScaleMultiplier);
			r.width = w;
			r.height = h;
			return r;
		}

		public static Resolution GetResolutionForAspect(int aspectW, int aspectH)
		{
			Resolution hmdResolution = GetSceneResolution();

			// We calcuate an optimal 16:9 resolution to use with the HMD resolution because that's the best aspect for the UI rendering
			Resolution closestToAspect = hmdResolution;
			closestToAspect.height = closestToAspect.width / aspectW * aspectH;
			closestToAspect.width += closestToAspect.width % 2;
			closestToAspect.height += closestToAspect.height % 2;
			return closestToAspect;
		}

		public static Resolution GetUnscaledSceneResolution()
		{
			var vr = SteamVR.instance;
			Resolution r = new Resolution();
			r.width = (int)vr.sceneWidth;
			r.height = (int)vr.sceneHeight;
			r.width += r.width % 2;
			r.height += r.height % 2;
			return r;
		}

		static public RenderTexture GetSceneTexture(EVREye eye, bool hdr)
		{
			var vr = SteamVR.instance;
			if (vr == null)
				return null;

			int w = (int)(vr.sceneWidth * sceneResolutionScale * sceneResolutionScaleMultiplier);
			int h = (int)(vr.sceneHeight * sceneResolutionScale * sceneResolutionScaleMultiplier);
			w += w % 2;
			h += h % 2;

			RenderTexture _sceneTexture = sceneTextures[(int)eye];

			int aa = QualitySettings.antiAliasing == 0 ? 1 : QualitySettings.antiAliasing;
			var format = hdr ? RenderTextureFormat.ARGBHalf : RenderTextureFormat.ARGB32;
			bool recreatedTex = false;
			if (_sceneTexture != null)
			{
				if (_sceneTexture.width != w || _sceneTexture.height != h || _sceneTexture.format != format)
				{
					Debug.Log($"Recreating scene texture.. Old: {_sceneTexture.width}x{_sceneTexture.height} MSAA={_sceneTexture.antiAliasing} [{aa}] New: {w}x{h} MSAA={aa} [{format}]");
					Destroy(_sceneTexture);
					_sceneTexture = null;
					recreatedTex = true;
				}
			}

			if (_sceneTexture == null)
			{
				Debug.Log($"Creating scene texture.. {w}x{h} MSAA={aa} [{format}]");
				_sceneTexture = new RenderTexture(w, h, 0, format, 0);
				_sceneTexture.depth = 32;
				_sceneTexture.antiAliasing = aa;
				_sceneTexture.useMipMap = false;

				// OpenVR assumes floating point render targets are linear unless otherwise specified.
				var colorSpace = (hdr && QualitySettings.activeColorSpace == ColorSpace.Gamma) ? EColorSpace.Gamma : EColorSpace.Auto;
				SteamVR.OpenVRMagic.SetColorSpace(colorSpace);
				if (recreatedTex)
				{
					OnResolutionChanged?.Invoke(w, h);
				}
			}

			sceneTextures[(int)eye] = _sceneTexture;

			return _sceneTexture;
		}

		#endregion

		#region Enable / Disable

		private void OnDisable()
		{
			SteamVR_Render.Remove(this);
		}

		public void Activate()
		{
			// Bail if no hmd is connected
			var vr = SteamVR.instance;
			if (vr == null)
			{
				if (head != null)
				{
					if (head.GetComponent<SteamVR_GameView>())
					{
						head.GetComponent<SteamVR_GameView>().enabled = false;
					}
					if (head.GetComponent<SteamVR_GameView>())
					{
						head.GetComponent<SteamVR_TrackedObject>().enabled = false;
					}
				}

				if (flip != null)
					flip.enabled = false;

				enabled = false;
				return;
			}

			if (blitMaterial == null)
			{
				blitMaterial = new Material(VRShaders.GetShader(VRShaders.VRShader.blit));
			}

			// Set remaining hmd specific settings
			camera = GetComponent<Camera>();
			camera.fieldOfView = vr.fieldOfView;
			camera.aspect = vr.aspect;
			camera.eventMask = 0;           // disable mouse events
			camera.orthographic = false;    // force perspective
			camera.enabled = true;
			camera.backgroundColor = new Color(0.0f, 0.0f, 0.0f);
			camera.clearFlags = CameraClearFlags.Color;

			HDCamera hdCamera = HDCamera.GetOrCreate(camera);
			if (hdCamera != null)
			{
				Debug.Log("Motion Vectors: " + hdCamera.frameSettings.IsEnabled(FrameSettingsField.MotionVectors));
				Debug.Log("Motion Blur: " + hdCamera.frameSettings.IsEnabled(FrameSettingsField.MotionBlur));
				hdCamera.frameSettings.SetEnabled(FrameSettingsField.MotionVectors, false);
				hdCamera.frameSettings.SetEnabled(FrameSettingsField.MotionBlur, false);
				Debug.Log("Motion Vectors: " + hdCamera.frameSettings.IsEnabled(FrameSettingsField.MotionVectors));
				Debug.Log("Motion Blur: " + hdCamera.frameSettings.IsEnabled(FrameSettingsField.MotionBlur));
			}

			TrueMask = camera.cullingMask;
			camera.cullingMask = 0;

			if (camera.actualRenderingPath != RenderingPath.Forward && QualitySettings.antiAliasing > 1)
			{
				Debug.LogWarning("MSAA only supported in Forward rendering path. (disabling MSAA)");
				QualitySettings.antiAliasing = 0;
			}

			if (!head)
			{
				_head = transform.parent;
			}

			// Ensure game view camera hdr setting matches
			if (head)
			{
				var headCam = head.GetComponent<Camera>();
				if (headCam != null)
				{
					headCam.renderingPath = camera.renderingPath;
				}
			}

			if (ears == null)
			{
				var e = transform.GetComponentInChildren<SteamVR_Ears>();
				if (e != null)
					_ears = e.transform;
			}

			if (ears != null)
				ears.GetComponent<SteamVR_Ears>().vrcam = this;

			SteamVR_Render.Add(this);
		}

		#endregion

		public void PreRender()
		{
			if (SteamVR.instance == null)
			{
				Debug.LogError("No SteamVR");
				return;
			}
			if (camera == null)
			{
				Debug.LogError("No Camera");
				return;
			}
			if (SteamVR.instance.eyes[(int)eye] == null)
			{
				Debug.LogError("No " + eye + " eye");
				return;
			}

			transform.localPosition = SteamVR.instance.eyes[(int)eye].pos;
			transform.localRotation = SteamVR.instance.eyes[(int)eye].rot;
			camera.targetTexture = GetSceneTexture(eye, camera.allowHDR);
			camera.cullingMask = TrueMask;

			if (SteamVR_Render.instance.cameraMask != null)
			{
				SteamVR_Render.instance.cameraMask.transform.position = transform.position;
				SteamVR_Render.instance.cameraMask.Set(SteamVR.instance, eye, camera);
			}
		}
		

		public static bool useHeadTracking = true;


	}
}