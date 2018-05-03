using System;
using System.Reflection;
using Harmony;

namespace NoWarpersAfterQuarantineShutdown {
   public static class Patcher {
      public static void log(string logMessage, params string[] arg) {
         if (arg.Length > 0)
            logMessage = String.Format(logMessage, arg);
         Console.WriteLine("[NoWarpersAfterQuarantineShutdown] {0}", logMessage);
      }

      public class ManageWarperSpawns {
         [HarmonyPrefix]
         [HarmonyPriority(Priority.First)]
         public static bool GenericSkipExecutionPatch() {
            if (StoryGoalCustomEventHandler.main.gunDisabled)
               return false;
            return true;
         }
      }
      public class ManageStoryEvents {
         [HarmonyPostfix]
         public static void PrecursorGunDisabledNotification() {
            ErrorMessage.AddMessage("Self-Warping Quarantine Enforcer Units are now disabled.");
         }
         [HarmonyPostfix]
         public static void AwakeGunDisabledCheck(StoryGoalCustomEventHandler __instance) {
            if (__instance.gunDeactivate != null && !String.IsNullOrEmpty(__instance.gunDeactivate.key))
               if (Story.StoryGoalManager.main.IsGoalComplete(__instance.gunDeactivate.key))
                  ManageStoryEvents.PrecursorGunDisabledNotification();
         }
      }

      public static void ApplyPatches() {
         log("Loading... v{0}", Assembly.GetExecutingAssembly().GetName().Version.ToString());

         HarmonyInstance harmony = HarmonyInstance.Create("mod.berkay2578.disablewarpers");
         if (harmony != null)
         {
            log("HarmonyInstance created.");

            // StoryGoalCustomEventHandler.Awake (Post)
            {
               harmony.Patch(
                   typeof(StoryGoalCustomEventHandler).GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Instance),
                   null,
                   new HarmonyMethod(typeof(ManageStoryEvents).GetMethod("AwakeGunDisabledCheck"))
               );
               log("StoryGoalCustomEventHandler.Awake (Post)");
            }

            // StoryGoalCustomEventHandler.DisableGun (Post)
            {
               harmony.Patch(
                   typeof(StoryGoalCustomEventHandler).GetMethod("DisableGun"),
                   null,
                   new HarmonyMethod(typeof(ManageStoryEvents).GetMethod("PrecursorGunDisabledNotification"))
               );
               log("StoryGoalCustomEventHandler.DisableGun (Post)");
            }

            // WarperSpawner.OnEnable (Pre)
            {
               harmony.Patch(
                   typeof(WarperSpawner).GetMethod("OnEnable", BindingFlags.NonPublic | BindingFlags.Instance),
                   new HarmonyMethod(typeof(ManageWarperSpawns).GetMethod("GenericSkipExecutionPatch")),
                   null
               );
               log("WarperSpawner.OnEnable (Pre)");
            }

            // WarperSpawner.Update (Pre)
            {
               harmony.Patch(
                  typeof(WarperSpawner).GetMethod("Update", BindingFlags.NonPublic | BindingFlags.Instance),
                  new HarmonyMethod(typeof(ManageWarperSpawns).GetMethod("GenericSkipExecutionPatch")),
                  null
               );
               log("WarperSpawner.Update (Pre)");
            }
            log("Patched successfully.");
         }
         else
         {
            log("HarmonyInstance() returned null.");
         }
      }
   }
}
