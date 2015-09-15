using System;
using Pluton;
using Pluton.Events;
using ProtoBuf;
using UnityEngine;

namespace NoDeathTraps
{
    public class NoDeathTraps : CSharpPlugin
    {
        public bool isTrap;
        public bool isOwner;
        public bool isFriend;
        //public BaseEntity creatorEntity;

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

        public void On_PlayerHurt(PlayerHurtEvent phe)
        {
            if (!phe.Attacker.IsPlayer())
            {
                if(phe.Attacker.baseEntity.creatorEntity.ToPlayer().userID == phe.Victim.GameID)
                {
                    phe.DamageAmounts = null;
                }
            }
        }
    }
}