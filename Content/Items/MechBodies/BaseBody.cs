using MechMod.Content.Mechs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace MechMod.Content.Items.MechBodies
{
    internal class BaseBody : ModItem, IMechParts
    {
        public override void SetDefaults()
        {
            Item.width = 20; // The width of the item's hitbox in pixels.
            Item.height = 20; // The height of the item's hitbox in pixels.
            Item.value = 10000; // The value of the item in copper coins.
            Item.rare = 2; // The rarity of the item.
        }

        public void ApplyStats(ModularMech mech)
        {
            mech.lifeBonus += 75;
        }
    }
}
