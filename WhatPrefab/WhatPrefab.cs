using Pluton;
using UnityEngine;

namespace WhatPrefab
{
	public class WhatPrefab : CSharpPlugin
	{
		const string _creator = "Corrosion X";
		const string _version = "0.1";
		public void On_PluginInit()
		{
			Commands.Register("start").setCallback("start");
			Commands.Register("stop").setCallback("stop");
		}

		public void start(string[] args, Player player)
		{
			Vector3 lookpos = player.GetLookPoint ();
			foreach (BaseNetworkable bn in UnityEngine.Object.FindObjectsOfType<BaseNetworkable>()) 
			{ 
				if(lookpos == bn.transform.position)
				{
					//use raycast
					player.Message("That prefab is: " + bn.name);
				}
			}
		}
	}
}

