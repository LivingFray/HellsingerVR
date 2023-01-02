using UnityEngine;

namespace HellsingerVR.ViewModels
{
	internal class VulcanViewModel : ViewModel
	{

		public VulcanViewModel()
		{
			OffsetVector = new Vector3(-0.2f, 0.0f, 0.0f);
			OffsetRotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
			MuzzleOffset = new Vector3(0.6f, 0.0f, 0.0f);
		}

		public override void OnEquip()
		{
			RootMesh = GameObject.Find("PlayerCharacter(Clone)/Unknown_Rig:Weapons/Unknown_Rig:p_Weapon_rig:Vulcan/Unknown_Rig:p_Weapon_rig:Vulcan_1p:vulcan_base").GetComponent<SkinnedMeshRenderer>();
			RootBone = GameObject.Find("PlayerCharacter(Clone)/Unknown_Rig:Root/Unknown_Rig:Hips/Unknown_Rig:Spine/Unknown_Rig:Spine1/Unknown_Rig:SpineX/Unknown_Rig:p_Weapon_rig:Root").transform;
		}
	}
}