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
        private Dictionary<string, object> NoChatSpamDict;
        public bool Welcome;
        public bool NoChatSpam;
        public int NoChatSpamCooldown;
        public int NoChatSpamMaxMessages;
        public string[] BroadcastMessage;
        public string[] HelpMessage;
        public string[] WelcomeMessage;

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
                float time = float.Parse(Config.GetSetting("Config", "permanentTime", "-1.0"));
                if (time != -1.0f)
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
            Version = "0.1.2 (beta)";
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

                ConfigFile.AddSetting("Config", "permanentTime", "-1.0");
                ConfigFile.AddSetting("Config", "timescale", "30.0");

                ConfigFile.AddSetting("Config", "broadcastInterval", "600000");
                ConfigFile.AddSetting("Config", "BroadcastMessage", "true");
                ConfigFile.AddSetting("Config", "HelpMessage", "true");
                ConfigFile.AddSetting("Config", "WelcomeMessage", "true");

                ConfigFile.AddSetting("Config", "StructureRecorder", "false");
                ConfigFile.AddSetting("Config", "NoChatSpam", "true");
                ConfigFile.AddSetting("Config", "NoChatSpamCooldown", "2000");
                ConfigFile.AddSetting("Config", "NoChatSpamMaxMessages", "4");

                ConfigFile.AddSetting("Config", "Server_Image", "");
                ConfigFile.AddSetting("Config", "Server_Description", "This server is running Pluton Framework and is awesome!");
                ConfigFile.AddSetting("Config", "Server_Url", "");

                ConfigFile.AddSetting("Config", "PvE", "true");

                ConfigFile.AddSetting("Commands", "ShowMyStats", "mystats");
                ConfigFile.AddSetting("Commands", "ShowStatsOther", "statsof");

                ConfigFile.AddSetting("Commands", "ShowLocation", "whereami");
                ConfigFile.AddSetting("Commands", "ShowOnlinePlayers", "players");

                ConfigFile.AddSetting("Commands", "Help", "help");
                ConfigFile.AddSetting("Commands", "Commands", "commands");

                ConfigFile.AddSetting("Commands", "Description", "whatis");
                ConfigFile.AddSetting("Commands", "Usage", "howto");
                ConfigFile.AddSetting("Commands", "About", "about");

                ConfigFile.AddSetting("Commands", "StartStructureRecording", "srstart");
                ConfigFile.AddSetting("Commands", "StopStructureRecording", "srstop");
                ConfigFile.AddSetting("Commands", "BuildStructure", "srbuild");

                ConfigFile.AddSetting("HelpMessage", "1", "");
                ConfigFile.AddSetting("BroadcastMessage", "1", "");
                ConfigFile.AddSetting("WelcomeMessage", "1", "");

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

            //Broadcast settings
            if (GetConfig.GetBoolSetting("Config", "Broadcast", true))
            {
                int broadcast_time = int.Parse(GetConfig.GetSetting("Config", "broadcastInterval", "600000"));
                Plugin.CreateTimer("Advertise", broadcast_time);
                if (GetConfig.EnumSection("BroadcastMessage") != null)
                {
                    BroadcastMessage = GetConfig.EnumSection("BroadcastMessage");
                }
                else
                {
                    BroadcastMessage = new [] { "This server is powered by Pluton, the new servermod!", "For more information visit our github repo: github.com/Notulp/Pluton or our forum: pluton-team.org" };
                }
            }

            //Help Message
            if (GetConfig.GetBoolSetting("Config", "Help", true))
            {
                HelpMessage = GetConfig.EnumSection("HelpMessage");
            }

            //Welcome Message
            Welcome = GetConfig.GetBoolSetting("Config", "Welcome", true);
            if (Welcome)
            {
                WelcomeMessage = GetConfig.EnumSection("WelcomeMessage");
            }
            //NoChatSpam Global variable
            NoChatSpam = GetConfig.GetBoolSetting("Config", "NoChatSpam", true);

            //Set New Server Settings
            ConVar.Server.pve = GetConfig.GetBoolSetting("Config", "PvE", true);
            ConVar.Server.description = GetConfig.GetSetting("Config", "Server_Description");
            ConVar.Server.headerimage = GetConfig.GetSetting("Config", "Server_Image");
            ConVar.Server.url = GetConfig.GetSetting("Config", "Server_Url");
        }

        public void On_PlayerConnected(Player player)
        {
            if (Welcome)
            {
                if (WelcomeMessage != null)
                {
                    foreach (string arg in WelcomeMessage)
                    {
                        player.Message(arg);
                    }
                }
                else
                {
                    player.Message("Welcome " + player.Name + "!");
                    player.Message(String.Format("This server is powered by Pluton[v{0}]!", Pluton.Bootstrap.Version));
                    player.Message("Visit pluton-team.org for more information or to report bugs!");
                }
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

        public void On_Chat(ChatEvent ce)
        {
            if (NoChatSpam)
            {
                var player = ce.User.GameID;
                DataStore.Add("NoChatSpamMsgCount", player, +1);
                var count = (int)DataStore.Get("NoChatSpamMsgCount", player);
                if (count >= NoChatSpamMaxMessages)
                {
                    ce.Cancel("Stop spamming chat!");
                    return;
                }
                if (count == 1)
                {
                    NoChatSpamDict.Add("NoChatSpamPID", player);
                    Plugin.CreateParallelTimer("NoChatSpam", NoChatSpamCooldown, NoChatSpamDict);
                }
            }
        }
    }
}