using Pluton;
using Pluton.Events;
using UnityEngine;
using System.Collections.Generic;
using JSON;


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
        const string example = "add/remove player name/steamid permission; add/remove group permission";

        public void On_PluginInit()
        {
            Author = "Pluton Team";
            About = "Control who can run which commands";
            Version = "0.3";
            IniParser settings = Plugin.CreateIni("Settings");
            if (settings != null)
            {
                settings["Settings"]["Debug"] = "false";
            }
            Commands.Register("permissions").setCallback(Permission).setDescription("Add or Remove people in permissions").setCommand(example);
            if (!Plugin.JsonFileExists("permissions"))
            {
                JSON.Object perms = new JSON.Object();
                JSON.Array players = new JSON.Array();
                JSON.Array groups = new JSON.Array();
                JSON.Array groupsPerms = new JSON.Array();
                JSON.Object jsonGroups = new JSON.Object();
                jsonGroups["name"] = new JSON.Value("default");
                jsonGroups["tag"] = new JSON.Value("[Default]");
                foreach (string command in dcommands)
                    groupsPerms.Add(new JSON.Value(command));
                jsonGroups["permissions"] = new JSON.Value(groupsPerms);
                jsonGroups["players"] = new JSON.Value("*");
                groups.Add(new JSON.Value(jsonGroups));
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
            var groups = JSON.Object.Parse(json_perms).GetObject("groups");
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

