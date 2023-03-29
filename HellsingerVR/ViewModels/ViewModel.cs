using HellsingerVR.Components;
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
			bool IsLeftHanded = HellsingerVR._instance.IsLeftHanded.Value;
			(Vector3 location, Quaternion rotation) = VRInputManager.GetHandTransform(IsLeftHanded);

			RootBone.rotation = rotation * OffsetRotation * RootBone.localRotation;

			Vector3 FirstBone = RootBone.rotation * RootMesh.bones[1].localPosition;

			RootBone.position = location + (RootBone.rotation * OffsetVector) - FirstBone;
		}
	}
}
