using Pluton;
using Pluton.Events;

namespace NoDurability
{
    public class NoDurability : CSharpPlugin
    {
        public void On_PluginInit()
        {
            Author = "Corrosion X";
            Version = "1.0.2";
            About = "Prevents usable items from taking condition damage";
        }
        public void On_ItemLoseCondition(ItemConditionEvent ilc)
        {
            {
                BasePlayer player = ilc.Item._item.GetOwnerPlayer();
                if (ilc.Item == null) return;
                if (player.IsAlive() && player != null)
                {
                    ilc.Item.Condition = ilc.Item._item.info.condition.max;
                }
            }
        }
    }
}