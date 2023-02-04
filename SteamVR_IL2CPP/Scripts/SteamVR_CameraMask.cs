using Assets.SteamVR_Standalone.Standalone;
using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;

namespace Valve.VR
{

	public class SteamVR_CameraMask : MonoBehaviour
	{
		public SteamVR_CameraMask(IntPtr value)
: base(value) { }

		public MeshRenderer meshRenderer;

		public static Material occlusionMaterial;


		private static Mesh[] hiddenAreaMeshes = new Mesh[2];


		public MeshFilter meshFilter;

		
		private void Awake()
		{
			this.meshFilter = GetComponent<MeshFilter>();
			if (this.meshFilter == null)
			{
				this.meshFilter = base.gameObject.AddComponent<MeshFilter>();
			}
			if (SteamVR_CameraMask.occlusionMaterial == null)
			{
				//SteamVR_CameraMask.occlusionMaterial = new Material(VRShaders.GetShader(VRShaders.VRShader.occlusion));
				SteamVR_CameraMask.occlusionMaterial = new Material(Shader.Find("HDRP/Unlit"));
				occlusionMaterial.color = Color.black;
			}
			meshRenderer = base.GetComponent<MeshRenderer>();
			if (meshRenderer == null)
			{
				meshRenderer = base.gameObject.AddComponent<MeshRenderer>();
			}
			meshRenderer.material = SteamVR_CameraMask.occlusionMaterial;
			meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
			meshRenderer.receiveShadows = false;
			meshRenderer.lightProbeUsage = LightProbeUsage.Off;
			meshRenderer.lightProbeUsage = LightProbeUsage.Off;
			meshRenderer.reflectionProbeUsage = ReflectionProbeUsage.Off;

			gameObject.layer = LayerMask.NameToLayer("UI");
		}

		[Il2CppInterop.Runtime.Attributes.HideFromIl2Cpp()]
		public void Set(SteamVR vr, EVREye eye, Camera camera)
		{
			if (SteamVR_CameraMask.hiddenAreaMeshes[(int)eye] == null)
			{
				SteamVR_CameraMask.hiddenAreaMeshes[(int)eye] = SteamVR_CameraMask.CreateHiddenAreaMesh(vr.hmd.GetHiddenAreaMesh(eye, EHiddenAreaMeshType.k_eHiddenAreaMesh_Standard), vr.textureBounds[(int)eye]);
			}
			this.meshFilter.mesh = SteamVR_CameraMask.hiddenAreaMeshes[(int)eye];

			// Scale to fit the camera

			Vector2 Size = camera.GetFrustumPlaneSizeAt(camera.nearClipPlane + 0.001f);

			// I don't know why, but it needs to be grown slightly to fit the screen
			float s = 1.03359375f;

			// Generated mesh assumes range of [-1,1] on x and y
			transform.localScale = new Vector3(Size.x * 0.5f * s, Size.y * 0.5f * s, 1.0f);
			transform.position = camera.transform.position + camera.transform.forward * (camera.nearClipPlane + 0.001f);
			transform.rotation = camera.transform.rotation;
		}


		public void Clear()
		{
			this.meshFilter.mesh = null;
		}


		public static Mesh CreateHiddenAreaMesh(HiddenAreaMesh_t src, VRTextureBounds_t bounds)
		{
			Debug.Log("Created Hidden Area Mesh");
			if (src.unTriangleCount == 0u)
			{
				float uMin = (2f * bounds.uMin) - 1f;
				float uMax = (2f * bounds.uMax) - 1f;
				float vMin = (2f * bounds.vMin) - 1f;
				float vMax = (2f * bounds.vMax) - 1f;

				Vector3[] verts = new Vector3[] {
						// Left
						new Vector3(-1, -1, 0),
						new Vector3(uMin, -1, 0),
						new Vector3(-1, 1, 0),
						new Vector3(uMin, 1, 0),
						// Right
						new Vector3(uMax, -1, 0),
						new Vector3(1, -1, 0),
						new Vector3(uMax, 1, 0),
						new Vector3(1, 1, 0)
				};

				int[] tris = new int[12]
				{
					// lower left triangle
					0, 2, 1,
					// upper right triangle
					2, 3, 1,

					// lower left triangle
					4, 6, 5,
					// upper right triangle
					6, 7, 5,
				};

				// Force the sides to not render
				Mesh m2 = new Mesh();
				m2.vertices = verts;
				m2.triangles = tris;
				m2.bounds = new Bounds(Vector3.zero, new Vector3(float.MaxValue, float.MaxValue, float.MaxValue));
				return m2;
			}
			float[] array = new float[src.unTriangleCount * 3u * 2u];
			Marshal.Copy(src.pVertexData, array, 0, array.Length);
			Vector3[] array2 = new Vector3[(src.unTriangleCount * 3u) + 12u];
			int[] array3 = new int[(src.unTriangleCount * 3u) + 24u];
			float num = (2f * bounds.uMin) - 1f;
			float num2 = (2f * bounds.uMax) - 1f;
			float num3 = (2f * bounds.vMin) - 1f;
			float num4 = (2f * bounds.vMax) - 1f;
			int num5 = 0;
			int num6 = 0;
			while (num5 < (long)(ulong)(src.unTriangleCount * 3u))
			{
				float x = SteamVR_Utils.Lerp(num, num2, array[num6++]);
				float y = SteamVR_Utils.Lerp(num3, num4, array[num6++]);
				array2[num5] = new Vector3(x, y, 0f);
				array3[num5] = num5;
				num5++;
			}
			int num7 = (int)(src.unTriangleCount * 3u);
			int num8 = num7;
			array2[num8++] = new Vector3(-1f, -1f, 0f);
			array2[num8++] = new Vector3(num, -1f, 0f);
			array2[num8++] = new Vector3(-1f, 1f, 0f);
			array2[num8++] = new Vector3(num, 1f, 0f);
			array2[num8++] = new Vector3(num2, -1f, 0f);
			array2[num8++] = new Vector3(1f, -1f, 0f);
			array2[num8++] = new Vector3(num2, 1f, 0f);
			array2[num8++] = new Vector3(1f, 1f, 0f);
			array2[num8++] = new Vector3(num, num3, 0f);
			array2[num8++] = new Vector3(num2, num3, 0f);
			array2[num8++] = new Vector3(num, num4, 0f);
			array2[num8++] = new Vector3(num2, num4, 0f);
			int num9 = num7;
			array3[num9++] = num7;
			array3[num9++] = num7 + 1;
			array3[num9++] = num7 + 2;
			array3[num9++] = num7 + 2;
			array3[num9++] = num7 + 1;
			array3[num9++] = num7 + 3;
			array3[num9++] = num7 + 4;
			array3[num9++] = num7 + 5;
			array3[num9++] = num7 + 6;
			array3[num9++] = num7 + 6;
			array3[num9++] = num7 + 5;
			array3[num9++] = num7 + 7;
			array3[num9++] = num7 + 1;
			array3[num9++] = num7 + 4;
			array3[num9++] = num7 + 8;
			array3[num9++] = num7 + 8;
			array3[num9++] = num7 + 4;
			array3[num9++] = num7 + 9;
			array3[num9++] = num7 + 10;
			array3[num9++] = num7 + 11;
			array3[num9++] = num7 + 3;
			array3[num9++] = num7 + 3;
			array3[num9++] = num7 + 11;
			array3[num9++] = num7 + 6;
			Mesh m = new Mesh();
			m.vertices = array2;
			m.triangles = array3;
			m.bounds = new Bounds(Vector3.zero, new Vector3(float.MaxValue, float.MaxValue, float.MaxValue));
			return m;
		}



	}
}
