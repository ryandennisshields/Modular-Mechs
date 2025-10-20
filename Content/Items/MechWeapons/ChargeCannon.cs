using MechMod.Common.Players;
using MechMod.Content.Mounts;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using XPT.Core.Audio.MP3Sharp.Decoding.Decoders.LayerIII;
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
        }

        public void SetStats(MechWeaponsPlayer weaponsPlayer)
        {
            weaponsPlayer.DamageClass = DamageClass.Magic; // Set DamageClass to Magic
            weaponsPlayer.useType = MechWeaponsPlayer.UseType.Point; // Set use type to Point
        }

        private int projectileType;

        private int damage;
        private int damageValue = 66;
        private float knockback;
        private float projSpeed;

        private int chargeTime; // Time the player has been charging the weapon
        private bool fireReady;

        private float scale = 0.5f;
        private int chargeSoundTimer;
        private int chargeSoundRate = 25;

        public void UseAbility(Player player, MechWeaponsPlayer weaponsPlayer, MechVisualPlayer visualPlayer, Vector2 mousePosition, bool toggleOn)
        {
            int manaCost = 1; // Mana cost for use
            if (player.statMana > manaCost) // If the player has enough mana,
            {
                weaponsPlayer.canUse = true; // Allow weapon use
                weaponsPlayer.useRate = 3;

                projectileType = ModContent.ProjectileType<ChargeProjectile>(); // Use the Laser Machinegun Laser projectile

                damage = weaponsPlayer.DamageCalc(damageValue, player);
                knockback = weaponsPlayer.KnockbackCalc(4, player);
                projSpeed = 25;
                weaponsPlayer.CritChanceCalc(4, player);

                // Consume mana and apply mana regen delay
                player.CheckMana(manaCost, true);
                player.manaRegenDelay = 120; // 2 seconds of mana regen delay

                chargeTime++;

                fireReady = true;

                if (chargeTime <= 50)
                {
                    damageValue++; // Increase damage as charge increases
                    scale += 0.01f; // Increase scale as charge increases
                }
                if (chargeTime == 50)
                    SoundEngine.PlaySound(SoundID.Item4, player.position); // Play a sound when the weapon is fully charged
                else if (chargeTime > 50 && chargeTime < 55)
                {
                    visualPlayer.weaponColour = Color.Green;
                    damage *= 2; // Perfect charge does double damage
                    scale = 2f; // Max scale on perfect charge
                }
                else if (chargeTime >= 55 && chargeTime < 65)
                {
                    visualPlayer.weaponColour = Color.Yellow;
                    damage /= 2; // Reset damage increase
                    scale = 1f; // Reset scale
                }
                else if (chargeTime >= 65 && chargeTime < 75)
                    visualPlayer.weaponColour = Color.Orange;
                else if (chargeTime >= 75)
                {
                    chargeTime = 75; // Cap charge time to prevent overflow

                    // Overchage state
                    visualPlayer.weaponColour = Color.Red;

                    weaponsPlayer.useRate = 6; // Slow down firing rate when overcharging

                    // Fire projectiles in random directions

                    // Get a random direction to fire lasers in
                    Vector2 offset = new(0, -38); // Offset to adjust the projectile's
                    Vector2 direction = Main.rand.NextVector2Unit() * new Vector2((float)Main.time, (float)Main.time); // Get a random direction and multiply it by time to get a more varied spread
                    direction.Normalize();
                    Vector2 velocity = direction * projSpeed;

                    // Adjust the spawn position to be at the end of the muzzle
                    Vector2 muzzleOffset = Vector2.Normalize(velocity) * 70f;
                    if (Collision.CanHit(player.Center + offset, 0, 0, player.Center + offset + muzzleOffset, 0, 0))
                    {
                        offset += muzzleOffset;
                    }

                    int projID = Projectile.NewProjectile(new EntitySource_Parent(player), player.Center + offset, velocity, projectileType, damage, knockback, player.whoAmI);
                    if (Main.projectile.IndexInRange(projID) && Main.projectile[projID].ModProjectile is ChargeProjectile proj) // Grab the active projectile instance
                    {
                        proj.Projectile.scale = scale;
                    }

                    player.Hurt(PlayerDeathReason.ByCustomReason(NetworkText.FromLiteral($"{player.name} lived life to the fullest as a disco ball.")), 20, 0, armorPenetration: 999); // Damage the player for overcharging

                    SoundEngine.PlaySound(SoundID.Item33, player.position); // Play a sound when the weapon is overcharging
                }
                else
                    visualPlayer.weaponColour = Color.White;

                int holdTime = 50; // Amount of time player holds out the weapon after ceasing to use
                visualPlayer.animationTimer = holdTime; // Set the animation timer to hold the weapon out
            }
            else // If not enough mana,
            {
                damageValue = 66; // Reset damage value
                chargeTime = 0;
                fireReady = false;
                scale = 0.5f; // Reset scale

                weaponsPlayer.canUse = false; // Disable weapon use
            }
        }

        public void UpdateAbility(Player player, MechWeaponsPlayer weaponsPlayer, MechVisualPlayer visualPlayer)
        {
            if (Main.mouseLeft && fireReady && chargeSoundTimer >= chargeSoundRate)
            {
                SoundEngine.PlaySound(SoundID.Item15, player.position); // Play Laser sound when the weapon is used
                chargeSoundTimer = 0;
            }

            if (chargeSoundTimer < chargeSoundRate)
                chargeSoundTimer++;

            if (Main.mouseLeftRelease && fireReady)
            {
                visualPlayer.weaponColour = Color.White;

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

                int projID = Projectile.NewProjectile(new EntitySource_Parent(player), player.Center + offset, velocity, projectileType, damage, knockback, player.whoAmI);
                if (Main.projectile.IndexInRange(projID) && Main.projectile[projID].ModProjectile is ChargeProjectile proj) // Grab the active projectile instance
                {
                    proj.Projectile.scale = scale;
                }

                SoundEngine.PlaySound(SoundID.Item33, player.position); // Play a sound when the weapon is overcharging
                if (scale == 2)
                {
                    SoundEngine.PlaySound(SoundID.Item12, player.position); // Play a sound when the weapon is fully charged
                }

                damageValue = 66; // Reset damage value
                chargeTime = 0;
                fireReady = false;
                scale = 0.5f; // Reset scale
            }
        }
    }

    public class ChargeProjectile : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = (int)(80 * Projectile.scale);
            Projectile.height = (int)(40 * Projectile.scale);
            Projectile.aiStyle = 1;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.timeLeft = 600;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 2;
            Projectile.penetrate = -1; // Infinite penetration

            AIType = ProjectileID.ZapinatorLaser; // Act exactly like default Bullet
        }

        public override void AI()
        {
            for (int i = 0; i < 3; i++)
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.PinkTorch, -Projectile.velocity.X * 0.5f, newColor: Color.Pink, Scale: Projectile.scale * 1.5f);
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 50; i++)
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.PinkTorch, newColor: Color.Pink, Scale: Projectile.scale * 1.5f);

            SoundEngine.PlaySound(SoundID.Item10, Projectile.position); // Play a sound when the projectile is killed
        }
    }
}
