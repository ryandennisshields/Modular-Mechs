using MechMod.Content.Items.MechWeapons;
using MechMod.Content.Mechs;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MechMod.Content.Items.MechHeads
{
    internal class FastHead : ModItem, IMechParts
    {
        public override string Texture => "MechMod/Content/Items/MechHeads/BaseHead";
        public override void SetDefaults()
        {
            Item.width = 20; // The width of the item's hitbox in pixels.
            Item.height = 20; // The height of the item's hitbox in pixels.
            Item.value = 10000; // The value of the item in copper coins.
            Item.rare = 2; // The rarity of the item.
        }

        public void ApplyStats(Player player, ModularMech mech)
        {
            Weapons.partCritChanceBonus *= 2f; // Double critical chance
        }

        // THESE ARE PLACEHOLDER, PARTS WILL BE SOLD BY NPC
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.DirtBlock, 10) // Example ingredient
                .AddTile(TileID.WorkBenches) // Required crafting station
                .Register();
        }
    }
}
