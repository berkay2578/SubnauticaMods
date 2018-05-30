using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;

using Harmony;
using SafeAutosave.SettingsManager;

namespace SafeAutosave {
   public static class Patcher {
      public static void log(string logMessage, bool displayToUser = false, params string[] arg) {
         string _toBePrinted = "[SafeAutosave] " + logMessage;
         if (arg.Length > 0)
            _toBePrinted = String.Format(_toBePrinted, arg);

         Console.WriteLine(_toBePrinted);
         if (displayToUser)
            ErrorMessage.AddDebug(_toBePrinted);
      }

      public static SettingsManager.Settings settings = null;

      public static class SaveManager {
         public static System.Timers.Timer retryDummy = new System.Timers.Timer() {
            Interval = 4000,
            Enabled = false
         };
         public static int retryDummy_TryCount = 3;
         public static float retryDummy_LastPauseIntervalInSeconds = 10.0f;

         public static float timeOfLastSave = 0.0f;

         public static MethodInfo mi_fnGetAllowSaving = null;
         public static MethodInfo mi_fnSaveGameAsync = null;
         public static void ForceSave(float pauseIntervalInSeconds = 10.0f, bool isCalledFromTimer = false) {
            pauseIntervalInSeconds = pauseIntervalInSeconds < 10.0f ? 10.0f : pauseIntervalInSeconds;
            if ((UnityEngine.Time.timeSinceLevelLoad - pauseIntervalInSeconds) < 0.0f
               || (UnityEngine.Time.timeSinceLevelLoad - timeOfLastSave) < 10.0f)
            {
               log("Not enough time has passed to save, skipping autosave!", true);
               return;
            }

            if (mi_fnGetAllowSaving == null)
            {
               log("mi_fnGetAllowSaving is null, loading MethodInfo.");
               mi_fnGetAllowSaving = typeof(IngameMenu).GetMethod("GetAllowSaving", BindingFlags.NonPublic | BindingFlags.Instance);
               if (mi_fnGetAllowSaving == null)
               {
                  log("mi_fnGetAllowSaving is still null, skipping autosave!");
                  ErrorMessage.AddDebug("[SafeAutosave] Could not autosave.");
                  return;
               }
            }
            if (mi_fnSaveGameAsync == null)
            {
               log("mi_fnSaveGameAsync is null, loading MethodInfo.");
               mi_fnSaveGameAsync = typeof(IngameMenu).GetMethod("SaveGameAsync", BindingFlags.NonPublic | BindingFlags.Instance);
               if (mi_fnSaveGameAsync == null)
               {
                  log("mi_fnSaveGameAsync is still null, skipping autosave!");
                  ErrorMessage.AddDebug("[SafeAutosave] Could not autosave.");
                  return;
               }
            }

            log("Saving...");
            if (!(bool)mi_fnGetAllowSaving.Invoke(IngameMenu.main, null))
            {
               log("IngameMenu.main.GetAllowSaving() returned false.");
               if (!isCalledFromTimer)
               {
                  log("Game does not allow saving currently, trying every 4 seconds for 3 times.", true);
                  retryDummy_TryCount = 3;
                  retryDummy_LastPauseIntervalInSeconds = pauseIntervalInSeconds;

                  retryDummy.Elapsed += trySavingAgain;
                  retryDummy.Start();
                  ;
               } else
               {
                  log("Try {0} failed.", false, retryDummy_TryCount.ToString());
                  retryDummy_TryCount--;
               }

               return;
            }
            if (isCalledFromTimer)
            {
               retryDummy.Stop();
               retryDummy.Elapsed -= trySavingAgain;
            }

            UWE.CoroutineHost.StartCoroutine((IEnumerator)mi_fnSaveGameAsync.Invoke(IngameMenu.main, null));
            timeOfLastSave = UnityEngine.Time.timeSinceLevelLoad;
            log("Saved successfully!");
         }
         public static void trySavingAgain(object sender, System.Timers.ElapsedEventArgs e) {
            if (retryDummy_TryCount > 0)
            {
               ForceSave(retryDummy_LastPauseIntervalInSeconds, true);
            } else
            {
               log("Failed after trying for 3 times.", true);
               retryDummy.Stop();
            }
         }

         [HarmonyPostfix]
         [HarmonyPriority(Priority.Last)]
         public static void SaveOnEntry(SubRoot __instance) {
            if (UnityEngine.Time.timeSinceLevelLoad < 10.0f)
               return; // Save game load

            if (__instance != null)
            {
               if (__instance.isBase && settings.PlayerBase.SaveOnEntry)
               {
                  if (!settings.PlayerBase.SaveEvenWhenFloodedOrDamaged && __instance.IsLeaking())
                  {
                     log("Base is leaking and settings.PlayerBase.SaveEvenWhenFloodedOrDamaged is set to false, skipping autosave!");
                     return;
                  }

                  ForceSave(settings.PlayerBase.PauseIntervalInSeconds);
               } else if (__instance.isCyclops && settings.Cyclops.SaveOnEntry)
               {
                  // Get SubRoot.live
                  LiveMixin live = null;
                  {
                     FieldInfo f_live = __instance.GetType().GetField("live", BindingFlags.NonPublic | BindingFlags.Instance);
                     var v_live = f_live?.GetValue(__instance);
                     live = v_live == null ? (LiveMixin)v_live : null;
                  }
                  if (live == null)
                  {
                     log("Could not get SubRoot.live on Cyclops entry, skipping autosave!");
                     return;
                  }
                  if (!settings.Cyclops.SaveEvenWhenFloodedOrDamaged
                     && (__instance.IsLeaking() || live.GetHealthFraction() < 1.0f))
                  {
                     log("Cyclops is leaking and/or is damaged, and settings.Cyclops.SaveEvenWhenFloodedOrDamaged is set to false, skipping autosave!");
                     return;
                  }

                  ForceSave(settings.Cyclops.PauseIntervalInSeconds);
               }
            }
         }

         [HarmonyPostfix]
         [HarmonyPriority(Priority.Last)]
         public static void SaveOnExit(SubRoot __instance) {
            if (__instance != null)
            {
               if (__instance.isBase && settings.PlayerBase.SaveOnExit)
               {
                  if (!settings.PlayerBase.SaveEvenWhenFloodedOrDamaged && __instance.IsLeaking())
                  {
                     log("Base is leaking and settings.PlayerBase.SaveEvenWhenFloodedOrDamaged is set to false, skipping autosave!");
                     return;
                  }

                  ForceSave();
               } else if (__instance.isCyclops && settings.Cyclops.SaveOnExit)
               {
                  // Get SubRoot.live
                  LiveMixin live = null;
                  {
                     FieldInfo f_live = __instance.GetType().GetField("live", BindingFlags.NonPublic | BindingFlags.Instance);
                     var v_live = f_live?.GetValue(__instance);
                     live = v_live == null ? (LiveMixin)v_live : null;
                  }
                  if (live == null)
                  {
                     log("Could not get SubRoot.live on Cyclops exit, skipping autosave!");
                     return;
                  }
                  if (!settings.Cyclops.SaveEvenWhenFloodedOrDamaged
                     && (__instance.IsLeaking() || live.GetHealthFraction() < 1.0f))
                  {
                     log("Cyclops is leaking and/or is damaged, and settings.Cyclops.SaveEvenWhenFloodedOrDamaged is set to false, skipping autosave!");
                     return;
                  }

                  ForceSave();
               }
            }
         }
      }

      public static void ApplyPatches() {
         log("Loading... v{0}", false, Assembly.GetExecutingAssembly().GetName().Version.ToString());

         HarmonyInstance harmony = HarmonyInstance.Create("mod.berkay2578.safeautosave");
         if (harmony != null)
         {
            log("HarmonyInstance created.");

            log("Reading settings.");
            {
               XmlSerializer serializer = new XmlSerializer(typeof(SettingsManager.Settings));
               using (StreamReader reader = new StreamReader("QMods\\SafeAutosave\\Settings.xml"))
                  settings = (SettingsManager.Settings)serializer.Deserialize(reader);
               serializer = null;

               if (settings == null)
               {
                  log("Could not load settings, exiting.");
                  return;
               }
            }

            {
               harmony.Patch(
                   typeof(SubRoot).GetMethod("OnPlayerEntered", BindingFlags.Public | BindingFlags.Instance),
                   null,
                   new HarmonyMethod(typeof(SaveManager).GetMethod("SaveOnEntry"))
               );
               log("Patched SubRoot.OnPlayerEntered");

               harmony.Patch(
                   typeof(SubRoot).GetMethod("OnPlayerExited", BindingFlags.Public | BindingFlags.Instance),
                   null,
                   new HarmonyMethod(typeof(SaveManager).GetMethod("SaveOnExit"))
               );
               log("Patched SubRoot.OnPlayerExited");
            }

            log("Patched successfully.");
         } else
         {
            log("HarmonyInstance() returned null.");
         }
      }
   }
}
