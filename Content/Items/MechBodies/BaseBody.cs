using MechMod.Common.Players;
using MechMod.Content.Mounts;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MechMod.Content.Items.MechBodies
{
    internal class BaseBody : ModItem, IMechParts
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
            mech.lifeBonus += 100; // 100 health bonus
        }

        public void BodyOffsets(Player player, string body) { }
    }
}
