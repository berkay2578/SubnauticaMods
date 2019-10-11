using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;

using Harmony;
using ManageCreatureSpawns.SettingsManager;

namespace ManageCreatureSpawns {
   public static class Patcher {
      public static void log(string logMessage, params string[] arg) {
         if (arg.Length > 0)
            logMessage = String.Format(logMessage, arg);
         Console.WriteLine("[ManageCreatureSpawns] {0}", logMessage);
      }

      public static HarmonyInstance harmony = null;
      public static Random rEngine = new Random();
      public static SettingsManager.Settings settings = null;

      public static class Manager {
         public static bool TryKillCreature(Creature creature) {
            if (creature != null
			   && creature.enabled
			   && creature.Friendliness.Value != 1
			   && creature.version == 3
			   && creature.gameObject != null)
            {
               var creatureConfiguration = settings.UnwantedCreaturesList.FirstOrDefault(c =>
                  c.Name.ToLowerInvariant() == creature.name.Replace("(Clone)", String.Empty).ToLowerInvariant());
               if (creatureConfiguration != null)
               {
                  if (rEngine.Next(0, 100) >= creatureConfiguration.SpawnConfiguration.SpawnChance)
                  {
                     creature.tag = "Untagged";
                     creature.version = 4;
                     creature.leashPosition = UnityEngine.Vector3.zero;

                     CreatureDeath cDeath = creature.gameObject.GetComponent<CreatureDeath>();
                     if (cDeath != null)
                     {
                        cDeath.eatable = null;
                        cDeath.respawn = creatureConfiguration.SpawnConfiguration.CanRespawn;
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
                     } else
                     {
                        creature.BroadcastMessage("OnKill");
                     }
                     return true;
                  }
				  else {
                     creature.version = 4;
                     CreatureDeath cDeath = creature.gameObject.GetComponent<CreatureDeath>();
                     if (cDeath != null) {
                        cDeath.respawn = creatureConfiguration.SpawnConfiguration.CanRespawn;
                     }
                  }
               }
            }
            return false;
         }

         [HarmonyPrefix]
         [HarmonyPriority(Priority.First)]
         public static bool GenericKillCreature(Creature __instance) {
            return !TryKillCreature(__instance);
         }

         [HarmonyPrefix]
         [HarmonyPriority(Priority.First)]
         public static bool CreatureActionKillCreature(Creature __instance, ref CreatureAction __result) {
            if (TryKillCreature(__instance))
            {
               __result = null;
               return false;
            }
            return true;
         }
      }

      public static void ApplyPatches() {
         log("Loading... v{0}", Assembly.GetExecutingAssembly().GetName().Version.ToString());

         harmony = HarmonyInstance.Create("mod.berkay2578.managecreaturespawns");
         if (harmony != null)
         {
            log("HarmonyInstance created.");

            log("Reading settings.");
            {
               XmlSerializer serializer = new XmlSerializer(typeof(SettingsManager.Settings));
               using (StreamReader reader = new StreamReader("QMods\\ManageCreatureSpawns\\Settings.xml"))
                  settings = (SettingsManager.Settings)serializer.Deserialize(reader);
               serializer = null;

               if (settings == null)
               {
                  log("Could not load settings, exiting.");
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
                  "InitializeAgain",
                  "InitializeOnce",
                  "OnDrop",
                  "OnTakeDamage",
                  "ProcessInfection",
                  "ScanCreatureActions",
                  "Update",
                  "UpdateBehaviour",
                  "Start"
               };
               List<string> creatureActionFunctionsToBePatched = new List<string>() {
                  "ChooseBestAction",
                  "GetBestAction",
                  "GetLastAction"
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
               foreach (string fn in creatureActionFunctionsToBePatched)
               {
                  harmony.Patch(
                      typeof(Creature).GetMethod(fn, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance),
                      new HarmonyMethod(typeof(Manager).GetMethod("CreatureActionKillCreature")),
                      null
                  );
                  log("Patched Creature.{0}", fn);
               }
            }
            log("Finished.");
         } else
         {
            log("HarmonyInstance() returned null.");
         }
      }
   }
}
