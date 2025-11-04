using MechMod.Common.Players;
using MechMod.Content.Mounts;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;

namespace MechMod.Content.Items.MechWeapons
{
    /// <summary>
    /// Weapon that summons drones to fight for the player.
    /// <para>Drones automatically target and attack enemies firing bullets as ammo.</para>
    /// <para>If the player has used all their summon slots, attacking as the player will make the drones fire missiles that track the player's cursor.</para>
    /// </summary>

    public class DroneSpawner : ModItem, IMechWeapon
    {
        public override void SetDefaults()
        {
            Item.value = Item.buyPrice(gold: 2);
            Item.rare = ItemRarityID.Orange;

            Item.useAmmo = AmmoID.Bullet; // Make the drones use Bullet ammo
        }

        public void SetStats(MechWeaponsPlayer weaponsPlayer)
        {
            weaponsPlayer.DamageClass = DamageClass.Summon; // Set DamageClass to Summon
            weaponsPlayer.useType = MechWeaponsPlayer.UseType.HoldUp; // Set use type to Hold Up
        }

        public void UseAbility(Player player, MechWeaponsPlayer weaponsPlayer, MechVisualPlayer visualPlayer, Vector2 mousePosition, bool toggleOn)
        {
            int currentMinions = 0; // Count of current minions
            foreach (Projectile proj in Main.ActiveProjectiles) // For each active projectile,
            {
                if (proj.owner == player.whoAmI && proj.minion && proj.minionSlots > 0) // If the projectile is a minion owned by the player,
                {
                    currentMinions++; // Increment the minion count
                }
            }

            weaponsPlayer.CritChanceCalc(6, player); // Crit chance is shared between bullets and missiles

            int manaCost = 10; // Mana cost for use
            if (currentMinions < player.maxMinions && player.statMana > manaCost) // If the player is not at max minions,
            {
                weaponsPlayer.canUse = true; // Allow use for this weapon

                // Create drones
                int projectileType = ModContent.ProjectileType<DroneProjectile>(); // Use a custom projectile for the drone
                weaponsPlayer.useRate = 30;
                Projectile.NewProjectile(new EntitySource_Parent(player), player.Center, new Vector2(0, 0), projectileType, 0, 0, player.whoAmI);
                player.AddBuff(ModContent.BuffType<DroneBuff>(), 2); // Apply the buff that signifies the minion is active

                // Consume mana and apply mana regen delay
                player.CheckMana(manaCost, true);
                player.manaRegenDelay = 120; // 2 seconds of mana regen delay

                SoundEngine.PlaySound(SoundID.NPCHit4, player.position); // Play metal sound when spawning drones
            }
            else if (player.ownedProjectileCounts[ModContent.ProjectileType<DroneProjectile>()] > 0) // Otherwise, as long as the player has at least one drone,
            {
                weaponsPlayer.canUse = true; // Allow use for this weapon

                // Fire missiles from drones
                int projectileType = ModContent.ProjectileType<DroneMissileProjectile>(); // Use a custom projectile for the missile

                // Missile properties
                int missileDamage = weaponsPlayer.DamageCalc(80, player);
                weaponsPlayer.useRate = 90;
                int missileKnockback = weaponsPlayer.KnockbackCalc(4, player);

                // Limit the number of missiles to the number of drones (failsafe to not spawn too many missiles)
                int missileCount = 0;
                int maxMissileCount = player.ownedProjectileCounts[ModContent.ProjectileType<DroneProjectile>()];

                // Create missile projectiles for each drone
                foreach (Projectile drone in Main.ActiveProjectiles) // For each active projectile,
                {
                    if (drone.owner == player.whoAmI && drone.type == ModContent.ProjectileType<DroneProjectile>() && missileCount < maxMissileCount) // If the projectile is a drone owned by the player and the missile count is less than the max,
                    {
                        // Create the missile projectile at each drone's position
                        Projectile.NewProjectile(new EntitySource_Parent(drone), drone.Center, new Vector2(0, 0), projectileType, missileDamage, missileKnockback, player.whoAmI);
                        missileCount++; // Increment the missile count
                        SoundEngine.PlaySound(SoundID.Item10 with { Volume = 0.5f }, drone.position); // Play launch sound when created
                    }
                }

                SoundEngine.PlaySound(SoundID.Item44, player.position); // Play summon sound when firing missiles
            }
            else // If not enough mana,
                weaponsPlayer.canUse = false; // Disable weapon use

            int holdTime = 20; // Amount of time player holds out the weapon after ceasing to use
            visualPlayer.animationTimer = holdTime; // Set the animation timer to hold the weapon out
        }

        public void UpdateAbility(Player player, MechWeaponsPlayer weaponsPlayer, MechVisualPlayer visualPlayer)
        {
            weaponsPlayer.updateRate = 30; // Target rate

            // Logic for setting the minion target
            if (weaponsPlayer.activateRightClick && !player.mouseInterface && weaponsPlayer.updateTimer >= weaponsPlayer.updateRate && weaponsPlayer.useTimer >= weaponsPlayer.useRate) // If the player right clicks (not over a UI element),
            {
                Main.NewText("true");

                NPC target = null;
                float best = 999999f; // Used to track what the "best" NPC to target is depending on distance to cursor
                Vector2 mouse = Main.MouseWorld;

                for (int i = 0; i < Main.maxNPCs; i++) // Check all NPCs
                {
                    NPC npc = Main.npc[i];

                    if (npc.CanBeChasedBy()) // If the NPC can be targeted,
                    {
                        bool underCursor = npc.Hitbox.Contains(mouse.ToPoint()); // Check if the NPC is under the cursor
                        float dist = Vector2.Distance(mouse, npc.Hitbox.ClosestPointInRect(mouse)); // Get the distance from the cursor to the NPC

                        if (underCursor || dist <= 80f) // If the NPC is under the cursor or within 80 pixels of it,
                        {
                            if (dist < best) // If this NPC is the closest one so far,
                            {
                                best = dist; // Update the best distance to new nearest distance
                                target = npc; // Set the target to this NPC
                            }
                        }
                    }
                }

                if (target != null) // If a target was found,
                {
                    if (player.MinionAttackTargetNPC == target.whoAmI) // If the target is already the current target,
                        player.MinionAttackTargetNPC = -1; // Clear the target
                    else // Otherwise,
                        player.MinionAttackTargetNPC = target.whoAmI; // Set the target
                }
                else // Otherwise,
                {
                    player.MinionAttackTargetNPC = -1; // Clear the target
                }

                SoundEngine.PlaySound(SoundID.Item44, player.position); // Play summon sound

                int holdTime = 20;
                visualPlayer.animationTimer = holdTime; // Set the animation timer to hold the weapon out

                // Set the last use direction based on the mouse position relative to the player
                if (Main.MouseWorld.X > player.MountedCenter.X)
                    visualPlayer.useDirection = 1;
                else
                    visualPlayer.useDirection = -1;
                if (!player.controlLeft || !player.controlRight)
                    player.direction = visualPlayer.useDirection; // Set the player's direction to the last use direction if not controlling horizontal movement

                weaponsPlayer.updateTimer = 0; // Reset the update timer
            }
        }
    }

    public class DroneProjectile : ModProjectile
    {

        public override void SetStaticDefaults()
        {
            // Set up logic behind the minion
            Main.projFrames[Projectile.type] = 4;
            ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true; // Although targeting usually requires the item to right click the target, custom logic is implemented in ModularMech to allow targeting
            Main.projPet[Projectile.type] = true;
            ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
            ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 80;
            Projectile.height = 40;
            Projectile.tileCollide = true;
            Projectile.friendly = true; // Don't deal contact damage to enemies
            Projectile.minion = true;
            Projectile.DamageType = DamageClass.Summon; // Minion should benefit from Summon type bonuses
            Projectile.minionSlots = 1f;
            Projectile.penetrate = -1; // Don't despawn on hitting tiles or enemies
        }

        public override bool? CanCutTiles()
        {
            return true;
        }

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];

            if (!CheckActive(owner))
            {
                return;
            }

            GeneralBehavior(owner, out Vector2 vectorToIdlePosition, out float distanceToIdlePosition); // Behaviour that always applies like keeping track of idle position, teleporting to player if the minion is too far, and preventing minion overlap
            SearchForTargets(owner, out bool foundTarget, out float distanceFromTarget, out Vector2 targetCenter); // Behavior to find a target within range and line of sight of the minion
            MechWeaponsPlayer weaponsOwner = owner.GetModPlayer<MechWeaponsPlayer>();
            Attack(owner, weaponsOwner, foundTarget, distanceFromTarget, targetCenter, distanceToIdlePosition, vectorToIdlePosition); // Behavior for attacking enemies or idling near the player
            Visuals(); // Handle visual effects and animation
        }

        // Function to check if the minion should remain active, and despawns it if not
        private bool CheckActive(Player owner)
        {
            if (owner.dead || !owner.active)
            {
                owner.ClearBuff(ModContent.BuffType<DroneBuff>());

                return false;
            }

            if (owner.HasBuff(ModContent.BuffType<DroneBuff>()))
            {
                Projectile.timeLeft = 2;
            }

            return true;
        }

        // Function for general behavior, like idling near the player, teleporting to the player if too far away, and preventing overlap with other minions
        private void GeneralBehavior(Player owner, out Vector2 vectorToIdlePosition, out float distanceToIdlePosition)
        {
            // Set idle position of the minion
            Vector2 idlePosition = owner.Center;
            idlePosition.Y -= 120f; // 10 tiles above the player
            float minionPositionOffsetX = (Projectile.minionPos * 40) * -owner.direction; // Projectile.minionPos allows the minions to wander around while active
            idlePosition.X += minionPositionOffsetX;

            // All of this code below this line is adapted from Spazmamini code (ID 388, aiStyle 66)

            // Teleport to player if distance is too big
            // Get distance between current position and idle position
            vectorToIdlePosition = idlePosition - Projectile.Center;
            distanceToIdlePosition = vectorToIdlePosition.Length();
            if (Main.myPlayer == owner.whoAmI && distanceToIdlePosition > 2000f) // If the projectile is too far away from the player,
            {
                Projectile.tileCollide = false; // Disable tile collision temporarily
                Projectile.position = idlePosition; // Return to player
                Projectile.velocity *= 0.1f; // Reset projectile velocity
                Projectile.netUpdate = true; // Sync the projectile's position with the server (as the projectile is making a large distance change)
            }
            else
            {
                Projectile.tileCollide = true; // Enable tile collision
            }

            // Prevent overlap with other minions
            float overlapVelocity = 0.04f;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile other = Main.projectile[i];

                // If projectile (minion) is overlapping with another one of the projectile's type, push them away from each other
                if (i != Projectile.whoAmI && other.active && other.owner == Projectile.owner && Math.Abs(Projectile.position.X - other.position.X) + Math.Abs(Projectile.position.Y - other.position.Y) < Projectile.width)
                {
                    if (Projectile.position.X < other.position.X) // If to the left of the other minion,
                    {
                        Projectile.velocity.X -= overlapVelocity; // Push left
                    }
                    else // Otherwise,
                    {
                        Projectile.velocity.X += overlapVelocity; // Push right
                    }

                    if (Projectile.position.Y < other.position.Y) // If below the other minion,
                    {
                        Projectile.velocity.Y -= overlapVelocity; // Push down
                    }
                    else // Otherwise,
                    {
                        Projectile.velocity.Y += overlapVelocity; // Push up
                    }
                }
            }
        }

        // Function to find a target for the minion to attack
        private void SearchForTargets(Player owner, out bool foundTarget, out float distanceFromTarget, out Vector2 targetCenter)
        {
            distanceFromTarget = 700f; // Distance within which the minion can acquire targets
            targetCenter = Projectile.position;
            foundTarget = false;

            if (owner.HasMinionAttackTargetNPC) // If the player has a target marked,
            {
                NPC npc = Main.npc[owner.MinionAttackTargetNPC]; // Get the target
                float between = Vector2.Distance(npc.Center, Projectile.Center); // Get the distance between the target and the minion

                if (between < 2000f) // If the target is within 2000 pixels,
                {
                    distanceFromTarget = between; // Set the distance to the target
                    targetCenter = npc.Center; // Set the center of the target
                    foundTarget = true; // Notify that a target has been found
                }
            }

            if (!foundTarget) // If no target is currently found,
            {
                // Find a target from all NPCs
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];

                    if (npc.CanBeChasedBy()) // If the NPC is a valid target,
                    {
                        float between = Vector2.Distance(npc.Center, Projectile.Center); // Get the distance between the target and the minion
                        bool closest = Vector2.Distance(Projectile.Center, targetCenter) > between; // Check if it is closer than the current target
                        bool inRange = between < distanceFromTarget; // Check if within the search distance
                        bool lineOfSight = Collision.CanHitLine(Projectile.position, Projectile.width, Projectile.height, npc.position, npc.width, npc.height); // Check if the minion has line of sight to the target
                        bool closeThroughWall = between < 100f; // If the target is close enough, allow it to be targeted through walls

                        if (((closest && inRange) || !foundTarget) && (lineOfSight || closeThroughWall)) // If it is the closest target, within range, and has line of sight,
                        {
                            distanceFromTarget = between; // Set the distance to the target
                            targetCenter = npc.Center; // Set the center of the target
                            foundTarget = true; // Notify that a target has been found
                        }
                    }
                }
            }
        }

        private int bulletTimer = 0; // Timer to determine when the minion can shoot bullets again

        // Function for the minion's attack behavior
        private void Attack(Player owner, MechWeaponsPlayer weaponsOwner, bool foundTarget, float distanceFromTarget, Vector2 targetCenter, float distanceToIdlePosition, Vector2 vectorToIdlePosition)
        {
            // Default movement parameters
            float speed = 8f;
            float inertia = 20f;

            if (foundTarget) // If the minion has found a target,
            {
                // Get the direction to move towards the target
                Vector2 direction = targetCenter - Projectile.Center;
                direction.Normalize();
                direction *= speed;

                if (distanceFromTarget > 400f) // If the target is far away,
                {
                    Projectile.velocity = (Projectile.velocity * (inertia - 1) + direction) / inertia; // Move towards the target
                }
                if (distanceFromTarget < 200f) // If the target is very close,
                {
                    Projectile.velocity = (Projectile.velocity * (inertia - 1) - direction) / inertia; // Move away from the target
                }

                Item droneItem = new();
                droneItem.SetDefaults(ModContent.ItemType<DroneSpawner>()); // Create an instance of the item that spawned the minion to access its ammo type
                owner.PickAmmo(droneItem, out int projectileType, out float _, out int _, out float _, out int usedAmmo); // Set the projectile type to use corresponding ammo and get the ammo item ID
                Item ammoItem = new();
                ammoItem.SetDefaults(usedAmmo); // Create an instance of the ammo item to be able to be consumed
                if (owner.CountItem(usedAmmo) > 0) // If the player has ammo,
                {
                    // Bullet properties
                    int bulletDamage = weaponsOwner.DamageCalc(50, owner);
                    int bulletKnockback = weaponsOwner.KnockbackCalc(2, owner);
                    int bulletRate = 30;
                    int bulletProjSpeed = 10;

                    if (bulletTimer < bulletRate)
                        bulletTimer++;

                    if (bulletTimer >= bulletRate) // If the minion is able to shoot,
                    {
                        // Get the direction and velocity towards the target
                        Vector2 bulletVelocity = targetCenter - Projectile.Center;
                        bulletVelocity.Normalize();
                        bulletVelocity *= bulletProjSpeed;

                        // Create the bullet projectile
                        Projectile.NewProjectile(new EntitySource_Parent(Projectile), Projectile.Center, bulletVelocity, projectileType, bulletDamage, bulletKnockback, Projectile.owner);
                        SoundEngine.PlaySound(SoundID.Item11 with { Volume = 0.5f }, Projectile.position); // Play gun sound when shooting

                        bulletTimer = 0; // Reset shoot timer
                        if (ammoItem.maxStack > 1) // Only consume if the item isn't an "endless" ammo type
                            owner.ConsumeItem(usedAmmo); // Consume ammo
                    }
                }
            }
            else // Otherwise,
            {
                if (distanceToIdlePosition > 400f) // If the minion is far away from the player,
                {
                    // Speed up the minion to catch up
                    Projectile.tileCollide = false; // Disable tile collision temporarily
                    speed = 16f;
                    inertia = 60f;
                }
                else // Otherwise,
                {
                    // Slow down the minion if close to the player
                    Projectile.tileCollide = true; // Enable tile collision
                    speed = 4f;
                    inertia = 80f;
                }

                if (distanceToIdlePosition > 20f) // If the minion is very close to the player,
                {
                    // Move back towards the player
                    vectorToIdlePosition.Normalize();
                    vectorToIdlePosition *= speed;
                    Projectile.velocity = (Projectile.velocity * (inertia - 1) + vectorToIdlePosition) / inertia;
                }
                else if (Projectile.velocity == Vector2.Zero) // If the projectile is not moving,
                {
                    // Make it move very slightly so it doesn't completely stop
                    Projectile.velocity.X = -0.15f;
                    Projectile.velocity.Y = -0.05f;
                }
            }
        }

        // Function for handling the minion's visuals
        private void Visuals()
        {
            Projectile.rotation = Projectile.velocity.X * 0.05f; // Lean slighlty based on horizontal movement

            Projectile.spriteDirection = Projectile.direction; // Face the correct direction

            // Handle animation
            int frameSpeed = 5;

            Projectile.frameCounter++;

            if (Projectile.frameCounter >= frameSpeed)
            {
                Projectile.frameCounter = 0;
                Projectile.frame++;

                if (Projectile.frame >= Main.projFrames[Projectile.type])
                {
                    Projectile.frame = 0;
                }
            }
        }
    }

    public class DroneMissileProjectile : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_350"; // Use Missile texture

        private float speed = 10f; // Speed of the missile
        private float rotateSpeed = 0.2f; // Rotation speed of the missile

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.friendly = true; // Can hit enemies
            Projectile.ignoreWater = true; // Ignore water
            Projectile.light = 0.5f; // Produce light
            Projectile.timeLeft = 100;
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
            if (Projectile.owner == Main.myPlayer) // If the projectile is owned by the local player,
            {
                Projectile.netUpdate = true; // Sync the projectile's position with the server (Main.MouseWorld would take the client's mouse position, not tracking the actual projectile owner's mouse position, so syncing is necessary)
                Vector2 direction = Main.MouseWorld - Projectile.Center; // Get the direction the missile needs to head
                direction.Normalize(); // Normalise the direction
                Projectile.velocity.X = MathHelper.SmoothStep(Projectile.velocity.X, direction.X * speed, rotateSpeed); // Smoothly adjust the X velocity
                Projectile.velocity.Y = MathHelper.SmoothStep(Projectile.velocity.Y, direction.Y * speed, rotateSpeed); // Smoothly adjust the Y velocity
            }
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2; // Rotate to face the headed direction
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

    public class DroneBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            // If the minions exist reset the buff time, otherwise remove the buff from the player
            if (player.ownedProjectileCounts[ModContent.ProjectileType<DroneProjectile>()] > 0)
            {
                player.buffTime[buffIndex] = 18000;
            }
            else
            {
                player.DelBuff(buffIndex);
                buffIndex--;
            }
        }
    }
}
