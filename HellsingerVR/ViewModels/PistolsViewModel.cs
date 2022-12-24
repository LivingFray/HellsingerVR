using HellsingerVR.Components;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace HellsingerVR.ViewModels
{
	class PistolsViewModel : ViewModel
	{
		GameObject leftHandBones;
		GameObject rightHandBones;

		SkinnedMeshRenderer[] CerberusMeshes;
		SkinnedMeshRenderer CerberusFrame;
		SkinnedMeshRenderer PersephoneFrame;

		Dictionary<Transform, Transform> LeftHandMap = new Dictionary<Transform, Transform>();

		public PistolsViewModel()
		{
			OffsetVector = new Vector3(-0.05f, 0.0f, 0.0f);
			OffsetRotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
			MuzzleOffset = new Vector3(0.4f, 0.0f, 0.0f);
		}

		public override void OnEquip()
		{
			leftHandBones = GameObject.Find("VR_LeftHandBones");
			rightHandBones = GameObject.Find("PlayerCharacter(Clone)/Unknown_Rig:Root/Unknown_Rig:Hips/Unknown_Rig:Spine/Unknown_Rig:Spine1/Unknown_Rig:SpineX/Unknown_Rig:p_Weapon_rig:Root");

			CerberusMeshes = GameObject.Find("PlayerCharacter(Clone)/Unknown_Rig:Weapons/Unknown_Rig:p_Weapon_rig:DualWieldPistols/Unknown_Rig:p_Weapon_rig:Cerberus").GetComponentsInChildren<SkinnedMeshRenderer>();
			CerberusFrame = GameObject.Find("PlayerCharacter(Clone)/Unknown_Rig:Weapons/Unknown_Rig:p_Weapon_rig:DualWieldPistols/Unknown_Rig:p_Weapon_rig:Cerberus/Unknown_Rig:p_Weapon_rig:Cerberus_Frame").GetComponent<SkinnedMeshRenderer>();
			PersephoneFrame = GameObject.Find("PlayerCharacter(Clone)/Unknown_Rig:Weapons/Unknown_Rig:p_Weapon_rig:DualWieldPistols/Unknown_Rig:p_Weapon_rig:Persephone/Unknown_Rig:p_Weapon_rig:Persephone_Frame").GetComponent<SkinnedMeshRenderer>();

			if (leftHandBones == null)
			{
				leftHandBones = UnityEngine.Object.Instantiate(rightHandBones);
				leftHandBones.name = "VR_LeftHandBones";
			}

			LeftHandMap.Clear();

			foreach (SkinnedMeshRenderer s in CerberusMeshes)
			{
				Transform[] bones = new Transform[s.bones.Count];
				bones[0] = leftHandBones.transform;
				bones[0].rotation = rightHandBones.transform.rotation;

				for (int i = 1; i < s.bones.Count; i++)
				{
					Transform o = rightHandBones.transform.Find(s.bones[i].name);
					Transform t = leftHandBones.transform.Find(s.bones[i].name);
					bones[i] = t;
					LeftHandMap.Add(t, o);
				}
				s.bones = bones;
			}
		}

		public override void Update()
		{
			// Cerberus (left)
			{
				(Vector3 location, Quaternion rotation) = VRInputManager.GetHandTransform(true);

				foreach (KeyValuePair<Transform, Transform> valuePair in LeftHandMap)
				{
					valuePair.Key.localPosition = valuePair.Value.localPosition;
					valuePair.Key.localRotation = valuePair.Value.localRotation;
					valuePair.Key.localScale    = valuePair.Value.localScale;
				}

				leftHandBones.transform.rotation = rotation * OffsetRotation * rightHandBones.transform.localRotation;

				Vector3 FirstBone = leftHandBones.transform.rotation * CerberusFrame.bones[1].localPosition;

				Vector3 targetPosition = (location + leftHandBones.transform.rotation * OffsetVector) - FirstBone;

				leftHandBones.transform.position = targetPosition;

			}

			// Persephone (right)
			{
				(Vector3 location, Quaternion rotation) = VRInputManager.GetHandTransform(false);

				rightHandBones.transform.rotation = rotation * OffsetRotation * rightHandBones.transform.localRotation;

				Vector3 FirstBone = rightHandBones.transform.rotation * PersephoneFrame.bones[1].localPosition;

				Vector3 targetPosition = (location + rightHandBones.transform.rotation * OffsetVector) - FirstBone;

				rightHandBones.transform.position = targetPosition;
			}
		}
	}
}
