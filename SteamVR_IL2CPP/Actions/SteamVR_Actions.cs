namespace Valve.VR
{
	public class SteamVR_Actions
	{
		// Movement
		private static SteamVR_Action_Vector2 p_default_Movement;
		private static SteamVR_Action_Vector2 p_default_Look;
		private static SteamVR_Action_Boolean p_default_Dash;
		private static SteamVR_Action_Boolean p_default_Jump;

		// Attack
		private static SteamVR_Action_Boolean p_default_Shoot;
		private static SteamVR_Action_Boolean p_default_ShootAlt;
		private static SteamVR_Action_Boolean p_default_Slaughter;
		private static SteamVR_Action_Boolean p_default_Reload;

		// Weapons
		private static SteamVR_Action_Boolean p_default_WeaponSwitchLeft;
		private static SteamVR_Action_Boolean p_default_WeaponSwitchRight;
		private static SteamVR_Action_Boolean p_default_WeaponSwitchPaz;

		// Misc
		private static SteamVR_Action_Boolean p_default_OpenMenu;
		private static SteamVR_Action_Pose p_default_Pose;
		private static SteamVR_Action_Pose p_default_PoseTip;


		public static SteamVR_Action_Vector2 default_Movement
		{
			get
			{
				return SteamVR_Actions.p_default_Movement.GetCopy<SteamVR_Action_Vector2>();
			}
		}

		public static SteamVR_Action_Vector2 default_Look
		{
			get
			{
				return SteamVR_Actions.p_default_Look.GetCopy<SteamVR_Action_Vector2>();
			}
		}

		public static SteamVR_Action_Boolean default_Dash
		{
			get
			{
				return SteamVR_Actions.p_default_Dash.GetCopy<SteamVR_Action_Boolean>();
			}
		}

		public static SteamVR_Action_Boolean default_Jump
		{
			get
			{
				return SteamVR_Actions.p_default_Jump.GetCopy<SteamVR_Action_Boolean>();
			}
		}

		public static SteamVR_Action_Boolean default_Shoot
		{
			get
			{
				return SteamVR_Actions.p_default_Shoot.GetCopy<SteamVR_Action_Boolean>();
			}
		}

		public static SteamVR_Action_Boolean default_ShootAlt
		{
			get
			{
				return SteamVR_Actions.p_default_ShootAlt.GetCopy<SteamVR_Action_Boolean>();
			}
		}

		public static SteamVR_Action_Boolean default_Slaughter
		{
			get
			{
				return SteamVR_Actions.p_default_Slaughter.GetCopy<SteamVR_Action_Boolean>();
			}
		}

		public static SteamVR_Action_Boolean default_Reload
		{
			get
			{
				return SteamVR_Actions.p_default_Reload.GetCopy<SteamVR_Action_Boolean>();
			}
		}

		public static SteamVR_Action_Boolean default_WeaponSwitchLeft
		{
			get
			{
				return SteamVR_Actions.p_default_WeaponSwitchLeft.GetCopy<SteamVR_Action_Boolean>();
			}
		}

		public static SteamVR_Action_Boolean default_WeaponSwitchRight
		{
			get
			{
				return SteamVR_Actions.p_default_WeaponSwitchRight.GetCopy<SteamVR_Action_Boolean>();
			}
		}

		public static SteamVR_Action_Boolean default_WeaponSwitchPaz
		{
			get
			{
				return SteamVR_Actions.p_default_WeaponSwitchPaz.GetCopy<SteamVR_Action_Boolean>();
			}
		}

		public static SteamVR_Action_Boolean default_OpenMenu
		{
			get
			{
				return SteamVR_Actions.p_default_OpenMenu.GetCopy<SteamVR_Action_Boolean>();
			}
		}

		public static SteamVR_Action_Pose default_Pose
		{
			get
			{
				return SteamVR_Actions.p_default_Pose.GetCopy<SteamVR_Action_Pose>();
			}
		}

		public static SteamVR_Action_Pose default_PoseTip
		{
			get
			{
				return SteamVR_Actions.p_default_PoseTip.GetCopy<SteamVR_Action_Pose>();
			}
		}

		private static void InitializeActionArrays()
		{
			Valve.VR.SteamVR_Input.actions = new Valve.VR.SteamVR_Action[] {
					default_Movement,
					default_Look,
					default_Dash,
					default_Jump,
					default_Shoot,
					default_ShootAlt,
					default_Slaughter,
					default_Reload,
					default_WeaponSwitchLeft,
					default_WeaponSwitchRight,
					default_WeaponSwitchPaz,
					default_OpenMenu,
					default_Pose,
					default_PoseTip
			};
			Valve.VR.SteamVR_Input.actionsIn = new Valve.VR.ISteamVR_Action_In[] {
					default_Movement,
					default_Look,
					default_Dash,
					default_Jump,
					default_Shoot,
					default_ShootAlt,
					default_Slaughter,
					default_Reload,
					default_WeaponSwitchLeft,
					default_WeaponSwitchRight,
					default_WeaponSwitchPaz,
					default_OpenMenu,
					default_Pose,
					default_PoseTip
			};
			Valve.VR.SteamVR_Input.actionsOut = new Valve.VR.ISteamVR_Action_Out[0];
			Valve.VR.SteamVR_Input.actionsVibration = new Valve.VR.SteamVR_Action_Vibration[0];
			Valve.VR.SteamVR_Input.actionsPose = new Valve.VR.SteamVR_Action_Pose[] {
					default_Pose,
					default_PoseTip
			};
			Valve.VR.SteamVR_Input.actionsBoolean = new Valve.VR.SteamVR_Action_Boolean[] {
					default_Dash,
					default_Jump,
					default_Shoot,
					default_ShootAlt,
					default_Slaughter,
					default_Reload,
					default_WeaponSwitchLeft,
					default_WeaponSwitchRight,
					default_WeaponSwitchPaz,
					default_OpenMenu
			};
			Valve.VR.SteamVR_Input.actionsSingle = new Valve.VR.SteamVR_Action_Single[0];
			Valve.VR.SteamVR_Input.actionsVector2 = new Valve.VR.SteamVR_Action_Vector2[] {
					default_Movement,
                    default_Look
			};
			Valve.VR.SteamVR_Input.actionsVector3 = new Valve.VR.SteamVR_Action_Vector3[0];
			Valve.VR.SteamVR_Input.actionsSkeleton = new Valve.VR.SteamVR_Action_Skeleton[0];
			Valve.VR.SteamVR_Input.actionsNonPoseNonSkeletonIn = new Valve.VR.ISteamVR_Action_In[] {
					default_Movement,
					default_Look,
					default_Dash,
					default_Jump,
					default_Shoot,
					default_ShootAlt,
					default_Slaughter,
					default_Reload,
					default_WeaponSwitchLeft,
					default_WeaponSwitchRight,
					default_WeaponSwitchPaz,
					default_OpenMenu
			};
		}

		private static void PreInitActions()
		{
			p_default_Movement = SteamVR_Action.Create<SteamVR_Action_Vector2>("/actions/default/in/Movement");
			p_default_Look = SteamVR_Action.Create<SteamVR_Action_Vector2>("/actions/default/in/Look");
			p_default_Dash = SteamVR_Action.Create<SteamVR_Action_Boolean>("/actions/default/in/Dash");
			p_default_Jump = SteamVR_Action.Create<SteamVR_Action_Boolean>("/actions/default/in/Jump");
			p_default_Shoot = SteamVR_Action.Create<SteamVR_Action_Boolean>("/actions/default/in/Shoot");
			p_default_ShootAlt = SteamVR_Action.Create<SteamVR_Action_Boolean>("/actions/default/in/ShootAlt");
			p_default_Slaughter = SteamVR_Action.Create<SteamVR_Action_Boolean>("/actions/default/in/Slaughter");
			p_default_Reload = SteamVR_Action.Create<SteamVR_Action_Boolean>("/actions/default/in/Reload");
			p_default_WeaponSwitchLeft = SteamVR_Action.Create<SteamVR_Action_Boolean>("/actions/default/in/WeaponSwitchLeft");
			p_default_WeaponSwitchRight = SteamVR_Action.Create<SteamVR_Action_Boolean>("/actions/default/in/WeaponSwitchRight");
			p_default_WeaponSwitchPaz = SteamVR_Action.Create<SteamVR_Action_Boolean>("/actions/default/in/WeaponSwitchPaz");
			p_default_OpenMenu = SteamVR_Action.Create<SteamVR_Action_Boolean>("/actions/default/in/OpenMenu");
			p_default_Pose = SteamVR_Action.Create<SteamVR_Action_Pose>("/actions/default/in/Pose");
			p_default_PoseTip = SteamVR_Action.Create<SteamVR_Action_Pose>("/actions/default/in/PoseTip");
		}

		public static SteamVR_Input_ActionSet_default _default
		{
			get
			{
				return p__default.GetCopy<SteamVR_Input_ActionSet_default>();
			}
		}

		private static void StartPreInitActionSets()
		{
			p__default = SteamVR_ActionSet.Create<SteamVR_Input_ActionSet_default>("/actions/default");
			SteamVR_Input.actionSets = new SteamVR_ActionSet[]
			{
				_default
			};
		}

		public static void PreInitialize()
		{
			StartPreInitActionSets();
			SteamVR_Input.PreinitializeActionSetDictionaries();
			PreInitActions();
			InitializeActionArrays();
			SteamVR_Input.PreinitializeActionDictionaries();
			SteamVR_Input.PreinitializeFinishActionSets();
		}

		private static SteamVR_Input_ActionSet_default p__default;
	}
}
