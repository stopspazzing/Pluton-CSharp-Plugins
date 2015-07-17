using Pluton;
using Pluton.Events;
using System;
using UnityEngine;

namespace Permissions
{
    public class Permissions : CSharpPlugin
    {
        public string Author = "Pluton Team";
        public string About = "Control who can run which commands";
        public string Version = "0.1";

        public void On_PluginInit()
        {
            IniParser settings = Plugin.CreateIni("Settings");
            if (settings != null){
                settings.AddSetting("Settings","Debug", "false");
            }
            Commands.Register("permissions").callback(Permission);
        }

        public void On_CommandPermission(CommandPermissionEvent cpe){
            if (cpe.User != cpe.User.Admin){
                //IniParser perm = Plugin.GetIni("Permissions");
                IniParser settings = Plugin.GetIni("Settings");
                bool debug = settings.GetBoolSetting("Settings", "Debug");
                //check permissions here
                if(!hasPermission){
                    cpe.BlockCommand("You Don't Have Permissions For That!");
                    return;
                }
                if(debug){
                    Debug.Log(cpe.User + " with steamid " + cpe.User.GameID + " attempted to executed command " + cpe.chatCommand);
                }    
            }
        }

        public void Permission(string[] args, Player player){
            if(player.Admin || hasPermission()){
                if (args.Length == 3){
                    

                }
            }
        }

        public bool hasPermission(ulong steamid){
            var player = Player.FindByGameID(steamid);
            //check return
        }
    }
}

