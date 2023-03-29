using UnityEngine;

namespace HellsingerVR.ViewModels
{
	internal class TerminusViewModel : ViewModel
	{

		public TerminusViewModel()
		{
			OffsetVector = new Vector3(0.0f, 0.0f, 0.0f);
			OffsetRotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
		}

		public override void OnEquip()
		{
			RootMesh = GameObject.Find("PlayerCharacter(Clone)/Unknown_Rig:Weapons/Unknown_Rig:p_Weapon_rig:Falx01/Unknown_Rig:p_Weapon_rig:Terminus_bone").GetComponent<SkinnedMeshRenderer>();
			RootBone = GameObject.Find("PlayerCharacter(Clone)/Unknown_Rig:Root/Unknown_Rig:Hips/Unknown_Rig:Spine/Unknown_Rig:Spine1/Unknown_Rig:SpineX/Unknown_Rig:p_Weapon_rig:Root").transform;
		}
	}
}
