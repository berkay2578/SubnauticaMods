using System;
using System.Collections.Generic;
using System.Xml.Serialization;

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
}