using System;
using System.Xml.Serialization;

namespace SafeAutosave {
   namespace SettingsManager {
      [Serializable()]
      [XmlRoot("SaveConfiguration")]
      public class SaveConfiguration {
         [XmlElement("SaveOnEntry")]
         public bool SaveOnEntry { get; set; } = true;

         [XmlElement("SaveOnExit")]
         public bool SaveOnExit { get; set; } = true;

         [XmlElement("SaveEvenWhenFloodedOrDamaged")]
         public bool SaveEvenWhenFloodedOrDamaged { get; set; } = false;

         [XmlElement("PauseIntervalInSeconds")]
         public float PauseIntervalInSeconds = 10.0f;
      }

      [Serializable()]
      [XmlRoot("Settings")]
      [XmlInclude(typeof(SaveConfiguration))]
      public class Settings {
         [XmlElement("PlayerBase")]
         public SaveConfiguration PlayerBase { get; set; } = new SaveConfiguration();

         [XmlElement("Cyclops")]
         public SaveConfiguration Cyclops { get; set; } = new SaveConfiguration();
      }
   }
}