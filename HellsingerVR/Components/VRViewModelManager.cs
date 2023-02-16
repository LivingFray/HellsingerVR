using HellsingerVR.ViewModels;
using System.Collections.Generic;
using UnityEngine;

namespace HellsingerVR.Components
{

	public class VRViewModelManager : MonoBehaviour
	{
		private float P_x = 0.0f;
		private float P_y = 0.0f;
		private float P_z = 0.0f;
		private Vector3 PositionOffset = Vector3.zero;

		// TODO: Spawned effects (like Paz's fire eyes) have offsets applied (as particle effects)

		private static Dictionary<PlayerWeaponType, ViewModel> ViewModels = new Dictionary<PlayerWeaponType, ViewModel>() {
			{ PlayerWeaponType.Falx, new TerminusViewModel() },
			{ PlayerWeaponType.RhythmWeapon, new PazViewModel() },
			{ PlayerWeaponType.Shotgun, new SoulCannonViewModel() },
			{ PlayerWeaponType.Pistols, new PistolsViewModel() },
			{ PlayerWeaponType.Vulcan, new VulcanViewModel() },
			{ PlayerWeaponType.Boomerang, new HellCrowViewModel() },
		};
		private ViewModel ActiveModel;

		public void OnDisable()
		{
			ActiveModel = null;
		}

		public void HideArms()
		{
			GameObject arms = GameObject.Find("PlayerCharacter(Clone)/Unknown_Rig:GEO/Unknown_Rig:Mesh_1p");
			if (arms)
			{
				arms.SetActive(false);
				// Maybe can do some funky ik?
			}

			// Stop strange first person camera offset applied in the shader
			SkinnedMeshRenderer[] smrs = GameObject.Find("PlayerCharacter(Clone)/Unknown_Rig:Weapons").GetComponentsInChildren<SkinnedMeshRenderer>(true);
			foreach (SkinnedMeshRenderer smr in smrs)
			{
				try
				{
					foreach (Material material in smr.materials)
					{
						material.SetFloat("_EnableFpsMode", 0.0f);
					}
				}
				catch
				{
					HellsingerVR._instance.Log.LogInfo(smr.name + ": Couldn't set EnableFpsMode to 0");
				}
			}
		}

		// Find somewhere to patch into to trigger this on weapon change
		public void OnChangeWeapon(PlayerWeaponType NewWeapon)
		{
			HellsingerVR._instance.Log.LogInfo("OnChangeWeapon: " + NewWeapon.ToString());

			if (ViewModels.ContainsKey(NewWeapon))
			{
				ActiveModel = ViewModels[NewWeapon];
				ActiveModel.OnEquip();
			}
			else
			{
				ActiveModel = null;
			}
		}

		public void LateUpdate()
		{
			if (ActiveModel == null)
			{
				return;
			}

			ActiveModel.Update();

			/*
			if (Input.GetKeyDown(KeyCode.U))
			{
				P_x += 0.05f;
				PositionOffset = new Vector3(P_x, P_y, P_z);
				HellsingerVR._instance.Log.LogInfo($"Pos: ({P_x}, {P_y}, {P_z})");
				ActiveModel.SetMuzzleOffset(PositionOffset);
			}
			if (Input.GetKeyDown(KeyCode.J))
			{
				P_x -= 0.05f;
				PositionOffset = new Vector3(P_x, P_y, P_z);
				HellsingerVR._instance.Log.LogInfo($"Pos: ({P_x}, {P_y}, {P_z})");
				ActiveModel.SetMuzzleOffset(PositionOffset);
			}
			if (Input.GetKeyDown(KeyCode.I))
			{
				P_y += 0.05f;
				PositionOffset = new Vector3(P_x, P_y, P_z);
				HellsingerVR._instance.Log.LogInfo($"Pos: ({P_x}, {P_y}, {P_z})");
				ActiveModel.SetMuzzleOffset(PositionOffset);
			}
			if (Input.GetKeyDown(KeyCode.K))
			{
				P_y -= 0.05f;
				PositionOffset = new Vector3(P_x, P_y, P_z);
				HellsingerVR._instance.Log.LogInfo($"Pos: ({P_x}, {P_y}, {P_z})");
				ActiveModel.SetMuzzleOffset(PositionOffset);
			}
			if (Input.GetKeyDown(KeyCode.O))
			{
				P_z += 0.05f;
				PositionOffset = new Vector3(P_x, P_y, P_z);
				HellsingerVR._instance.Log.LogInfo($"Pos: ({P_x}, {P_y}, {P_z})");
				ActiveModel.SetMuzzleOffset(PositionOffset);
			}
			if (Input.GetKeyDown(KeyCode.L))
			{
				P_z -= 0.05f;
				PositionOffset = new Vector3(P_x, P_y, P_z);
				HellsingerVR._instance.Log.LogInfo($"Pos: ({P_x}, {P_y}, {P_z})");
				ActiveModel.SetMuzzleOffset(PositionOffset);
			}
			*/
		}

		public static Vector3 GetMuzzleOffset(PlayerWeaponType weaponType)
		{
			if (ViewModels.ContainsKey(weaponType))
			{
				return ViewModels[weaponType].GetMuzzleOffset();
			}
			return Vector3.zero;
		}
	}
}