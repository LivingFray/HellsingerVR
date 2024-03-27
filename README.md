# Following the announcement of an official VR port for Metal:Hellsinger this mod is no longer being maintained


# HellsingerVR
![GitHub release (latest SemVer)](https://img.shields.io/github/v/release/LivingFray/HellsingerVR?sort=semver)

A VR Mod for Metal: Hellsinger

Made possible by "borrowing", and slightly modifying, DSprtn's modified SteamVR plugin: https://github.com/DSprtn/SteamVR_Standalone_IL2CPP

## Installation
- Download the latest version of the mod from the [releases page](https://github.com/LivingFray/HellsingerVR/releases)
- Locate your game's installation folder
  - For steam: Library>Metal:Hellsinger>Right Click>Manage>Browse local files
  - For xbox app: Metal:Hellsinger>3 dots>Manage>Files>Browse>Metal- Hellsinger>Content
- Extract the mod zip into the top level of the game folder
![image](https://user-images.githubusercontent.com/5647734/209443220-1f0d75ed-72a6-4e25-a2b5-2e1f52ac7ef1.png)
- Launch the game normally

## Controls
Oculus rift:
![metalhellsingercontrols_oculus](https://user-images.githubusercontent.com/5647734/209443777-53d1a8ff-bd56-4010-8c38-23935eba000b.png)

Valve index:
![metalhellsingercontrols_index](https://user-images.githubusercontent.com/5647734/209446100-5b68c669-94e1-414e-be5d-22a362600866.png)


## Config
Aspects of the mod can be configured in LivingFray.HellsingerVR.cfg, found in BepInEx/config.
- Enabled: Disable this mod without uninstalling it
- Left handed: One handed weapons will be held in the left hand, does not flip joystick bindings
- DisableMotionControls: Disables the motion controls portion of the mod, allowing you to play with a gamepad
- Snap turn amount: Angle in degrees to turn per snap turn. Defaults to 0, which enables smooth turning
- Movement type: VR device movement stick is relative to (head, hand, offhand)
- Reticle location: Where the reticle/beat indicator is drawn (target, sights, head) defaults to target, which attaches the reticle to the world object currently being aimed at
- Show health on hand: Display health bar on the offhand or floating in front of your head
- Show ultimate on hand: Display ultimate charge bar on the offhand or floating in front of your head
- Show fury on hand: Display fury meter on the offhand or floating in front of your head
- Show boss on hand: Display bosses' health bars on the offhand or floating in front of your head
- Menu UI Distance: Distance from the head menu UI will be displayed in meters
- MoveUIVertically: Should the game ui also move vertically to align with the HMD's facing direction, or just horizontally
- Game UI Distance: Distance from the head game UI elements not moved into the world (e.g. health, reticle, slaughter indicators) will be displayed in meters
- PostProcessingLevel: Disables some of the more costly post processing effects for performance. Options are 0 (only non-vr compatible effects such as DoF and motion blur), 1 (most non essential post processing effects), 99 (everything). Any other value will be rounded down
- BeatVibrationStrength: Strength of vibration effect played on the beat (0 will disable it entirely)
- BeatVibrationFrequency: Frequency the motors in the controller vibrate at
- BeatVibrationLength: Time in milliseconds to vibrate for on each beat
- BeatVibrationOffset: Time in milliseconds to offset the vibration on each beat

## Known issues
### Game focus
To get past the logos/login screen the game needs to be in focus. If you can't login in (by pressing either trigger) check the game is in focus

### Performance
Metal: Hellsinger was not designed for VR and can be quite graphically intensive. Turning down the graphics settings in game can help somewhat (going from high to mid resulted in the most noticable performance gain during testing, with going from mid to low having minimal additional impact). Using [vrperfkit](https://github.com/fholger/vrperfkit) can help considerably at the cost of some graphical fidelity. 


## Uninstalling
Delete the BepInEx folder, winhttp.dll and any other files added while installing the mod.
Verifying the game integrity through steam/xbox may also work.
