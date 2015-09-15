using Pluton;
using Pluton.Events;

namespace AcceptRules
{
    public class AcceptRules : CSharpPlugin
    {
        public string ruleslist;
        string json;
        int i;
        //        public void On_ServerInit(){
        //            ConVar.Server.official = true;
        //            ConVar.Server.pve = false;
        //        }
        public void On_PluginInit()
        {
            Author = "Corrosion X";
            Version = "1.0";
            About = "Requires players to accept rules or be disconnected.";
            ServerConsoleCommands.Register("kick.player")
                .setCallback(Kickplayer)
                .setDescription("Kicks player if they disagree to rules.")
                .setUsage("");
            if (!Plugin.IniExists("rules"))
            {
                IniParser ini = Plugin.CreateIni("rules");
                ini.AddSetting("Rules", "1", "No Cheating.");
                ini.AddSetting("Rules", "2", "No Hacking.");
                ini.AddSetting("Rules", "3", "No Racism/Hate/Offensive text/symbols/images.");
                ini.Save();
            }
            IniParser getini = Plugin.GetIni("rules");
            ruleslist = "<color=white>Welcome to " + ConVar.Server.hostname + " !</color> \n <color=red>By joining this server you agree to the following rules:</color> \n \n";
            foreach (string arg in getini.EnumSection("Rules"))
            {
                i += 1;
                ruleslist += "<color=white>" + i + "</color>" + ". <color=red>" + getini.GetSetting("Rules", arg) + "</color> \n";
            }
            json = @"[	
                        {
                            ""name"": ""AcceptRules"",
                            ""parent"": ""Overlay"",
                            ""components"":
                            [
                                {
                                     ""type"":""UnityEngine.UI.Image"",
                                     ""color"":""0.1 0.1 0.1 1"",
                                }, 
                                {    
                                    ""type"":""RectTransform"",
                                    ""anchormin"": ""0 0"",
                                    ""anchormax"": ""1 1""
                                },
                                {  
                                    ""type"":""NeedsCursor""
                                }
                            ]
                        },
                        {
                            ""parent"": ""AcceptRules"",
                            ""components"":
                            [
                                {
                                    ""type"":""UnityEngine.UI.Text"",
                                    ""text"":""{ruleslist}"",
                                    ""fontSize"":20,
                                    ""align"": ""MiddleCenter"",
                                },
                                { 
                                    ""type"":""RectTransform"",
                                    ""anchormin"": ""0 0.1"",
                                    ""anchormax"": ""1 0.9""
                                }
                            ]
                        }, 
                        {
                            ""name"": ""BtnAccept"",
                            ""parent"": ""AcceptRules"",
                            ""components"":
                            [
                                {
                                    ""type"":""UnityEngine.UI.Button"",
                                    ""close"":""AcceptRules"",
                                    ""color"": ""0.08 0.71 0.12 0.2"",
                                    ""imagetype"": ""Tiled""
                                },
                                {
                                    ""type"":""RectTransform"",
                                    ""anchormin"": ""0.2 0.16"",
                                    ""anchormax"": ""0.4 0.20""
                                }
                            ]
                        },
                        {
                            ""parent"": ""BtnAccept"",
                            ""components"":
                            [
                                {
                                    ""type"":""UnityEngine.UI.Text"",
                                    ""text"":""Accept"",
                                    ""fontSize"":20,
                                    ""align"": ""MiddleCenter""
                                }
                            ]
                        },
                        {
                            ""name"": ""BtnDontAccept"",
                            ""parent"": ""AcceptRules"",
                            ""components"":
                            [
                                {
                                    ""close"":""AcceptRules"",
                                    ""command"":""kick.player"",
                                    ""type"": ""UnityEngine.UI.Button"",
                                    ""color"": ""0.9 0.23 0.23 0.2"",
                                    ""imagetype"": ""Tiled""
                                },
                                {
                                    ""type"":""RectTransform"",
                                    ""anchormin"": ""0.6 0.16"",
                                    ""anchormax"": ""0.8 0.20""
                                }
                            ]
                        },
                        {
                            ""parent"": ""BtnDontAccept"",
                            ""components"":
                            [
                                {
                                    ""type"":""UnityEngine.UI.Text"",
                                    ""text"":""Dont Accept"",
                                    ""fontSize"":20,
                                    ""align"": ""MiddleCenter""
                                }
                            ]
                        }
                    ]";
        }

        public void On_PlayerConnected(Player player)
        {
            CommunityEntity.ServerInstance.ClientRPCEx(new Network.SendInfo() { connection = player.basePlayer.net.connection }, null, "AddUI", json.Replace("{ruleslist}", ruleslist));
            //var commui = new PlutonUIEntity(player.basePlayer.net.connection);
            //if (commui == null)
            //    Logger.LogError("0 CE");

            //var rulespanel = commui.AddPanel(
            //                     "RulesPanel",
            //                     "Overlay"
            //                 );
            //rulespanel.AddComponent<Pluton.PlutonUI.NeedsCursor>();
            //rulespanel.AddComponent(new Pluton.PlutonUI.RectTransform()
            //{
            //    anchormin = "0 0",
            //    anchormax = "1 1"
            //});
            //rulespanel.AddComponent(new Pluton.PlutonUI.Image()
            //{
            //    color = "0.9 0.9 0.9 1",
            //});
            //var rulelist = commui.AddPanel(null, "RulesPanel");
            //rulelist.AddComponent(new Pluton.PlutonUI.Text()
            //{
            //    text = rulelist,
            //    fontSize = 20,
            //    align = "MiddleCenter"
            //});
            ////foreach (string rule in rules)
            ////{
            ////    rulelist.AddComponent(new Pluton.PlutonUI.Text()
            ////        {
            ////            text = rule,
            ////            fontSize = 24,
            ////            align = "MiddleCenter"
            ////        });
            ////}
            //rulelist.AddComponent(new Pluton.PlutonUI.RectTransform()
            //{
            //    anchormin = "0 0.20",
            //    anchormax = "1 0.9"
            //});
            ////Agree Button logic below
            //var AgreeBtn = commui.AddPanel("AgreeBtn", "RulesPanel");
            //AgreeBtn.AddComponent(new Pluton.PlutonUI.Button()
            //{
            //    close = "RulesPanel",
            //    color = "0.9 0.8 0.3 0.8",
            //    imagetype = "Tiled"
            //});
            //AgreeBtn.AddComponent(new Pluton.PlutonUI.RectTransform()
            //{
            //    anchormin = "0.3 0.15",
            //    anchormax = "0.7 0.2"
            //});
            //var btn1 = commui.AddPanel(null, "AgreeBtn");
            //btn1.AddComponent(new Pluton.PlutonUI.Text()
            //{
            //    text = "I Agree",
            //    fontSize = 20,
            //    align = "MiddleCenter"
            //});

            ////Disagree button logic below
            ////var DisagreeBtn = commui.AddPanel("DisagreeBtn", "RulesPanel");
            ////DisagreeBtn.AddComponent(new Pluton.PlutonUI.Button()
            ////{
            ////    close = "RulesPanel",
            ////    command = "client.disconnect",
            ////    color = "0.9 0.8 0.3 0.8",
            ////    imagetype = "Tiled"
            ////});
            ////DisagreeBtn.AddComponent(new Pluton.PlutonUI.RectTransform()
            ////{
            ////    anchormin = "0.3 0.15",
            ////    anchormax = "0.7 0.2"
            ////});
            ////var btn2 = commui.AddPanel(null, "DisagreeBtn");
            ////btn2.AddComponent(new Pluton.PlutonUI.Text()
            ////{
            ////    text = "I Disagree",
            ////    fontSize = 20,
            ////    align = "MiddleCenter"
            ////});

            //commui.CreateUI();
        }

        public void Kickplayer(string[] args)
        {
        }

        public void On_ClientConsole(ClientConsoleEvent cce){
            if (cce.Cmd == "kick.player"){
                cce.User.Kick();
                return;
            }
        }
    }
}