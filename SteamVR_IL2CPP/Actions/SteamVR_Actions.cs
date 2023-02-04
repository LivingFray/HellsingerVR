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
		private static SteamVR_Action_Vibration p_default_Vibration;

		// Menu
		private static SteamVR_Action_Vector2 p_menu_Navigate;
		private static SteamVR_Action_Boolean p_menu_Select;
		private static SteamVR_Action_Boolean p_menu_Back;
		private static SteamVR_Action_Boolean p_menu_PrevTab;
		private static SteamVR_Action_Boolean p_menu_NextTab;
		private static SteamVR_Action_Boolean p_menu_CloseMenu;



		public static SteamVR_Action_Vector2 default_Movement
		{
			get
			{
				return p_default_Movement.GetCopy<SteamVR_Action_Vector2>();
			}
		}

		public static SteamVR_Action_Vector2 default_Look
		{
			get
			{
				return p_default_Look.GetCopy<SteamVR_Action_Vector2>();
			}
		}

		public static SteamVR_Action_Boolean default_Dash
		{
			get
			{
				return p_default_Dash.GetCopy<SteamVR_Action_Boolean>();
			}
		}

		public static SteamVR_Action_Boolean default_Jump
		{
			get
			{
				return p_default_Jump.GetCopy<SteamVR_Action_Boolean>();
			}
		}

		public static SteamVR_Action_Boolean default_Shoot
		{
			get
			{
				return p_default_Shoot.GetCopy<SteamVR_Action_Boolean>();
			}
		}

		public static SteamVR_Action_Boolean default_ShootAlt
		{
			get
			{
				return p_default_ShootAlt.GetCopy<SteamVR_Action_Boolean>();
			}
		}

		public static SteamVR_Action_Boolean default_Slaughter
		{
			get
			{
				return p_default_Slaughter.GetCopy<SteamVR_Action_Boolean>();
			}
		}

		public static SteamVR_Action_Boolean default_Reload
		{
			get
			{
				return p_default_Reload.GetCopy<SteamVR_Action_Boolean>();
			}
		}

		public static SteamVR_Action_Boolean default_WeaponSwitchLeft
		{
			get
			{
				return p_default_WeaponSwitchLeft.GetCopy<SteamVR_Action_Boolean>();
			}
		}

		public static SteamVR_Action_Boolean default_WeaponSwitchRight
		{
			get
			{
				return p_default_WeaponSwitchRight.GetCopy<SteamVR_Action_Boolean>();
			}
		}

		public static SteamVR_Action_Boolean default_WeaponSwitchPaz
		{
			get
			{
				return p_default_WeaponSwitchPaz.GetCopy<SteamVR_Action_Boolean>();
			}
		}

		public static SteamVR_Action_Boolean default_OpenMenu
		{
			get
			{
				return p_default_OpenMenu.GetCopy<SteamVR_Action_Boolean>();
			}
		}

		public static SteamVR_Action_Pose default_Pose
		{
			get
			{
				return p_default_Pose.GetCopy<SteamVR_Action_Pose>();
			}
		}

		public static SteamVR_Action_Pose default_PoseTip
		{
			get
			{
				return p_default_PoseTip.GetCopy<SteamVR_Action_Pose>();
			}
		}

		public static SteamVR_Action_Vibration default_Vibration
		{
			get
			{
				return p_default_Vibration.GetCopy<SteamVR_Action_Vibration>();
			}
		}

		public static SteamVR_Action_Vector2 menu_Navigate
		{
			get
			{
				return p_menu_Navigate.GetCopy<SteamVR_Action_Vector2>();
			}
		}

		public static SteamVR_Action_Boolean menu_Select
		{
			get
			{
				return p_menu_Select.GetCopy<SteamVR_Action_Boolean>();
			}
		}
		public static SteamVR_Action_Boolean menu_Back
		{
			get
			{
				return p_menu_Back.GetCopy<SteamVR_Action_Boolean>();
			}
		}
		public static SteamVR_Action_Boolean menu_PrevTab
		{
			get
			{
				return p_menu_PrevTab.GetCopy<SteamVR_Action_Boolean>();
			}
		}
		public static SteamVR_Action_Boolean menu_NextTab
		{
			get
			{
				return p_menu_NextTab.GetCopy<SteamVR_Action_Boolean>();
			}
		}
		public static SteamVR_Action_Boolean menu_CloseMenu
		{
			get
			{
				return p_menu_CloseMenu.GetCopy<SteamVR_Action_Boolean>();
			}
		}

		private static void InitializeActionArrays()
		{
			SteamVR_Input.actions = new SteamVR_Action[] {
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
					default_PoseTip,
					default_Vibration,
					menu_Navigate,
					menu_Select,
					menu_Back,
					menu_PrevTab,
					menu_NextTab,
					menu_CloseMenu
			};
			SteamVR_Input.actionsIn = new ISteamVR_Action_In[] {
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
					default_PoseTip,
					menu_Navigate,
					menu_Select,
					menu_Back,
					menu_PrevTab,
					menu_NextTab,
					menu_CloseMenu
			};
			SteamVR_Input.actionsOut = new ISteamVR_Action_Out[] {
				default_Vibration
			};
			SteamVR_Input.actionsVibration = new SteamVR_Action_Vibration[] {
					default_Vibration
			};
			SteamVR_Input.actionsPose = new SteamVR_Action_Pose[] {
					default_Pose,
					default_PoseTip
			};
			SteamVR_Input.actionsBoolean = new SteamVR_Action_Boolean[] {
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
					menu_Select,
					menu_Back,
					menu_PrevTab,
					menu_NextTab,
					menu_CloseMenu
			};
			SteamVR_Input.actionsSingle = new SteamVR_Action_Single[0];
			SteamVR_Input.actionsVector2 = new SteamVR_Action_Vector2[] {
					default_Movement,
					default_Look,
					menu_Navigate
			};
			SteamVR_Input.actionsVector3 = new SteamVR_Action_Vector3[0];
			SteamVR_Input.actionsSkeleton = new SteamVR_Action_Skeleton[0];
			SteamVR_Input.actionsNonPoseNonSkeletonIn = new ISteamVR_Action_In[] {
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
					menu_Navigate,
					menu_Select,
					menu_Back,
					menu_PrevTab,
					menu_NextTab,
					menu_CloseMenu
			};
		}

		private static void PreInitActions()
		{
			p_default_Movement = SteamVR_Action.Create<SteamVR_Action_Vector2>("/actions/game/in/Movement");
			p_default_Look = SteamVR_Action.Create<SteamVR_Action_Vector2>("/actions/game/in/Look");
			p_default_Dash = SteamVR_Action.Create<SteamVR_Action_Boolean>("/actions/game/in/Dash");
			p_default_Jump = SteamVR_Action.Create<SteamVR_Action_Boolean>("/actions/game/in/Jump");
			p_default_Shoot = SteamVR_Action.Create<SteamVR_Action_Boolean>("/actions/game/in/Shoot");
			p_default_ShootAlt = SteamVR_Action.Create<SteamVR_Action_Boolean>("/actions/game/in/ShootAlt");
			p_default_Slaughter = SteamVR_Action.Create<SteamVR_Action_Boolean>("/actions/game/in/Slaughter");
			p_default_Reload = SteamVR_Action.Create<SteamVR_Action_Boolean>("/actions/game/in/Reload");
			p_default_WeaponSwitchLeft = SteamVR_Action.Create<SteamVR_Action_Boolean>("/actions/game/in/WeaponSwitchLeft");
			p_default_WeaponSwitchRight = SteamVR_Action.Create<SteamVR_Action_Boolean>("/actions/game/in/WeaponSwitchRight");
			p_default_WeaponSwitchPaz = SteamVR_Action.Create<SteamVR_Action_Boolean>("/actions/game/in/WeaponSwitchPaz");
			p_default_OpenMenu = SteamVR_Action.Create<SteamVR_Action_Boolean>("/actions/game/in/OpenMenu");
			p_default_Pose = SteamVR_Action.Create<SteamVR_Action_Pose>("/actions/game/in/Pose");
			p_default_PoseTip = SteamVR_Action.Create<SteamVR_Action_Pose>("/actions/game/in/PoseTip");
			p_default_Vibration = SteamVR_Action.Create<SteamVR_Action_Vibration>("/actions/game/out/Vibration");

			p_menu_Navigate = SteamVR_Action.Create<SteamVR_Action_Vector2>("/actions/menu/in/Navigate");
			p_menu_Select = SteamVR_Action.Create<SteamVR_Action_Boolean>("/actions/menu/in/Select");
			p_menu_Back = SteamVR_Action.Create<SteamVR_Action_Boolean>("/actions/menu/in/Back");
			p_menu_PrevTab = SteamVR_Action.Create<SteamVR_Action_Boolean>("/actions/menu/in/PrevTab");
			p_menu_NextTab = SteamVR_Action.Create<SteamVR_Action_Boolean>("/actions/menu/in/NextTab");
			p_menu_CloseMenu = SteamVR_Action.Create<SteamVR_Action_Boolean>("/actions/menu/in/CloseMenu");
		}

		public static SteamVR_Input_ActionSet_default _default
		{
			get
			{
				return p__default.GetCopy<SteamVR_Input_ActionSet_default>();
			}
		}

		public static SteamVR_Input_ActionSet_menu _menu
		{
			get
			{
				return p__menu.GetCopy<SteamVR_Input_ActionSet_menu>();
			}
		}

		private static void StartPreInitActionSets()
		{
			p__default = SteamVR_ActionSet.Create<SteamVR_Input_ActionSet_default>("/actions/game");
			p__menu = SteamVR_ActionSet.Create<SteamVR_Input_ActionSet_menu>("/actions/menu");
			SteamVR_Input.actionSets = new SteamVR_ActionSet[]
			{
				_default,
				_menu
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
		private static SteamVR_Input_ActionSet_menu p__menu;
	}
}
