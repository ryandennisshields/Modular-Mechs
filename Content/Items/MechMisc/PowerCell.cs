using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using MechMod.Common.Players;

namespace MechMod.Content.Items.MechMisc
{
    /// <summary>
    /// Item that allows the player to upgrade the Mech, providing it with bonuses like a longer duration, health bonus for not using a Booster, and upgraded visuals.
    /// </summary>

    public class PowerCell : ModItem
    {
        public override void SetDefaults()
        {
            Item.autoReuse = false;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useAnimation = 15;
            Item.useTime = 15;
            Item.UseSound = SoundID.Research;
            Item.noMelee = true;
            Item.consumable = true;

            Item.rare = ItemRarityID.Blue;
            Item.value = Item.buyPrice(gold: 20);
        }

        public override bool? UseItem(Player player)
        {
            if (Main.myPlayer == player.whoAmI)
            {
                var modPlayer = player.GetModPlayer<MechModPlayer>();
                if (modPlayer.powerCellActive) // If the player has already used the power cell,
                    Main.NewText("Power Cell is already active!", 255, 0, 0); // Notify the player that they already have the power cell active
                else // Otherwise,
                    modPlayer.powerCellActive = true; // Activate the power cell bonuses
            }
            return true;
        }
    }
}
