namespace Valve.VR
{
	public class SteamVR_Input_ActionSet_menu : SteamVR_ActionSet
    {
		public virtual SteamVR_Action_Vector2 Navigate { get { return SteamVR_Actions.menu_Navigate; } }
		public virtual SteamVR_Action_Boolean Select { get { return SteamVR_Actions.menu_Select; } }
		public virtual SteamVR_Action_Boolean Back { get { return SteamVR_Actions.menu_Back; } }
		public virtual SteamVR_Action_Boolean PrevTab { get { return SteamVR_Actions.menu_PrevTab; } }
		public virtual SteamVR_Action_Boolean NextTab { get { return SteamVR_Actions.menu_NextTab; } }
		public virtual SteamVR_Action_Boolean CloseMenu { get { return SteamVR_Actions.menu_CloseMenu; } }
	}
}
