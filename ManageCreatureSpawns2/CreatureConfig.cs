using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace ManageCreatureSpawns2
{
    class CreatureConfig
    {
        public String name { get; set; }
        public bool canSpawn { get; set; }
        public int spawnChance { get; set; }
    }
}
