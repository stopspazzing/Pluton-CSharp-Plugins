using Pluton;
using Pluton.Events;

namespace InfiniteAmmo
{
	public class InfiniteAmmo : CSharpPlugin
	{
        public void On_ItemUsed(ItemUsedEvent iue){
			if(iue.Item.Name == "F1 Grenade"){
				foreach(ItemDefinition item in ItemManager.GetItemDefinitions())
					if (item.displayName.ToString () ==  iue.Item.Name)
						iue.Item._item.GetOwnerPlayer ().inventory.containerBelt.AddItem(item,1);
			}
			if(iue.Item.Name == "Beancan Grenade"){
				foreach(ItemDefinition item in ItemManager.GetItemDefinitions())
					if (item.displayName.ToString () ==  iue.Item.Name)
						iue.Item._item.GetOwnerPlayer ().inventory.containerBelt.AddItem(item,1);
			}
		}

		public void On_WeaponThrow(WeaponThrowEvent wte)
		{
			Item Item = wte.Weapon.GetItem ();
			if (Item.info.displayName.ToString () == "Beancan Grenade") {
				wte.Player.Message ("Beancan");
				return;
			}
			if (Item.info.displayName.ToString () == "F1 Grenade") {
				wte.Player.Message ("F1 Grenade");
				return;
			}
			wte.Weapon.GetItem ().amount++;
		}

		public void On_Shooting(ShootEvent se)
		{
			se.BaseProjectile.primaryMagazine.contents = se.BaseProjectile.primaryMagazine.capacity;
			se.BaseProjectile.GetItem().condition = se.BaseProjectile.GetItem().info.condition.max;
			se.BaseProjectile.SendNetworkUpdateImmediate();
		}

		public void On_RocketShooting(RocketShootEvent rse)
		{
			rse.BaseLauncher.primaryMagazine.contents = rse.BaseLauncher.primaryMagazine.capacity;
			rse.BaseLauncher.GetItem().condition = rse.BaseLauncher.GetItem().info.condition.max;
			rse.BaseLauncher.SendNetworkUpdateImmediate();
		}
	}
}