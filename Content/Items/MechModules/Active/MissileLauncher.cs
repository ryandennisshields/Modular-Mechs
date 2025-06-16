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
    public class MissileLauncher : ModItem, IMechModule
    {
        public override void SetDefaults()
        {
            Item.width = 20; // The width of the item's hitbox in pixels.
            Item.height = 20; // The height of the item's hitbox in pixels.
            Item.value = 10000; // The value of the item in copper coins.
            Item.rare = 3; // The rarity of the item.
        }

        public ModuleSlot moduleSlot => ModuleSlot.Active; // Active slot
        public ModuleType moduleType => ModuleType.Persistent; // Persistent effect

        private bool fireMissiles = false;
        private int missilesFired = 0; // Counter for missiles fired

        private int timer;
        // should be set to 1800
        private int cooldown = 30; // Cooldown in frames (30 seconds)

        private int delayTimer;
        private int fireDelay = 5; // Delay between missile launches in frames (0.5 seconds)

        private int missileDamage = 50; // Damage dealt by the missile
        private float missileKnockback = 10f; // Knockback applied by the missile
        private int missileCount = 5; // Number of missiles to fire
        private int missileType = ModContent.ProjectileType<MissileProjectile>(); // Type of the missile projectile

        public void ModuleEffect(ModularMech mech, Player player)
        {
            if (MechMod.MechActivateModule.JustPressed && timer >= cooldown && Main.myPlayer == player.whoAmI)
            {
                fireMissiles = true;
                missilesFired = 0; // Reset the missile counter
                delayTimer = fireDelay; // Fire first missile immediately
            }

            if (fireMissiles) // If missiles are to be fired,
            {
                if (delayTimer >= fireDelay && missilesFired < missileCount) // If delay has passed and haven't fired all missiles yet,
                {
                    Projectile.NewProjectile(
                        new EntitySource_Parent(player),
                        player.MountedCenter + new Vector2(0, -40),
                        Vector2.Zero,
                        missileType,
                        missileDamage,
                        missileKnockback,
                        player.whoAmI
                    ); // Create a missile
                    missilesFired++; /// Increment the missile counter
                    delayTimer = 0; // Reset the delay timer
                }

                delayTimer++; // Constantly increase the delay timer while firing missiles

                if (missilesFired >= missileCount) // After firing all missiles,
                {
                    fireMissiles = false; // Stop firing missiles
                    timer = 0; // Reset the cooldown timer
                }
            }

            if (timer < cooldown)
                timer++;
        }
    }

    public class MissileProjectile : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_350";

        private float speed = 10f; // Speed of the missile
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
            float offset = -10f; // How far behind the missile to spawn the dust
            Vector2 behind = Projectile.Center - Vector2.UnitY.RotatedBy(Projectile.rotation) * offset;
            // Torch dust
            Dust.NewDust(behind - new Vector2(Projectile.width / 2, Projectile.height / 2), Projectile.width, Projectile.height, DustID.Torch, 0f, 0f, 100, default, 1f);
            // Smoke dust
            Dust.NewDust(behind - new Vector2(Projectile.width / 2, Projectile.height / 2), Projectile.width, Projectile.height, DustID.Smoke, 0f, 0f, 100, default, 0.5f);

            NPC target = FindNearestNPC();
            if (Projectile.timeLeft > 240)
            {
                Projectile.velocity.Y -= 0.015f * speed;
            }
            else if (Projectile.timeLeft <= 240)
            {
                if (target != null)
                {
                    Vector2 direction = target.Center - Projectile.Center;
                    direction.Normalize();
                    Projectile.velocity = direction * speed; // Adjust speed as needed
                    Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2; // Rotate to face the target
                }
                else
                    Projectile.Kill();
            }
        }

        private NPC FindNearestNPC()
        {
            NPC nearestNPC = null;
            float detectDistance = detectRadius * detectRadius; // Get squared detect distance for performance
            foreach (NPC npc in Main.npc)
            {
                if (npc.CanBeChasedBy())
                {
                    float distance = Vector2.DistanceSquared(npc.Center, Projectile.Center); // Get squared distance between NPC and projectile for performance
                    if (distance < detectDistance) // If distance to NPC is less than the detect distance,
                    {
                        detectDistance = distance; // Update the detect distance to the current NPC's distance
                        nearestNPC = npc; // Set the nearest NPC to the current one
                    }
                }
            }
            return nearestNPC;
        }

        public override void OnKill(int timeLeft)
        {
            // Create an explosion effect when the missile is destroyed
            for (int i = 0; i < 20; i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Torch, 0f, 0f, 100, default, 1f);
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke, 0f, 0f, 100, default, 1f);
            } 
        }
    }
}
