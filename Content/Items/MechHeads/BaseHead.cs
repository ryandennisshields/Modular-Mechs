using MechMod.Content.Items.MechWeapons;
using MechMod.Content.Mounts; 
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MechMod.Content.Items.MechHeads
{
    internal class BaseHead : ModItem, IMechParts
    {
        public override void SetDefaults()
        {
            Item.width = 20; // The width of the item's hitbox in pixels.
            Item.height = 20; // The height of the item's hitbox in pixels.
            Item.value = Item.buyPrice(gold: 1);
            Item.rare = 2; // The rarity of the item.
        }

        public void ApplyStats(Player player, ModularMech mech)
        {
            mech.lifeBonus += 50; // 50 health bonus
            Weapons.partDamageBonus += 0.10f; // 10% damage bonus
        }
    }
}
