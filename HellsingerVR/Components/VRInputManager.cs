using Il2CppInterop.Runtime;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Unity.Collections;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem;
using Valve.VR;
using System.Diagnostics;
using Il2CppMicrosoft.Win32;
using Outsiders.GUI;

namespace HellsingerVR.Components
{
    public class VRInputManager : MonoBehaviour
    {

        public static bool LastHandToShootWasLeft = false;

        float TimeOffset = 0.0f;

        Gamepad VRInput;

        FirstPersonController fpController;

		bool WasPrevWeaponPressed = false;
        bool WasNextWeaponPressed = false;

        public static (Vector3, Quaternion) GetHandTransform(bool LeftHand = false)
        {
			Vector3 location = SteamVR_Input.GetAction<SteamVR_Action_Pose>("Pose").GetLocalPosition(LeftHand ? SteamVR_Input_Sources.LeftHand : SteamVR_Input_Sources.RightHand);
			Quaternion rotation = SteamVR_Input.GetAction<SteamVR_Action_Pose>("Pose").GetLocalRotation(LeftHand ? SteamVR_Input_Sources.LeftHand : SteamVR_Input_Sources.RightHand);

			location = HellsingerVR.rig.transform.TransformPoint(location);

			rotation = HellsingerVR.rig.transform.rotation * rotation * HellsingerVR.HandOffset;

            return (location, rotation);
		}

        static bool IsInGame()
        {
            return !HellsingerVR.IsPaused && !HellsingerVR.IsLoading && HellsingerVR.rig && HellsingerVR.rig.InLevel;
        }

        public void Awake()
        {
            TimeOffset = Time.realtimeSinceStartup;
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
            }

            InputEventPtr ptr;

            ModifiedCode.StateEvent_From(VRInput, new InputEventPtr(), out ptr);



            // Do various input mapping here (preferably in an intelligent way)

            // Currently assumes default controller binding!
            try
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

                // Weapon switching here...

                float rightShoulder = 0.0f;
                float dpadUp = 0.0f;
                float dpadDown = 0.0f;
                float dpadLeft = 0.0f;
                float dpadRight = 0.0f;

                if (IsInGame())
                {
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
                                bool WeaponSwitchLeft = SteamVR_Input.GetBooleanAction("WeaponSwitchLeft").state;
                                bool WeaponSwitchRight = SteamVR_Input.GetBooleanAction("WeaponSwitchRight").state;

                                bool SwitchToPrev = !WasPrevWeaponPressed && WeaponSwitchLeft;
                                bool SwitchToNext = !WasNextWeaponPressed && WeaponSwitchRight;
                                bool SwitchToPaz = SteamVR_Input.GetBooleanAction("WeaponSwitchPaz").state;

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

                                        dpadUp = NewIndex == 0 ? 1.0f : 0.0f;
                                        dpadDown = NewIndex == 1 ? 1.0f : 0.0f;
                                        dpadLeft = NewIndex == 2 ? 1.0f : 0.0f;
                                        dpadRight = NewIndex == 3 ? 1.0f : 0.0f;
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
            catch (Exception e)
            {
                HellsingerVR._instance.Log.LogError(e.Message);
            }
            InputSystem.QueueEvent(ptr);

            LoginHack.PressAnyKey();
        }

        #region Input Converter functions
        // Convert movement taking into account hmd/hand based movement config
        Vector2 GetMovementVector()
        {
            SteamVR_Action_Vector2 input = SteamVR_Input.GetVector2Action("Movement");
            // TODO: Implement options
            bool bUseHMD = true;

            // Don't do inputs in menus, n.b. need to account for pause
            if (!IsInGame())
            {
                //return Vector2.zero;
                // Temp? Either way should replace it really once proper UI interaction is in
                return input.GetAxis(SteamVR_Input_Sources.Any);
            }

            // TODO: Actually account for hand, also need to know which hand actually has the stick bound (activeDevice?)
            float rotation = (bUseHMD ?
                HellsingerVR.rig.head.rotation :
                input.activeDevice == SteamVR_Input_Sources.LeftHand ?
                    HellsingerVR.rig.leftHand.rotation :
                    HellsingerVR.rig.rightHand.rotation
                ).eulerAngles.y;

            if (HellsingerVR.rig.PlayerTransform)
            {
                rotation -= HellsingerVR.rig.PlayerTransform.rotation.eulerAngles.y;
            }

            return Quaternion.Euler(0, 0, -rotation) * input.GetAxis(SteamVR_Input_Sources.Any);
        }

        float GetLookValue()
        {

            if (!IsInGame())
            {
                return 0.0f;
            }

            return SteamVR_Input.GetVector2Action("Look").axis.x;
        }

        float GetDashing()
        {
            if (!IsInGame())
            {
                return 0.0f;
            }

            return SteamVR_Input.GetBooleanAction("Dash").state ? 1.0f : 0.0f;
        }

        float GetJumping()
        {
            if (!IsInGame())
            {
                // Press a in menus
                return SteamVR_Input.GetBooleanAction("Shoot").state || SteamVR_Input.GetBooleanAction("ShootAlt").state ? 1.0f : 0.0f;
            }

            return SteamVR_Input.GetBooleanAction("Jump").state ? 1.0f : 0.0f;
        }

        float GetShooting()
        {
            if (!IsInGame())
            {
                return 0.0f;
            }

            // Need to check the active weapon here to handle dual wielding?

            SteamVR_Action_Boolean fireAction = SteamVR_Input.GetBooleanAction("Shoot");
            SteamVR_Action_Boolean altFireAction = SteamVR_Input.GetBooleanAction("ShootAlt");


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

            // Return true if ONE is active, need to replace this to account for one handed weapons
            return DidShoot ? 1.0f : 0.0f;
        }

        float GetUltimate()
        {
            if (!IsInGame())
            {
                return 0.0f;
            }

            bool Shooting = SteamVR_Input.GetBooleanAction("Shoot").state;
            bool AltShooting = SteamVR_Input.GetBooleanAction("ShootAlt").state;

            return Shooting && AltShooting ? 1.0f : 0.0f;
        }

        float GetSlaughtering()
        {
            if (!IsInGame())
            {
                return 0.0f;
            }

            return SteamVR_Input.GetBooleanAction("Slaughter").state ? 1.0f : 0.0f;
        }

        float GetReloading()
        {
            if (!IsInGame())
            {
                return 0.0f;
            }

            return SteamVR_Input.GetBooleanAction("Reload").state ? 1.0f : 0.0f;
        }

        float GetPausing()
        {
            // This one *Should* fire when paused, but not on the title
            if (!(HellsingerVR.rig && HellsingerVR.rig.InLevel && !HellsingerVR.IsLoading))
            {
                return 0.0f;
            }

            return SteamVR_Input.GetBooleanAction("OpenMenu").state ? 1.0f : 0.0f;
        }
        #endregion
    }
}
