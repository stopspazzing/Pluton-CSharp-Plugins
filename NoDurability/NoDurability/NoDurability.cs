using Pluton;

namespace NoDurability
{
	public class NoDurability : CSharpPlugin
	{
		const string _creator = "Corrosion X";
		const string _version = "0.4";
		public void On_UseItem(Item item, int amountToConsume)
		{
			if (item == null) return;
			BasePlayer player = item.GetOwnerPlayer();
			if (player == null) return;
			if (player.IsAlive())
			{
				item.condition = item.maxCondition;
			}
		}
	}
}

