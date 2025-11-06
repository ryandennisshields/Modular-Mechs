using MechMod.Common.Players;
using MechMod.Content.Mounts;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

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
        }

        public void SetStats(MechWeaponsPlayer weaponsPlayer)
        {
            weaponsPlayer.DamageClass = DamageClass.Magic; // Set DamageClass to Magic
            weaponsPlayer.useType = MechWeaponsPlayer.UseType.Point; // Set use type to Point
        }

        private int projectileType; // Type of projectile to be fired

        // Projectile properties
        private int damage;
        private int damageValue = 66;
        private float knockback;
        private float projSpeed;

        private int chargeTime; // Time the player has been charging the weapon
        private bool fireReady;

        private float scale = 0.5f; // Scale of the projectile
        private int chargeSoundTimer; // Timer to control charge sound playback
        private int chargeSoundRate = 25; // Rate at which charge sound plays

        public void UseAbility(Player player, MechWeaponsPlayer weaponsPlayer, MechVisualPlayer visualPlayer, Vector2 mousePosition, bool toggleOn)
        {
            int manaCost = 2; // Mana cost for use
            bool manaCheck = player.CheckMana(manaCost, true); // Check and consume mana
            player.manaRegenDelay = 120; // 2 seconds of mana regen delay
            if (manaCheck) // If the player has enough mana,
            {
                weaponsPlayer.canUse = true; // Allow weapon use
                weaponsPlayer.useRate = 3;

                projectileType = ModContent.ProjectileType<ChargeProjectile>(); // Get the projectile type

                // Calculate projectile properties
                damage = weaponsPlayer.DamageCalc(damageValue, player);
                knockback = weaponsPlayer.KnockbackCalc(4, player);
                projSpeed = 25;
                weaponsPlayer.CritChanceCalc(4, player);

                chargeTime++; // Increment charge time (frames weapon has been charging)

                fireReady = true; // Notify the weapon is ready to fire

                if (chargeTime <= 50) // While charging up to 50 (charging up to perfect window),
                {
                    damageValue++; // Increase damage as charge increases
                    scale += 0.01f; // Increase scale as charge increases
                }
                if (chargeTime == 50) // If the charge time is 50 (perfect timing window notifier),
                    SoundEngine.PlaySound(SoundID.Item4, player.position); // Play a sound for the perfect timing window
                else if (chargeTime > 50 && chargeTime < 55) // If the charge time is within 50 to 55 (perfect charge window),
                {
                    visualPlayer.weaponColour = Color.Green; // Change weapon colour to green
                    damage *= 2; // Double the damage
                    scale = 2f; // Double the scale
                }
                else if (chargeTime >= 55 && chargeTime < 65) // If the charge time is within 55 to 65 (overcharge starting),
                {
                    visualPlayer.weaponColour = Color.Yellow; // Change weapon colour to yellow
                    damage /= 2; // Reset damage increase
                    scale = 1f; // Reset scale
                }
                else if (chargeTime >= 65 && chargeTime < 75) // If the charge time is within 65 to 75 (further overcharge),
                    visualPlayer.weaponColour = Color.Orange; // Change weapon colour to orange
                else if (chargeTime >= 75) // If the charge time is 75 or more (overcharge state),
                {
                    chargeTime = 75; // Cap charge time to prevent overflow

                    // Overchage state
                    visualPlayer.weaponColour = Color.Red; // Change weapon colour to red
                    weaponsPlayer.useRate = 6; // Slow down firing rate when overcharging

                    CreateProjectile(player); // Create the projectile

                    player.Hurt(PlayerDeathReason.ByCustomReason(NetworkText.FromLiteral($"{player.name} lived life to the fullest as a disco ball.")), (int)(player.statLifeMax2 * 0.05f), 0, armorPenetration: 999); // Hurt the player for 10% of their max health

                    SoundEngine.PlaySound(SoundID.Item33, player.position); // Play a sound when firing
                }
                else
                    visualPlayer.weaponColour = Color.White; // Reset weapon colour

                int holdTime = 50; // Amount of time player holds out the weapon after ceasing to use
                visualPlayer.animationTimer = holdTime; // Set the animation timer to hold the weapon out
            }
            else // If not enough mana,
            {
                ResetValues(); // Reset charge values

                weaponsPlayer.canUse = false; // Disable weapon use
            }
        }

        public void UpdateAbility(Player player, MechWeaponsPlayer weaponsPlayer, MechVisualPlayer visualPlayer)
        {
            if (Main.mouseLeft && fireReady && chargeSoundTimer >= chargeSoundRate) // If the player is holding left mouse button, the weapon is ready to fire, and the sound timer is equal to or greater than the sound rate,
            {
                SoundEngine.PlaySound(SoundID.Item15, player.position); // Play charging sound while charging
                chargeSoundTimer = 0; // Reset sound timer
            }

            if (chargeSoundTimer < chargeSoundRate)
                chargeSoundTimer++; // Increment sound timer

            if (Main.mouseLeftRelease && fireReady) // If the player releases left mouse button and the weapon is ready to fire,
            {
                visualPlayer.weaponColour = Color.White; // Reset weapon colour

                CreateProjectile(player); // Create the projectile

                SoundEngine.PlaySound(SoundID.Item33, player.position); // Play a sound when firing
                if (scale == 2) // If the weapon was perfectly charged,
                {
                    SoundEngine.PlaySound(SoundID.Item12, player.position); // Play another sound while firing
                }

                ResetValues(); // Reset charge values
            }
        }

        // Function to create and fire the projectile
        private void CreateProjectile(Player player)
        {
            Vector2 offset = new(0, -40); // Offset to adjust the projectile's spawn position relative to the mech's center
            Vector2 direction;
            if (chargeTime >= 75)
                direction = (Main.rand.NextVector2Unit() * new Vector2((float)Main.time, (float)Main.time)) - offset; // Get a random direction and multiply it by time to get a more varied spread, adjusting for the offset
            else
                direction = (Main.MouseWorld - player.Center) - offset; // Get the direction towards the mouse cursor, adjusting for the offset
            direction.Normalize();
            Vector2 velocity = direction * projSpeed;

            // Adjust the spawn position to be at the end of the muzzle
            Vector2 muzzleOffset = Vector2.Normalize(velocity) * 65f;
            if (Collision.CanHit(player.Center + offset, 0, 0, player.Center + offset + muzzleOffset, 0, 0))
            {
                offset += muzzleOffset;
            }

            // Create projectile
            int projID = Projectile.NewProjectile(new EntitySource_Parent(player), player.Center + offset, velocity, projectileType, damage, knockback, player.whoAmI);
            if (Main.projectile.IndexInRange(projID) && Main.projectile[projID].ModProjectile is ChargeProjectile proj) // Grab the active projectile instance
            {
                proj.Projectile.scale = scale; // Set the projectile scale
            }
        }

        // Function to reset charge-related values
        private void ResetValues()
        {
            damageValue = 66; // Reset damage value
            chargeTime = 0;
            fireReady = false;
            scale = 0.5f; // Reset scale
        }
    }

    public class ChargeProjectile : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = (int)(80 * Projectile.scale); // Change width depending on scale
            Projectile.height = (int)(40 * Projectile.scale); // Ditto
            Projectile.aiStyle = ProjAIStyleID.Arrow;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.light = 0.5f; // Produce light
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 2; // Update more frequently for a faster projectile
            Projectile.penetrate = -1; // Infinite penetration

            AIType = ProjectileID.ZapinatorLaser; // Act exactly like a zapinator laser
        }

        public override void AI()
        {
            // Create trailing dust behind projectile
            for (int i = 0; i < 3; i++)
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.PinkTorch, -Projectile.velocity.X * 0.5f, newColor: Color.Pink, Scale: Projectile.scale * 1.5f);
        }

        public override void OnKill(int timeLeft)
        {
            // Create explosion dust on projectile kill
            for (int i = 0; i < 50; i++)
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.PinkTorch, newColor: Color.Pink, Scale: Projectile.scale * 1.5f);

            SoundEngine.PlaySound(SoundID.Item10, Projectile.position); // Play a sound when the projectile is killed
        }
    }
}
