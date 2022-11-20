namespace Valve.VR
{
	public class SteamVR_Input_ActionSet_default : SteamVR_ActionSet
    {
		public virtual SteamVR_Action_Vector2 Movement { get { return SteamVR_Actions.default_Movement; } }
		public virtual SteamVR_Action_Vector2 Look { get { return SteamVR_Actions.default_Look; } }
		public virtual SteamVR_Action_Boolean Dash { get { return SteamVR_Actions.default_Dash; } }
		public virtual SteamVR_Action_Boolean Jump { get { return SteamVR_Actions.default_Jump; } }
		public virtual SteamVR_Action_Boolean Shoot { get { return SteamVR_Actions.default_Shoot; } }
		public virtual SteamVR_Action_Boolean ShootAlt { get { return SteamVR_Actions.default_ShootAlt; } }
		public virtual SteamVR_Action_Boolean Slaughter { get { return SteamVR_Actions.default_Slaughter; } }
		public virtual SteamVR_Action_Boolean Reload { get { return SteamVR_Actions.default_Reload; } }
		public virtual SteamVR_Action_Boolean WeaponSwitchLeft { get { return SteamVR_Actions.default_WeaponSwitchLeft; } }
		public virtual SteamVR_Action_Boolean WeaponSwitchRight { get { return SteamVR_Actions.default_WeaponSwitchRight; } }
		public virtual SteamVR_Action_Boolean WeaponSwitchPaz { get { return SteamVR_Actions.default_WeaponSwitchPaz; } }
		public virtual SteamVR_Action_Boolean OpenMenu { get { return SteamVR_Actions.default_OpenMenu; } }
		public virtual SteamVR_Action_Pose Pose { get { return SteamVR_Actions.default_Pose; } }
		public virtual SteamVR_Action_Pose PoseTip { get { return SteamVR_Actions.default_PoseTip; } }
	}
}
