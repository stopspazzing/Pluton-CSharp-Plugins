using Pluton;
using Pluton.Events;

namespace InfiniteAmmo
{
	public class InfiniteAmmo : CSharpPlugin
	{
		const string _creator = "Corrosion X";
		const string _version = "0.6";
		public void On_WeaponThrow(WeaponThrowEvent wt)
		{
			if (wt.Player != null) 
			{
				if (wt.Weapon != null)
				{
					Item item = wt.Weapon.GetItem();
					wt.Player.Inventory._inv.GiveItem(item.amount += 1);
				}
				else
				{
					return;
				}
			}
		}
		public void On_Shoot(ShootEvent se)
		{
			if(se.Player != null)
			{
				se.BaseProjectile.primaryMagazine.contents = se.BaseProjectile.primaryMagazine.capacity;
				se.BaseProjectile.GetItem().condition = se.BaseProjectile.GetItem().info.condition.max;
				se.BaseProjectile.SendNetworkUpdateImmediate();
			}
		}
		public void On_RocketShoot(RocketShootEvent rse)
		{
			if(rse.Player != null)
			{
				rse.BaseLauncher.primaryMagazine.contents = rse.BaseLauncher.primaryMagazine.capacity;
				rse.BaseLauncher.GetItem().condition = rse.BaseLauncher.GetItem().info.condition.max;
				rse.BaseLauncher.SendNetworkUpdateImmediate();
			}
		}

	}
}

