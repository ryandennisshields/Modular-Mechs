using Microsoft.Xna.Framework;
using MechMod.Content.Mounts;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using static MechMod.Content.Mounts.IMechModule;
using Terraria.ID;
using Terraria.DataStructures;
using System.Runtime.CompilerServices;

namespace MechMod.Content.Items.MechModules.Active
{
    public class Repair : ModItem, IMechModule
    {
        public override void SetDefaults()
        {
            Item.width = 20; // The width of the item's hitbox in pixels.
            Item.height = 20; // The height of the item's hitbox in pixels.
            Item.value = Item.buyPrice(gold: 20);
            Item.rare = 3; // The rarity of the item.
        }

        public ModuleSlot moduleSlot => ModuleSlot.Active; // Active slot
        public ModuleType moduleType => ModuleType.Persistent; // Persistent effect

        private int timer;
        // should be set to 1800
        private int cooldown = 1800; // Cooldown in frames (30 seconds)

        public void ModuleEffect(ModularMech mech, Player player)
        {
            if (MechMod.MechActivateModule.JustPressed && timer >= cooldown)
            {
                player.Heal(player.statLifeMax2 / 2); // Heal the player by half of their max health
            }

            if (timer < cooldown)
                timer++;
        }
    }
}
