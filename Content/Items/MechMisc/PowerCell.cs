using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using MechMod.Common.UI;
using MechMod.Common.Players;

namespace MechMod.Content.Items.MechMisc
{
    public class PowerCell : ModItem
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
            Item.consumable = true;

            Item.rare = ItemRarityID.Blue;
            Item.value = Item.buyPrice(gold: 20);
        }

        public override bool? UseItem(Player player)
        {
            if (Main.myPlayer == player.whoAmI)
            {
                var modPlayer = player.GetModPlayer<MechModPlayer>();
                if (modPlayer.powerCellActive)
                    Main.NewText("Power Cell is already active!", 255, 0, 0);
                else
                    modPlayer.powerCellActive = true;
            }
            return true;
        }
    }
}
