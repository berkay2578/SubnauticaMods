using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using System;
using System.Collections.Generic;
using HarmonyLib;
using System.Reflection;


namespace ManageCreatureSpawns2
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInProcess("Subnautica.exe")]
    public class Plugin : BaseUnityPlugin
    {

        private Harmony harmony = null;

        private void Awake()
        {
            // Plugin startup logic
            BindConfig();
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

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

            configService.unwantedCreatures = new Dictionary<string, CreatureConfig>();
            Array.ForEach(Enum.GetNames(typeof(CreatureName)), name =>
            {
                CreatureConfig config = new CreatureConfig();
                config.name = name;
                config.canSpawn = Config.Bind("UnwantedCreatures", $"{name}CanSpawn", true, $"Set if {name} can spawn");
                config.spawnChance = Config.Bind("UnwantedCreatures", $"{name}SpawnChance", 100, $"Set probability (in percentage) that {name} will spawn (0-100)");
                configService.unwantedCreatures[name.ToLowerInvariant()] = config;
            });
        }
    }
}
