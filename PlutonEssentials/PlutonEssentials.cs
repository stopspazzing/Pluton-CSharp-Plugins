using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Pluton;
using Pluton.Events;
using UnityEngine;

namespace PlutonEssentials
{
    public partial class PlutonEssentials : CSharpPlugin
    {
        public Dictionary<string, Structure> Structures;
        public bool Welcome;
        public bool NoChatSpam;
        public bool NoChatSpamIgnoreMod;
        public int NoChatSpamCooldown;
        public int NoChatSpamMaxMessages;
        public List<string> BroadcastMessage = new List<string>();
        public List<string> HelpMessage = new List<string>();
        public List<string> WelcomeMessage = new List<string>();
        public bool UpdateAvailable;
        private const string latest = "http://dl.pluton-team.org/latest.json";
        private const string stable = "http://dl.pluton-team.org/stable.json";
        private const string apikey = "";
        private const string apiurl = "http://api.steampowered.com/ISteamUser/GetPlayerBans/v1/?key={0}&steamids={1}";
        public bool NoVacBans;
        public int MaxVacBans;
        private bool StructureRecorder;
        private const string gcmAuth = "";

        public void On_ServerInit()
        {
            ServicePointManager.ServerCertificateValidationCallback = (sendersendersendersender, certificatecertificatecertificatecertificate, chainchainchainchain, sslPolicyErrorssslPolicyErrorssslPolicyErrorssslPolicyErrors) =>
            true;
            Structures = new Dictionary<string, Structure>();
            LoadStructures();
            DataStore.Flush("NoChatSpamMsgCount");
            DataStore.Flush("NoChatSpamTimeStamp");
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
            Version = "0.1.4 (beta)";
            About = "All non-core Pluton commands and functions all rolled into a plugin.";

            //Create Config if doesnt exist else load it
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
                ConfigFile.AddSetting("Config", "Broadcast", "true");
                ConfigFile.AddSetting("Config", "Help", "true");
                ConfigFile.AddSetting("Config", "Welcome", "true");

                ConfigFile.AddSetting("Config", "StructureRecorder", "false");

                ConfigFile.AddSetting("Config", "Server_Image", "");
                ConfigFile.AddSetting("Config", "Server_Description", "This server is running Pluton Framework and is awesome!");
                ConfigFile.AddSetting("Config", "Server_Url", "");

                ConfigFile.AddSetting("Config", "NoChatSpam", "true");
                ConfigFile.AddSetting("Config", "NoVacBans", "false");
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

                ConfigFile.AddSetting("BroadcastMessage", "1", "This server is powered by Pluton, the new servermod!");
                ConfigFile.AddSetting("BroadcastMessage", "2", "For more information visit our github repo: github.com/Notulp/Pluton or our forum: pluton-team.org");

                ConfigFile.AddSetting("WelcomeMessage", "1", "This server is powered by Pluton!");
                ConfigFile.AddSetting("WelcomeMessage", "2", "Visit pluton-team.org for more information or to report bugs!");

                ConfigFile.AddSetting("NoChatSpam", "NoChatSpamCooldown", "2000");
                ConfigFile.AddSetting("NoChatSpam", "NoChatSpamMaxMessages", "4");
                ConfigFile.AddSetting("NoChatSpam", "NoChatSpamIgnoreMod", "false");

                ConfigFile.AddSetting("NoVacBans", "MaxAllowed", "2");

                ConfigFile.Save();
            }

            IniParser GetConfig = Plugin.GetIni("PlutonEssentials");
            //Set Callbacks for commands
            Commands.Register(GetConfig.GetSetting("Commands", "ShowMyStats")).setCallback(Mystats);
            Commands.Register(GetConfig.GetSetting("Commands", "ShowStatsOther")).setCallback(Statsof);
            Commands.Register(GetConfig.GetSetting("Commands", "ShowLocation")).setCallback(Whereami);
            Commands.Register(GetConfig.GetSetting("Commands", "ShowOnlinePlayers")).setCallback(Players);
            Commands.Register(GetConfig.GetSetting("Commands", "Help")).setCallback(Help);
            Commands.Register(GetConfig.GetSetting("Commands", "Commands")).setCallback(CommandS);
            Commands.Register(GetConfig.GetSetting("Commands", "Description")).setCallback(Whatis);
            Commands.Register(GetConfig.GetSetting("Commands", "Usage")).setCallback(Howto);
            Commands.Register(GetConfig.GetSetting("Commands", "About")).setCallback(AboutCMD);

            //Structure Recorder
            StructureRecorder = bool.Parse(GetConfig["Config"]["StructureRecorder"]) == false;
            if (StructureRecorder)
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
            if (GetConfig.GetBoolSetting("Config", "Broadcast"))
            {
                int broadcast_time = int.Parse(GetConfig.GetSetting("Config", "broadcastInterval", "600000"));
                Plugin.CreateTimer("Advertise", broadcast_time);
                foreach (string key in GetConfig.EnumSection("BroadcastMessage"))
                {
                    if (key == null)
                    {
                        return;
                    }
                    BroadcastMessage.Add(GetConfig.GetSetting("BroadcastMessage", key));
                }
            }

            //Help Message
            if (GetConfig.GetBoolSetting("Config", "Help", true))
            {
                foreach (string key in GetConfig.EnumSection("HelpMessage"))
                {
                    if (key == null)
                    {
                        return;
                    }
                    HelpMessage.Add(GetConfig.GetSetting("HelpMessage", key));
                }
            }

            //Welcome Message
            Welcome = GetConfig.GetBoolSetting("Config", "Welcome", true);
            if (Welcome)
            {
                foreach (string key in GetConfig.EnumSection("WelcomeMessage"))
                {
                    if (key == null)
                    {
                        return;
                    }
                    WelcomeMessage.Add(GetConfig.GetSetting("WelcomeMessage", key));
                }
            }

            //NoChatSpam Global variable
            NoChatSpam = GetConfig.GetBoolSetting("Config", "NoChatSpam", true);
            NoChatSpamCooldown = int.Parse(GetConfig.GetSetting("NoChatSpam", "NoChatSpamCooldown", "2000"));
            NoChatSpamMaxMessages = int.Parse(GetConfig.GetSetting("NoChatSpam", "NoChatSpamMaxMessages", "4"));
            NoChatSpamIgnoreMod = GetConfig.GetBoolSetting("NoChatSpam", "NoChatSpamIgnoreMod");

            //Set New Server Settings
            ConVar.Server.pve = GetConfig.GetBoolSetting("Config", "PvE", true);
            ConVar.Server.description = GetConfig.GetSetting("Config", "Server_Description");
            ConVar.Server.headerimage = GetConfig.GetSetting("Config", "Server_Image");
            ConVar.Server.url = GetConfig.GetSetting("Config", "Server_Url");

            //Check for updates
            var getjson = JSON.Object.Parse(Web.GET(latest)).GetString("plutonVersion");
            var getver = Pluton.Bootstrap.Version;
            if (getjson != getver)
            {
                UpdateAvailable = true;
            }

            //NoVacBans
            NoVacBans = GetConfig.GetBoolSetting("Config", "NoVacBans", false);
            MaxVacBans = int.Parse(GetConfig.GetSetting("NoVacBans", "MaxVacBans", "2"));
        }

        public void On_PlayerConnected(Player player)
        {
            if (Welcome)
            {
                player.Message("Welcome " + player.Name + "!");
                foreach (string arg in WelcomeMessage)
                {
                    if (arg != null)
                    {
                        player.Message(arg);
                    }
                }
            }
            if (player.Admin && UpdateAvailable)
            {
                player.Message("There is an update for Pluton available!");
            }
            else if (player.Admin)
            {
                player.Message("There is no update for Pluton available.");
            }

            if (NoVacBans && !player.Admin)
            {
                var pID = player.GameID;
                try
                {
                    var getjson = JSON.Object.Parse(Web.GET(string.Format(apiurl, apikey, pID))).GetObject("players").GetInt("NumberOfVACBans");
                    var count = getjson;
                    if (count >= MaxVacBans)
                    {
                        player.Kick("You have too many Vac Bans!");
                    }
                    //debug
                    Server.BroadcastFrom("NoVacBans", "Player " + player.Name + " has " + count + " on file.");
                }
                catch (Exception e)
                {
                    Logger.LogException(e);
                }
            }
        }

        public void On_CombatEntityHurt(CombatEntityHurtEvent cehe)
        {
            if (StructureRecorder)
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
        }

        public void On_Placement(BuildingEvent be)
        {
            if (StructureRecorder)
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

        public void On_Chat(ChatEvent ce)
        {
            if (NoChatSpam && !ce.User.Admin)
            {
                if (NoChatSpamIgnoreMod && ce.User.Moderator)
                {
                    return;
                }
                try
                {
                    var player = ce.User.GameID;
                    var time = Plugin.GetTimestamp();
                    if (!DataStore.ContainsKey("NoChatSpamMsgCount", player))
                    {
                        Server.Broadcast("player spam count set to 0");
                        DataStore.Add("NoChatSpamMsgCount", player, 0);
                        DataStore.Add("NoChatSpamTimeStamp", player, time);
                    }
                    var count = (int)DataStore.Get("NoChatSpamMsgCount", player);
                    var stamp = (float)DataStore.Get("NoChatSpamTimeStamp", player);
                    var cooldown = (int)DataStore.Get("NoChatSpamCooldown", player);
                    DataStore.Add("NoChatSpamMsgCount", player, count + 1);
                    Server.Broadcast("player count + 1");
                    if (count >= NoChatSpamMaxMessages && stamp + cooldown > time)
                    {
                        ce.Cancel();
                        ce.User.MessageFrom("NoChatSpam", "Stop spamming chat!");
                        return;
                    }
                    DataStore.Remove("NoChatSpamMsgCount", player);
                    DataStore.Remove("NoChatSpamTimeStamp", player);
                }
                catch (Exception e)
                {
                    Logger.LogException(e);
                }
            }
            if (ce.Arg.ArgsStr.ToLower().Contains("admin"))
            {
                ce.User.Message("Contacting an admin.");
                var test = @"{""data"":{""message"":""{0}""},""to"":""/topics/global""}";
                byte[] requestData = Encoding.UTF8.GetBytes(test.Replace("{0}", ce.Arg.ArgsStr));
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://android.googleapis.com/gcm/send");
                request.Method = "POST";
                request.Headers.Add("Authorization", "key=" + gcmAuth);
                request.ContentType = "application/json";
                using (Stream st = request.GetRequestStream())
                    st.Write(requestData, 0, requestData.Length);
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    reader.ReadToEnd();
                }
            }
        }
    }
}