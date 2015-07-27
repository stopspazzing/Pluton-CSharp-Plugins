using Pluton;
using Pluton.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PlutonEssentials
{
    public partial class PlutonEssentials : CSharpPlugin
    {
        public Dictionary<string, Structure> Structures;

        public void On_ServerInit()
        {
            Structures = new Dictionary<string, Structure>();
            LoadStructures();

            if (Plugin.IniExists("PlutonEssentials"))
            {
                IniParser Config = Plugin.GetIni("PlutonEssentials");
                float craft = float.Parse(Config.GetSetting("Config", "craftTimescale", "1.0"));
                Server.CraftingTimeScale = craft;
                float resource = float.Parse(Config.GetSetting("Config", "resourceGatherMultiplier", "1.0"));
                World.ResourceGatherMultiplier = resource;
                float time = float.Parse(Config.GetSetting("Config", "permanentTime", "-1"));
                if (time != -1f)
                {
                    World.Time = time;
                    World.FreezeTime();
                    return;
                }
                World.Timescale = float.Parse(Config.GetSetting("Config", "timescale", "30"));
            }
        }

        public void On_PluginInit()
        {
            Author = "Pluton Team";
            Version = "0.0.0.1 pre-alpha (early-access)[beta build]";
            About = "All non-core Pluton commands and functions all rolled into a plugin.";
            if (Plugin.IniExists("PlutonEssentials"))
            {
                Debug.Log("PlutonEssentials config loaded!");
            }
            else
            {
                IniParser ConfigFile = Plugin.CreateIni("PlutonEssentials");
                Debug.Log("PlutonEssentials config created!");
                Debug.Log("The config will be filled with the default values.");
                ConfigFile.AddSetting("Config", "craftTimescale", "1.0");
                ConfigFile.AddSetting("Config", "resourceGatherMultiplier", "1.0");

                ConfigFile.AddSetting("Config", "permanentTime", "-1");
                ConfigFile.AddSetting("Config", "timescale", "30.0");

                ConfigFile.AddSetting("Config", "broadcastInterval", "600000");
                ConfigFile.AddSetting("Config", "welcomeMessage", "true");

                ConfigFile.AddSetting("Config", "StructureRecorder", "false");

                ConfigFile.AddSetting("Commands", "ShowMyStats", "mystats");
                ConfigFile.AddSetting("Commands", "ShowStatsOther", "statsof");

                ConfigFile.AddSetting("Commands", "ShowLocation", "whereami");
                ConfigFile.AddSetting("Commands", "ShowOnlinePlayers", "players");

                ConfigFile.AddSetting("Commands", "Help", "Help");
                ConfigFile.AddSetting("Commands", "Commands", "Commands");

                ConfigFile.AddSetting("Commands", "Description", "whatis");
                ConfigFile.AddSetting("Commands", "Usage", "howto");
                ConfigFile.AddSetting("Commands", "About", "about");

                ConfigFile.AddSetting("Commands", "StartStructureRecording", "srstart");
                ConfigFile.AddSetting("Commands", "StopStructureRecording", "srstop");
                ConfigFile.AddSetting("Commands", "BuildStructure", "srbuild");

                ConfigFile.AddSetting("HelpMessage", "help_string0", "This is an empty help section");

                ConfigFile.AddSetting("BroadcastMessage", "msg0", "This server is powered by Pluton, the new servermod!");
                ConfigFile.AddSetting("BroadcastMessage", "msg1", "For more information visit our github repo: github.com/Notulp/Pluton or our forum: pluton-team.org");
                ConfigFile.Save();
            }
            IniParser GetConfig = Plugin.GetIni("PlutonEssentials");
            Commands.Register(GetConfig.GetSetting("Commands", "ShowMyStats")).setCallback(Mystats);
            Commands.Register(GetConfig.GetSetting("Commands", "ShowStatsOther")).setCallback(Statsof);
            Commands.Register(GetConfig.GetSetting("Commands", "ShowLocation")).setCallback(Whereami);
            Commands.Register(GetConfig.GetSetting("Commands", "ShowOnlinePlayers")).setCallback(Players);
            Commands.Register(GetConfig.GetSetting("Commands", "Help")).setCallback(Help);
            Commands.Register(GetConfig.GetSetting("Commands", "Commands")).setCallback(CommandS);
            Commands.Register(GetConfig.GetSetting("Commands", "Description")).setCallback(Whatis);
            Commands.Register(GetConfig.GetSetting("Commands", "Usage")).setCallback(Howto);
            Commands.Register(GetConfig.GetSetting("Commands", "About")).setCallback(AboutCMD);

            if (GetConfig.GetBoolSetting("Config", "StructureRecorder"))
            {
                Commands.Register(GetConfig.GetSetting("Commands", "StartStructureRecording")).setCallback(Srstart);
                Commands.Register(GetConfig.GetSetting("Commands", "StopStructureRecording")).setCallback(Srstop);
                Commands.Register(GetConfig.GetSetting("Commands", "BuildStructure")).setCallback(Srbuild);
            }

            DataStore.Flush("StructureRecorder");
            DataStore.Flush("StructureRecorderEveryone");

            if (Server.Loaded)
            {
                Structures = new Dictionary<string, Structure>();
                LoadStructures();
            }

            if (GetConfig.GetSetting("Config", "broadcastInterval") == null)
            {
                return;
            }

            int broadcast_time = int.Parse(GetConfig.GetSetting("Config", "broadcastInterval", "600000"));
            Plugin.CreateTimer("Advertise", broadcast_time);
        }

        public void On_PlayerConnected(Player player)
        {
            IniParser GetConfig = Plugin.GetIni("PlutonEssentials");
            if (GetConfig.GetBoolSetting("Config", "welcomeMessage", true))
            {
                player.Message("Welcome " + player.Name + "!");
                player.Message(String.Format("This server is powered by Pluton[v{0}]!", Pluton.Bootstrap.Version));
                player.Message("Visit pluton-team.org for more information or to report bugs!");
            }
        }

        public void On_CombatEntityHurt(CombatEntityHurtEvent cehe)
        {
            if (cehe.Attacker == null || cehe.Victim == null)
            {
                return;
            }

            Player player = cehe.Attacker.ToPlayer();
            BuildingPart bp = cehe.Victim.ToBuildingPart();
            if (player == null || bp == null)
            {
                return;
            }
            string name;

            if (DataStore.ContainsKey("StructureRecorder", player.SteamID))
            {
                name = (string)DataStore.Get("StructureRecorder", player.SteamID);
            }
            else if (DataStore.ContainsKey("StructureRecorderEveryone", "ON"))
            {
                name = (string)DataStore.Get("StructureRecorderEveryone", "ON");
            }
            else
            {
                return;
            }

            Structure structure;
            Structures.TryGetValue(name, out structure);
            if (structure == null)
            {
                return;
            }

            foreach (int dmg in Enumerable.Range(0, cehe.DamageAmounts.Length))
            {
                cehe.DamageAmounts[dmg] = 0f;
            }

            if (cehe.DamageType == Rust.DamageType.Bullet)
            {
                RecordAllConnected(structure, bp);
                player.Message("Added everything including connected parts");
                return;
            }
            structure.AddComponent(bp);
            player.Message("Added " + bp.Name);
        }

        public void On_Placement(BuildingEvent be)
        {
            Player player = be.Builder;
            string name;
            if (DataStore.ContainsKey("StructureRecorder", player.SteamID))
            {
                name = (string)DataStore.Get("StructureRecorder", player.SteamID);
            }
            else if (DataStore.ContainsKey("StructureRecorderEveryone", "ON"))
            {
                name = (string)DataStore.Get("StructureRecorderEveryone", "ON");
            }
            else
            {
                return;
            }
            BuildingPart bp = be.BuildingPart;
            Structure structure;
            Structures.TryGetValue(name, out structure);
            if (structure == null)
            {
                return;
            }

            structure.AddComponent(bp);
            player.Message("Added " + bp.Name);
        }
    }
}