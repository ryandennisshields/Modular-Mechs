using MechMod.Content.Mounts;
using Terraria;
using Terraria.ModLoader;
using static MechMod.Content.Mounts.IMechModule;
using Terraria.ID;
using MechMod.Common.Players;

namespace MechMod.Content.Items.MechModules.Passive
{
    /// <summary>
    /// Passive Module that allows the player to pan the camera further out when holding right-click with a ranged weapon (similar to Terraria's scope accessories), gaining a small damage bonus but reducing movement speed.
    /// </summary>

    public class Brace : ModItem, IMechModule
    {
        public override void SetDefaults()
        {
            Item.value = Item.buyPrice(gold: 4);
            Item.rare = ItemRarityID.Orange;
        }

        public ModuleSlot MSlot => ModuleSlot.Passive; // Passive slot
        public ModuleType MType => ModuleType.Persistent; // Persistent effect

        private bool changed = false; // Tracker for if stats have been changed

        private const float damageBonus = 0.1f; // 10% damage bonus
        private const float speedReduction = 0.9f; // 10% speed reduction

        public void ModuleEffect(ModularMech mech, Player player, MechModPlayer modPlayer, MechWeaponsPlayer weaponsPlayer)
        {
            if (Main.mouseRight && weaponsPlayer.DamageClass == DamageClass.Ranged) // If player is holding right-click and the weapon is ranged,
            {
                player.scope = true; // Scope out
                if (!changed) // If stats haven't been changed yet,
                {
                    // Apply stat changes
                    weaponsPlayer.partDamageBonus += damageBonus;
                    modPlayer.groundHorizontalSpeed *= speedReduction;
                    modPlayer.groundJumpSpeed *= speedReduction;
                    modPlayer.flightHorizontalSpeed *= speedReduction;
                    modPlayer.flightJumpSpeed *= speedReduction;
                    changed = true;
                }
            }
            else if (changed) // If player is not holding right-click and stats have been changed,
            { 
                // Reset stat changes
                weaponsPlayer.partDamageBonus -= damageBonus;
                modPlayer.groundJumpSpeed /= speedReduction;
                modPlayer.groundHorizontalSpeed /= speedReduction;
                modPlayer.flightJumpSpeed /= speedReduction;
                modPlayer.flightHorizontalSpeed /= speedReduction;
                changed = false;
            }
        }
    }
}
