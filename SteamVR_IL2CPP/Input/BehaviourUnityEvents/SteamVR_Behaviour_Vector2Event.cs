//======= Copyright (c) Valve Corporation, All rights reserved. ===============

using SteamVR_IL2CPP.Util;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace Valve.VR
{
    [Serializable]
    public class SteamVR_Behaviour_Vector2Event : CustomUnityEvent<SteamVR_Behaviour_Vector2, SteamVR_Input_Sources, Vector2, Vector2> { }
}