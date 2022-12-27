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

Configuration for Manage Creature Spawns 2 works just like the original with a `Settings.xml` file that you use to set
which creatures you would like to manage spawn rates for and an HTML page that provides the XML for anybody unfamiliar
with XML.

If you're familiar with XML and editing .xml files, there are instructions for constructing the XML in the
`Settings.xml` file. Just look up the creatures you want in the `List of creatures.txt` file and build your spawn
configuration from there. If you're unfamiliar with XML or you want to go the easy route, follow the steps below.

1. Open the `GenerateSettings.html` file in Google Chrome (this should work in any browser, but it was only tested in
Chrome).
2. Select "Subnautica" for your game.
3. Click the "Add New Creature" button.
4. Pick the creature you want to modify spawn rates of.
5. Uncheck the box if you don't want the creature to ever spawn.
6. Set the spawn percentage if you didn't uncheck the box. Only 0-100 are valid.
7. If you are not done adding creatures, repeat steps 3-6. Otherwise, go to step 8.
8. Click "Generate Settings"
9. Copy all of the text that was created below.
10. Open the `Settings.xml` file in a text editor. Note: Windows (and maybe other OSs) will try to open the file in a
web browser instead of a text editor by default. You will not be able to edit the settings in the browser. You will
need to tell your OS to open it with a text editor. In Windows, this can be done by right-clicking the file, then
selecting "Open with" and selecting Notepad.
11. Delete everything in this file.
12. Paste the text you copied in step 9.
13. Save and close the file. You're done.

Note: While the name of this mod may suggest that you can increase spawn rates above 100%, that's not how this mod
works. Anything above 100 will be treated as 100 (normal spawn rates).

## Nexus Mods

This version of the mod does not exist on Nexus Mods at this time. The reason for this is that I am considering making
another mod page and I have not reviewed TOS or any other rules as far as making sure that I am allowed to have
effectively 2 of the same mod even though they don't support the same versions of the game and they have different
requirements. I expect that this won't be a problem and that this mod will become available on Nexus Mods soon enough,
but for the time being, you will only be able to get this mod through my GitHub or any forks.