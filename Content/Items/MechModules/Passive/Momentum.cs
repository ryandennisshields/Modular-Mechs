using MechMod.Common.Players;
using MechMod.Content.Mounts;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static MechMod.Content.Mounts.IMechModule;

namespace MechMod.Content.Items.MechModules.Passive
{
    /// <summary>
    /// Passive Module that increases horizontal movement speed with melee weapons and increases damage depending on movement speed.
    /// </summary>

    public class Momentum : ModItem, IMechModule
    {
        public override void SetDefaults()
        {
            Item.value = Item.buyPrice(gold: 4);
            Item.rare = ItemRarityID.Orange;
        }

        public ModuleSlot MSlot => ModuleSlot.Passive; // Passive slot
        public ModuleType MType => ModuleType.Persistent; // Persistent effect

        private float speedIncrease = 1.2f; // 20% speed increase
        private float damageIncreaseForVelocity = 0.015f; // Damage increase per unit of speed

        private float baseFinalDamageModifier; // Base final damage modifier to build upon
        private float bonusDamage; // Current bonus damage being applied

        public void ModuleEffect(ModularMech mech, Player player, MechModPlayer modPlayer, MechWeaponsPlayer weaponsPlayer, MechVisualPlayer visualPlayer)
        {
            if (weaponsPlayer.DamageClass == DamageClass.Melee)
            {
                if (!modPlayer.grantedBonuses) // If bonuses haven't been granted yet,
                {
                    // Increase horizontal speed
                    modPlayer.groundHorizontalSpeed *= speedIncrease;
                    modPlayer.flightHorizontalSpeed *= speedIncrease;

                    baseFinalDamageModifier = weaponsPlayer.finalDamageModifier; // Store the base final damage modifier
                }

                // Get the player's X and Y velocity magnitudes
                float speedX = player.velocity.X > 0 ? player.velocity.X : -player.velocity.X;
                float speedY = player.velocity.Y > 0 ? player.velocity.Y : -player.velocity.Y;

                float targetBonus = speedX > speedY ? speedX * damageIncreaseForVelocity : speedY * damageIncreaseForVelocity; // Increase target damage based on the greater of X or Y velocity

                bonusDamage = MathHelper.Lerp(bonusDamage, targetBonus, 1f); // Interpolate to the new bonus damage

                weaponsPlayer.finalDamageModifier = baseFinalDamageModifier + bonusDamage; // Apply the bonus damage to the final damage modifier
            }
        }
    }
}
