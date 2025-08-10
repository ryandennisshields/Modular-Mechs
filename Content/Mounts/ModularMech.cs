using MechMod.Common.Players;
using MechMod.Content.Buffs;
using MechMod.Content.Dusts;
using MechMod.Content.Items.MechWeapons;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace MechMod.Content.Mounts
{
    public interface IMechParts
    {
        void ApplyStats(Player player, ModularMech mech);
    }

    public interface IMechWeapon
    {
        void SetStats(Player player);
        void UseAbility(Player player, Vector2 mousePosition, bool toggleOn);
        //public DamageClass damageClass { get; }
        //Weapons.UseType useType { get; }
    }

    public interface IMechModule
    {
        public enum ModuleSlot
        {
            Passive,
            Active
        }
        public ModuleSlot moduleSlot { get; }

        public enum ModuleType
        {
            Persistent,
            OnMount,
            OnDismount
        }
        public ModuleType moduleType { get; }

        void ModuleEffect(ModularMech mech, Player player);
    }

    public class ModularMech : ModMount
    {
        public bool allowDown;

        public override void SetStaticDefaults()
        {
            // Misc
            MountData.fallDamage = 0;
            MountData.constantJump = true;
            MountData.blockExtraJumps = true;
            //MountData.buff = ModContent.BuffType<MechBuff>();
            // Effects
            MountData.spawnDust = DustID.Smoke;
            // Frame data and player offsets
            MountData.totalFrames = 4; // Although the actual frame count is different, animations breaks if this value is any higher or lower
            MountData.heightBoost = 41; // Height between the mount and the ground (player's hitbox position)
            MountData.playerYOffsets = Enumerable.Repeat(41, MountData.totalFrames).ToArray(); // Fills an array with values for less repeating code
            MountData.playerHeadOffset = 43; // Changes player's head position (mainly to show the correct player head position on map)
            // Standing
            // All set to 0 as there is no standing animation
            MountData.standingFrameCount = 0;
            MountData.standingFrameDelay = 0;
            MountData.standingFrameStart = 0;
            // Running
            MountData.runningFrameCount = 7;
            MountData.runningFrameDelay = 25;
            MountData.runningFrameStart = 1;
            // Flying
            MountData.flyingFrameCount = 1;
            MountData.flyingFrameDelay = 0;
            MountData.flyingFrameStart = 8;
            // In-air
            MountData.inAirFrameCount = 1;
            MountData.inAirFrameDelay = 0;
            MountData.inAirFrameStart = 9;
            // Idle
            // All set to 0 as there is no idle animation
            MountData.idleFrameCount = 0;
            MountData.idleFrameDelay = 0;
            MountData.idleFrameStart = 0;
            MountData.idleFrameLoop = true;
            // Swim
            MountData.swimFrameCount = MountData.inAirFrameCount;
            MountData.swimFrameDelay = MountData.inAirFrameDelay;
            MountData.swimFrameStart= MountData.inAirFrameStart;

            MountData.acceleration = 0f;
            MountData.runSpeed = 0f;
            MountData.swimSpeed = 0f;

            MountData.jumpHeight = 0;
            MountData.jumpSpeed = 0f;

            MountData.dashSpeed = 0f;

            MountData.flightTimeMax = 0;
            allowDown = false;

            if (!Main.dedServ)
            {
                MountData.textureWidth = MountData.backTexture.Width();
                MountData.textureHeight = MountData.backTexture.Height();
            }
        }

        public int lifeBonus;
        private bool grantedLifeBonus;

        public int armourBonus;

        public override void SetMount(Player player, ref bool skipDust)
        {
            MechModPlayer modPlayer = player.GetModPlayer<MechModPlayer>();

            if (modPlayer.powerCellActive) // Give the player the mech buff for a set duration (longer if the player has a power cell active)
                player.AddBuff(ModContent.BuffType<MechBuff>(), 7200);
            else
                player.AddBuff(ModContent.BuffType<MechBuff>(), 3600);

            SoundEngine.PlaySound(SoundID.Research, player.position); // Play Research sound when mounting the mech

            // Hide the Player
            player.opacityForAnimation = 0;

            if (modPlayer.equippedParts[MechMod.weaponIndex].ModItem is IMechWeapon weapon)
                weapon.SetStats(player); // Set the weapon stats based on the equipped weapon

            //Weapons.DamageClass = modPlayer.equippedParts[MechMod.weaponIndex].ModItem is IMechWeapon weapon ? weapon.damageClass : DamageClass.Generic;

            // Apply Part Stats
            ApplyPartStats(player, modPlayer.equippedParts[MechMod.headIndex], modPlayer.equippedParts[MechMod.bodyIndex], modPlayer.equippedParts[MechMod.armsIndex], modPlayer.equippedParts[MechMod.legsIndex], modPlayer.equippedParts[MechMod.boosterIndex]);

            foreach (var part in player.GetModPlayer<MechModPlayer>().equippedParts)
            {
                if (part.ModItem is IMechModule mechModule)
                {
                    if (mechModule.moduleType == IMechModule.ModuleType.OnMount)
                    {
                        mechModule.ModuleEffect(this, player); // Apply the module effect on mount
                    }
                }
            }
        }

        public int mechDebuffDuration; // Duration that the player can't resummon the mech and is debuffed for
        public int launchForce; // Force applied to the player when dismounting the mech

        public override void Dismount(Player player, ref bool skipDust)
        {
            var modPlayer = player.GetModPlayer<MechModPlayer>();

            player.ClearBuff(ModContent.BuffType<MechBuff>()); // Clear the mech buff

            SoundEngine.PlaySound(SoundID.Research, player.position); // Play Research sound when dismounting the mech

            mechDebuffDuration = 900;
            launchForce = -10;

            foreach (var part in player.GetModPlayer<MechModPlayer>().equippedParts)
            {
                if (part.ModItem is IMechModule mechModule)
                {
                    if (mechModule.moduleType == IMechModule.ModuleType.OnDismount)
                    {
                        mechModule.ModuleEffect(this, player); // Apply the module effect on dismount
                    }
                }
            }

            // Debuff the Player
            int mechDebuff = ModContent.BuffType<MechDebuff>();
            player.AddBuff(mechDebuff, (int)(mechDebuffDuration / // Debuff duration scales higher as the player's health gets lower relative to max health
                ((float)player.statLife <= (float)player.statLifeMax2 * 0.25 ? // If statement for making sure the debuff duration isn't too punishing at low health
                ((float)player.statLifeMax2 * 0.25) / (float)player.statLifeMax2
                : (float)player.statLife / (float)player.statLifeMax2)));
            player.opacityForAnimation = 1; // Make Player visible
            player.velocity.Y = launchForce; // Launch the Player upwards

            // Reset stat values
            lifeBonus = 0;
            armourBonus = 0;
        }

        // Seperate variables for the mount's jump and run speeds for ground and flight states
        public float groundJumpSpeed = 0f;
        public float groundHorizontalSpeed = 0f;
        public float flightJumpSpeed = 0f;
        public float flightHorizontalSpeed = 0f;

        public override void UpdateEffects(Player player)
        {
            var modPlayer = player.GetModPlayer<MechModPlayer>();

            if (player.mount._frameState == Mount.FrameFlying)
            {
                // Disable player's ability to hover while flying
                if (!allowDown)
                    player.controlDown = false;

                MountData.jumpSpeed = flightJumpSpeed;
                MountData.runSpeed = flightHorizontalSpeed;
                MountData.swimSpeed = flightHorizontalSpeed;
            }
            else
            {
                MountData.jumpSpeed = groundJumpSpeed;
                MountData.runSpeed = groundHorizontalSpeed;
                MountData.swimSpeed = groundHorizontalSpeed;
            }

            Effects(player, modPlayer);

            // Grant life bonus
            player.statLifeMax2 += lifeBonus;
            if (grantedLifeBonus == false)
            {
                player.statLife += lifeBonus; // Increase player's health to match new max health
                grantedLifeBonus = true;
            }
            // Grant armour bonus
            player.statDefense += armourBonus;

            if (modPlayer.equippedParts[MechMod.weaponIndex].ModItem is IMechWeapon weapon)
            {
                if (Weapons.canUse)
                {
                    if (modPlayer.animationTimer > 0 || modPlayer.animationProgress > 0) // Only run animations if timer or progress is active
                    {
                        if (Weapons.useType == Weapons.UseType.Swing) // Constantly update animation
                        {
                            WeaponUseAnimation(player, Weapons.UseType.Swing, weapon);
                        }
                        else if (Weapons.useType == Weapons.UseType.Point) // Only animate for one frame
                        {
                            if (!modPlayer.animateOnce)
                            {
                                WeaponUseAnimation(player, Weapons.UseType.Point, weapon);
                                modPlayer.animateOnce = true;
                            }
                        }
                    }
                    if (Weapons.timer >= Weapons.attackRate)
                    {
                        modPlayer.animateOnce = false; // Reset the animate once bool when the weapon is ready to attack again
                    }
                }
                if (!Main.mouseLeft && modPlayer.animationTimer <= 0 && modPlayer.animationProgress <= 0)
                {
                    modPlayer.armFrame = -1; // Reset the arm frame to default
                    modPlayer.weaponScale = 0f; // Hide the weapon when not in use
                    modPlayer.useDirection = 0; // Reset last use direction
                }
            }

            // Sync animations between clients
            //if (Main.netMode == NetmodeID.MultiplayerClient)
            //{
            //    ModPacket packet = Mod.GetPacket();
            //    packet.Write((byte)MechMod.MessageType.WeaponAnimationSync);
            //    packet.Write((byte)player.whoAmI);
            //    packet.Write(modPlayer.armFrame);
            //    packet.Write(modPlayer.animationTimer);
            //    packet.Write(modPlayer.animationProgress);
            //    packet.Write(modPlayer.useDirection);
            //    packet.Send();
            //}

            foreach (var part in player.GetModPlayer<MechModPlayer>().equippedParts)
            {
                if (part.ModItem is IMechModule mechModule)
                {
                    if (mechModule.moduleType == IMechModule.ModuleType.Persistent)
                    {
                        mechModule.ModuleEffect(this, player); // Apply the module effect while mech is active
                    }
                }
            }
        }

        private int boosterTimer = 0;

        private int stepTimer = 0;
        private bool changeposition = false;

        private void Effects(Player player, MechModPlayer modPlayer)
        {
            #region Booster

            DashPlayer dashPlayer = player.GetModPlayer<DashPlayer>();
            int boosterDuration = 30;

            if (modPlayer.equippedParts[MechMod.boosterIndex] != null && dashPlayer.dashActive || player.mount._frameState == Mount.FrameInAir || player.mount._frameState == Mount.FrameFlying)
            {
                if (boosterTimer < boosterDuration)
                    boosterTimer++;
                if (boosterTimer >= boosterDuration)
                {
                    if (dashPlayer.dashActive || player.mount._frameState == Mount.FrameFlying)
                    {
                        SoundEngine.PlaySound(SoundID.Item13, player.position); // Play Rocket Boots/Jetpack sound for Booster use
                        boosterTimer = 0; // Reset the timer
                    }
                }

                float dustSpeedX = 0;
                float dustSpeedY = 0;

                if (dashPlayer.dashActive)
                {
                    dustSpeedX = 5;
                    dustSpeedY = 0;
                }
                else if (player.mount._frameState == Mount.FrameInAir || player.mount._frameState == Mount.FrameFlying)
                {
                    dustSpeedX = 0;
                    dustSpeedY = player.controlJump ? 14 : 8;
                }
                else
                {
                    dustSpeedX = 0;
                    dustSpeedY = 0;
                }

                float dustCenterXLeft = 8;
                float dustCenterXRight = 2;

                float dustOffsetX = 0;

                for (int i = 0; i < 20; i++)
                {
                    switch (i)
                    {
                        case 0:
                            dustOffsetX = -8;
                            break;
                        case 5:
                            dustOffsetX = 8;
                            break;
                        case 10:
                            dustOffsetX = -8;
                            break;
                        case 15:
                            dustOffsetX = 8;
                            break;
                    }
                    float posX = player.direction == -1 ? player.position.X + dustCenterXLeft + dustOffsetX : player.position.X + dustCenterXRight + dustOffsetX;
                    float posY = (i < 10) ? player.position.Y - 5 : player.position.Y + 15;
                    //int dust = Dust.NewDust(new Vector2(posX, posY), 1, 1, DustID.Flare, dustSpeedX, dustSpeedY);
                    int dust = Dust.NewDust(new Vector2(posX, posY), 1, 1, ModContent.DustType<BoosterDust>(), dustSpeedX, dustSpeedY);
                    Main.dust[dust].customData = player; // Use custom data to hide dust if behind Mech
                }
            }
            else
            {
                boosterTimer = boosterDuration;
            }

            #endregion

            #region Step

            float stepSpeed = 30 / (player.velocity.Length() / 2);
            int positionRight = player.direction == -1 ? 6 : 2;
            int positionLeft = player.direction == -1 ? -14 : -20;

            if (stepTimer < stepSpeed)
                stepTimer++;
            if (stepTimer >= stepSpeed && player.mount._frameState == Mount.FrameRunning)
            {
                SoundEngine.PlaySound(SoundID.NPCHit3, player.position); // Play Dig sound for step use
                if (!changeposition)
                {
                    for (int i = 0; i < 15; i++)
                        Dust.NewDust(new Vector2(player.position.X + positionRight, player.position.Y + 80), 30, 1, DustID.Smoke, player.velocity.X * 0.2f, player.velocity.Y * 0.2f); // Create dust when running
                    changeposition = true;
                }
                else
                {
                    for (int i = 0; i < 15; i++)
                        Dust.NewDust(new Vector2(player.position.X + positionLeft, player.position.Y + 80), 30, 1, DustID.Smoke, player.velocity.X * 0.2f, player.velocity.Y * 0.2f); // Create dust when running
                    changeposition = false;
                }
                stepTimer = 0;
            }

            #endregion
        }

        //private Texture2D headTexture;
        //private Texture2D bodyTexture;
        //private Texture2D armsTexture;
        //private Texture2D legsTexture;
        //private Texture2D weaponTexture; // Used so the equipped weapon can be drawn while in use

        //private int armFrame = -1; // Used for controlling the current arm frame
        //private int armAnimationFrames = 11; // Total number of frames that the arm texture has (to include the many arm rotations/positions for weapon animation)

        //private Vector2 weaponPosition = Vector2.Zero; // Used for positioning the weapon when it is drawn
        //private float weaponRotation = 0f; // Used for rotating the weapon when it is drawn
        //private Vector2 weaponOrigin = Vector2.Zero; // Used so a different origin can be set for rotation
        //private float weaponScale = 1f; // Used so the weapon can be hidden when needed
        //private SpriteEffects weaponSpriteEffects = SpriteEffects.None; // Used so the weapon's sprite can be flipped when needed

        public override bool Draw(List<DrawData> playerDrawData, int drawType, Player drawPlayer, ref Texture2D texture, ref Texture2D glowTexture, ref Vector2 drawPosition, ref Rectangle frame, ref Color drawColor, ref Color glowColor, ref float rotation, ref SpriteEffects spriteEffects, ref Vector2 drawOrigin, ref float drawScale, float shadow)
        {
            //grantedLifeBonus = false;
            //armourBonus = 0;
            //Weapons.partDamageBonus = 0;
            //Weapons.partAttackSpeedBonus = 0;

            if (drawType == 0)
            {

                // Apply visuals to the Mech
                //var modPlayer = drawPlayer.GetModPlayer<MechModPlayer>();
                //if (!modPlayer.equippedParts[MechMod.headIndex].IsAir)
                //    modPlayer.headTexture = Mod.Assets.Request<Texture2D>($"Content/Items/MechHeads/{modPlayer.equippedParts[MechMod.headIndex].ModItem.GetType().Name}Visual").Value;
                //if (!modPlayer.equippedParts[MechMod.bodyIndex].IsAir)
                //    modPlayer.bodyTexture = Mod.Assets.Request<Texture2D>($"Content/Items/MechBodies/{modPlayer.equippedParts[MechMod.bodyIndex].ModItem.GetType().Name}Visual").Value;
                //if (!modPlayer.equippedParts[MechMod.armsIndex].IsAir)
                //    modPlayer.armsTexture = Mod.Assets.Request<Texture2D>($"Content/Items/MechArms/{modPlayer.equippedParts[MechMod.armsIndex].ModItem.GetType().Name}Visual").Value;
                //if (!modPlayer.equippedParts[MechMod.legsIndex].IsAir)
                //    modPlayer.legsTexture = Mod.Assets.Request<Texture2D>($"Content/Items/MechLegs/{modPlayer.equippedParts[MechMod.legsIndex].ModItem.GetType().Name}Visual").Value;

                // Apply visuals to the Mech (TEMPORARY)
                var modPlayer = drawPlayer.GetModPlayer<MechModPlayer>();
                if (!modPlayer.equippedParts[MechMod.headIndex].IsAir)
                    modPlayer.headTexture = Mod.Assets.Request<Texture2D>($"Content/Items/MechHeads/BaseHeadVisual").Value;
                if (!modPlayer.equippedParts[MechMod.bodyIndex].IsAir)
                    modPlayer.bodyTexture = Mod.Assets.Request<Texture2D>($"Content/Items/MechBodies/BaseBodyVisual").Value;
                if (!modPlayer.equippedParts[MechMod.armsIndex].IsAir)
                {
                    modPlayer.armsRTexture = Mod.Assets.Request<Texture2D>($"Content/Items/MechArms/BaseArmsRVisual").Value;
                    modPlayer.armsLTexture = Mod.Assets.Request<Texture2D>($"Content/Items/MechArms/BaseArmsLVisual").Value;
                }
                if (!modPlayer.equippedParts[MechMod.legsIndex].IsAir)
                {
                    modPlayer.legsRTexture = Mod.Assets.Request<Texture2D>($"Content/Items/MechLegs/BaseLegsRVisual").Value;
                    modPlayer.legsLTexture = Mod.Assets.Request<Texture2D>($"Content/Items/MechLegs/BaseLegsLVisual").Value;
                }

                if (!modPlayer.equippedParts[MechMod.weaponIndex].IsAir)
                    modPlayer.weaponTexture = Mod.Assets.Request<Texture2D>($"Content/Items/MechWeapons/{modPlayer.equippedParts[MechMod.weaponIndex].ModItem.GetType().Name}").Value;
                else
                    modPlayer.weaponTexture = null; // If no weapon is equipped, set the weapon texture to null

                Rectangle setArmFrame = frame; // Get the default frame logic as a new rectangle
                if (modPlayer.armFrame >= 0) // If the arm frame is manually set,
                {
                    int frameHeight = modPlayer.armsLTexture.Height / modPlayer.armAnimationFrames; // Calculate the height of each frame based on the total height of the texture and the number of frames
                    setArmFrame = new Rectangle(0, modPlayer.armFrame * frameHeight, modPlayer.armsLTexture.Width, frameHeight); // Change the set arm frame to a new rectangle based on the arm frame and the height of each frame
                }

                int visualDirection = modPlayer.useDirection != 0 ? modPlayer.useDirection : drawPlayer.direction; // Use the use direction if it is not 0, otherwise use the player's direction
                spriteEffects = visualDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

                // Positions should be universal for each Mech Part
                // If a Part is misaligned, the position of the sprite on it's canvas should be modified to fit (Base Parts are the "default" position, right in the middle of their canvas)

                // Draw right arm first
                playerDrawData.Add(new DrawData(modPlayer.armsRTexture, drawPosition + new Vector2(23 * visualDirection, -30), frame, drawColor, rotation, drawOrigin, drawScale, spriteEffects));

                // Draw right leg
                playerDrawData.Add(new DrawData(modPlayer.legsRTexture, drawPosition + new Vector2(18 * visualDirection, 18), frame, drawColor, rotation, drawOrigin, drawScale, spriteEffects));

                // Draw body
                playerDrawData.Add(new DrawData(modPlayer.bodyTexture, drawPosition + new Vector2(0, -13), frame, drawColor, rotation, drawOrigin, drawScale, spriteEffects));

                // Draw left leg
                playerDrawData.Add(new DrawData(modPlayer.legsLTexture, drawPosition + new Vector2(-8 * visualDirection, 18), frame, drawColor, rotation, drawOrigin, drawScale, spriteEffects));

                // Draw head
                playerDrawData.Add(new DrawData(modPlayer.headTexture, drawPosition + new Vector2(4 * visualDirection, -40), frame, drawColor, rotation, drawOrigin, drawScale, spriteEffects));

                // Draw weapon
                if (modPlayer.weaponTexture != null)
                    playerDrawData.Add(new DrawData(modPlayer.weaponTexture, drawPosition + modPlayer.weaponPosition, null, drawColor, modPlayer.weaponRotation, modPlayer.weaponOrigin, modPlayer.weaponScale, modPlayer.weaponSpriteEffects));

                // Draw left arm last

                // WEIRD BAND AID:
                // For some reason, very specifically when the arm frame is manually set (like in weapon usage) *and* the player is facing left,
                // the position of the arm is off by -10 pixels on the X axis, so the position needs to be adjusted for that specific case
                playerDrawData.Add(new DrawData(modPlayer.armsLTexture, drawPosition + new Vector2(modPlayer.armFrame >= 0 ? visualDirection == -1 ? 33 : -23 : -23 * visualDirection, -30), setArmFrame, drawColor, rotation, drawOrigin, drawScale, spriteEffects));
            }

            return false;
        }

        public override void UseAbility(Player player, Vector2 mousePosition, bool toggleOn)
        {
            // Block weapon usage if mousing over any user interface element
            if (player.mouseInterface)
                return;

            var modPlayer = player.GetModPlayer<MechModPlayer>();

            if (modPlayer.equippedParts[MechMod.weaponIndex].ModItem is IMechWeapon weapon)
            {
                if (player.whoAmI == Main.myPlayer && Main.mouseLeft && Weapons.timer >= Weapons.attackRate) // Attack when ready
                {
                    if (Main.MouseWorld.X > player.MountedCenter.X)
                        modPlayer.useDirection = 1;
                    else
                        modPlayer.useDirection = -1;
                    if (!player.controlLeft || !player.controlRight)
                        player.direction = modPlayer.useDirection; // Set the player's direction to the last use direction if not controlling horizontal movement
                    weapon.UseAbility(player, mousePosition, toggleOn);
                    if (Weapons.canUse)
                    {
                        Weapons.timer = 0;
                    }
                }

                if (Weapons.timer < Weapons.attackRate)
                    Weapons.timer++; // Increment the timer until it reaches the attack rate
            }
            else
            {
                return;
            }
        }

        // Function for weapon use animations like pointing and swinging 
        private void WeaponUseAnimation(Player player, Weapons.UseType useType, IMechWeapon weapon)
        {
            var modPlayer = player.GetModPlayer<MechModPlayer>();

            int weaponPositionX = 0; // X position of the weapon
            int weaponPositionY = 0; // Y position of the weapon

            int weaponOriginOffsetX = 0; // X offset of the weapon origin
            int weaponOriginOffsetY = 0; // Y offset of the weapon origin

            switch (useType)
            {
                case Weapons.UseType.Point:

                    modPlayer.weaponScale = 1f;

                    weaponPositionX = 3;
                    weaponPositionY = -55;

                    weaponOriginOffsetX = -26;
                    weaponOriginOffsetY = 0;

                    if (Main.mouseLeft) // Only rotate on fire
                    {
                        float pointAngle = (Main.MouseWorld - player.MountedCenter).ToRotation(); // Get the angle between the mouse position and the player mounted center
                        float pointAngleDeg = MathHelper.ToDegrees(pointAngle); // Convert the angle to degrees for easier calculations
                        modPlayer.weaponRotation = pointAngle; // Set the weapon rotation to the angle between the mouse and the player

                        // Check if the mouse is at different angles relative to the player (so if mouse is pointing up, the arm will point up, if mouse is pointing down, the arm will point down, etc.)
                        if (pointAngleDeg >= -135 && pointAngleDeg <= -45)
                        {
                            modPlayer.armFrame = 13; // Pointing angled up
                        }
                        else if (pointAngleDeg >= -45 && pointAngleDeg <= 45)
                        {
                            modPlayer.armFrame = 12; // Pointing horizontal right
                        }
                        else if (pointAngleDeg <= -135 || pointAngleDeg >= 135)
                        {
                            modPlayer.armFrame = 12; // Pointing horizontal left
                        }
                        else if (pointAngleDeg <= 135 && pointAngleDeg >= 45)
                        {
                            modPlayer.armFrame = 11; // Pointing angled down
                        }
                    }
                    break;
                case Weapons.UseType.Swing:

                    modPlayer.weaponScale = 1f;

                    weaponPositionX = -10;
                    weaponPositionY = -45;

                    weaponOriginOffsetX = -70;
                    weaponOriginOffsetY = 45 * modPlayer.useDirection;

                    if (player.whoAmI == Main.myPlayer) // Progress calculation should only be done on the client player
                    {
                        float progress = 1f - (modPlayer.animationProgress / Weapons.attackRate); // Calculate the progress of the animation based on the animation progress (equal to projectile's life time) and attack rate

                        modPlayer.armFrame = (int)MathHelper.Lerp(15, 11, progress); // Lerp the arm through the up, angled up, horizontal and angled down frames respectively (11 is used as starting value to make the up frame last longer)

                        // Calculate the swing's starting and ending angle and lerp it between the angle
                        float startAngle = -2f * modPlayer.useDirection;
                        float endAngle = 1.5f * modPlayer.useDirection;
                        float swingAngle = MathHelper.Lerp(startAngle, endAngle, progress);

                        modPlayer.weaponRotation = swingAngle + (modPlayer.useDirection == -1 ? MathHelper.Pi : 0f); // Set the weapon rotation to the swing angle (that also changes depending on last use direction)
                    }
                    break;
            }
            modPlayer.weaponPosition = new Vector2(weaponPositionX * modPlayer.useDirection, weaponPositionY); // Setting a new position lets a weapon be futher out or further up from the player
            modPlayer.weaponOrigin = new Vector2(modPlayer.weaponTexture.Width / 2 + weaponOriginOffsetX, modPlayer.weaponTexture.Height / 2 + weaponOriginOffsetY); // Setting a new origin gives the weapon a different rotation point
            modPlayer.weaponSpriteEffects = modPlayer.useDirection == -1 ? SpriteEffects.FlipVertically : SpriteEffects.None; // Flip the weapon sprite based on the player's direction
        }

        public void ApplyPartStats(Player player, Item equippedHead, Item equippedBody, Item equippedArms, Item equippedLegs, Item equippedBooster)
        {
            var modPlayer = player.GetModPlayer<MechModPlayer>();

            // Reset weapon stats that are added/multiplied before applying new ones
            Weapons.partDamageBonus = 0f;
            Weapons.partCritChanceBonus = 0f;
            Weapons.partAttackSpeedBonus = 0f;
            Weapons.partKnockbackBonus = 0f;

            if (!equippedBody.IsAir)
                if (modPlayer.equippedParts[MechMod.bodyIndex].ModItem is IMechParts body)
                    body.ApplyStats(player, this);
            if (!equippedArms.IsAir)
                if (modPlayer.equippedParts[MechMod.armsIndex].ModItem is IMechParts arms)
                    arms.ApplyStats(player, this);
            if (!equippedLegs.IsAir)
                if (modPlayer.equippedParts[MechMod.legsIndex].ModItem is IMechParts legs)
                    legs.ApplyStats(player, this);
            if (!equippedBooster.IsAir)
            {
                if (modPlayer.equippedParts[MechMod.boosterIndex].ModItem is IMechParts booster)
                    booster.ApplyStats(player, this);
            }
            else
            {
                // Unique as a player can go without a Booster, but the other Parts are required

                MountData.flightTimeMax = 0;
                MountData.usesHover = false;

                player.GetModPlayer<DashPlayer>().ableToDash = false;
                player.GetModPlayer<DashPlayer>().dashCoolDown = 0;
                player.GetModPlayer<DashPlayer>().dashDuration = 0;
                player.GetModPlayer<DashPlayer>().dashVelo = 0f;

                if (modPlayer.powerCellActive)
                    lifeBonus += 100;
            }
            // Apply head stats last as it can have multiplicative effects
            if (!equippedHead.IsAir)
                if (modPlayer.equippedParts[MechMod.headIndex].ModItem is IMechParts head)
                    head.ApplyStats(player, this);
        }

        public class DashPlayer : ModPlayer
        {
            public bool ableToDash;
            public bool dashActive; // Used to check if the player is currently dashing

            public int dashCoolDown;
            public int dashDuration;

            public float dashVelo;

            private int dashDelay = 0;
            private int dashTimer = 0; // CAN BE USED LATER FOR EFFECTS MID-DASH (See ExampleMod's Shield Accessory)

            public override void PreUpdateMovement()
            {
                if (ableToDash && Player.mount.Active && Player.mount.Type == ModContent.MountType<ModularMech>())
                {
                    if (MechMod.MechDashKeybind.JustPressed && dashDelay == 0)
                    {
                        int dashDir = 0;
                        if (Player.controlRight)
                            dashDir = 1;
                        else if (Player.controlLeft)
                            dashDir = -1;
                        else
                            dashDir = Player.direction;

                        float newVelo = Player.velocity.X;
                        newVelo = dashDir * dashVelo;

                        dashDelay = dashCoolDown;
                        dashTimer = dashDuration;
                        Player.velocity.X = newVelo;
                        //Player.mount._frameState = Mount.FrameDashing;
                    }

                    if (dashDelay > 0)
                        dashDelay--;
                }

                if (dashTimer > 0)
                {
                    dashTimer--;
                    dashActive = true;
                    if (dashTimer <= 0)
                    {
                        dashActive = false;
                        if (Player.direction == 1 ? Player.velocity.X > Player.mount.RunSpeed : Player.velocity.X < -Player.mount.RunSpeed)
                            Player.velocity.X -= Player.mount.RunSpeed * Player.direction; // Stop the dash when the timer runs out
                    }
                }
            }

        }
    }
}
