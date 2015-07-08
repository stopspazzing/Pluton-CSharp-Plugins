using System;
using UnityEngine;
using Pluton;
using Pluton.Events;

namespace EasyAttachEntities
{
	//-->Created by Corrosion X
	//-->Version 0.7
	public class EasyAttachEntities : CSharpPlugin
	{
		public string string_attachedEntities = "AttachedEntities";

		public string string_EntitiesWithAttachments = "EntitiesWithAttachments";

		public void On_Plugininit()
		{
			Commands.Register("attach").setCallback("attachplayer");
			Commands.Register("detach").setCallback("detach");
			Commands.Register("attanimal").setCallback("attachtoanimal");
		}

		public BaseEntity AttachToEntity(BaseEntity e, String whatthing, String towhere, Boolean Spawn01)
		{
			BaseEntity parachute = GameManager.server.CreateEntity(whatthing, default(Vector3), default(Quaternion));
			if (parachute)
			{
				parachute.SetParent(e, towhere);
				parachute.Spawn(Spawn01);
			}
			return parachute;
		}

		public BaseEntity AttachToPlayer(Player p, String whatthing, String towhere, Boolean Spawn01)
		{
			BaseEntity e = p.basePlayer;
			BaseEntity parachute = GameManager.server.CreateEntity(whatthing, default(Vector3), default(Quaternion));
			if (parachute)
			{
				parachute.SetParent(e, towhere);
				parachute.Spawn(Spawn01);
			}
			return parachute;
		}

		public void attachplayer(string[] args, Player player)
		{
				if (args.Length < 2)
				{
					Logger.Log ("not enough arguments: attachplayer(" + String.Join(" ", args) + ", " + player.Name + ")");
					return;
				}

				string what = args[0];
				string where = args[1];

				Logger.Log (String.Format("Attaching {0} to {1}'s {2}", what, player.Name, where));

				BaseEntity attached = (BaseEntity)GlobalData[string_attachedEntities + player.SteamID];

				if (attached != null)
				{
					player.Message("You already have an object attached to you!");
				}
				else
				{
					BaseEntity attach = AttachToPlayer(player, what, where, true);
					GlobalData [string_attachedEntities + player.SteamID] = attach;
				}

				player.Message("Attach to player executed");
		}

		public void detach(string[] args, Player player)
		{
				BaseEntity attached = (BaseEntity)GlobalData[string_attachedEntities + player.SteamID];
				BaseEntity attached2 = (BaseEntity)GlobalData[string_EntitiesWithAttachments + player.SteamID];

				if (args[0] == "all")
				{
					if (attached != null)
					{
						attached.Kill();
						GlobalData.Remove(string_attachedEntities + player.SteamID);
					}
					if (attached2 != null)
					{
						attached2.Kill();
						GlobalData.Remove(string_EntitiesWithAttachments + player.GameID);
						player.Message("All Created Entities Destroyed");
					}
				}else{
				BaseEntity isattached = (BaseEntity)DataStore.Get("attached", player.SteamID);
				if (isattached == null)
					Util.DestroyEntity(isattached);
				else
					player.Message("You dont have anything attached!");
				player.Message("Deattach from player executed");
		}

		public void attachtoanimal(string[] args, Player player)
		{
				if (args.Length < 3)
				{
					Logger.Log ("not enough arguments: attachtoanimal(" + String.Join(" ", args) + ", " + player.Name + ")");
					return;
				}

				string animal = args [0];
				string what = args [1];
				string where = args [2];
				BaseEntity attached = (BaseEntity)GlobalData [string_EntitiesWithAttachments + player.SteamID];
				if (attached != null)
				{
					player.Message ("You already have an animal with attachments!");
				}
				else
				{
					BaseEntity animale = World.SpawnAnimal (animal, player.Location);
					BaseEntity attacheds = AttachToEntity (animale, what, where, true);
					GlobalData [string_EntitiesWithAttachments + player.SteamID] = attacheds;
				}
				player.Message ("attach to animal executed");
		}

		public void test(string[] args, Player player)
		{
			BaseEntity attached = AttachToPlayer(player, "weapons/melee/boneknife_wm", "head", true);
			player.Message("Attached entity to your head!");
			DataStore.Add("attached", player.SteamID, attached);
		}
	}
}