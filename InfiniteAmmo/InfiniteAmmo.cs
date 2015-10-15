using Pluton;
using Pluton.Events;

namespace InfiniteAmmo
{
    public class InfiniteAmmo : CSharpPlugin
    {
        public bool AdminOnly;

        public void On_PluginInit()
        {
            Author = "Corrosion X";
            Version = "1.1.2";
            About = "Infinite ammo for guns, rockets, and throwables.";
            if (!Plugin.IniExists("InfiniteAmmo"))
            {
                IniParser settings = Plugin.CreateIni("Settings");
                settings.AddSetting("Settings", "AdminOnly?", "true");
                settings.Save();
            }
            IniParser getconfig = Plugin.GetIni("Settings");
            AdminOnly = getconfig.GetBoolSetting("Settings", "AdminOnly?", true);
        }

        public void On_WeaponThrow(WeaponThrowEvent wte)
        {
            if (AdminOnly && !wte.Player.Admin)
            {
                return;
            }
            wte.Weapon.GetItem().
        }

        public void On_Shooting(ShootEvent se)
        {
            if (AdminOnly && !se.Player.Admin)
            {
                return;
            }
            se.BaseProjectile.primaryMagazine.contents = se.BaseProjectile.primaryMagazine.capacity;
            se.BaseProjectile.GetItem().condition = se.BaseProjectile.GetItem().info.condition.max;
            se.BaseProjectile.SendNetworkUpdateImmediate();
        }

        public void On_RocketShooting(RocketShootEvent rse)
        {
            if (AdminOnly && !rse.Player.Admin)
            {
                return;
            }
            rse.BaseLauncher.primaryMagazine.contents = rse.BaseLauncher.primaryMagazine.capacity;
            rse.BaseLauncher.GetItem().condition = rse.BaseLauncher.GetItem().info.condition.max;
            rse.BaseLauncher.SendNetworkUpdateImmediate();
        }
    }
}