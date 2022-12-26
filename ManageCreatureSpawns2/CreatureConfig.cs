using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace ManageCreatureSpawns2
{
    class CreatureConfig
    {
        public String name { get; set; }
        public ConfigEntry<bool> canSpawn;
        public ConfigEntry<int> spawnChance;
    }

    enum CreatureName
    {
        Biter,
        Biter_02,
        BirdBehaviour,
        Bladderfish,
        BladderFishSchool,
        Bleeder,
        BloomCreature,
        BoneShark,
        Boomerang,
        BoomerangFishSchool,
        BoomerangLava,
        CaveCrawler,
        CaveCrawler_03,
        CrabSnake,
        CrabSquid,
        Crash,
        CuteFish,
        Eyeye,
        EyeyeLava,
        Floater,
        Garryfish,
        GasoPod,
        GhostLeviatanVoid,
        GhostLeviathan,
        GhostLeviathanJuvenile,
        GhostRay,
        GhostRayBlue,
        GhostRayRed,
        Grabcrab,
        Grower,
        Holefish,
        HoleFishSchool,
        Hoopfish,
        HoopFish_02,
        HoopFish_02_School,
        HoopFishSchool,
        Hoverfish,
        Jellyray,
        Jumper,
        LavaLarva,
        LavaLizard,
        Leviathan,
        Mesmer,
        Oculus,
        OculusFish,
        Peeper,
        Precursor_Droid,
        RabbitRay,
        ReaperLeviathan,
        Reefback,
        ReefbackBaby,
        Reginald,
        RockGrub,
        SandShark,
        SeaDragon,
        SeaEmperorBaby,
        SeaEmperorJuvenile,
        SeaTreader,
        Shocker,
        Slime,
        Skyray,
        Spadefish,
        SpineEel,
        Stalker,
        Warper
    }
}
