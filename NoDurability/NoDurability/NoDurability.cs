using Pluton;
using Pluton.Events;

namespace NoDurability
{
	public class NoDurability : CSharpPlugin
	{
		const string author = "Corrosion X";
		const string version = "0.7";
		public void On_UseItem(UseItemEvent eie)
		{
			BasePlayer player = eie.Item._item.GetOwnerPlayer();
			if (player == null) return;
			if (eie.Item == null) return;
			if (player.IsAlive())
			{
				eie.Item._item.condition = eie.Item._item.maxCondition;
			}
		}
	}
}

