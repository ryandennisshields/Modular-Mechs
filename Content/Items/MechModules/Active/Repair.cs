using MechMod.Common.Players;
using MechMod.Content.Buffs;
using MechMod.Content.Mounts;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static MechMod.Content.Mounts.IMechModule;

namespace MechMod.Content.Items.MechModules.Active
{
    public class Repair : ModItem, IMechModule
    {
        public override void SetDefaults()
        {
            Item.value = Item.buyPrice(gold: 8);
            Item.rare = ItemRarityID.Orange;
        }

        public ModuleSlot MSlot => ModuleSlot.Active; // Active slot
        public ModuleType MType => ModuleType.Persistent; // Persistent effect

        private int cooldown = 1800; // Cooldown in frames (30 seconds)

        public void ModuleEffect(ModularMech mech, Player player, MechModPlayer modPlayer, MechWeaponsPlayer weaponsPlayer)
        {
            if (MechMod.MechActivateModule.JustPressed && !player.HasBuff(ModContent.BuffType<Cooldown>()))
            {
                player.AddBuff(ModContent.BuffType<Cooldown>(), cooldown);
                player.Heal(player.statLifeMax2 / 2); // Heal the player by half of their max health
            }
        }
    }
}
