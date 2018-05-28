using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;

using Harmony;

namespace ManageCreatureSpawns {
   namespace SettingsManager {
      [Serializable()]
      [XmlRoot("SpawnConfiguration")]
      public class SpawnConfiguration {
         [XmlElement("CanSpawn")]
         public bool CanSpawn { get; set; } = true;

         [XmlElement("SpawnChanceOutOf100")]
         public int SpawnChance { get; set; } = 100;
      }

      [Serializable()]
      [XmlRoot("Creature")]
      [XmlInclude(typeof(SpawnConfiguration))]
      public class Creature {
         [XmlElement("Name")]
         public string Name { get; set; } = "Placeholder Name";

         [XmlElement("SpawnConfiguration")]
         public SpawnConfiguration SpawnConfiguration { get; set; } = null;

         public override string ToString() {
            return String.Format(
               "Creature {{\r\n" +
               "  Name: {0}\r\n" +
               "  SpawnConfiguration {{\r\n" +
               "     CanSpawn:    {1}\r\n" +
               "     SpawnChance: {2}\r\n" +
               "  }}\r\n" +
               "}}",
               Name, SpawnConfiguration.CanSpawn, SpawnConfiguration.SpawnChance);
         }
      }

      [Serializable()]
      [XmlRoot("Settings")]
      [XmlInclude(typeof(Creature))]
      public class Settings {
         [XmlArray("UnwantedCreatures")]
         [XmlArrayItem("Creature")]
         public List<Creature> UnwantedCreaturesList { get; set; } = new List<Creature>();
      }
   }

   public static class Patcher {
      public static void log(string logMessage, params string[] arg) {
         if (arg.Length > 0)
            logMessage = String.Format(logMessage, arg);
         Console.WriteLine("[ManageCreatureSpawns] {0}", logMessage);
      }

      public static Random rEngine = new Random();
      public static SettingsManager.Settings settings = null;

      public static class Manager {
         public static bool TryKillCreature(Creature creature) {
            if (creature != null && creature.enabled
               && creature.gameObject != null
               && creature.liveMixin != null && creature.liveMixin.IsAlive())
            {
               var creatureConfiguration = settings.UnwantedCreaturesList.FirstOrDefault(c => creature.gameObject.name.Contains(c.Name));
               if (creatureConfiguration != null)
               {
                  if (!creatureConfiguration.SpawnConfiguration.CanSpawn
                     || rEngine.Next(0, 100) <= creatureConfiguration.SpawnConfiguration.SpawnChance)
                  {
                     creature.gameObject.SetActive(false);
                     CreatureDeath cDeath = creature.gameObject.GetComponent<CreatureDeath>();
                     if (cDeath != null)
                     {
                        cDeath.eatable = null;
                        cDeath.respawn = false;
                        cDeath.removeCorpseAfterSeconds = 0.1f;
                     }

                     creature.leashPosition = UnityEngine.Vector3.zero;
                     if (creature.liveMixin.data != null)
                     {
                        creature.liveMixin.data.deathEffect = null;
                        creature.liveMixin.data.passDamageDataOnDeath = false;
                        creature.liveMixin.data.broadcastKillOnDeath = true;
                        creature.liveMixin.data.destroyOnDeath = true;
                        creature.liveMixin.data.explodeOnDestroy = false;
                     }

                     creature.liveMixin.Kill();
                     return true;
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

         HarmonyInstance harmony = HarmonyInstance.Create("mod.berkay2578.managecreaturespawns");
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
         }
         else
         {
            log("HarmonyInstance() returned null.");
         }
      }
   }
}
