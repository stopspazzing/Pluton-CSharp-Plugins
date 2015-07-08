using System;
using UnityEngine;
using Pluton;
using System.Timers;
using System.Collections.Generic;

namespace PlutonEssentials
{
	public class PlutonEssentials : CSharpPlugin
	{
		const string _version = "0.9.1";
		const string _creator = "Pluton Team";
		List<ChatCommands> cc = new List<ChatCommands>();

		public static ServerTimers Timers;
		public static PlutonEssentials Instance;

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
			Instance = this;
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
			Commands.Register(GetConfig.GetSetting("Commands", "ShowMyStats", "mystats")).setCallback("mystats");
			Commands.Register(GetConfig.GetSetting("Commands", "ShowStatsOther", "statsof")).setCallback("statsof");
			Commands.Register(GetConfig.GetSetting("Commands", "ShowLocation", "whereami")).setCallback("whereami");
			Commands.Register(GetConfig.GetSetting("Commands", "ShowOnlinePlayers", "players")).setCallback("players");
			Commands.Register(GetConfig.GetSetting("Commands", "Help", "help")).setCallback("help");
			Commands.Register(GetConfig.GetSetting("Commands", "Commands", "commands")).setCallback("commands");
			Commands.Register(GetConfig.GetSetting("Commands", "Description", "whatis")).setCallback("whatis");
			Commands.Register(GetConfig.GetSetting("Commands", "Usage", "howto")).setCallback("howto");
		}

		public void ReloadTimers()
		{
			if (Timers != null)
				Timers.Dispose();
			IniParser ConfigFile = Plugin.GetIni("PlutonEssentials") ;
			var broadcast = ConfigFile.GetSetting("Config", "broadcastInterval", "600000");
			if (broadcast != null) {
				double ads = Double.Parse(broadcast);

				Timers = new ServerTimers(ads);
				Timers.Start();
			}
		}

		public void mystats(string[] args, Player player) {
			PlayerStats stats = player.Stats;
			player.Message(String.Format("You have {0} kills and {1} deaths!", stats.Kills, stats.Deaths));
			player.Message(String.Format("You have taken {0} dmg, and caused {1} in total!", stats.TotalDamageTaken, stats.TotalDamageDone));
			return;
		}

		public void statsof(string[] args, Player player) {
			Player pOther = Player.Find(String.Join(" ", args[0]));
			if (pOther != null) {
				PlayerStats stats2 = pOther.Stats;
				player.Message(String.Format(pOther.Name + " has {0} kills and {1} deaths!", stats2.Kills, stats2.Deaths));
				player.Message(String.Format(pOther.Name + " has taken {0} dmg, and caused {1} in total!", stats2.TotalDamageTaken, stats2.TotalDamageDone));
				return;
			}
			player.Message("Can't find player: " + String.Join(" ", args[0]));
			return;
		}

		public void whereami(string[] args, Player player)
		{
			player.Message (player.Location.ToString ());
			return;
		}

		public void players(string[] args, Player player)
		{
			string msg = Server.Players.Count == 1 ? "You are alone!" : String.Format ("There are: {0} players online!", Server.Players.Count);
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
			foreach (KeyValuePair<string, BasePlugin> pl in PluginLoader.GetInstance().Plugins) {
				cc.Add(pl.Value.chatCommands);
			}
			List<string> list = new List<string>();
			foreach (ChatCommands cm in cc) {
				list.AddRange(cm.getCommands());
			}
			player.Message(String.Join(", ", list.ToArray()));
		}

		public void whatis(string[] args, Player player)
		{
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
					player.Message(String.Join("\r\n", list.ToArray()));
			}
		}

		public void howto(string[] args, Player player)
		{
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
					player.Message(String.Format("/{0} {1}", args[0], item));
				}
			}
		}

		public static void Advertise()
		{
			IniParser ConfigFile = Instance.Plugin.GetIni("PlutonEssentials") ;
			foreach (string arg in ConfigFile.EnumSection("BroadcastMessages")) {
				Instance.Server.Broadcast(ConfigFile.GetSetting("BroadcastMessages", arg));
			}
		}
	}

	public class ServerTimers
	{
		public readonly System.Timers.Timer _adstimer;

		public ServerTimers(double ads)
		{
			_adstimer = new System.Timers.Timer(ads);

			Debug.Log("Broadcast timer started!");
			_adstimer.Elapsed += new ElapsedEventHandler(this._adstimer_Elapsed);
		}

		public void Dispose()
		{
			Stop();
			_adstimer.Dispose();
		}

		public void Start()
		{
			_adstimer.Start();
		}

		public void Stop()
		{
			_adstimer.Stop();
		}

		private void _adstimer_Elapsed(object sender, ElapsedEventArgs e)
		{
			PlutonEssentials.Advertise();
		}

	}
}