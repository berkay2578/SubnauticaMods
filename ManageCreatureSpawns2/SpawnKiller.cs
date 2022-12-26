using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using BepInEx.Logging;
using System.Threading;

namespace ManageCreatureSpawns2
{
    class SpawnKiller
    {
        private static ManualLogSource log = Logger.CreateLogSource("ManageCreatureSpawns2.SpawnKiller");
        private static ConfigService configService = ConfigService.Instance;
        private static HashSet<string> creaturesFound = new HashSet<string>();

        private static readonly Mutex randomMutex = new Mutex();
        public static Random rEngine = new Random();

        private static readonly Mutex statsMutex = new Mutex();
        private static Dictionary<string, HashSet<Creature>> creatureMap = new Dictionary<string, HashSet<Creature>>();
        private static Dictionary<string, int> creaturesKilled = new Dictionary<string, int>();

        [HarmonyPrefix]
        [HarmonyPriority(Priority.First)]
        public static bool ProcessCreature(Creature __instance)
        {
            Creature creature = __instance;
            if (creature == null || !creature.enabled || creature.gameObject == null)
            {
                return true;
            }

            string realName = creature.name.Replace("(Clone)", String.Empty);
            var creatureConfig = configService.unwantedCreatures.GetOrDefault(realName.ToLowerInvariant(), null);

            try
            {
                LogCreatures(realName);
            } catch(Exception e)
            {
                log.LogError(e);
            }

            if (creatureConfig == null || !CanDie(creatureConfig))
            {
                return true;
            }

            randomMutex.WaitOne();
            int random = rEngine.Next(1, 101);
            randomMutex.ReleaseMutex();

            try
            {
                DebugStats(creature, creatureConfig, random);
            }
            catch (Exception e)
            {
                log.LogError(e);
            }

            if (ShouldKill(creatureConfig, random))
            {
                Kill(creature);
                return false;
            }

            return true;
        }

        private static void Kill(Creature creature)
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
        }

        private static void LogCreatures(string creatureName)
        {


            if (configService.configIsDebuggingEnabled.Value && configService.configIsCreatureListEnabled.Value)
            {
                if (creaturesFound.Add(creatureName))
                {
                    List<string> listOfCreatures = new List<string>();
                    creaturesFound.ForEach(c =>
                    {
                        listOfCreatures.Add(c);
                    });
                    listOfCreatures.Sort();
                    log.LogDebug(string.Format("Found creatures:\n'{0}'", String.Join("',\n'", listOfCreatures)));
                }
            }
        }

        private static void DebugStats(Creature creature, CreatureConfig creatureConfiguration, int random)
        {
            if (configService.configIsDebuggingEnabled.Value)
            {
                statsMutex.WaitOne();

                string creatureName = creatureConfiguration.name;
                HashSet<Creature> creatures = creatureMap.GetOrDefault(creatureName, new HashSet<Creature>());

                creatures.Add(creature);
                creatureMap[creatureName] = creatures;

                int killed = creaturesKilled.GetOrDefault(creatureName, 0);
                if (ShouldKill(creatureConfiguration, random))
                {
                    killed++;
                }
                creaturesKilled[creatureName] = killed;

                foreach (var pair in creatureMap)
                {
                    string name = pair.Key;
                    int numFound = pair.Value.Count;
                    int numKilled = creaturesKilled[name];
                    double percSpawned = ((double)(numFound - numKilled) / (double)numFound) * 100d;
                    log.LogDebug(String.Format("{0}: {1} {0}(s) found. {2} {0}(s) killed. {3}% of {0}(s) spawned.", name, numFound, numKilled, percSpawned));
                }
                log.LogDebug("");

                statsMutex.ReleaseMutex();
            }
        }

        private static bool ShouldKill(CreatureConfig config, int random)
        {
            return !config.canSpawn.Value || random > config.spawnChance.Value;
        }

        private static bool CanDie(CreatureConfig config)
        {
            return !(config.canSpawn.Value && config.spawnChance.Value >= 100);
        }
    }
}
