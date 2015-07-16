using Pluton;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace PlutonEssentials
{
    public partial class PlutonEssentials
	{
		public void Mystats(string[] args, Player player)
		{
			PlayerStats stats = player.Stats;
			player.Message(string.Format("You have {0} kills and {1} deaths!", stats.Kills, stats.Deaths));
			player.Message(string.Format("You have taken {0} dmg, and caused {1} in total!", stats.TotalDamageTaken, stats.TotalDamageDone));
			return;
		}

		public void Statsof(string[] args, Player player)
		{
			Player pOther = Player.Find(string.Join(" ", args));
			if (pOther != null) {
				PlayerStats stats2 = pOther.Stats;
				player.Message(string.Format(pOther.Name + " has {0} kills and {1} deaths!", stats2.Kills, stats2.Deaths));
				player.Message(string.Format(pOther.Name + " has taken {0} dmg, and caused {1} in total!", stats2.TotalDamageTaken, stats2.TotalDamageDone));
				return;
			}
			player.Message("Can't find player: " + string.Join(" ", args));
			return;
		}

		public void Whereami(string[] args, Player player)
		{
			player.Message (player.Location.ToString ());
			return;
		}

		public void Players(string[] args, Player player)
		{
			string msg = Server.Players.Count == 1 ? "You are alone!" : string.Format ("There are: {0} players online!", Server.Players.Count);
			player.Message (msg);
			return;
		}

		public void Help(string[] args, Player player)
		{
			IniParser ConfigFile = Plugin.GetIni("PlutonEssentials") ;
			foreach (string key in ConfigFile.EnumSection("HelpMessage")) {
				player.Message(ConfigFile.GetSetting("HelpMessage", key) );
			}
		}

		public void CommandS(string[] args, Player player)
		{
			var cc = new List<ChatCommands>();
			foreach (KeyValuePair<string, BasePlugin> pl in PluginLoader.GetInstance().Plugins) {
				cc.Add(pl.Value.chatCommands);
			}
			var list = new List<string>();
			foreach (ChatCommands cm in cc) {
				list.AddRange(cm.getCommands());
			}
			player.Message(string.Join(", ", list.ToArray()));
		}

		public void Whatis(string[] args, Player player)
		{
			var cc = new List<ChatCommands>();
			foreach (KeyValuePair<string, BasePlugin> pl in PluginLoader.GetInstance().Plugins) {
				cc.Add(pl.Value.chatCommands);
			}
			if (args.Length < 1)
				player.Message("You must provide a command name");
			else {
				var list = new List<string>();
				foreach (ChatCommands cm in cc) {
					list.AddRange(cm.getDescriptions(args[0]));
				}
				if (list.Count > 0)
					player.Message(string.Join("\r\n", list.ToArray()));
			}
		}

		public void Howto(string[] args, Player player)
		{
			var cc = new List<ChatCommands>();
			foreach (KeyValuePair<string, BasePlugin> pl in PluginLoader.GetInstance().Plugins) {
				cc.Add(pl.Value.chatCommands);
			}
			if (args.Length < 1)
				player.Message("You must provide a command name");
			else {
				var list = new List<string>();
				foreach (ChatCommands cm in cc) {
					list.AddRange(cm.getUsages(args[0]));
				}
				foreach (var item in list) {
					player.Message(string.Format("/{0} {1}", args[0], item));
				}
			}
		}

		public void Srstart(string[] args, Player player)
		{
			if (args.Length == 0 || (args.Length == 1 && args[0] == ""))
			{
				player.Message("USAGE: /srstart StructureName");
				return;
			}
			if (DataStore.GetInstance().ContainsKey("StructureRecorder", player.SteamID))
			{
				player.Message("Recording is already running");
				return;
			}
			string name = args[0];
			StartRecording(name, player.SteamID);
			player.Message("Recording was started on name \"" + name + "\"");
		}

		public void Srstop(string[] args, Player player)
		{
			if (!DataStore.ContainsKey("StructureRecorder", player.SteamID))
			{
				player.Message("There is nothing to stop");
				player.Message("Start one using: /srstart StructureName");
				return;
			}
            string name = (string)DataStore.Get("StructureRecorder", player.SteamID);
			DataStore.Remove("StructureRecorder", player.SteamID);
			StopRecording(name);
			player.Message("Recording was stopped");
		}

		public void Srbuild(string[] args, Player player)
		{
			if (args.Length == 0 || (args.Length == 1 && args[0] == ""))
			{
				player.Message("USAGE: /srbuild StructureName");
				return;
			}
			Structure structure;
			Structures.TryGetValue(args[0], out structure);
			if (structure == null)
			{
				player.Message("Building wasn't found by name \"" + args[0] + "\"");
				return;
			}
			structure.Build(player.Location);
			player.Message("Building was spawned");
		}
		public void StartRecording(string name, string steamID = "")
		{
			if (Structures.ContainsKey(name))
			{
				Structures.Remove(name);
				string path = Path.Combine(Util.GetStructuresFolder(), name + ".sps");
				if (File.Exists(path))
				{
					File.Delete(path);
				}
			}
			if (steamID == "")
			{
				DataStore.Add("StructureRecorderEveryone", "ON", name);
			}
			else
			{
				DataStore.Add("StructureRecorder", steamID, name);
			}
			Structure structure = CreateStructure(name);
			Structures.Add(name, structure);
		}

		public void StopRecording(string name)
		{
			Structure structure;
			Structures.TryGetValue(name, out structure);
			DataStore.Remove("StructureRecorder", name);
			structure.Export();
		}

		public Structure CreateStructure(string name)
		{
			return new Structure(name);
		}

		public void LoadStructures()
		{
			string path = Util.GetStructuresFolder();
			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}
			var structuresPath = new DirectoryInfo(path);
			Structures.Clear();
			foreach (FileInfo file in structuresPath.GetFiles())
			{
				if (file.Extension.ToLower() == ".sps")
				{
					using (var stream = new FileStream(file.FullName, FileMode.Open))
					{
						var formatter = new BinaryFormatter();
						object thing = formatter.Deserialize(stream);
						var structure = thing as Structure;
						Structures.Add(file.Name.Substring(0, file.Name.Length - 5), structure);
					}
				}
			}
		}

		static void RecordAllConnected(Structure structure, BuildingPart bp)
		{
			var partList = new List<object>();
			var posList = new List<Vector3>();
			int layerMasks = LayerMask.GetMask("Construction", "Construction Trigger", "Deployed");
			partList.Add(bp.buildingBlock);
			posList.Add(bp.Location);
			structure.AddComponent(bp);
			for (int i = 0; i < posList.Count; i++)
			{
				Collider[] colliders = Physics.OverlapSphere(posList[i], 3f, layerMasks);
				foreach (Collider collider in colliders)
				{
					if (collider.isTrigger) continue;
					if (collider.GetComponentInParent<BuildingBlock>() != null)
					{
						BuildingBlock buildingBlock = collider.GetComponentInParent<BuildingBlock>();
						if (!partList.Contains(buildingBlock))
						{
							partList.Add(buildingBlock);
							posList.Add(buildingBlock.transform.position);
							structure.AddComponent(new BuildingPart(buildingBlock));
						}
					}
					else if (collider.GetComponentInParent<Deployable>() != null)
					{
						Deployable deployable = collider.GetComponentInParent<Deployable>();
						if (!partList.Contains(deployable))
						{
							partList.Add(deployable);
							posList.Add(deployable.transform.position);
							structure.AddComponent(deployable);
						}
					}
					else if (collider.GetComponentInParent<Spawnable>() != null)
					{
						Spawnable spawnable = collider.GetComponentInParent<Spawnable>();
						if (!partList.Contains(spawnable))
						{
							partList.Add(spawnable);
							posList.Add(spawnable.transform.position);
							structure.AddComponent(spawnable);
						}
					}
				}
			}
		}
	}
}

