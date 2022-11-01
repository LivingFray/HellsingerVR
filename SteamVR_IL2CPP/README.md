# SteamVR_Standalone_IL2CPP
A modified SteamVR plugin that can be injected into Non-VR enabled Unity projects for VR rendering and VR input/interaction. Modified for IL2CPP. Specifically tailored for GTFO. Some adjustments required! 

## How to use:

#### CONFIGURATION 

Set BEPINEX_PATH as a env variable to point to the bepinex folder of the game you're trying to mod. (i.e. ...\SteamApps\$GAME$\Bepinex), this will grab most references. Newtonsoft should be pulled automatically.

You need to replace the memory offset in the SteamVR/ExternalPluginFunctionExtractor class with the memory offset of FindAndLoadUnityPlugin 
from UnityPlayer.DLL (in your game install folder) for the exact Unity version your game is using.

var loadLibraryAddress = module.BaseAddress + >>> 0x786D00 <<<

I recommend using Ghidra or IDA to get the mem address.

PDB files for the il2cpp UnityPlayer ship with the Unity editor and can be found under 
Editor\Data\PlaybackEngines\windowsstandalonesupport\Variations\win64_nondevelopment_il2cpp
You need to have Il2CPP support enabled in the given Unity install for them to appear.

#### INITIALIZATION

Call SteamVR.Initialize(false) before the game loads. Add SteamVR_Camera on the main player character. 
You may need to tweak the Expand() function in SteamVR_Camera depending on the needs of your game.
For rendering UI look at Eusth's VRGIN approach or at GTFO VR if you're ok with just using SteamVR_Overlay for it.
