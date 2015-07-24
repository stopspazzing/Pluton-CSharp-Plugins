using Pluton;
using UnityEngine;
using System.Collections.Generic;
using System;
using System.Threading;

namespace WhatPrefab
{
	public class WhatPrefab : CSharpPlugin
	{
		public void On_PluginInit()
		{
            About = "";
            Author = "Corrosion X";
            Version = "0.1";
			Commands.Register("start").setCallback(Start);
			Commands.Register("stop").setCallback(Stop);
		}

        public void Start(string[] args, Player player)
        {
            var dict = Plugin.CreateDict();
            dict.Add("gid", player.GameID);
            Plugin.CreateParallelTimer("Timer_",5000,dict).Start();
		}

        public void Stop(string[] args, Player player)
        {
            try
            {
                CommunityEntity.ServerInstance.ClientRPCEx(new Network.SendInfo() 
                    { 
                        connection = player.basePlayer.net.connection 
                    }, null, "DestroyUI", "testpanel7766");
            }
            catch(ArgumentNullException){

            }

            if(Plugin.ParallelTimers != null)
            {
                foreach(var t in Plugin.ParallelTimers )
                {
                    if (t.Args.ContainsValue(player.GameID.ToString()))
                    { 
                        t.Kill();
                    }
                }
            }
        }

        public void Timer_Callback(TimerCallback timer)
        {
            var gid = timer.GetFieldValue("gid");
            var player = Server.Players[gid];
            try
            {
                CommunityEntity.ServerInstance.ClientRPCEx(new Network.SendInfo() 
                { 
                    connection = player.basePlayer.net.connection 
                }, null, "DestroyUI", "testpanel7766");
            }
            catch(ArgumentNullException){
                
            }
            var commui = new PlutonUIEntity(player.basePlayer.net.connection);
            var testpanel7766 = commui.AddPanel(
                "TestPanel7766",
                "Overlay"
            );
            testpanel7766.AddComponent (new Pluton.PlutonUI.RectTransform(){
                anchormin = "0 0",
                anchormax = "1 1"
            });

            var nonamepanel = commui.AddPanel (null, "TestPanel7766");
            nonamepanel.AddComponent (new Pluton.PlutonUI.RectTransform(){
                anchormin = "0 0.5",
                anchormax = "1 0.9"
            });
            nonamepanel.AddComponent (new Pluton.PlutonUI.Text(){
                text = "Prefab Name",
                fontSize = 20,
                align = "TopCenter"
            });
            Vector3 loc = player.GetLookPoint();
            RaycastHit[] hit = Physics.RaycastAll(loc, Vector3.down);
            foreach (RaycastHit x in hit)
            {
                if (x.collider.gameObject.ToBaseEntity() != null)
                {
                    nonamepanel.AddComponent (new Pluton.PlutonUI.Text () {
                        text = x.collider.gameObject.ToBaseEntity().name,
                        fontSize = 20,
                        align = "MiddleCenter"
                    });
                }
            }
            commui.CreateUI();
        }
	}
}

