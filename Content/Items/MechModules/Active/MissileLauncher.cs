using MechMod.Common.Players;
using MechMod.Content.Buffs;
using MechMod.Content.Mounts;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static MechMod.Content.Mounts.IMechModule;

namespace MechMod.Content.Items.MechModules.Active
{
    /// <summary>
    /// Active Module that launches homing missiles from the Mech.
    /// </summary>

    public class MissileLauncher : ModItem, IMechModule
    {
        public override void SetDefaults()
        {
            Item.value = Item.buyPrice(gold: 8);
            Item.rare = ItemRarityID.Orange;
        }

        public ModuleSlot MSlot => ModuleSlot.Active; // Active slot
        public ModuleType MType => ModuleType.Persistent; // Persistent effect

        private bool fireMissiles = false; // Tracker for if missiles are to be fired
        private int missilesFired = 0; // Counter for missiles fired

        private int cooldown = 1200; // Cooldown in frames (20 seconds)

        private int delayTimer; // Timer for delay
        private int fireDelay = 5; // Delay between individual missile launches in frames (0.5 seconds)

        // Missile projectile properties
        private DamageClass missileClass = DamageClass.Default;
        private int missileDamage = 50;
        private int missileKnockback = 10;
        private int missileCount = 5;
        private int missileType = ModContent.ProjectileType<MissileProjectile>();

        public void ModuleEffect(ModularMech mech, Player player, MechModPlayer modPlayer, MechWeaponsPlayer weaponsPlayer)
        {
            if (MechMod.MechActivateModule.JustPressed && !player.HasBuff(ModContent.BuffType<Cooldown>()) && Main.myPlayer == player.whoAmI) // If the player presses the "MechActivateModule" binding and the player is not on cooldown,
            {
                player.AddBuff(ModContent.BuffType<Cooldown>(), cooldown); // Add cooldown
                fireMissiles = true; // Begin firing missiles
                missilesFired = 0; // Reset the missile counter
                delayTimer = fireDelay; // Fire first missile immediately
            }

            if (fireMissiles) // If missiles are to be fired,
            {
                if (delayTimer >= fireDelay && missilesFired < missileCount) // If delay has passed and haven't fired all missiles yet,
                {
                    // Create missile projectile
                    Projectile.NewProjectile(
                        new EntitySource_Parent(player),
                        player.MountedCenter + new Vector2(0, -40),
                        Vector2.Zero,
                        missileType,
                        weaponsPlayer.DamageCalc(missileDamage, player, missileClass),
                        weaponsPlayer.KnockbackCalc(missileKnockback, player, missileClass),
                        player.whoAmI
                    );
                    missilesFired++; // Increment the missile counter
                    delayTimer = 0; // Reset the delay timer
                    SoundEngine.PlaySound(SoundID.Item10, player.position); // Play launch sound when created
                }

                delayTimer++; // Constantly increase the delay timer while firing missiles

                if (missilesFired >= missileCount) // After firing all missiles,
                    fireMissiles = false; // Stop firing missiles
            }
        }
    }

    /// <summary>
    /// Custom projectile for the missiles.
    /// </summary>

    public class MissileProjectile : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_350"; // Use Missile texture

        private float speed = 10f; // Speed of the missile
        private float rotateSpeed = 0.2f; // Rotation speed of the missile
        private float detectRadius = 1000f; // Detection radius of missile

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true; // Cultist should not be homed on
        }

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.friendly = true; // Can hit enemies
            Projectile.ignoreWater = true; // Ignore water
            Projectile.light = 0.5f; // Produce light
            Projectile.timeLeft = 300;
        }

        public override void AI()
        {
            // Create trailing dust behind missiles
            float offset = -10f; // How far behind the missile to spawn the dust
            Vector2 behind = Projectile.Center - Vector2.UnitY.RotatedBy(Projectile.rotation) * offset;
            // Trail dust
            Dust.NewDust(behind - new Vector2(Projectile.width / 2, Projectile.height / 2), Projectile.width, Projectile.height, DustID.Torch, 0f, 0f, 100, default, 1f);
            Dust.NewDust(behind - new Vector2(Projectile.width / 2, Projectile.height / 2), Projectile.width, Projectile.height, DustID.Smoke, 0f, 0f, 100, default, 0.5f);

            // Tracking logic
            NPC target = FindNearestNPC(); // Find nearest enemy
            if (Projectile.timeLeft > 240) // For 1 second,
            {
                Projectile.velocity.Y -= 0.015f * speed; // Missile flies upwards
            }
            else if (Projectile.timeLeft <= 240) // For the rest of the projectile's duration,
            {
                if (target != null) // If there is a target,
                {
                    // Home onto the target
                    Vector2 direction = target.Center - Projectile.Center; // Get the direction the missile needs to head
                    direction.Normalize(); // Normalise the direction
                    Projectile.velocity.X = MathHelper.SmoothStep(Projectile.velocity.X, direction.X * speed, rotateSpeed); // Smoothly adjust the X velocity
                    Projectile.velocity.Y = MathHelper.SmoothStep(Projectile.velocity.Y, direction.Y * speed, rotateSpeed); // Smoothly adjust the Y velocity
                    Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2; // Rotate to face the target
                }
                else // If there is no target,
                    Projectile.Kill(); // Destroy the projectile
            }
        }

        // Function to find the nearest NPC to be locked onto
        private NPC FindNearestNPC()
        {
            NPC nearestNPC = null;
            float detectDistance = detectRadius * detectRadius; // Get squared detect distance for performance
            foreach (NPC npc in Main.npc) // For each NPC available,
            {
                if (npc.CanBeChasedBy()) // Check if the NPC can be homed onto
                {
                    float distance = Vector2.DistanceSquared(npc.Center, Projectile.Center); // Get squared distance between NPC and projectile for performance
                    if (distance < detectDistance) // If distance to NPC is less than the detect distance,
                    {
                        detectDistance = distance; // Update the detect distance to the current NPC's distance
                        nearestNPC = npc; // Set the nearest NPC to the current one
                    }
                }
            }
            return nearestNPC; // Return the nearest NPC
        }

        public override void OnKill(int timeLeft)
        {
            // Create an explosion effect when the missile is destroyed
            for (int i = 0; i < 20; i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Torch, 0f, 0f, 100, default, 1f);
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke, 0f, 0f, 100, default, 1f);
            }
            SoundEngine.PlaySound(SoundID.Item14, Projectile.position); // Play explosion sound when destroyed
        }
    }
}
