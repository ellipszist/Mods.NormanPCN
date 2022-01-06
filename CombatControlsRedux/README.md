# Combat Controls Redux

## Credits
This Mod is an adaption of the work of DjStln (DJ-STLN), who created the original Combat Controls Mod.
That mod is available at https://www.nexusmods.com/stardewvalley/mods/2590

## Permissions

Combat Controls Redux is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

The Combat Controls Redux mod is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY;
without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
See the GNU General Public License for more details.

I don't really care if you redistribute it or alter it or use it in compilations.
I'd ask that you give credit to myself and DJ-STLN, that's all.

Source code is available at
https://github.com/NormanPCN/StardewValleyMods/tree/main/CombatControlsRedux

## Config

config.json is available to edit and configure the Mods functions.
 After the first time the Mod is started it will create a default config.json file in the Mods folder.
 config.json is a simple text file and easy to edit. It might be hard to find the Mods folder for some.

Combat Controls Redux also supports the Generic Mod Config Menu (GMCM) interface.
GMCM is a much easier way to configure Combat Controls Redux.
GMCM provides a graphical user interface to configure the Mod settings.
You can configure from the game title screen and/or during the game.
GMCM is available at https://www.nexusmods.com/stardewvalley/mods/5098
The NexusMods page shows the locations of the GMCM config button(s).

## Description

This Mod is intended to be used with a mouse. That is it's reason for existence.  
This Mod does support button remapping and thus other controller types. Control from other than a mouse has not been tested.

### Fixed Mouseclick. Character facing direction  
Config setting = MouseFix. Default = true.  
Turns the character in the direction of your mouse click. This functions with Swords, Clubs, Daggers and Scythe. 
It also operates with the dagger special attack (right click).
Tools like the pickaxe operate as they normally do in the game.
The game's normal functions for turning a character with mouse clicks check what specific tile is clicked,
requiring that the exact tile next to the character is clicked in order to turn the character towards it.
This mod makes it so that clicking in the general direction of a tile is enough to turn the character.

### Auto Swing  
Config setting = AutoSwing. Default = false.  
Holding the Use Tool button down (left click) will continuously swing the weapon. For swords and clubs.

### Auto Swing Dagger  
Config setting = AutoSwingDagger. Default = true.  
Holding the Use Tool button down (left click) will continuously attack with a dagger. Daggers attack very fast and auto swing can be very effective.

### Slick Moves!  
Config setting = SlickMoves. Default = true.  
Slick Moves does not function unless Mouse Fix is also enabled (true).
When running and attacking to the sides, the character will slide along the ground while swinging the weapon.
For swords and clubs.


> ### Sword Secondary Attack Dash  
> Config setting = SwordSpecialSlickMove. Default = true.  
> A quick dash while using the secondary attack (right click). Use by hitting the use tool button (left click) immediately after the secondary attack and moving in the direction you want to dash. You can use this to close distance while blocking, or dash across a gap during the block.
> Enabled for swords.

> ### Club Special Slick Move  
> Config setting = ClubSpecialSlickMove. Default = false.  
> This allows the club special attack to do the sliding/dashing slick move. Use by hitting the use tool button (left click) immediately after the secondary attack (right click).
> One may not want the club special area attack to move the player during the attack. This can help you keep your distance from enemies. 

> ### Slide Velocity  
> Config setting = SlideVelocity. Default = 4.0
> The movement velocity of the normal attack slick moves slide. Swords and clubs. Controls the speed and distance of the slide.

> ### Special Slide Velocity  
> Config setting = SpecialSlideVelocity. Default = 2.8
> The movement velocity for special attack slick moves. Swords and clubs. Controls the speed and distance of the slide.

## Changlog

v1.0.0:  
 Initial release. 