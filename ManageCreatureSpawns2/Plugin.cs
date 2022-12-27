using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using System;
using System.Collections.Generic;
using HarmonyLib;
using System.Reflection;
using System.IO;
using System.Xml.Serialization;
using System.Xml;

namespace ManageCreatureSpawns2
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInProcess("Subnautica.exe")]
    public class Plugin : BaseUnityPlugin
    {

        private Harmony harmony = null;

        private void Awake()
        {
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
            // Plugin startup logic
            BindConfig();
            SettingsManager.Settings settings;
            try
            {
                settings = LoadCreatureSettings();
            } catch(Exception ex)
            {
                Logger.LogError("Could not load creature settings");
                Logger.LogError(ex);
                return;
            }

            ConfigService cs = ConfigService.Instance;
            settings.UnwantedCreaturesList.ForEach(creature =>
            {
                CreatureConfig config = new CreatureConfig();
                config.name = creature.Name;
                config.canSpawn = creature.SpawnConfiguration.CanSpawn;
                config.spawnChance = creature.SpawnConfiguration.SpawnChance;
                cs.unwantedCreatures[creature.Name.ToLowerInvariant()] = config;
            });

            harmony = new Harmony(PluginInfo.PLUGIN_GUID);
            if(harmony != null)
            {
                Logger.LogInfo("Patching Creature events");
                List<string> genericFunctionsToBePatched = new List<string>() { "Start" };

                genericFunctionsToBePatched.ForEach(fn =>
                {
                    harmony.Patch(
                            typeof(Creature).GetMethod(fn, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance),
                            new HarmonyMethod(typeof(SpawnKiller).GetMethod("ProcessCreature")),
                            null
                        );
                    Logger.LogInfo(string.Format("Patched Creature.{0}", fn));
                });
                Logger.LogInfo("Setup Complete");
            }
            else
            {
                Logger.LogError("Harmony instance not created");
            }
        }

        private void BindConfig()
        {
            ConfigService configService = ConfigService.Instance;

            configService.configIsDebuggingEnabled = Config.Bind("Debugging", "IsDebugEnabled", false, "Enable debugging tools");
            configService.configIsCreatureListEnabled = Config.Bind(
                "Debugging",
                "IsCreatureListEnabled",
                false,
                "Log all encountered creatures during session"
            );
        }

        private SettingsManager.Settings LoadCreatureSettings()
        {
            SettingsManager.Settings settings = null;
            string location = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            DirectoryInfo dir = new DirectoryInfo(location);
            Logger.LogDebug($"mod directory:  {dir.FullName}");
            FileInfo[] settingsFiles = dir.GetFiles("Settings.xml");

            if (settingsFiles.Length < 1)
            {
                Logger.LogError("Manage Creature Spawns could not find \"Settings.xml\" in mod folder. Manage Creature Spawns is now disabled.");
                throw new Exception("Could not find Settings.xml");
            }
            else if (settingsFiles.Length > 1)
            {
                List<string> settingsFilesNames = new List<string>();
                settingsFiles.ForEach(file => { settingsFilesNames.Add(file.Name); });
                Logger.LogWarning($"Multiple settings files found. Using first one available. {settingsFilesNames}");
            }
            string settingsFileName = settingsFiles[0].FullName;

            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(SettingsManager.Settings));

                Logger.LogInfo("Filtering out comments");
                // load document
                XmlDocument doc = new XmlDocument();
                doc.Load(settingsFileName);

                // remove all comments
                XmlNodeList l = doc.SelectNodes("//comment()");
                foreach (XmlNode node in l) node.ParentNode.RemoveChild(node);

                // store to memory stream and rewind
                MemoryStream ms = new MemoryStream();
                doc.Save(ms);
                ms.Seek(0, SeekOrigin.Begin);

                Logger.LogInfo("Reading settings.");
                settings = (SettingsManager.Settings)serializer.Deserialize(XmlReader.Create(ms));
                serializer = null;
            }
            catch (Exception ex)
            {
                Logger.LogError("Exception occurred while loading settings");
                Logger.LogError(ex);
                Logger.LogError("Manage Creature Spawns could not load settings from \"Settings.xml\".  Manage Creature Spawns is now disabled.");
                throw new Exception("Could not load settings", ex);
            }
            Logger.LogInfo("Settings loaded");

            return settings;
        }
    }
}
