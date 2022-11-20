using HellsingerVR.Components;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace HellsingerVR.ViewModels
{
	public abstract class ViewModel
	{
		protected SkinnedMeshRenderer RootMesh;
		protected Transform RootBone;

		protected Vector3 OffsetVector;
		protected Quaternion OffsetRotation;

		public abstract void OnEquip();
		public virtual void Update()
		{
			// TODO: Handedness
			bool IsRightHanded = true;
			(Vector3 location, Quaternion rotation) = VRInputManager.GetHandTransform(!IsRightHanded);

			RootBone.rotation = rotation * OffsetRotation * RootBone.localRotation;

			Vector3 FirstBone = RootBone.rotation * RootMesh.bones[1].localPosition;

			RootBone.position = (location + OffsetVector) - FirstBone;
		}
	}
}
