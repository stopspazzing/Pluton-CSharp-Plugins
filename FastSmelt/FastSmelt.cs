using Pluton;
using System;

namespace FastSmelt
{
    public class FastSmelt : CSharpPlugin
    {
        public bool AllowBurntMeat;
        public float CharcoalChance;
        public float ConsumeChance;
        public float ProductionMultiplier;

        public void On_PluginInit()
        {
            Author = "Corrosion X";
            Version = "1.0";
            About = "Configurable furnance cooking options.";
            IniParser settings = Plugin.CreateIni("Settings");
            if (settings != null){
                settings.AddSetting("Settings", "CharcoalChance", "1.5f");
                settings.AddSetting("Settings", "ConsumeChance", "0.5f");
                settings.AddSetting("Settings", "ProductionMultiplier", "1.0f");
                settings.AddSetting("Settings", "AllowBurntMeat", "false");
            }
            settings.Save();
            CharcoalChance = float.Parse(settings.GetSetting("CharcoalChance", "1.5f"));
            ConsumeChance = float.Parse(settings.GetSetting("ConsumeChance", "0.5f"));
            ProductionMultiplier = float.Parse(settings.GetSetting("ProductionMultiplier", "1.0f"));
            AllowBurntMeat = bool.Parse(settings.GetSetting("AllowBurntMeat", "false"));
        }

        public void On_ConsumeFuel(BaseOven oven, Item fuel, ItemModBurnable burnable)
        {
            var byproductChance = burnable.byproductChance * CharcoalChance;
            if (oven.allowByproductCreation && burnable.byproductItem != null && UnityEngine.Random.Range(0.0f, 1f) <= byproductChance)
            {
                Item item = ItemManager.Create(burnable.byproductItem, burnable.byproductAmount);
                if (!item.MoveToContainer(oven.inventory))
                    item.Drop(oven.inventory.dropPosition, oven.inventory.dropVelocity);
            }

            for (int i = 0; i < oven.inventorySlots; i++)
            {
                try
                {
                    var sItem = oven.inventory.GetSlot(i);
                    if (sItem == null || !sItem.IsValid())
                        continue;

                    var isCookable = sItem.info.GetComponent<ItemModCookable>();
                    if (isCookable == null)
                        continue;

                    if (isCookable.becomeOnCooked.category == ItemCategory.Food && sItem.info.shortname.Trim().EndsWith("_cooked", StringComparison.Ordinal) && AllowBurntMeat)
                    {
                        continue;
                    }

                    var consumeAmt = (int)Math.Ceiling(ProductionMultiplier * (UnityEngine.Random.Range(0f, 1f) <= ConsumeChance ? 1 : 0));
                    var Amount = sItem.amount;

                    if (Amount < consumeAmt)
                    {
                        consumeAmt = Amount;
                    }

                    consumeAmt = TakeFromInventorySlot(oven.inventory, sItem.info.itemid, consumeAmt, i);

                    if (consumeAmt <= 0)
                    {
                        continue;
                    }

                    var smeltItem = ItemManager.Create(isCookable.becomeOnCooked, isCookable.amountOfBecome * consumeAmt);
                    if (!smeltItem.MoveToContainer(oven.inventory))
                    {
                        smeltItem.Drop(oven.inventory.dropPosition, oven.inventory.dropVelocity);
                    }
                }
                catch (InvalidOperationException)
                {
                }
            }
        }

        private static int TakeFromInventorySlot(ItemContainer container, int itemId, int amount, int slot)
        {
            var slotItem = container.GetSlot(slot);
            if (slotItem.info.itemid == itemId && !slotItem.IsBlueprint())
            {
                if (slotItem.amount > amount)
                {
                    slotItem.MarkDirty();
                    slotItem.amount -= amount;
                    return amount;
                }

                amount = slotItem.amount;
                slotItem.RemoveFromContainer();
                return amount;
            }
            return 0;
        }
    }
}