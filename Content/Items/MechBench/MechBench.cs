using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using MechMod.Common.UI;

namespace MechMod.Content.Items.MechBench
{
    public class MechBench : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.maxStack = 1;
            Item.useTurn = true;
            Item.autoReuse = false;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useAnimation = 15;
            Item.useTime = 15;
            Item.UseSound = SoundID.Research;
            Item.noMelee = true;

            Item.rare = ItemRarityID.Blue;
            Item.value = Item.sellPrice(silver: 45);
        }

        // THESE ARE PLACEHOLDER, PARTS WILL BE SOLD BY NPC
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.DirtBlock, 10) // Example ingredient
                .AddTile(TileID.WorkBenches) // Required crafting station
                .Register();
        }

        public override bool? UseItem(Player player)
        {
            if (Main.myPlayer == player.whoAmI)
            {
                ModContent.GetInstance<MechBenchUISystem>().ShowMyUI();
            }
            return true;
        }
    }
}
