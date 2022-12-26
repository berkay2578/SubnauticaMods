using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace ManageCreatureSpawns2
{
    class ConfigService
    {
        public ConfigEntry<bool> configIsDebuggingEnabled { get; set; }
        public  ConfigEntry<bool> configIsCreatureListEnabled { get; set; }
        public Dictionary<string, CreatureConfig> unwantedCreatures { get; set; }

        private ConfigService()
        {

        }

        private static ConfigService instance = null;

        public static ConfigService Instance
        {
            get
            {
                if(instance == null)
                {
                    instance = new ConfigService();
                }
                return instance;
            }
        }



    }
}
