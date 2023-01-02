using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine;

namespace Assets.SteamVR_Standalone.Standalone
{
	public static class VRShaders
	{
		public enum VRShader
		{
			blit,
			blitFlip,
			overlay,
			occlusion,
			fade
		}

		private static AssetBundle assetBundle;
		private static Shader blit;
		private static Shader blitFlip;
		private static Shader overlay;
		private static Shader occlusion;
		private static Shader fade;

		public static Shader GetShader(VRShader shader)
		{
			if (blit == null)
			{
				TryLoadShaders();
			}

			switch (shader)
			{
				case VRShader.blit:
					return blit;
				case VRShader.blitFlip:
					return blitFlip;
				case VRShader.overlay:
					return overlay;
				case VRShader.occlusion:
					return occlusion;
				case VRShader.fade:
					return fade;
			}
			Debug.LogWarning("No valid shader found");
			return null;
		}

		public class AssetBundle : UnityEngine.Object
		{
			private delegate System.IntPtr LoadAsset_InternalDelegate(System.IntPtr @this, System.IntPtr name, System.IntPtr type);

			private static readonly LoadAsset_InternalDelegate LoadAsset_InternalDelegateField = IL2CPP.ResolveICall<LoadAsset_InternalDelegate>("UnityEngine.AssetBundle::LoadAsset_Internal");

			private delegate System.IntPtr GetAllAssetNamesDelegate(System.IntPtr @this);

			private static readonly GetAllAssetNamesDelegate GetAllAssetNamesDelegateField = IL2CPP.ResolveICall<GetAllAssetNamesDelegate>("UnityEngine.AssetBundle::GetAllAssetNames");

			public AssetBundle(System.IntPtr ptr) : base(ptr)
			{
			}

			public UnityEngine.Object LoadAsset(string name)
			{
				return LoadAsset(name, Il2CppSystem.Type.GetTypeFromHandle(RuntimeReflectionHelper.GetRuntimeTypeHandle<UnityEngine.Object>()));
			}

			public UnityEngine.Object LoadAsset(string name, Il2CppSystem.Type type)
			{
				if (name == null)
				{
					//throw new Il2CppSystem.NullReferenceException("The input asset name cannot be null.");
				}

				if (name.Length == 0)
				{
					//throw new Il2CppSystem.ArgumentException("The input asset name cannot be empty.");
				}

				if ((object)type == null)
				{
					//throw new Il2CppSystem.NullReferenceException("The input type cannot be null.");
				}

				return LoadAsset_Internal(name, type);
			}

			public UnityEngine.Object LoadAsset_Internal(string name, Il2CppSystem.Type type)
			{
				System.IntPtr intPtr = LoadAsset_InternalDelegateField(IL2CPP.Il2CppObjectBaseToPtrNotNull(this), IL2CPP.ManagedStringToIl2Cpp(name), IL2CPP.Il2CppObjectBaseToPtr(type));
				return (intPtr != (System.IntPtr)0) ? new UnityEngine.Object(intPtr) : null;
			}

			private delegate System.IntPtr LoadFromFile_InternalDelegate(System.IntPtr path, uint crc, ulong offset);
			private static readonly LoadFromFile_InternalDelegate LoadFromFile_InternalDelegateField = IL2CPP.ResolveICall<LoadFromFile_InternalDelegate>("UnityEngine.AssetBundle::LoadFromFile_Internal");

			public static AssetBundle LoadFromFile(string path)
			{
				return LoadFromFile_Internal(path, 0u, 0uL);
			}

			public static AssetBundle LoadFromFile_Internal(string path, uint crc, ulong offset)
			{
				System.IntPtr intPtr = LoadFromFile_InternalDelegateField(IL2CPP.ManagedStringToIl2Cpp(path), crc, offset);
				return (intPtr != (System.IntPtr)0) ? new AssetBundle(intPtr) : null;
			}

			public Il2CppStringArray GetAllAssetNames()
			{
				System.IntPtr intPtr = GetAllAssetNamesDelegateField(IL2CPP.Il2CppObjectBaseToPtrNotNull(this));
				return (intPtr != (System.IntPtr)0) ? new Il2CppStringArray(intPtr) : null;
			}
		}


		public static void TryLoadShaders()
		{
			if (assetBundle == null)
			{
				assetBundle = AssetBundle.LoadFromFile(Application.streamingAssetsPath + "/vrshaders");
				if (assetBundle == null)
				{
					Debug.LogError("No assetbundle present!");
					return;
				}
			}
			Debug.Log("Loading shaders from asset bundle...");

			occlusion = assetBundle.LoadAsset("assets/steamvr/resources/steamvr_hiddenarea.shader").Cast<Shader>();
			blit = assetBundle.LoadAsset("assets/steamvr/resources/steamvr_blit.shader").Cast<Shader>();
			blitFlip = assetBundle.LoadAsset("assets/steamvr/resources/steamvr_blitFlip.shader").Cast<Shader>();
			overlay = assetBundle.LoadAsset("assets/steamvr/resources/steamvr_overlay.shader").Cast<Shader>();
			fade = assetBundle.LoadAsset("assets/steamvr/resources/steamvr_fade.shader").Cast<Shader>();
			string[] allAssetNames = assetBundle.GetAllAssetNames();
			for (int i = 0; i < allAssetNames.Length; i++)
			{
				Debug.Log(allAssetNames[i]);
			}
		}
	}
}
