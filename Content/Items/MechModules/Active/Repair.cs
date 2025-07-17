using Microsoft.Xna.Framework;
using MechMod.Content.Mechs;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using static MechMod.Content.Mechs.IMechModule;
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
            Item.value = 10000; // The value of the item in copper coins.
            Item.rare = 3; // The rarity of the item.
        }

        // THESE ARE PLACEHOLDER, PARTS WILL BE SOLD BY NPC
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.DirtBlock, 10) // Example ingredient
                .AddTile(TileID.WorkBenches) // Required crafting station
                .Register();
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
