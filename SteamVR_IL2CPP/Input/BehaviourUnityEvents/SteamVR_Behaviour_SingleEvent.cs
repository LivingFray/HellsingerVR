//======= Copyright (c) Valve Corporation, All rights reserved. ===============

using SteamVR_IL2CPP.Util;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppInterop.Runtime.Runtime;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Valve.VR
{
    [Serializable]
    public class SteamVR_Behaviour_SingleEvent : CustomUnityEvent<SteamVR_Behaviour_Single, SteamVR_Input_Sources, float, float>
    {

    }
}