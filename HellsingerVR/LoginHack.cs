using System;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem;
using Valve.VR;
using System.Diagnostics;
using System.Runtime.InteropServices;
using UnityEngine;

namespace HellsingerVR
{
	// This hack will only work on windows (thankfully the game hasn't been released on linux...)
	// I tried to do this properly but I can't for the life of me find where they actually check inputs for this screen
	public class LoginHack
	{
		static bool WasKeyPressed = false;

		static bool HasFocused = false;

		static bool IsPrePreLogin = true;

		static uint KEYDOWN = 0x0100;
		static uint KEYUP = 0x0101;

		static int VK_ESC = 0x1B;


		[DllImport("user32.dll")]
		static extern bool PostMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);

		[DllImport("user32.dll")]
		static extern bool SetForegroundWindow(IntPtr hWnd);

		public static void PressAnyKey()
		{
			TitleScreenAnimation titleScreenAnimation = UnityEngine.Object.FindObjectOfType<TitleScreenAnimation>();
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
					Process thisProcess = Process.GetCurrentProcess();
					SetForegroundWindow(thisProcess.MainWindowHandle);
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
					UnityEngine.Debug.Log("Faking keypress for login");
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
				UnityEngine.Debug.Log("Releasing held fake keypress for login");
			}
		}
	}
}
