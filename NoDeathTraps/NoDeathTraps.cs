using System;
using Pluton;
using ProtoBuf;

namespace NoDeathTraps
{
    public class NoDeathTraps : CSharpPlugin
    {
        public void On_PluginInit()
        {
            Author = "Corrosion X";
            Version = "0.1";
            About = "Prevent traps from being set off by their owners.";
            IniParser ini = Plugin.CreateIni();
            ini.AddSetting("Settings", "something","or something");
        }

        public void On_Placement()
        {
            
            
        }
    }
}