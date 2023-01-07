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
3. Open the `ManageCreatureSpawns2` folder.
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

## Debugging

There are some basic debugging features included with this mod. After running the mod the first time, you can open the
config file `com.arnoldrex.mods.subnautica.managecreaturespawns2.cfg` in the config folder under BepInEx. In there you
will find the debugging configuration defaulted to disabling the debugging. Set the settings you need to true and you
will be able to see a lot more debugging info. Don't forget to set your configs in the `BepInEx.cfg` file to allow the
debug logs to be printed to the console and the output file. You will need these settings to be able to see any of the
debugging info from the mod.

## Contributing

I don't suspect there will be any contributions, but if you are interested, here's how you can help.

First, this is built in Visual Studio 2019. You can pick whatever IDE you prefer, but I will only provide instructions
for Visual Studio because that's all I personally know.

### Setting up Visual Studio 2019

I'm going to skip installing Visual Studio. There are plenty of other, better guides out there to help you with that.

Follow the [BepInEx Guide](https://docs.bepinex.dev/articles/dev_guide/plugin_tutorial/1_setup.html) to setting up your
dev environment. If you already have Visual Studio installed, you can skip straight past the .NET SDK and picking your
IDE sections. You can come back here when you're done with that.

Now, you'll need to do some NuGet configuration. Your NuGet package manager probably doesn't recognize a lot of the
dependencies if this is your first time. Go to the Solution Explorer on the right (assuming default layout), open the
ManageCreatureSpawns2 project, if not already open, and right-click on "Dependencies" then click on "Manage NuGet
Packages...". Near the top right of your main view pane, you should see a gear icon. Click it. This should open the
"NuGet Package Manager" -> "Package Sources" options. Add a new package source with the green "+". The source should be
"https://nuget.bepinex.dev/v3/index.json", and the name I chose was BepInEx. Now your package manager should be able to
import the packages needed from BepInEx.

### Contributing

#### Code Formatting

I use the Visual Studio 2019 auto formatter for my code. I request that everybody do the same. If there are massive
formatting changes when you use the auto formatter, undo those changes and just format the code you have added or
modified. If you are experiencing massive formatting changes every time, let me know and I will try to determine a
better standard.

#### Coding Best Practices

Please use the C# coding best practices including naming. I am not super familiar with them myself, so if you see best
practices not being followed in any existing code, please make the changes and we can discuss them during Pull Request
review if needed.

#### Plugin Info

Use the PluginInfo.cs file to make version changes. **DO NOT** modify `PLUGIN_GUID` or `PLUGIN_NAME` unless you are
forking a new project. These must remain the same or it could conflict with other mods or break dependencies (not that
I imagine there will be dependencies). Seriously, don't do it.

When updating the version, please follow [Semantic Versioning Standards](https://semver.org/) for the new version you
are creating. It is required by BepInEx. When you create a pull request, please justify the version changes you made
in the pull request description.

#### Mod Files

Minimize the amount of changes you do to the auxiliary files. Since these files are all intended to help the user set
up their settings, the only changes done to them should be to better describe how to properly configure the settings.

Do not forget to load the files into the zip folder. The zip folder will be used for the Nexus Mods download and it
must include all the required files to help people get the mod up and running. Make sure to include all of the files
below inside of a folder called `ManageCreatureSpawns2`. This will make it easier for users to extract the mod.

* Mod files
  * ManageCreatureSpawns2.dll
  * ManageCreatureSpawns2.deps.json
  * ManageCreatureSpawns2.pdb
* Settings files
  * Settings.xml
  * Settings helpers
    * GenerateSettings.html
    * "List of creatures.txt"
    * subnauticaCreatures.js
    * belowZeroCreatures.js

#### README

Please update the readme with your version details including all the changes that were made to the mod. If there are
any changes to the way the mod works, update the relevant README sections.

## Versions

<!-- Put new versions here, newest first. -->

### Version 2.0.0

This is the initial version of the mod. 2.0.0 was chosen to avoid versioning conflicts with the legacy
ManageCreatureSpawns on NexusMods.

* Built on BepInEx instead of QModManager
* New unique ID
* Change kill logic to prevent permanent extinction events even when the mod is uninstalled. Will probably still
  permanently kill reaper leviathans.
* Debug configurations relocated to config file.