using Pluton;
using System;
using UnityEngine;
using System.Configuration;

namespace ItemCustomizer
{
	public class ItemCustomizer : CSharpPlugin
	{
		public void On_ServerInit()
		{
			Check ();
			Set ();
		}
		private void Check()
		{
			int count = 0;
			IniParser ini = Plugin.CreateIni("ItemDefs");
			foreach (ItemDefinition itemDef in ItemManager.GetItemDefinitions())
			{
				if (ini.GetSetting(itemDef.displayName.english,"amountType") == ""){
					ini.AddSetting(itemDef.displayName.english, "amountType", itemDef.amountType.ToString());
					ini.AddSetting(itemDef.displayName.english, "condition.max", itemDef.condition.max.ToString());
					ini.AddSetting(itemDef.displayName.english, "condition.repairable", itemDef.condition.repairable.ToString());
					ini.AddSetting(itemDef.displayName.english, "displayDescription", itemDef.displayDescription.english);
					ini.AddSetting(itemDef.displayName.english, "displayName", itemDef.displayName.english);
					ini.AddSetting(itemDef.displayName.english, "flags", itemDef.flags.ToString());
					ini.AddSetting(itemDef.displayName.english, "iconSprite.name", itemDef.iconSprite.name);
					ini.AddSetting(itemDef.displayName.english, "itemid", itemDef.itemid.ToString());
					ini.AddSetting(itemDef.displayName.english, "itemType", itemDef.itemType.ToString());
					ini.AddSetting(itemDef.displayName.english, "maxDraggable", itemDef.maxDraggable.ToString());
					ini.AddSetting(itemDef.displayName.english, "name", itemDef.name);
					ini.AddSetting(itemDef.displayName.english, "rarity", itemDef.rarity.ToString());
					ini.AddSetting(itemDef.displayName.english, "shortname", itemDef.shortname);
					ini.AddSetting(itemDef.displayName.english, "stackable", itemDef.stackable.ToString());
					ini.AddSetting(itemDef.displayName.english, "worldModel", itemDef.worldModel.ToString());
					ini.Save();
					count += 1;
				}
				ini.Save();
			}
			ini.Save();
			Util.Log ("-------- ItemDefs --------");
			Util.Log ("| Found: " + count + " new items          |");
			Util.Log ("----------------------------");
		}
		private void Set()
		{
			IniParser ini = Plugin.GetIni("ItemDefs");
			foreach (ItemDefinition itemDef in ItemManager.GetItemDefinitions())
			{
				foreach(string key in ini.EnumSection (itemDef.displayName.english)){
					if (ItemManager.FindItemDefinition(key) == null){
						Util.Log ("Failed to set "+ itemDef.displayName.english + key);
						continue;
					}
					//stacks
					int sizes = int.Parse(ini.GetSetting ("stackable", key));
					ItemManager.FindItemDefinition(key).stackable = sizes;
					//amountType
					string type = ini.GetSetting ("amountType", key);
					ItemManager.FindItemDefinition(key).amountType.ToString () = type;
					//max condition
					float max = float.Parse (ini.GetSetting("condition.max", key));
					ItemManager.FindItemDefinition(key)
				}
			}

		}
	}
}