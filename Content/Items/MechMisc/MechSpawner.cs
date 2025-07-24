using System;
using System.Collections.Generic;
using MechMod.Common.Players;
using MechMod.Common.UI;
using MechMod.Content.Buffs;
using MechMod.Content.Mounts;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace MechMod.Content.Items.MechMisc
{
    public class MechSpawner : ModItem
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
            Item.rare = ItemRarityID.Cyan;
            Item.value = Item.buyPrice(silver: 50);
            Item.mountType = ModContent.MountType<ModularMech>();
        }
    }
}