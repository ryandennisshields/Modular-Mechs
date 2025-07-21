using Microsoft.Xna.Framework;
using MechMod.Content.Mounts;
using Terraria;
using Terraria.ModLoader;
using static MechMod.Content.Mounts.IMechModule;
using Terraria.DataStructures;
using Terraria.ID;
using MechMod.Content.Items.MechWeapons;
using Microsoft.VisualBasic;

namespace MechMod.Content.Items.MechModules.Passive
{
    public class Brace : ModItem, IMechModule
    {
        public override void SetDefaults()
        {
            Item.width = 20; // The width of the item's hitbox in pixels.
            Item.height = 20; // The height of the item's hitbox in pixels.
            Item.value = Item.buyPrice(gold: 10);
            Item.rare = 3; // The rarity of the item.
        }

        public ModuleSlot moduleSlot => ModuleSlot.Passive; // Passive slot
        public ModuleType moduleType => ModuleType.Persistent; // Persistent effect

        private bool changed = false;

        private const float damageBonus = 0.1f; // 10% damage bonus
        private const float speedReduction = 0.9f; // 10% speed reduction

        public void ModuleEffect(ModularMech mech, Player player)
        {
            if (Main.mouseRight && Weapons.DamageClass == DamageClass.Ranged) // If player is holding right-click and the weapon is ranged,
            {
                player.scope = true; // Scope out
                if (!changed) // If stats haven't been changed yet,
                { // Change stats
                    Weapons.partDamageBonus += damageBonus; // Apply damage bonus
                    mech.groundHorizontalSpeed *= speedReduction;
                    mech.groundJumpSpeed *= speedReduction;
                    mech.flightHorizontalSpeed *= speedReduction;
                    mech.flightJumpSpeed *= speedReduction;
                    changed = true;
                }
            }
            else if (changed) // If player is not holding right-click and stats have been changed,
            { // Reset stats
                Weapons.partDamageBonus -= damageBonus;
                mech.groundJumpSpeed /= speedReduction;
                mech.groundHorizontalSpeed /= speedReduction;
                mech.flightJumpSpeed /= speedReduction;
                mech.flightHorizontalSpeed /= speedReduction;
                changed = false;
            }
        }
    }
}
