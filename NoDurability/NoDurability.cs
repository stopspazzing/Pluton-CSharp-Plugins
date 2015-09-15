using Pluton;
using Pluton.Events;

namespace NoDurability
{
    public class NoDurability : CSharpPlugin
    {
        public void On_ItemLoseCondition(ItemConditionEvent ilc)
        {
            {
                BasePlayer player = ilc.Item._item.GetOwnerPlayer();
                if (ilc.Item == null) return;
                if (player.IsAlive() && player == null)
                {
                    ilc.Item._item.condition = ilc.Item._item.maxCondition;
                }
            }
        }
    }
}

