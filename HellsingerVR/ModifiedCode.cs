using Il2CppInterop.Runtime;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Unity.Collections;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem;

namespace HellsingerVR
{
	public class ModifiedCode
	{
		// "Slightly" confusing magic to call StateEvent.From() in cpp domain
		public unsafe static NativeArray<byte> StateEvent_From(InputDevice device, [Out] InputEventPtr eventPtr, out InputEventPtr outEventPtr, Allocator allocator = Allocator.Temp)
		{
			IntPtr* ptr = stackalloc IntPtr[3];
			*ptr = IL2CPP.Il2CppObjectBaseToPtr(device);
			*(InputEventPtr**)((byte*)ptr + checked(1u * unchecked((nuint)sizeof(IntPtr)))) = &eventPtr;
			*(Allocator**)((byte*)ptr + checked(2u * unchecked((nuint)sizeof(IntPtr)))) = &allocator;

			Type type = typeof(StateEvent);
			System.Reflection.FieldInfo info = type.GetField("NativeMethodInfoPtr_From_Public_Static_NativeArray_1_Byte_InputDevice_InputEventPtr_Allocator_0", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
			IntPtr value = (IntPtr)info.GetValue(null);

			IntPtr exc = IntPtr.Zero;
			IntPtr pointer = IL2CPP.il2cpp_runtime_invoke(value, (IntPtr)0, (void**)ptr, ref exc);
			Il2CppException.RaiseExceptionIfNecessary(exc);

			outEventPtr = eventPtr;

			return new NativeArray<byte>(pointer);
		}
	}
}
