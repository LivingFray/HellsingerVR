//======= Copyright (c) Valve Corporation, All rights reserved. ===============

using SteamVR_IL2CPP.Util;
using System;
using UnityEngine;

namespace Valve.VR
{
	[Serializable]
	public class SteamVR_Behaviour_Vector3Event : CustomUnityEvent<SteamVR_Behaviour_Vector3, SteamVR_Input_Sources, Vector3, Vector3> { }
}