using Pluton;
using UnityEngine;
using System;

namespace WhatPrefab
{
	public class WhatPrefab : CSharpPlugin
	{
		public void On_PluginInit()
		{
			Commands.Register("start").setCallback("start");
			Commands.Register("stop").setCallback("stop");
		}

		public void start(string[] args, Player player)
		{
			Vector3 lookpos = player.GetLookPoint ();
			foreach (BaseNetworkable bn in UnityEngine.Object.FindObjectsOfType<BaseNetworkable>()) { 
				if(lookpos == bn.transform.position){
					player.
				}
			}

		}

	}
}

