using MechMod.Common.Players;
using MechMod.Content.Mounts;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static System.Net.Mime.MediaTypeNames;

namespace MechMod.Content.Items.MechWeapons
{
    /// <summary>
    /// Weapon that charges up, and on charge release it fires a powerful laser for a short duration towards the cursor.
    /// <para>If the player charges for too long, lasers will start firing in random directions and the player will be damaged for as long as they continue overcharging.</para>
    /// <para>Releasing the charge perfectly (just before overcharge) does more damage.</para>
    /// </summary>

    public class ChargeCannon : ModItem, IMechWeapon
    {
        public override void SetDefaults()
        {
            Item.value = Item.buyPrice(gold: 2);
            Item.rare = ItemRarityID.Orange;

            Item.useAmmo = AmmoID.Bullet; // Make the weapon use Bullet ammo
        }

        public void SetStats(MechWeaponsPlayer weaponsPlayer)
        {
            weaponsPlayer.DamageClass = DamageClass.Magic; // Set DamageClass to Magic
            weaponsPlayer.useType = MechWeaponsPlayer.UseType.Point; // Set use type to Point
        }

        private bool fireReady;

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
                weaponsPlayer.useRate = weaponsPlayer.AttackSpeedCalc(22, player);
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
                fireReady = true;

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

        public void UpdateAbility(Player player, MechWeaponsPlayer weaponsPlayer, MechVisualPlayer visualPlayer) 
        {
            if (Main.mouseLeftRelease && fireReady)
            {
                for (int i = 0; i < 50; i++)
                    Projectile.NewProjectile(new EntitySource_Parent(player), player.Center, new Vector2(99999.0f, 0.0f), ProjectileID.DD2BetsyFireball, 10, 4, player.whoAmI);
                fireReady = false;
            }
        }
    }
}
