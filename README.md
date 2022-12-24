# HellsingerVR
A VR Mod for Metal: Hellsinger

Made possible by "borrowing", and slightly modifying, DSprtn's modified SteamVR plugin: https://github.com/DSprtn/SteamVR_Standalone_IL2CPP

## Installation
- Download the latest version of the mod from the releases page
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
Coming soon

## Config
Aspects of the mod can be configured in LivingFray.HellsingerVR.cfg, found in BepInEx/config.
- Enabled: Disable this mod without uninstalling it
- Left handed: One handed weapons will be held in the left hand, does not flip joystick bindings
- Snap turn amount: COMING SOON - Will control snap turning speed, currently only smooth turning is supported
- Movement type: VR device movement stick is relative to (head, hand, offhand)
- Reticle location: Where the reticle/beat indicator is drawn (target, sights, head) defaults to target, which attaches the reticle to the world object currently being aimed at
- Show health on hand: Display health bar on the offhand or floating in front of your head
- Show ultimate on hand: Display ultimate charge bar on the offhand or floating in front of your head
- Show fury on hand: Display fury meter on the offhand or floating in front of your head
- Show boss on hand: Display bosses' health bars on the offhand or floating in front of your head

## Known issues
### Game focus
To get past the logos/login screen the game needs to be in focus. If you can't login in (by pressing either trigger) check the game is in focus

### Performance
Metal: Hellsinger was not designed for VR and can be quiet graphically intensive. Turning down the graphics settings in game can help somewhat (going from high to mid resulted in the most noticable performance gain during testing, with going from mid to low having minimal additional impact). Using [vrperfkit](https://github.com/fholger/vrperfkit) can help considerably at the cost of some graphical fidelity.

### Room scale
Currently the HMD's position only affects the camera, your hitbox will remain in the middle of the play area

### Display mirroring
The VR view is currently not mirrored to the flat display, and for performance reasons the flat display has most of its rendering disabled. For now use steamvr's vr view window.

## Uninstalling
Delete the BepInEx folder, winhttp.dll and any other files added while installing the mod.
Verifying the game integrity through steam/xbox may also work.
