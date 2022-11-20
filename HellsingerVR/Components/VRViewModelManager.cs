using HellsingerVR.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace HellsingerVR.Components
{

	public class VRViewModelManager : MonoBehaviour
	{

		float R_x = 0.0f;
		float R_y = 0.0f;
		float R_z = 0.0f;
		Quaternion RotationOffset;

		// TODO: Spawned effects (like Paz's fire eyes) have offsets applied (as particle effects)

		static Dictionary<PlayerWeaponType, ViewModel> ViewModels = new Dictionary<PlayerWeaponType, ViewModel>() {
			{ PlayerWeaponType.Falx, new TerminusViewModel() },
			{ PlayerWeaponType.RhythmWeapon, new PazViewModel() },
			{ PlayerWeaponType.Shotgun, new SoulCannonViewModel() },
			{ PlayerWeaponType.Pistols, new PistolsViewModel() },
			{ PlayerWeaponType.Vulcan, new VulcanViewModel() },
			{ PlayerWeaponType.Boomerang, new HellCrowViewModel() },
		};

		ViewModel ActiveModel;

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
					Debug.Log(smr.name + ": Couldn't set EnableFpsMode to 0");
				}
			}
		}

		// Find somewhere to patch into to trigger this on weapon change
		public void OnChangeWeapon(PlayerWeaponType NewWeapon)
		{
			Debug.Log("OnChangeWeapon: " + NewWeapon.ToString());

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


			if (Input.GetKeyDown(KeyCode.U))
			{
				R_x += 5.0f;
				RotationOffset = Quaternion.Euler(R_x, R_y, R_z);
				Debug.Log($"({R_x}, {R_y}, {R_z})");
			}
			if (Input.GetKeyDown(KeyCode.J))
			{
				R_x -= 5.0f;
				RotationOffset = Quaternion.Euler(R_x, R_y, R_z);
				Debug.Log($"({R_x}, {R_y}, {R_z})");
			}
			if (Input.GetKeyDown(KeyCode.I))
			{
				R_y += 5.0f;
				RotationOffset = Quaternion.Euler(R_x, R_y, R_z);
				Debug.Log($"({R_x}, {R_y}, {R_z})");
			}
			if (Input.GetKeyDown(KeyCode.K))
			{
				R_y -= 5.0f;
				RotationOffset = Quaternion.Euler(R_x, R_y, R_z);
				Debug.Log($"({R_x}, {R_y}, {R_z})");
			}
			if (Input.GetKeyDown(KeyCode.O))
			{
				R_z += 5.0f;
				RotationOffset = Quaternion.Euler(R_x, R_y, R_z);
				Debug.Log($"({R_x}, {R_y}, {R_z})");
			}
			if (Input.GetKeyDown(KeyCode.L))
			{
				R_z -= 5.0f;
				RotationOffset = Quaternion.Euler(R_x, R_y, R_z);
				Debug.Log($"({R_x}, {R_y}, {R_z})");
			}

		}

	}
}