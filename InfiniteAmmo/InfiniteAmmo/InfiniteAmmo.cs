using System;
using Pluton;
using Pluton.Events;

namespace InfiniteAmmo
{
	public class InfiniteAmmo : CSharpPlugin
	{
		const string _creator = "Corrosion X";
		const string _version = "0.6";
		public void On_WeaponThrow(WeaponThrowEvent wte)
		{
				wte.Weapon.GetItem().amount++;
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