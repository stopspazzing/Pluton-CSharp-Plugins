using Pluton;

namespace InfiniteAmmo
{
	public class InfiniteAmmo : CSharpPlugin
	{
		const string _creator = "Corrosion X";
		const string _version = "0.1";
		public void On_WeaponThrow(ThrownWeapon thrownWeapon, BaseEntity.RPCMessage msg)
		{
			if (thrownWeapon.ownerPlayer.IsAlive()) 
			{
				if (thrownWeapon.GetItem() != null)
				{
					Item item = thrownWeapon.GetItem();
					thrownWeapon.ownerPlayer.inventory.GiveItem(item);
				}
				else
				{
					return;
				}
			}
		}
		public void On_Shoot(BaseProjectile baseProjectile, BaseEntity.RPCMessage msg)
		{
			if(baseProjectile.ownerPlayer.IsAlive())
			{
				baseProjectile.primaryMagazine.contents = baseProjectile.primaryMagazine.capacity;
				baseProjectile.GetItem().condition = baseProjectile.GetItem().info.condition.max;
				baseProjectile.SendNetworkUpdateImmediate();
			}
		}
		public void On_RocketShoot(BaseLauncher baseLauncher, BaseEntity.RPCMessage msg, BaseEntity baseEntity)
		{
			if(baseLauncher.ownerPlayer.IsAlive())
			{
				baseLauncher.primaryMagazine.contents = baseLauncher.primaryMagazine.capacity;
				baseLauncher.GetItem().condition = baseLauncher.GetItem().info.condition.max;
				baseLauncher.SendNetworkUpdateImmediate();
			}
		}

	}
}

