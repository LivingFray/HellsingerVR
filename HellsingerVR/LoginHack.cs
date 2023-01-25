using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using Valve.VR;

namespace HellsingerVR
{
	public class LoginHack
	{
		[DllImport("user32.dll")]
		private static extern bool PostMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);

		[DllImport("user32.dll")]
		private static extern IntPtr GetActiveWindow();

		[DllImport("user32.dll")]
		private static extern bool SetForegroundWindow(IntPtr hWnd);
		[DllImport("user32.dll")]
		private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

		public static void FocusGame()
		{
			SetForegroundWindow(GetActiveWindow());
			ShowWindow(GetActiveWindow(), 3);
			HellsingerVR._instance.Log.LogInfo("Bringing Unity into foreground");
		}

		
	}
}
