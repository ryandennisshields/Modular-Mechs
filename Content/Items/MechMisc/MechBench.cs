using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using MechMod.Common.UI;

namespace MechMod.Content.Items.MechMisc
{
    /// <summary>
    /// Item that serves as the way to access the <see cref="MechBenchUI"/> so the player can modify their Mech.
    /// </summary>

    public class MechBench : ModItem
    {
        public override void SetDefaults()
        {
            Item.autoReuse = false;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useAnimation = 15;
            Item.useTime = 15;
            Item.UseSound = SoundID.Research;
            Item.noMelee = true;
            Item.rare = ItemRarityID.Cyan;
            Item.value = Item.buyPrice(silver: 50);
        }

        public override bool? UseItem(Player player)
        {
            if (Main.myPlayer == player.whoAmI) // If the player is using the item,
            {
                ModContent.GetInstance<MechBenchUISystem>().ShowMyUI(); // Show the MechBenchUI
            }
            return true;
        }
    }
}
