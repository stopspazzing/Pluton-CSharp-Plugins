using System;
using Pluton;
using UnityEngine;

namespace EasyEntitySpawner
{
	//-->Created by Corrosion X
	//-->Version 0.8
	public class EasyEntitySpawner : CSharpPlugin
	{
		public void On_Plugininit()
		{
			Commands.Register ("spawnhere").setCallback ("spawnhere");
			Commands.Register("spawn").setCallback("spawn");
			Commands.Register ("spawnhelp").setCallback ("spawnhelp");
			Commands.Register("test").setCallback("test");
		}

		public void On_PlayerConnected(Player player)
		{
			player.Message ("Spawn in Entities and Animals! Type /spawnhelp for more help");
		}

		public void spawnhereCallback(String[] args, Player player)
		{
			string entity = args[0];
			int count;
			bool success = int.TryParse(args[1], out count);
			if (!success)
				count = 1;
			Vector3 loc = player.Location;
			spawnit(entity, loc, count);
		}

		public void spawnCallback(String[] args, Player player)
		{
			string entity = args[0];
			int count;
			bool success = int.TryParse(args[1], out count);
			if (!success)
				count = 1;
			Vector3 loc2 = player.Location;
			Vector3 lookpos = player.GetLookPoint();
			float dist = Util.GetVectorsDistance(loc2, lookpos);
			if (dist > 50.0)
			{
				player.Message("Distance is too far from your current location. Please look where you want it to spawn");
			}
			else
			{
				Vector3 loc = lookpos;
				spawnit(entity, loc, count);
			}
		}

		public BaseEntity spawnit(String entity, Vector3 loc, int count)
		{	
			BaseEntity newentity = GameManager.server.CreateEntity(entity, loc, default(Quaternion));
			if (newentity)
			{
				newentity.Spawn(true);
			}
			return newentity;
		}
	}
}