using System;
using UnityEngine;
using Pluton;
using System.Timers;
using System.Collections.Generic;

namespace PlutonEssentials
{
	public class PlutonEssentials : CSharpPlugin
	{
		const string author = "Pluton Team";
		const string version = "0.9.1";

		private Timer aTimer;

		public void On_ServerInit()
		{
			if (Plugin.IniExists("PlutonEssentials")) {
				IniParser Config = Plugin.GetIni("PlutonEssentials") ;
				float craft = float.Parse(Config.GetSetting("Config", "craftTimescale", "1.0"));
				Server.CraftingTimeScale = craft;
				float resource = float.Parse(Config.GetSetting("Config", "resourceGatherMultiplier", "1.0"));
				World.ResourceGatherMultiplier = resource;
				float time = float.Parse(Config.GetSetting("Config", "permanentTime", "-1"));
				if (time != -1f) {
					World.Time = time;
					World.FreezeTime ();
				} else {
					World.Timescale = float.Parse (Config.GetSetting ("Config", "timescale", "30"));
				}
			}
		}

		public void On_PluginInit()
		{
			if (Plugin.IniExists("PlutonEssentials")) {
				Debug.Log("PlutonEssentials config loaded!");
			} else {
				IniParser ConfigFile = Plugin.CreateIni("PlutonEssentials");
				Debug.Log("PlutonEssentials config created!");
				Debug.Log("The config will be filled with the default values.");
				ConfigFile.AddSetting("Config", "craftTimescale", "1.0");
				ConfigFile.AddSetting("Config", "resourceGatherMultiplier", "1.0");

				ConfigFile.AddSetting("Config", "permanentTime", "-1");
				ConfigFile.AddSetting("Config", "timescale", "30.0");

				ConfigFile.AddSetting("Config", "broadcastInterval", "600000");

				ConfigFile.AddSetting("Commands", "ShowMyStats", "mystats");
				ConfigFile.AddSetting("Commands", "ShowStatsOther", "statsof");

				ConfigFile.AddSetting("Commands", "ShowLocation", "whereami");
				ConfigFile.AddSetting("Commands", "ShowOnlinePlayers", "players");

				ConfigFile.AddSetting("Commands", "Help", "help");
				ConfigFile.AddSetting("Commands", "Commands", "commands");

				ConfigFile.AddSetting("Commands", "Description", "whatis");
				ConfigFile.AddSetting("Commands", "Usage", "howto");

				ConfigFile.AddSetting("HelpMessage", "help_string0", "This is an empty help section");

				ConfigFile.AddSetting("BroadcastMessage", "msg0", "This server is powered by Pluton, the new servermod!");
				ConfigFile.AddSetting("BroadcastMessage", "msg1", "For more information visit our github repo: github.com/Notulp/Pluton or our forum: pluton-team.org");
				ConfigFile.Save();
			}
			IniParser GetConfig = Plugin.GetIni("PlutonEssentials");
			Commands.Register(GetConfig.GetSetting("Commands", "ShowMyStats", "mystats")).setCallback(mystats);
			Commands.Register(GetConfig.GetSetting("Commands", "ShowStatsOther", "statsof")).setCallback(statsof);
			Commands.Register(GetConfig.GetSetting("Commands", "ShowLocation", "whereami")).setCallback(whereami);
			Commands.Register(GetConfig.GetSetting("Commands", "ShowOnlinePlayers", "players")).setCallback(players);
			Commands.Register(GetConfig.GetSetting("Commands", "Help", "help")).setCallback(help);
			Commands.Register(GetConfig.GetSetting("Commands", "Commands", "commands")).setCallback(commands);
			Commands.Register(GetConfig.GetSetting("Commands", "Description", "whatis")).setCallback(whatis);
			Commands.Register(GetConfig.GetSetting("Commands", "Usage", "howto")).setCallback(howto);
			int broadcast_time = int.Parse(GetConfig.GetSetting("Config", "broadcastInterval", "600000"));
			aTimer = new Timer(broadcast_time);
			aTimer.Elapsed += Advertise;
			aTimer.Enabled = true;
        }

		public void mystats(string[] args, Player player)
        {
			PlayerStats stats = player.Stats;
			player.Message(string.Format("You have {0} kills and {1} deaths!", stats.Kills, stats.Deaths));
			player.Message(string.Format("You have taken {0} dmg, and caused {1} in total!", stats.TotalDamageTaken, stats.TotalDamageDone));
			return;
		}

		public void statsof(string[] args, Player player)
        {
			Player pOther = Player.Find(string.Join(" ", args[0]));
			if (pOther != null) {
				PlayerStats stats2 = pOther.Stats;
				player.Message(string.Format(pOther.Name + " has {0} kills and {1} deaths!", stats2.Kills, stats2.Deaths));
				player.Message(string.Format(pOther.Name + " has taken {0} dmg, and caused {1} in total!", stats2.TotalDamageTaken, stats2.TotalDamageDone));
				return;
			}
			player.Message("Can't find player: " + string.Join(" ", args[0]));
			return;
		}

		public void whereami(string[] args, Player player)
		{
			player.Message (player.Location.ToString ());
			return;
		}

		public void players(string[] args, Player player)
		{
			string msg = Server.Players.Count == 1 ? "You are alone!" : string.Format ("There are: {0} players online!", Server.Players.Count);
			player.Message (msg);
			return;
		}

		public void help(string[] args, Player player)
		{
			IniParser ConfigFile = Plugin.GetIni("PlutonEssentials") ;
			foreach (string key in ConfigFile.EnumSection("HelpMessage")) {
				player.Message(ConfigFile.GetSetting("HelpMessage", key) );
			}
		}

		public void commands(string[] args, Player player)
		{
			List<ChatCommands> cc = new List<ChatCommands>();
			foreach (KeyValuePair<string, BasePlugin> pl in PluginLoader.GetInstance().Plugins) {
				cc.Add(pl.Value.chatCommands);
			}
			List<string> list = new List<string>();
			foreach (ChatCommands cm in cc) {
				list.AddRange(cm.getCommands());
			}
			player.Message(string.Join(", ", list.ToArray()));
		}

		public void whatis(string[] args, Player player)
		{
			List<ChatCommands> cc = new List<ChatCommands>();
			foreach (KeyValuePair<string, BasePlugin> pl in PluginLoader.GetInstance().Plugins) {
				cc.Add(pl.Value.chatCommands);
			}
			if (args.Length < 1)
				player.Message("You must provide a command name");
			else {
				List<string> list = new List<string>();
				foreach (ChatCommands cm in cc) {
					list.AddRange(cm.getDescriptions(args[0]));
				}
				if (list.Count > 0)
					player.Message(string.Join("\r\n", list.ToArray()));
			}
		}

		public void howto(string[] args, Player player)
		{
			List<ChatCommands> cc = new List<ChatCommands>();
			foreach (KeyValuePair<string, BasePlugin> pl in PluginLoader.GetInstance().Plugins) {
				cc.Add(pl.Value.chatCommands);
			}
			if (args.Length < 1)
				player.Message("You must provide a command name");
			else {
				List<string> list = new List<string>();
				foreach (ChatCommands cm in cc) {
					list.AddRange(cm.getUsages(args[0]));
				}
				foreach (var item in list) {
					player.Message(string.Format("/{0} {1}", args[0], item));
				}
			}
		}

		void Advertise(object source, ElapsedEventArgs e)
		{
			IniParser ConfigFile = Plugin.GetIni("PlutonEssentials") ;
			foreach (string arg in ConfigFile.EnumSection("BroadcastMessage")) {
				Server.Broadcast(ConfigFile.GetSetting("BroadcastMessage", arg));
			}
		}
	}
}