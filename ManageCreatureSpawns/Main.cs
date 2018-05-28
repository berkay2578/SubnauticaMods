using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;

using Harmony;

namespace ManageCreatureSpawns {
   public static class Patcher {
      public static void log(string logMessage, params string[] arg) {
         if (arg.Length > 0)
            logMessage = String.Format(logMessage, arg);
         Console.WriteLine("[ManageCreatureSpawns] {0}", logMessage);
      }

      [Serializable()]
      [XmlRoot("Settings")]
      public class Settings {
         [XmlArray("UnwantedCreatures")]
         [XmlArrayItem("CreatureName")]
         public List<string> UnwantedCreaturesList { get; set; }
      }
      public static Settings settings = null;

      public class Manager {
         [HarmonyPrefix]
         [HarmonyPriority(Priority.First)]
         public static bool GenericKillCreature(Creature __instance) {
            if (__instance != null && __instance.enabled
               && __instance.gameObject != null
               && __instance.liveMixin != null && __instance.liveMixin.IsAlive())
            {
               if (!String.IsNullOrEmpty(settings.UnwantedCreaturesList.FirstOrDefault(s => __instance.gameObject.name.Contains(s))))
               {
                  CreatureDeath cDeath = __instance.gameObject.GetComponent<CreatureDeath>();
                  if (cDeath != null)
                  {
                     cDeath.eatable = null;
                     cDeath.respawn = false;
                     cDeath.removeCorpseAfterSeconds = 0.1f;
                  }

                  __instance.leashPosition = UnityEngine.Vector3.zero;
                  __instance.liveMixin.data.deathEffect = null;
                  __instance.liveMixin.data.passDamageDataOnDeath = false;
                  __instance.liveMixin.data.broadcastKillOnDeath = true;
                  __instance.liveMixin.data.destroyOnDeath = true;
                  __instance.liveMixin.data.explodeOnDestroy = false;
                  __instance.liveMixin.Kill();
                  return false;
               }
            }
            return true;
         }

         [HarmonyPrefix]
         [HarmonyPriority(Priority.First)]
         public static bool CreatureActionKillCreature(Creature __instance, ref CreatureAction __result) {
            if (__instance != null && __instance.enabled
               && __instance.gameObject != null
               && __instance.liveMixin != null && __instance.liveMixin.IsAlive())
            {
               if (!String.IsNullOrEmpty(settings.UnwantedCreaturesList.FirstOrDefault(s => __instance.gameObject.name.Contains(s))))
               {
                  CreatureDeath cDeath = __instance.gameObject.GetComponent<CreatureDeath>();
                  if (cDeath != null)
                  {
                     cDeath.eatable = null;
                     cDeath.respawn = false;
                     cDeath.removeCorpseAfterSeconds = 0.1f;
                  }

                  __instance.leashPosition = UnityEngine.Vector3.zero;
                  __instance.liveMixin.data.deathEffect = null;
                  __instance.liveMixin.data.passDamageDataOnDeath = false;
                  __instance.liveMixin.data.broadcastKillOnDeath = true;
                  __instance.liveMixin.data.destroyOnDeath = true;
                  __instance.liveMixin.data.explodeOnDestroy = false;
                  __instance.liveMixin.Kill();

                  __result = null;
                  return false;
               }
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
               XmlSerializer serializer = new XmlSerializer(typeof(Settings));
               using (StreamReader reader = new StreamReader("QMods\\ManageCreatureSpawns\\Settings.xml"))
                  settings = (Settings)serializer.Deserialize(reader);
               serializer = null;

               if (settings == null)
               {
                  log("Could not load settings, exiting.");
                  return;
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
