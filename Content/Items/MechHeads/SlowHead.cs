using MechMod.Content.Items.MechWeapons;
using MechMod.Content.Mounts;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MechMod.Content.Items.MechHeads
{
    internal class SlowHead : ModItem, IMechParts
    {
        public override string Texture => "MechMod/Content/Items/MechHeads/BaseHead";
        public override void SetDefaults()
        {
            Item.width = 20; // The width of the item's hitbox in pixels.
            Item.height = 20; // The height of the item's hitbox in pixels.
            Item.value = Item.buyPrice(gold: 4);
            Item.rare = 2; // The rarity of the item.
        }

        public void ApplyStats(Player player, ModularMech mech)
        {
            mech.armourBonus = player.statDefense; // Double the armour
        }
    }
}
