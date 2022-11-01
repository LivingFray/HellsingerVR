﻿//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: Generates a mesh based on field of view.
//
//=============================================================================

using System;
using UnityEngine;

namespace Valve.VR
{

	public class SteamVR_Frustum : MonoBehaviour
    {

        public SteamVR_Frustum(IntPtr value)
: base(value) { }

        public SteamVR_TrackedObject.EIndex index;

        public float fovLeft = 45, fovRight = 45, fovTop = 45, fovBottom = 45, nearZ = 0.5f, farZ = 2.5f;

        public void UpdateModel()
        {
            fovLeft = SteamVR_Standalone_IL2CPP.Util.Mathf.Clamp(fovLeft, 1, 89);
            fovRight = SteamVR_Standalone_IL2CPP.Util.Mathf.Clamp(fovRight, 1, 89);
            fovTop = SteamVR_Standalone_IL2CPP.Util.Mathf.Clamp(fovTop, 1, 89);
            fovBottom = SteamVR_Standalone_IL2CPP.Util.Mathf.Clamp(fovBottom, 1, 89);
            farZ = SteamVR_Standalone_IL2CPP.Util.Mathf.Max(farZ, nearZ + 0.01f);
            nearZ = SteamVR_Standalone_IL2CPP.Util.Mathf.Clamp(nearZ, 0.01f, farZ - 0.01f);

            var lsin = SteamVR_Standalone_IL2CPP.Util.Mathf.Sin(-fovLeft * SteamVR_Standalone_IL2CPP.Util.Mathf.Deg2Rad);
            var rsin = SteamVR_Standalone_IL2CPP.Util.Mathf.Sin(fovRight * SteamVR_Standalone_IL2CPP.Util.Mathf.Deg2Rad);
            var tsin = SteamVR_Standalone_IL2CPP.Util.Mathf.Sin(fovTop * SteamVR_Standalone_IL2CPP.Util.Mathf.Deg2Rad);
            var bsin = SteamVR_Standalone_IL2CPP.Util.Mathf.Sin(-fovBottom * SteamVR_Standalone_IL2CPP.Util.Mathf.Deg2Rad);

            var lcos = SteamVR_Standalone_IL2CPP.Util.Mathf.Cos(-fovLeft * SteamVR_Standalone_IL2CPP.Util.Mathf.Deg2Rad);
            var rcos = SteamVR_Standalone_IL2CPP.Util.Mathf.Cos(fovRight * SteamVR_Standalone_IL2CPP.Util.Mathf.Deg2Rad);
            var tcos = SteamVR_Standalone_IL2CPP.Util.Mathf.Cos(fovTop * SteamVR_Standalone_IL2CPP.Util.Mathf.Deg2Rad);
            var bcos = SteamVR_Standalone_IL2CPP.Util.Mathf.Cos(-fovBottom * SteamVR_Standalone_IL2CPP.Util.Mathf.Deg2Rad);

            var corners = new Vector3[] {
            new Vector3(lsin * nearZ / lcos, tsin * nearZ / tcos, nearZ), //tln
			new Vector3(rsin * nearZ / rcos, tsin * nearZ / tcos, nearZ), //trn
			new Vector3(rsin * nearZ / rcos, bsin * nearZ / bcos, nearZ), //brn
			new Vector3(lsin * nearZ / lcos, bsin * nearZ / bcos, nearZ), //bln
			new Vector3(lsin * farZ  / lcos, tsin * farZ  / tcos, farZ ), //tlf
			new Vector3(rsin * farZ  / rcos, tsin * farZ  / tcos, farZ ), //trf
			new Vector3(rsin * farZ  / rcos, bsin * farZ  / bcos, farZ ), //brf
			new Vector3(lsin * farZ  / lcos, bsin * farZ  / bcos, farZ ), //blf
		};

            var triangles = new int[] {
		//	0, 1, 2, 0, 2, 3, // near
		//	0, 2, 1, 0, 3, 2, // near
		//	4, 5, 6, 4, 6, 7, // far
		//	4, 6, 5, 4, 7, 6, // far
			0, 4, 7, 0, 7, 3, // left
			0, 7, 4, 0, 3, 7, // left
			1, 5, 6, 1, 6, 2, // right
			1, 6, 5, 1, 2, 6, // right
			0, 4, 5, 0, 5, 1, // top
			0, 5, 4, 0, 1, 5, // top
			2, 3, 7, 2, 7, 6, // bottom
			2, 7, 3, 2, 6, 7, // bottom
		};

            int j = 0;
            var vertices = new Vector3[triangles.Length];
            var normals = new Vector3[triangles.Length];
            for (int i = 0; i < triangles.Length / 3; i++)
            {
                var a = corners[triangles[i * 3 + 0]];
                var b = corners[triangles[i * 3 + 1]];
                var c = corners[triangles[i * 3 + 2]];
                var n = Vector3.Cross(b - a, c - a).normalized;
                normals[i * 3 + 0] = n;
                normals[i * 3 + 1] = n;
                normals[i * 3 + 2] = n;
                vertices[i * 3 + 0] = a;
                vertices[i * 3 + 1] = b;
                vertices[i * 3 + 2] = c;
                triangles[i * 3 + 0] = j++;
                triangles[i * 3 + 1] = j++;
                triangles[i * 3 + 2] = j++;
            }

            var mesh = new Mesh();
            mesh.vertices = vertices;
            mesh.normals = normals;
            mesh.triangles = triangles;

            GetComponent<MeshFilter>().mesh = mesh;
        }

        private void OnDeviceConnected(int i, bool connected)
        {
            if (i != (int)index)
                return;

            GetComponent<MeshFilter>().mesh = null;

            if (connected)
            {
                var system = OpenVR.System;
                if (system != null && system.GetTrackedDeviceClass((uint)i) == ETrackedDeviceClass.TrackingReference)
                {
                    var error = ETrackedPropertyError.TrackedProp_Success;
                    var result = system.GetFloatTrackedDeviceProperty((uint)i, ETrackedDeviceProperty.Prop_FieldOfViewLeftDegrees_Float, ref error);
                    if (error == ETrackedPropertyError.TrackedProp_Success)
                        fovLeft = result;

                    result = system.GetFloatTrackedDeviceProperty((uint)i, ETrackedDeviceProperty.Prop_FieldOfViewRightDegrees_Float, ref error);
                    if (error == ETrackedPropertyError.TrackedProp_Success)
                        fovRight = result;

                    result = system.GetFloatTrackedDeviceProperty((uint)i, ETrackedDeviceProperty.Prop_FieldOfViewTopDegrees_Float, ref error);
                    if (error == ETrackedPropertyError.TrackedProp_Success)
                        fovTop = result;

                    result = system.GetFloatTrackedDeviceProperty((uint)i, ETrackedDeviceProperty.Prop_FieldOfViewBottomDegrees_Float, ref error);
                    if (error == ETrackedPropertyError.TrackedProp_Success)
                        fovBottom = result;

                    result = system.GetFloatTrackedDeviceProperty((uint)i, ETrackedDeviceProperty.Prop_TrackingRangeMinimumMeters_Float, ref error);
                    if (error == ETrackedPropertyError.TrackedProp_Success)
                        nearZ = result;

                    result = system.GetFloatTrackedDeviceProperty((uint)i, ETrackedDeviceProperty.Prop_TrackingRangeMaximumMeters_Float, ref error);
                    if (error == ETrackedPropertyError.TrackedProp_Success)
                        farZ = result;

                    UpdateModel();
                }
            }
        }

        void OnEnable()
        {
            GetComponent<MeshFilter>().mesh = null;
            SteamVR_Events.DeviceConnected.Listen(OnDeviceConnected);
        }

        void OnDisable()
        {
            SteamVR_Events.DeviceConnected.Remove(OnDeviceConnected);
            GetComponent<MeshFilter>().mesh = null;
        }

#if UNITY_EDITOR
        void Update()
        {
            if (!Application.isPlaying)
                UpdateModel();
        }
#endif
    }
}