using Terraria.ModLoader;
using Terraria;
using MechMod.Content.Mounts;
using Terraria.ID;
using Terraria.DataStructures;
using Microsoft.Xna.Framework;
using MechMod.Common.Players;
using Terraria.Audio;

namespace MechMod.Content.Items.MechWeapons
{
    /// <summary>
    /// Weapon that fires a laser beam towards the cursor consuming mana.
    /// </summary>

    public class LaserGun : ModItem, IMechWeapon
    {
        public override void SetDefaults()
        {
            Item.value = Item.buyPrice(gold: 4);
            Item.rare = ItemRarityID.Orange;
        }

        public void SetStats(MechWeaponsPlayer weaponsPlayer)
        {
            weaponsPlayer.DamageClass = DamageClass.Magic; // Set DamageClass to Magic
            weaponsPlayer.useType = MechWeaponsPlayer.UseType.Point; // Set use type to Point
        }

        public void UseAbility(Player player, MechWeaponsPlayer weaponsPlayer, MechVisualPlayer visualPlayer, Vector2 mousePosition, bool toggleOn)
        {
            int manaCost = 6; // Mana cost for use
            if (player.statMana > manaCost) // If the player has enough mana,
            {
                weaponsPlayer.canUse = true; // Allow weapon use

                int projectileType = ProjectileID.LaserMachinegunLaser; // Use the Laser Machinegun Laser projectile

                // Calculate projectile properties
                int damage = weaponsPlayer.DamageCalc(66, player);
                weaponsPlayer.CritChanceCalc(4, player);
                weaponsPlayer.attackRate = weaponsPlayer.AttackSpeedCalc(22, player);
                float knockback = weaponsPlayer.KnockbackCalc(4, player);
                float projSpeed = 12;

                // Get the direction and velocity towards the mouse cursor, adjusting for the offset
                Vector2 offset = new(0, -38); // Offset to adjust the projectile's spawn position relative to the mech's center
                Vector2 direction = (Main.MouseWorld - player.Center) - offset;
                direction.Normalize();
                Vector2 velocity = direction * projSpeed;

                // Adjust the spawn position to be at the end of the muzzle
                Vector2 muzzleOffset = Vector2.Normalize(velocity) * 70f;
                if (Collision.CanHit(player.Center + offset, 0, 0, player.Center + offset + muzzleOffset, 0, 0))
                {
                    offset += muzzleOffset;
                }

                // Create projectile
                Projectile.NewProjectile(new EntitySource_Parent(player), player.Center + offset, velocity, projectileType, damage, knockback, player.whoAmI);

                // Consume mana and apply mana regen delay
                player.CheckMana(manaCost, true);
                player.manaRegenDelay = 120; // 2 seconds of mana regen delay

                int holdTime = 50; // Amount of time player holds out the weapon after ceasing to use
                visualPlayer.animationTimer = holdTime; // Set the animation timer to hold the weapon out
                SoundEngine.PlaySound(SoundID.Item12, player.position); // Play Laser sound when the weapon is used
            }
            else // If not enough mana,
                weaponsPlayer.canUse = false; // Disable weapon use
        }
    }
}
