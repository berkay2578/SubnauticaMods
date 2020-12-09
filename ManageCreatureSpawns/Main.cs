using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using System.Threading;

using HarmonyLib;
using ManageCreatureSpawns.SettingsManager;
using QModManager.Utility;

namespace ManageCreatureSpawns
{
    public static class Patcher
    {
        public static void log(string logMessage, params string[] arg)
        {
            log(Logger.Level.Info, logMessage, arg);
        }

        public static void log(Logger.Level logLevel, string message, params object[] args)
        {
            string logMessage = message;
            if (args.Length > 0)
            {
                logMessage = string.Format(message, args);
            }

            Logger.Log(logLevel, logMessage);
        }

        public static Harmony harmony = null;
        public static Random rEngine = new Random();
        public static SettingsManager.Settings settings = null;

        public static class Manager
        {
            private static readonly Mutex randomMutex = new Mutex();
            private static readonly bool isDebugEnabled = settings.IsDebugEnabled;
            private static readonly Mutex statsMutex = new Mutex();
            private static Dictionary<string, HashSet<Creature>> creatureMap = new Dictionary<string, HashSet<Creature>>();
            private static Dictionary<string, int> creaturesKilled = new Dictionary<string, int>();

            public static bool TryKillCreature(Creature creature)
            {
                if (creature != null && creature.enabled
                   && creature.gameObject != null)
                {
                    var creatureConfiguration = settings.UnwantedCreaturesList.FirstOrDefault(c =>
                       c.Name.ToLowerInvariant() == creature.name.Replace("(Clone)", String.Empty).ToLowerInvariant());
                    if (creatureConfiguration != null)
                    {
                        randomMutex.WaitOne();
                        int random = rEngine.Next(1, 101);
                        randomMutex.ReleaseMutex();

                        if (isDebugEnabled)
                        {
                            statsMutex.WaitOne();

                            string creatureName = creatureConfiguration.Name;
                            HashSet<Creature> creatures;
                            if (!creatureMap.TryGetValue(creatureName, out creatures))
                            {
                                creatures = new HashSet<Creature>();
                            }
                            creatures.Add(creature);
                            creatureMap[creatureName] = creatures;

                            if (!creatureConfiguration.SpawnConfiguration.CanSpawn
                               || random >= creatureConfiguration.SpawnConfiguration.SpawnChance)
                            {
                                int killed = 0;
                                if (!creaturesKilled.TryGetValue(creatureName, out killed))
                                {
                                    killed = 0;
                                }
                                killed++;
                                creaturesKilled[creatureName] = killed;
                            }

                            foreach (var pair in creatureMap)
                            {
                                string name = pair.Key;
                                int numFound = pair.Value.Count;
                                int numKilled = creaturesKilled[name];
                                double percSpawned = ((double)(numFound - numKilled) / (double)numFound) * 100d;
                                log(Logger.Level.Debug, "{0}: {1} {0}(s) found. {2} {0}(s) killed. {3}% of {0}(s) spawned.", name, numFound, numKilled, percSpawned);
                            }
                            log(Logger.Level.Debug, "");

                            statsMutex.ReleaseMutex();
                        }

                        if (!creatureConfiguration.SpawnConfiguration.CanSpawn
                           || random >= creatureConfiguration.SpawnConfiguration.SpawnChance)
                        {
                            creature.tag = "Untagged";
                            creature.leashPosition = UnityEngine.Vector3.zero;

                            CreatureDeath cDeath = creature.gameObject.GetComponent<CreatureDeath>();
                            if (cDeath != null)
                            {
                                cDeath.eatable = null;
                                cDeath.respawn = false;
                                cDeath.removeCorpseAfterSeconds = 1.0f;
                            }
                            if (creature.liveMixin != null && creature.liveMixin.IsAlive())
                            {
                                if (creature.liveMixin.data != null)
                                {
                                    creature.liveMixin.data.deathEffect = null;
                                    creature.liveMixin.data.passDamageDataOnDeath = false;
                                    creature.liveMixin.data.broadcastKillOnDeath = true;
                                    creature.liveMixin.data.destroyOnDeath = true;
                                    creature.liveMixin.data.explodeOnDestroy = false;
                                }
                                creature.liveMixin.Kill();
                            }
                            else
                            {
                                creature.BroadcastMessage("OnKill");
                            }
                            return true;
                        }
                    }
                }
                return false;
            }

            [HarmonyPrefix]
            [HarmonyPriority(Priority.First)]
            public static bool GenericKillCreature(Creature __instance)
            {
                return !TryKillCreature(__instance);
            }
        }

        public static void ApplyPatches()
        {
            log("Loading... v{0}", Assembly.GetExecutingAssembly().GetName().Version.ToString());

            harmony = new Harmony("mod.berkay2578.managecreaturespawns");
            if (harmony != null)
            {
                log("Harmony instance created.");

                log("Reading settings.");
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(SettingsManager.Settings));
                    using (StreamReader reader = new StreamReader("QMods\\ManageCreatureSpawns\\Settings.xml"))
                        settings = (SettingsManager.Settings)serializer.Deserialize(reader);
                    serializer = null;

                    if (settings == null)
                    {
                        log(Logger.Level.Error, "Could not load settings, exiting.");
                        return;
                    }

                    foreach (var item in settings.UnwantedCreaturesList)
                    {
                        log("Loaded creature configuration: \r\n{0}", item.ToString());
                    }
                }

                log("Patching Creature events");
                {
                    List<string> genericFunctionsToBePatched = new List<string>() {
                        "Start"
                    };

                    foreach (string fn in genericFunctionsToBePatched)
                    {
                        harmony.Patch(
                            typeof(Creature).GetMethod(fn, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance),
                            new HarmonyMethod(typeof(Manager).GetMethod("GenericKillCreature")),
                            null
                        );
                        log("Patched Creature.{0}", fn);
                    }
                }
                log("Finished.");
            }
            else
            {
                log(Logger.Level.Error, "HarmonyInstance() returned null.");
            }
        }
    }
}
