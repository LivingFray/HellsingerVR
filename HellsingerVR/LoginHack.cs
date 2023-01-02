using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using Valve.VR;

namespace HellsingerVR
{
	// This hack will only work on windows (thankfully the game hasn't been released on linux...)
	// I tried to do this properly but I can't for the life of me find where they actually check inputs for this screen
	public class LoginHack
	{
		private static bool WasKeyPressed = false;
		private static bool HasFocused = false;
		private static bool IsPrePreLogin = true;
		private static uint KEYDOWN = 0x0100;
		private static uint KEYUP = 0x0101;
		private static int VK_ESC = 0x1B;


		[DllImport("user32.dll")]
		private static extern bool PostMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);

		[DllImport("user32.dll")]
		private static extern IntPtr GetActiveWindow();

		[DllImport("user32.dll")]
		private static extern bool SetForegroundWindow(IntPtr hWnd);

		private static TitleScreenAnimation titleScreenAnimation;

		public static void PressAnyKey()
		{
			if (titleScreenAnimation == null)
			{
				titleScreenAnimation = UnityEngine.Object.FindObjectOfType<TitleScreenAnimation>();
			}

			if (titleScreenAnimation)
			{
				HellsingerVR.IsPreLogin = titleScreenAnimation.IsOnPressAnyKeyScreen();
				if (HellsingerVR.IsPreLogin)
				{
					IsPrePreLogin = false;
				}
			}

			if (IsPrePreLogin || HellsingerVR.IsPreLogin)
			{
				Keyboard keyboard = InputSystem.GetDevice<Keyboard>();
				if (keyboard == null)
				{
					HellsingerVR._instance.Log.LogError("No keyboard");
					return;
				}

				if (!HasFocused)
				{
					HasFocused = true;
					SetForegroundWindow(GetActiveWindow());
					HellsingerVR._instance.Log.LogInfo("Bringing Unity into foreground");
				}

				InputEventPtr keyboardPtr;

				ModifiedCode.StateEvent_From(keyboard, new InputEventPtr(), out keyboardPtr);

				SteamVR_Action_Boolean select = SteamVR_Input.GetBooleanAction("menu", "Select", true);

				bool Pressed = select.state;

				if (Pressed != WasKeyPressed)
				{
					Process thisProcess = Process.GetCurrentProcess();
					PostMessage(thisProcess.MainWindowHandle, Pressed ? KEYDOWN : KEYUP, VK_ESC, 0);
					WasKeyPressed = Pressed;
					HellsingerVR._instance.Log.LogInfo("Faking keypress for login");
				}

				keyboard.spaceKey.WriteValueIntoEvent(Pressed ? 1.0f : 0.0f, keyboardPtr);
				InputSystem.QueueEvent(keyboardPtr);
				return;
			}

			// Reset state
			if (WasKeyPressed)
			{
				Process thisProcess = Process.GetCurrentProcess();
				PostMessage(thisProcess.MainWindowHandle, KEYUP, VK_ESC, 0);
				WasKeyPressed = false;
				HellsingerVR._instance.Log.LogInfo("Releasing held fake keypress for login");
			}
		}
	}
}
