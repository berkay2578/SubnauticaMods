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
using QModManager.API;
using System.Xml;

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
            private static HashSet<string> creaturesFound = new HashSet<string>();

            public static bool TryKillCreature(Creature creature)
            {
                if (creature != null && creature.enabled
                   && creature.gameObject != null)
                {
                    string realName = creature.name.Replace("(Clone)", String.Empty);
                    var creatureConfiguration = settings.UnwantedCreaturesList.FirstOrDefault(c =>
                       c.Name.ToLowerInvariant() == realName.ToLowerInvariant());

                    if (settings.IsDebugEnabled && settings.IsCreatureListEnabled)
                    {
                        if (creaturesFound.Add(realName))
                        {
                            List<string> listOfCreatures = creaturesFound.ToList();
                            listOfCreatures.Sort();
                            log(Logger.Level.Debug, "Found creatures:\n'{0}'", String.Join("',\n'", listOfCreatures));
                        }
                    }

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

                if(!LoadSettings())
                {
                    return;
                }

                foreach (var item in settings.UnwantedCreaturesList)
                {
                    log("Loaded creature configuration: \r\n{0}", item.ToString());
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

        private static bool LoadSettings()
        {
            string location = QModServices.Main.GetMyMod().LoadedAssembly.Location;
            log(Logger.Level.Debug, "mod location:  {0}", location);
            DirectoryInfo dir = Directory.GetParent(location);
            log(Logger.Level.Debug, "mod directory:  {0}", dir.FullName);
            FileInfo[] settingsFiles = dir.GetFiles("Settings.xml");

            if (settingsFiles.Length < 1)
            {
                AlertUser("Manage Creature Spawns could not find \"Settings.xml\" in mod folder. Manage Creature Spawns is now disabled.");
                return false;
            }
            else if (settingsFiles.Length > 1)
            {
                List<string> settingsFilesNames = new List<string>();
                settingsFiles.ForEach(file => { settingsFilesNames.Add(file.Name); });
                log(Logger.Level.Warn, "Multiple settings files found. Using first one available. {0}", settingsFilesNames);
            }
            string settingsFileName = settingsFiles[0].FullName;

            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(SettingsManager.Settings));

                log("Filtering out comments");
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

                log("Reading settings.");
                settings = (SettingsManager.Settings)serializer.Deserialize(XmlReader.Create(ms));
                serializer = null;
            }
            catch (Exception ex)
            {
                Logger.Log(Logger.Level.Error, "Exception occurred while loading settings", ex);
                AlertUser("Manage Creature Spawns could not load settings from \"Settings.xml\".  Manage Creature Spawns is now disabled.");
                return false;
            }

            return true;
        }

        private static void AlertUser(string message)
        {
            Thread t = new Thread(() =>
            {
                Thread.Sleep(10000);
                Logger.Log(Logger.Level.Error, message, null, true);
            });
            t.Start();
        }
    }
}
