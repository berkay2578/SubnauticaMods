# Manage Creature Spawns 2

Manage Creature Spawns 2 is a remaking of the original Manage Creature Spawns. Subnautica underwent a massive update
called the Living Large update where the game engine version was updated and many new features were added. As part of
this new update, QModManager broke and the team decided to deprecate it. Given that QModManager is broken and will not
be fixed, a new version of the mod running on BepInEx had to be made. Subnautica has also split its game into Legacy
and 2.0 to support existing mods in the legacy version while providing the new features in the 2.0 version. Given this
split, the old Manage Creature Spawns will continue to work with QModManager in Legacy while Manage Creature Spawns 2
will work with BepInEx in 2.0.

## Installing

1. Install BepInEx. Instructions are included on the mod page.
2. Extract the .zip file into the plugins folder of the BepInEx folder.
3. Start Subnautica and exit after reaching the start screen. This generates the config file for mod configuration.
4. Configure the mod (see below).
5. Start playing

## Configuration

Manage Creature Spawns 2 with BepInEx creates its own configuration file with all the creatures in the base game
included. You can find it in the config folder under the BepInEx folder. Follow the instructions below to configure
Manage Creature Spawns 2. Note: as of version 2.0.0, only base game creatures can have their spawn configurations
modified. For all other creatures, you will have to use the legacy version or wait until a version that supports other
creatures is made available.

1. Locate the `ManageCreatureSpawns2.cfg` file in the config folder.
2. Open in any text editor
3. Browse or search for the creature you want to modify spawning rates for. I'll use the example `GhostRayBlue`.
4. If you want to disable spawning completely, set the `CanSpawn` configuration for your creature to `false`. E.G.
`GhostRayBlueCanSpawn = false`.
5. If you want to change the spawn rate, leave the `CanSpawn` configuration as `true` and set the `SpawnChance` value
to whatever percentage you want to adjust the spawn rate to (1-100). E.G. `GhostRayBlueSpawnChance = 30`. Note: this
value can only be set to whole numbers. Anything below 1 will result in no spawning, and anything above 100 will result
in default spawn rates. There is no way to increase the spawn rate of any creatures.

## Nexus Mods

This version of the mod does not exist on Nexus Mods at this time. The reason being that I am considering making
another mod page and I have not reviewed TOS or any other rules as far as making sure that I am allowed to have
effectively 2 of the same mod even though they don't support the same versions of the game, and they have different
requirements. I expect that this won't be a problem and that this mod will become available on Nexus Mods soon enough,
but for the time being, you will only be able to get this mod through my GitHub or any branches.