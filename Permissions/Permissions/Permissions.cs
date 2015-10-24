using System.Collections.Generic;
using JSON;
using Pluton;
using Pluton.Events;
using UnityEngine;

namespace Permissions
{
    public class Permissions : CSharpPlugin
    {
        bool debug;
        string json_perms;
        string[] dcommands = { "players", "help", "whois", "whatis", "whereami", "howto", "commands" };

        //default commands for PlutonEssentials
        const string noperms = "You Don't Have Permissions For That!";

        //Displayed when player doesn't have permissions.
        const string example = "add/remove player/group name/steamid permission";

        public void On_PluginInit()
        {
            Author = "Pluton Team";
            About = "Control who can run which commands";
            Version = "0.4";
            IniParser settings = Plugin.CreateIni("Settings");
            if (settings != null)
            {
                settings["Settings"]["Debug"] = "false";
            }
            Commands.Register("permissions").setCallback(Permission).setDescription("Add or Remove people in permissions").setCommand(example);
            if (!Plugin.JsonFileExists("permissions"))
            {
                //{"players":{"734583538534583":["godmode","teleport"],"734582394234223":["jail","pizza","yolo"]},"groups":{"Default":{"name":"default","tag":"[Default]","permissions":["players","help","whois","whatis","whereami","howto","commands"],"players":["*"]},"Admins":{"name":"Admins","tag":"[Admins]","permissions":["godmode","pikachu","squirdle"],"players":["734583538534583","73423422334583"]}}}
                JSON.Object perms = new JSON.Object();

                //Top level
                JSON.Object players = new JSON.Object();
                JSON.Object groups = new JSON.Object();

                // Groups 2nd level (Players is default empty)
                JSON.Object groupsdefault = new JSON.Object();
                JSON.Object groupsadmin = new JSON.Object();
                JSON.Object groupsmod = new JSON.Object();
                JSON.Object groupsdon = new JSON.Object();

                // Groups 3 level - Default
                JSON.Array groupsdefperms = new JSON.Array();
                JSON.Array groupsdefplayers = new JSON.Array();
                groupsdefault["name"] = new JSON.Value("default");
                groupsdefault["tag"] = new JSON.Value("[Default]");
                foreach (string command in dcommands)
                    groupsdefperms.Add(new JSON.Value(command));
                groupsdefault["permissions"] = new JSON.Value(groupsdefperms);
                groupsdefplayers.Add(new JSON.Value("*"));
                groupsdefault["players"] = new JSON.Value(groupsdefplayers);

                // Groups 3 level - Admin
                JSON.Array groupsadmperms = new JSON.Array() { "ban", "kick", "mute" };
                JSON.Array groupsadmplayers = new JSON.Array();
                groupsadmin["name"] = new JSON.Value("admin");
                groupsadmin["tag"] = new JSON.Value("[Admin]");
                //groupsadmperms.Add(new JSON.Value(groupsadmperms));
                groupsadmin["permissions"] = new JSON.Value(groupsadmperms);
                groupsadmplayers.Add(new JSON.Value(""));
                groupsadmin["players"] = new JSON.Value(groupsadmplayers);

                // Groups 3 level - Moderator
                JSON.Array groupsmodperms = new JSON.Array() { "kick", "mute" };
                JSON.Array groupsmodplayers = new JSON.Array();
                groupsmod["name"] = new JSON.Value("moderator");
                groupsmod["tag"] = new JSON.Value("[Mod]");
                //groupsadmperms.Add(new JSON.Value(groupsmodperms));
                groupsmod["permissions"] = new JSON.Value(groupsmodperms);
                groupsmodplayers.Add(new JSON.Value(""));
                groupsmod["players"] = new JSON.Value(groupsmodplayers);

                // Groups 3 level - Donator
                JSON.Array groupsdonperms = new JSON.Array() { "funstuff", "extrakits" };
                JSON.Array groupsdonplayers = new JSON.Array();
                groupsdon["name"] = new JSON.Value("donator");
                groupsdon["tag"] = new JSON.Value("[Donator]");
                //groupsdonperms.Add(new JSON.Value(groupsdonperms));
                groupsdon["permissions"] = new JSON.Value(groupsdonperms);
                groupsdonplayers.Add(new JSON.Value(""));
                groupsdon["players"] = new JSON.Value(groupsdonplayers);

                //Combine all groups into main
                groups["Default"] = new JSON.Value(groupsdefault);
                groups["Admin"] = new JSON.Value(groupsadmin);
                groups["Moderator"] = new JSON.Value(groupsmod);
                groups["Donator"] = new JSON.Value(groupsmod);

                //Finish up by combining players and groups into 1 and stringify
                perms["players"] = new JSON.Value(players);
                perms["groups"] = new JSON.Value(groups);
                string json = perms.ToString();
                Plugin.ToJsonFile("permissions", json);
            }
            IniParser getsettings = Plugin.GetIni("Settings");
            debug = getsettings["Settings"]["Debug"].ToLower() == "true";
            json_perms = Plugin.FromJsonFile("permissions");
        }

        public void On_CommandPermission(CommandPermissionEvent cpe)
        {
            if (debug)
            {
                if (!HasPermission(cpe.User.GameID, cpe.Cmd))
                {
                    Debug.Log(cpe.User + " with steamid " + cpe.User.GameID + " attempted to executed command " + cpe.ChatCommand);
                }
                return;
            }
        }

        public void Permission(string[] args, Player player)
        {
            //var groups = JSON.Object.Parse(json_perms).GetObject("groups");
            var players = JSON.Object.Parse(json_perms).GetObject("players");

            if (HasPermission(player.GameID, "permission"))
            {
                if (args.Length >= 3)
                {
                    //Add Permissions
                    if (args[0].ToLower() == "add")
                    {
                        //addto players
                        if (args[1].ToLower() == "player")
                        {
                            //get player
                            Player oplayer = Player.Find(string.Join(" ", args[2]));
                            if (oplayer != null)
                            {
                                var pID = oplayer.GameID.ToString();
                                if (!players.ContainsKey(pID))
                                {
                                    //players[-1].Array[-1] = pID;
                                    players[pID].Array[0] = oplayer.Name; 
                                    players[pID].Obj["Groups"].Array[0] = "Default";
                                    players[pID].Obj["Permissions"].Array[0] = null;
                                }
                                if (args[2].ToLower() == "groups")
                                {
                                    if (args[3] != null)
                                    {
                                        //need to add check if already contains value in array
                                        foreach (Value array in players[pID].Obj["Groups"].Array)
                                        {
                                            if (array.ToString() == args[3])
                                            {
                                                player.Message("Error: This Already Contains" + args[3]);
                                                return;
                                            }
                                        }
                                        players[pID].Obj["Groups"].Array[-1] = args[3];
                                        player.Message("The Permissions group:" + args[3] + " has been added to player: " + oplayer.Name);
                                        return;
                                    }


                                }

                            }
                            player.Message("Can't find player: " + string.Join(" ", args));
                            return;
                        }
                        //create/addto groups
                        if (args[1].ToLower() == "group")
                        {

                        }
                    }
                    //Remove Permissions
                    if (args[0] == "remove")
                    {

                    }
                }
                else
                {
                    player.Message(example);
                }
            }
        }

        public bool HasPermission(ulong steamid, string permission)
        {
            var player = Player.FindByGameID(steamid);
            if (player.Admin)
                return true;
            var steam = steamid.ToString();
            var defgroup = JSON.Object.Parse(json_perms).GetObject("Groups").GetObject("Default");
            var players = JSON.Object.Parse(json_perms).GetObject("Players");
            if (players.ContainsKey(steam))
            {
                var perms = players.GetObject(steam).GetArray("Permissions");
                foreach (Value json in perms)
                {
                    if (json.ToString() == permission)
                        return true;
                }
            }
            foreach (KeyValuePair<string, Value> json in defgroup)
            {
                if (json.Key.Contains(permission))
                    return true;
            }
            player.Message(noperms);
            return false;
        }
    }
}

