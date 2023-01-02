using SteamVR_Standalone_IL2CPP.Util;
using System;
using UnityEngine;

namespace SteamVR_Standalone_IL2CPP.Standalone
{
	internal class MelonCoroutineCallbacks : MonoBehaviour
	{

		public MelonCoroutineCallbacks(IntPtr value)
: base(value) { }

		private void Start()
		{
			DontDestroyOnLoad(this.gameObject);
		}

		private void Update()
		{
			MelonCoroutines.Process();

		}

		private static int lastFrame = 0;

		private void OnGUI()
		{
			int currFrame = Time.frameCount;
			if (lastFrame != currFrame)
			{
				MelonCoroutines.ProcessWaitForEndOfFrame();
				lastFrame = currFrame;
			}
		}

		private void FixedUpdate()
		{
			MelonCoroutines.ProcessWaitForFixedUpdate();
		}
	}
}
