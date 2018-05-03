using Harmony;

namespace NoWarpersAfterQuarantineShutdown {
   public static class Patcher {
      public class ManageWarperSpawns {
         [HarmonyPriority(Priority.First)]
         public static bool GenericSkipExecutionPatch() {
            if (StoryGoalCustomEventHandler.main.gunDisabled)
               return false;
            return true;
         }

         public class ManageStoryEvents {
            public static void PrecursorGunDisabledNotification() {
               ErrorMessage.AddMessage("Self-Warping Quarantine Enforcer Units are now disabled.");
            }
            public static void AwakeGunDisabledCheck(StoryGoalCustomEventHandler __instance) {
               if (__instance.gunDeactivate.key != null)
                  if (Story.StoryGoalManager.main.IsGoalComplete(__instance.gunDeactivate.key))
                     ManageStoryEvents.PrecursorGunDisabledNotification();
            }
         }
      }

      public static void ApplyPatches() {
         HarmonyInstance harmony = HarmonyInstance.Create("mod.berkay2578.disablewarpers");
         if (harmony != null)
         {
            // StoryGoalCustomEventHandler.Awake (Post)
            {
               harmony.Patch(
                   typeof(StoryGoalCustomEventHandler).GetMethod("Awake"),
                   null,
                   new HarmonyMethod(typeof(ManageWarperSpawns.ManageStoryEvents).GetMethod("AwakeGunDisabledCheck"))
               );
            }

            // StoryGoalCustomEventHandler.DisableGun (Post)
            {
               harmony.Patch(
                   typeof(StoryGoalCustomEventHandler).GetMethod("DisableGun"),
                   null,
                   new HarmonyMethod(typeof(ManageWarperSpawns.ManageStoryEvents).GetMethod("PrecursorGunDisabledNotification"))
               );
            }

            // WarperSpawner.OnEnable (Pre)
            {
               harmony.Patch(
                   typeof(WarperSpawner).GetMethod("OnEnable"),
                   new HarmonyMethod(typeof(ManageWarperSpawns).GetMethod("GenericSkipExecutionPatch")),
                   null
               );
            }

            // WarperSpawner.Update (Pre)
            {
               harmony.Patch(
                  typeof(WarperSpawner).GetMethod("Update"),
                  new HarmonyMethod(typeof(ManageWarperSpawns).GetMethod("GenericSkipExecutionPatch")),
                  null
               );
            }
         }
         else
         {
            ErrorMessage.AddError("[NoWarpersAfterQuarantineShutdown] Harmony couldn't be initialized.");
         }
      }
   }
}
