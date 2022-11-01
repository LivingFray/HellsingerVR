// Decompiled with JetBrains decompiler
// Type: Valve.VR.SteamVR_Behaviour
// Assembly: SteamVR, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DF474E11-42EA-4738-BF41-6A2D38F0B79C
// Assembly location: S:\SteamLibrary\steamapps\common\GTFO\GTFO_Data\BrokenAssembly20012020\Managed\SteamVR.dll

using SteamVR_Standalone_IL2CPP.Standalone;
using SteamVR_Standalone_IL2CPP.Util;
using System;
using UnityEngine;
using UnityEngine.XR;

namespace Valve.VR
{
	public class SteamVR_Behaviour : MonoBehaviour
    {
        public SteamVR_Behaviour(IntPtr value)
: base(value) { }

        public static bool forcingInitialization = false;
        internal static bool isPlaying = false;
        private static bool initializing = false;
        protected static int lastFrameCount = -1;
        public bool initializeSteamVROnAwake = true;
        public bool doNotDestroy = true;
        private const string openVRDeviceName = "OpenVR";
        private static SteamVR_Behaviour _instance;

        public SteamVR_Render steamvr_render;
        private bool loadedOpenVRDeviceSuccess;

        public static SteamVR_Behaviour instance
        {
            get
            {
                if ((UnityEngine.Object)SteamVR_Behaviour._instance == (UnityEngine.Object)null)
                    SteamVR_Behaviour.Initialize(false);
                return SteamVR_Behaviour._instance;
            }
        }

        public static void Initialize(bool forceUnityVRToOpenVR = false)
        {
            if (!((UnityEngine.Object)SteamVR_Behaviour._instance == (UnityEngine.Object)null) || SteamVR_Behaviour.initializing)
                return;
            SteamVR_Behaviour.initializing = true;
            GameObject gameObject1 = (GameObject)null;
            if (forceUnityVRToOpenVR)
                SteamVR_Behaviour.forcingInitialization = true;
            SteamVR_Render objectOfType1 = UnityEngine.Object.FindObjectOfType<SteamVR_Render>();
            if ((UnityEngine.Object)objectOfType1 != (UnityEngine.Object)null)
                gameObject1 = objectOfType1.gameObject;
            SteamVR_Behaviour objectOfType2 = UnityEngine.Object.FindObjectOfType<SteamVR_Behaviour>();
            if ((UnityEngine.Object)objectOfType2 != (UnityEngine.Object)null)
                gameObject1 = objectOfType2.gameObject;
            if ((UnityEngine.Object)gameObject1 == (UnityEngine.Object)null)
            {
                SteamVR.Log.LogInfo("Creating SteamVR Object");
                GameObject gameObject2 = new GameObject("[SteamVR]");
				SteamVR.Log.LogInfo("Adding Behaviour component");
				SteamVR_Behaviour._instance = gameObject2.AddComponent<SteamVR_Behaviour>();
				SteamVR.Log.LogInfo("Adding Render component");
				SteamVR_Behaviour._instance.steamvr_render = gameObject2.AddComponent<SteamVR_Render>();
				SteamVR.Log.LogInfo("Adding Melon Coroutine component");
				gameObject2.AddComponent<MelonCoroutineCallbacks>();
                DontDestroyOnLoad(gameObject2);
            }
            else
            {
                SteamVR_Behaviour steamVrBehaviour = gameObject1.GetComponent<SteamVR_Behaviour>();
                if ((UnityEngine.Object)steamVrBehaviour == (UnityEngine.Object)null)
                    steamVrBehaviour = gameObject1.AddComponent<SteamVR_Behaviour>();
                if ((UnityEngine.Object)objectOfType1 != (UnityEngine.Object)null)
                {
                    steamVrBehaviour.steamvr_render = objectOfType1;
                }
                else
                {
                    steamVrBehaviour.steamvr_render = gameObject1.GetComponent<SteamVR_Render>();
                    if ((UnityEngine.Object)steamVrBehaviour.steamvr_render == (UnityEngine.Object)null)
                        steamVrBehaviour.steamvr_render = gameObject1.AddComponent<SteamVR_Render>();
                }
                SteamVR_Behaviour._instance = steamVrBehaviour;
            }
            if ((UnityEngine.Object)SteamVR_Behaviour._instance != (UnityEngine.Object)null && SteamVR_Behaviour._instance.doNotDestroy)
                UnityEngine.Object.DontDestroyOnLoad((UnityEngine.Object)SteamVR_Behaviour._instance.transform.root.gameObject);
            SteamVR_Behaviour.initializing = false;
        }

        protected void Awake()
        {
            SteamVR_Behaviour.isPlaying = true;
            if (!this.initializeSteamVROnAwake || SteamVR_Behaviour.forcingInitialization)
                return;
            this.InitializeSteamVR(false);
        }

        public void InitializeSteamVR(bool forceUnityVRToOpenVR = false)
        {
            if (forceUnityVRToOpenVR)
            {
                throw new System.Exception("This should not be used");
            }
            else
                SteamVR.Initialize(false);
        }

        private void XRDevice_deviceLoaded(string deviceName)
        {
            if (deviceName == "OpenVR")
            {
                this.loadedOpenVRDeviceSuccess = true;
            }
            else
            {
                Debug.LogError(("<b>[SteamVR]</b> Tried to async load: OpenVR. Loaded: " + deviceName), (UnityEngine.Object)this);
                this.loadedOpenVRDeviceSuccess = true;
            }
        }

        private void EnableOpenVR()
        {
            XRSettings.enabled = true;
            SteamVR.Initialize(false);
            SteamVR_Behaviour.forcingInitialization = false;
        }

        protected void OnEnable()
        {
            UnityHooks.OnBeforeRender += this.OnBeforeRender;
            SteamVR_Events.System(EVREventType.VREvent_Quit).Listen((this.OnQuit));
        }

        protected void OnDisable()
        {
            UnityHooks.OnBeforeRender -= this.OnBeforeRender;
            SteamVR_Events.System(EVREventType.VREvent_Quit).Remove((this.OnQuit));
        }

        protected void OnBeforeRender()
        {
            this.PreCull();
        }

        protected void PreCull()
        {
            if (Time.frameCount == SteamVR_Behaviour.lastFrameCount)
                return;
            SteamVR_Behaviour.lastFrameCount = Time.frameCount;
            SteamVR_Input.OnPreCull();
        }

        protected void FixedUpdate()
        {
            SteamVR_Input.FixedUpdate();
        }

        protected void LateUpdate()
        {
            SteamVR_Input.LateUpdate();
        }

        protected void Update()
        {
            SteamVR_Input.Update();
        }

        protected void OnQuit(VREvent_t vrEvent)
        {
            Application.Quit();
        }
    }
}
