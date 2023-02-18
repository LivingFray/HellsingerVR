using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using Valve.VR;

namespace HellsingerVR.Components
{
	public class VRInputManager : MonoBehaviour
	{
		public static int PendingSnapDirection = 0;
		public static bool HasPendingSnapMove = false;
		public static bool LastHandToShootWasLeft = false;
		private float TimeOffset = 0.0f;
		private Gamepad VRInput;
		private FirstPersonController fpController;
		private PlayerWeaponType LastWeapon = PlayerWeaponType.Falx;
		private bool WasPrevWeaponPressed = false;
		private bool WasNextWeaponPressed = false;
		private bool AlignToHead;
		private bool UseLeftHand;
		private bool UseSnapTurn;

		private static bool init = false;

		static SteamVR_Action_Pose pose;
		static SteamVR_Action_Pose poseTip;

		SteamVR_Action_Vector2 moveAction;
		SteamVR_Action_Vector2 lookAction;
		SteamVR_Action_Boolean fireAction;
		SteamVR_Action_Boolean altFireAction;
		SteamVR_Action_Boolean dashAction;
		SteamVR_Action_Boolean jumpAction;
		SteamVR_Action_Boolean slaughterAction;
		SteamVR_Action_Boolean reloadAction;
		SteamVR_Action_Boolean gameMenuAction;
		SteamVR_Action_Boolean weaponSwitchLeftAction;
		SteamVR_Action_Boolean weaponSwitchRightAction;
		SteamVR_Action_Boolean weaponSwitchPazAction;

		SteamVR_Action_Vector2 navigateMenuAction;
		SteamVR_Action_Boolean menuSelectAction;
		SteamVR_Action_Boolean menuBackAction;
		SteamVR_Action_Boolean menuPrevAction;
		SteamVR_Action_Boolean menuNextAction;
		SteamVR_Action_Boolean menuCloseAction;

		public static (Vector3, Quaternion) GetHandTransform(bool LeftHand = false)
		{
			if (!init)
			{
				pose = SteamVR_Input.GetAction<SteamVR_Action_Pose>("Pose");
				poseTip = SteamVR_Input.GetAction<SteamVR_Action_Pose>("PoseTip");
				init = true;
			}

			Vector3 location = pose.GetLocalPosition(LeftHand ? SteamVR_Input_Sources.LeftHand : SteamVR_Input_Sources.RightHand);
			Quaternion rotation = poseTip.GetLocalRotation(LeftHand ? SteamVR_Input_Sources.LeftHand : SteamVR_Input_Sources.RightHand);

			if (HellsingerVR._instance.DisableMotionControls.Value)
			{
				// Fake hand positions in gamepad mode
				rotation = HellsingerVR.rig.head.localRotation;
				location = HellsingerVR.rig.head.localPosition;
				if (LeftHand)
				{
					location += rotation * new Vector3(-0.3f, -0.25f, 0.75f);
				}
				else
				{
					location += rotation * new Vector3(0.3f, -0.25f, 0.75f);
				}

			}

			location = HellsingerVR.rig.transform.TransformPoint(location);

			rotation = HellsingerVR.rig.transform.rotation * rotation * HellsingerVR.HandOffset;

			return (location, rotation);
		}

		private static bool IsInGame()
		{
			return !HellsingerVR.IsPaused && !HellsingerVR.IsLoading && HellsingerVR.rig && HellsingerVR.rig.InLevel;
		}

		public void Awake()
		{
			TimeOffset = Time.realtimeSinceStartup;
			string movementType = HellsingerVR._instance.MovementType.Value.ToLower();
			switch (movementType)
			{
				case "hand":
				case "mainhand":
					AlignToHead = false;
					UseLeftHand = HellsingerVR._instance.IsLeftHanded.Value;
					break;
				case "offhand":
					AlignToHead = false;
					UseLeftHand = !HellsingerVR._instance.IsLeftHanded.Value;
					break;
				default:
					AlignToHead = true;
					UseLeftHand = HellsingerVR._instance.IsLeftHanded.Value;
					break;
			}
			UseSnapTurn = HellsingerVR._instance.SnapTurningAngle.Value > 0.0f;
		}

		// Can't do the update early, so do it at the end of the frame ready for the next frame
		public void LateUpdate()
		{
			// Let steam input finish loading
			if (Time.realtimeSinceStartup < TimeOffset + 5.0f)
			{
				return;
			}

			if (VRInput == null)
			{
				VRInput = InputSystem.GetDevice<Gamepad>();
				if (VRInput == null)
				{
					VRInput = InputSystem.AddDevice<Gamepad>();
				}

				moveAction = SteamVR_Input.GetVector2Action("game", "Movement");
				lookAction = SteamVR_Input.GetVector2Action("game", "Look");
				fireAction = SteamVR_Input.GetBooleanAction("game", "Shoot");
				altFireAction = SteamVR_Input.GetBooleanAction("game", "ShootAlt");
				dashAction = SteamVR_Input.GetBooleanAction("game", "Dash");
				jumpAction = SteamVR_Input.GetBooleanAction("game", "Jump");
				slaughterAction = SteamVR_Input.GetBooleanAction("game", "Slaughter");
				reloadAction = SteamVR_Input.GetBooleanAction("game", "Reload");
				gameMenuAction = SteamVR_Input.GetBooleanAction("game", "OpenMenu");
				weaponSwitchLeftAction = SteamVR_Input.GetBooleanAction("game", "WeaponSwitchLeft");
				weaponSwitchRightAction = SteamVR_Input.GetBooleanAction("game", "WeaponSwitchRight");
				weaponSwitchPazAction = SteamVR_Input.GetBooleanAction("game", "WeaponSwitchPaz");

				navigateMenuAction = SteamVR_Input.GetVector2Action("menu", "Navigate");
				menuSelectAction = SteamVR_Input.GetBooleanAction("menu", "Select");
				menuBackAction = SteamVR_Input.GetBooleanAction("menu", "Back");
				menuPrevAction = SteamVR_Input.GetBooleanAction("menu", "PrevTab");
				menuNextAction = SteamVR_Input.GetBooleanAction("menu", "NextTab");
				menuCloseAction = SteamVR_Input.GetBooleanAction("menu", "CloseMenu");
			}

			InputEventPtr ptr;

			ModifiedCode.StateEvent_From(VRInput, new InputEventPtr(), out ptr);

			// Currently assumes default controller binding!
			try
			{
				if (IsInGame())
				{
					UpdateGameInputs(ptr);
				}
				else
				{
					UpdateMenuInputs(ptr);
				}
			}
			catch (Exception e)
			{
				HellsingerVR._instance.Log.LogError(e.ToString());
			}
			InputSystem.QueueEvent(ptr);

		}

		private void UpdateMenuInputs(InputEventPtr ptr)
		{
			// Navigation
			Vector2 nav = GetMenuVector();
			VRInput.leftStick.x.WriteValueIntoEvent(nav.x, ptr);
			VRInput.leftStick.y.WriteValueIntoEvent(nav.y, ptr);
			// Select
			VRInput.aButton.WriteValueIntoEvent(GetMenuSelect(), ptr);
			// Back
			VRInput.bButton.WriteValueIntoEvent(GetMenuBack(), ptr);
			// PrevTab
			VRInput.leftShoulder.WriteValueIntoEvent(GetMenuPrevTab(), ptr);
			// NextTab
			VRInput.rightShoulder.WriteValueIntoEvent(GetMenuNextTab(), ptr);
			// CloseMenu
			VRInput.startButton.WriteValueIntoEvent(GetMenuClose(), ptr);
		}

		private void UpdateGameInputs(InputEventPtr ptr)
		{
			// Movement
			Vector2 movement = GetMovementVector();
			VRInput.leftStick.x.WriteValueIntoEvent(movement.x, ptr);
			VRInput.leftStick.y.WriteValueIntoEvent(movement.y, ptr);
			// Look
			VRInput.rightStick.x.WriteValueIntoEvent(GetLookValue(), ptr);
			// Dash
			VRInput.leftShoulder.WriteValueIntoEvent(GetDashing(), ptr);
			// Jump
			VRInput.aButton.WriteValueIntoEvent(GetJumping(), ptr);
			// Shoot/ShootAlt
			VRInput.rightTrigger.WriteValueIntoEvent(GetShooting(), ptr);
			// Ultimate
			VRInput.leftTrigger.WriteValueIntoEvent(GetUltimate(), ptr);
			// Slaughter
			VRInput.rightStickButton.WriteValueIntoEvent(GetSlaughtering(), ptr);
			// Reload
			VRInput.xButton.WriteValueIntoEvent(GetReloading(), ptr);

			// Weapon switching

			float rightShoulder = 0.0f;
			float dpadUp = 0.0f;
			float dpadDown = 0.0f;
			float dpadLeft = 0.0f;
			float dpadRight = 0.0f;

			if (fpController == null)
			{
				fpController = FindObjectOfType<FirstPersonController>();
			}

			if (fpController != null)
			{
				Player player = fpController.m_player;
				if (player != null)
				{
					WeaponAbilityController weaponAbilityController = player.m_weaponAbilityController;
					if (weaponAbilityController != null)
					{
						LastWeapon = weaponAbilityController.m_activeWeaponType;
						bool WeaponSwitchLeft = weaponSwitchLeftAction.state;
						bool WeaponSwitchRight = weaponSwitchRightAction.state;

						bool SwitchToPrev = !WasPrevWeaponPressed && WeaponSwitchLeft;
						bool SwitchToNext = !WasNextWeaponPressed && WeaponSwitchRight;
						bool SwitchToPaz = weaponSwitchPazAction.state;

						WasPrevWeaponPressed = WeaponSwitchLeft;
						WasNextWeaponPressed = WeaponSwitchRight;

						if (SwitchToPaz)
						{
							rightShoulder = 1.0f;
						}
						else
						{
							int MoveIndex = 0;
							if (SwitchToNext)
							{
								MoveIndex = 1;
							}
							else if (SwitchToPrev)
							{
								MoveIndex = -1;
							}

							if (MoveIndex != 0)
							{
								int NewIndex = weaponAbilityController.m_carriedWeapons.IndexOf(weaponAbilityController.m_activeWeaponType) + MoveIndex;

								if (NewIndex < 0) NewIndex += weaponAbilityController.m_carriedWeapons.Count;
								NewIndex %= weaponAbilityController.m_carriedWeapons.Count;

								PlayerWeaponType NewWeapon = weaponAbilityController.m_carriedWeapons[NewIndex];
								
								if (NewWeapon == PlayerWeaponType.Falx)
								{
									dpadUp = 1.0f;
								}
								else if (NewWeapon == PlayerWeaponType.RhythmWeapon)
								{
									dpadDown = 1.0f;
								}
								else if (NewWeapon == weaponAbilityController.m_favoriteWeapon1)
								{
									dpadLeft = 1.0f;
								}
								else
								{
									dpadRight = 1.0f;
								}
							}
						}
					}
				}
			}

			VRInput.rightShoulder.WriteValueIntoEvent(rightShoulder, ptr);
			VRInput.dpad.up.WriteValueIntoEvent(dpadUp, ptr);
			VRInput.dpad.down.WriteValueIntoEvent(dpadDown, ptr);
			VRInput.dpad.left.WriteValueIntoEvent(dpadLeft, ptr);
			VRInput.dpad.right.WriteValueIntoEvent(dpadRight, ptr);

			// OpenMenu
			VRInput.startButton.WriteValueIntoEvent(GetPausing(), ptr);
		}

		#region Input Converter functions
		private Vector2 GetMenuVector()
		{
			return navigateMenuAction.GetAxis(SteamVR_Input_Sources.Any);
		}

		private float GetMenuSelect()
		{
			return menuSelectAction.state ? 1.0f : 0.0f;
		}

		private float GetMenuBack()
		{
			return menuBackAction.state ? 1.0f : 0.0f;
		}

		private float GetMenuPrevTab()
		{
			return menuPrevAction.state ? 1.0f : 0.0f;
		}

		private float GetMenuNextTab()
		{
			return menuNextAction.state ? 1.0f : 0.0f;
		}

		private float GetMenuClose()
		{
			return menuCloseAction.state ? 1.0f : 0.0f;
		}


		// Convert movement taking into account hmd/hand based movement config
		private Vector2 GetMovementVector()
		{
			Quaternion rotationQuat;

			if (AlignToHead)
			{
				rotationQuat = HellsingerVR.rig.head.rotation;
			}
			else
			{
				rotationQuat = GetHandTransform(UseLeftHand).Item2;
			}
			float rotation = rotationQuat.eulerAngles.y;

			if (HellsingerVR.rig.PlayerTransform)
			{
				rotation -= HellsingerVR.rig.PlayerTransform.rotation.eulerAngles.y;
			}

			return Quaternion.Euler(0, 0, -rotation) * moveAction.GetAxis(SteamVR_Input_Sources.Any);
		}

		private float GetLookValue()
		{
			if (UseSnapTurn)
			{
				const float Deadzone = 0.25f;
				float Val = lookAction.axis.x;
				
				HasPendingSnapMove = false;

				if (Math.Abs(Val) < Deadzone)
				{
					PendingSnapDirection = 0;
				}
				else if (Math.Sign(Val) != Math.Sign(PendingSnapDirection))
				{
					PendingSnapDirection = Math.Sign(Val);
					HasPendingSnapMove = true;
				}
				return 0.0f;
			}
			return lookAction.axis.x;
		}

		private float GetDashing()
		{
			return dashAction.state ? 1.0f : 0.0f;
		}

		private float GetJumping()
		{
			return jumpAction.state ? 1.0f : 0.0f;
		}

		private float GetShooting()
		{
			bool Shooting = fireAction.state;
			bool AltShooting = altFireAction.state;

			bool DidShoot = (Shooting || AltShooting) && !(Shooting && AltShooting);

			if (DidShoot)
			{
				if (Shooting)
				{
					LastHandToShootWasLeft = fireAction.activeDevice == SteamVR_Input_Sources.LeftHand;
				}
				else
				{
					LastHandToShootWasLeft = altFireAction.activeDevice == SteamVR_Input_Sources.LeftHand;
				}
			}

			// Filter out shots from the wrong hand when wielding a one handed gun
			bool CanDualWield = LastWeapon == PlayerWeaponType.Pistols || LastWeapon == PlayerWeaponType.Boomerang;
			if (!CanDualWield)
			{
				DidShoot &= LastHandToShootWasLeft == HellsingerVR._instance.IsLeftHanded.Value;
			}

			// Return true if ONE is active, need to replace this to account for one handed weapons
			return DidShoot ? 1.0f : 0.0f;
		}

		private float GetUltimate()
		{
			bool Shooting = fireAction.state;
			bool AltShooting = altFireAction.state;

			bool FireUlt = Shooting && AltShooting;

			// Since ult uses both hands, force it to be the dominant hand
			LastHandToShootWasLeft = HellsingerVR._instance.IsLeftHanded.Value;

			return FireUlt ? 1.0f : 0.0f;
		}

		private float GetSlaughtering()
		{
			return slaughterAction.state ? 1.0f : 0.0f;
		}

		private float GetReloading()
		{
			return reloadAction.state ? 1.0f : 0.0f;
		}

		private float GetPausing()
		{
			return gameMenuAction.state ? 1.0f : 0.0f;
		}
		#endregion
	}
}
