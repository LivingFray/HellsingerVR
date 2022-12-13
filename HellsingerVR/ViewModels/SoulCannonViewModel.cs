using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace HellsingerVR.ViewModels
{
	class SoulCannonViewModel : ViewModel
	{

		public SoulCannonViewModel()
		{
			OffsetVector = new Vector3(-0.15f, -0.05f, 0.0f);
			OffsetRotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
		}

		public override void OnEquip()
		{
			RootMesh = GameObject.Find("PlayerCharacter(Clone)/Unknown_Rig:Weapons/Unknown_Rig:p_Weapon_rig:SM_SoulCannon/Unknown_Rig:p_Weapon_rig:BoneHandle/Unknown_Rig:p_Weapon_rig:BoneHandle 1").GetComponent<SkinnedMeshRenderer>();
			RootBone = GameObject.Find("PlayerCharacter(Clone)/Unknown_Rig:Root/Unknown_Rig:Hips/Unknown_Rig:Spine/Unknown_Rig:Spine1/Unknown_Rig:SpineX/Unknown_Rig:p_Weapon_rig:Root").transform;
		}
	}
}