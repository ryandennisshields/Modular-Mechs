using MechMod.Common.Players;
using MechMod.Content.Buffs;
using MechMod.Content.Dusts;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;

using Terraria.ModLoader;

namespace MechMod.Content.Mounts
{
    public interface IMechParts
    {
        void ApplyStats(Player player, MechModPlayer modPlayer, MechWeaponsPlayer weaponsPlayer, ModularMech mech);

        void BodyOffsets(MechVisualPlayer visualPlayer, string body);
    }

    public interface IMechWeapon
    {
        void SetStats(MechWeaponsPlayer weaponsPlayer);

        void UseAbility(Player player, MechWeaponsPlayer weaponsPlayer, MechVisualPlayer visualPlayer, Vector2 mousePosition, bool toggleOn);
    }

    public interface IMechModule
    {
        public enum ModuleSlot
        {
            Passive,
            Active
        }
        public ModuleSlot MSlot { get; }

        public enum ModuleType
        {
            Persistent,
            OnMount,
            OnDismount
        }
        public ModuleType MType { get; }

        void ModuleEffect(ModularMech mech, Player player, MechModPlayer modPlayer, MechWeaponsPlayer weaponsPlayer);
    }

    public class ModularMech : ModMount
    {
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
            MountData.totalFrames = 1; // Although the actual frame count is different, animations breaks if this value is any higher or lower
            MountData.heightBoost = 41; // Height between the mount and the ground (player's hitbox position)
            MountData.playerYOffsets = [.. Enumerable.Repeat(41, MountData.totalFrames)]; // Fills an array with values for less repeating code
            MountData.playerHeadOffset = 43; // Changes player's head position (mainly to show the correct player head position on map)
            // Standing
            // All set to 0 as there is no standing animation
            MountData.standingFrameCount = 0;
            MountData.standingFrameDelay = 0;
            MountData.standingFrameStart = 0;
            // Running
            MountData.runningFrameCount = 6;
            MountData.runningFrameDelay = 25;
            MountData.runningFrameStart = 1;
            // Flying
            MountData.flyingFrameCount = 1;
            MountData.flyingFrameDelay = 0;
            MountData.flyingFrameStart = 7;
            // In-air
            MountData.inAirFrameCount = 1;
            MountData.inAirFrameDelay = 0;
            MountData.inAirFrameStart = 8;
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

            if (!Main.dedServ)
            {
                MountData.textureWidth = 150;
                MountData.textureHeight = 150;
            }
        }

        public override void SetMount(Player player, ref bool skipDust)
        {
            MechModPlayer modPlayer = player.GetModPlayer<MechModPlayer>();
            MechVisualPlayer visualPlayer = player.GetModPlayer<MechVisualPlayer>();
            MechWeaponsPlayer weaponsPlayer = player.GetModPlayer<MechWeaponsPlayer>();

            modPlayer.allowDown = false;

            if (modPlayer.powerCellActive) // Give the player the mech buff for a set duration (longer if the player has a power cell active)
                player.AddBuff(ModContent.BuffType<MechBuff>(), 5400);
            else
                player.AddBuff(ModContent.BuffType<MechBuff>(), 2700);

            SoundEngine.PlaySound(SoundID.Research, player.position); // Play Research sound when mounting the mech

            // Hide the Player
            player.opacityForAnimation = 0;

            ApplyParts(player, modPlayer, visualPlayer, weaponsPlayer);

            modPlayer.grantedLifeBonus = false;

            foreach (var part in player.GetModPlayer<MechModPlayer>().equippedParts)
            {
                if (part.ModItem is IMechModule mechModule)
                {
                    if (mechModule.MType == IMechModule.ModuleType.OnMount)
                    {
                        mechModule.ModuleEffect(this, player, modPlayer, weaponsPlayer); // Apply the module effect on mount
                    }
                }
            }
        }

        private void ApplyParts(Player player, MechModPlayer modPlayer, MechVisualPlayer visualPlayer, MechWeaponsPlayer weaponsPlayer)
        {
            // Apply Part Stats (
            ApplyPartStats(player, modPlayer.equippedParts[MechMod.headIndex], modPlayer.equippedParts[MechMod.bodyIndex], modPlayer.equippedParts[MechMod.armsIndex], modPlayer.equippedParts[MechMod.legsIndex], modPlayer.equippedParts[MechMod.boosterIndex]);

            // Apply Weapon Stats (denotes a weapon's use type and damage class)
            if (modPlayer.equippedParts[MechMod.weaponIndex].ModItem is IMechWeapon weapon)
                weapon.SetStats(weaponsPlayer);

            // Apply Body Offsets
            foreach (var part in player.GetModPlayer<MechModPlayer>().equippedParts)
            {
                if (part.ModItem is IMechParts mechPart)
                {
                    mechPart.BodyOffsets(visualPlayer, modPlayer.equippedParts[MechMod.bodyIndex].ModItem.GetType().Name);
                }
            }

            // Apply Part Visuals
            if (!modPlayer.equippedParts[MechMod.headIndex].IsAir)
            {
                string headPath = modPlayer.powerCellActive
                    ? $"Content/Items/MechHeads/{modPlayer.equippedParts[MechMod.headIndex].ModItem.GetType().Name}Visual"
                    : $"Content/Items/MechHeads/Pre{modPlayer.equippedParts[MechMod.headIndex].ModItem.GetType().Name}Visual";
                visualPlayer.headTexture = Mod.Assets.Request<Texture2D>(headPath);
            }

            if (!modPlayer.equippedParts[MechMod.bodyIndex].IsAir)
            {
                string bodyPath = modPlayer.powerCellActive
                    ? $"Content/Items/MechBodies/{modPlayer.equippedParts[MechMod.bodyIndex].ModItem.GetType().Name}Visual"
                    : $"Content/Items/MechBodies/Pre{modPlayer.equippedParts[MechMod.bodyIndex].ModItem.GetType().Name}Visual";
                visualPlayer.bodyTexture = Mod.Assets.Request<Texture2D>(bodyPath);
            }

            if (!modPlayer.equippedParts[MechMod.armsIndex].IsAir)
            {
                string armsR = modPlayer.powerCellActive
                    ? $"Content/Items/MechArms/{modPlayer.equippedParts[MechMod.armsIndex].ModItem.GetType().Name}RVisual"
                    : $"Content/Items/MechArms/Pre{modPlayer.equippedParts[MechMod.armsIndex].ModItem.GetType().Name}RVisual";
                string armsL = modPlayer.powerCellActive
                    ? $"Content/Items/MechArms/{modPlayer.equippedParts[MechMod.armsIndex].ModItem.GetType().Name}LVisual"
                    : $"Content/Items/MechArms/Pre{modPlayer.equippedParts[MechMod.armsIndex].ModItem.GetType().Name}LVisual";
                visualPlayer.armsRTexture = Mod.Assets.Request<Texture2D>(armsR);
                visualPlayer.armsLTexture = Mod.Assets.Request<Texture2D>(armsL);
            }

            if (!modPlayer.equippedParts[MechMod.legsIndex].IsAir)
            {
                string legsR = modPlayer.powerCellActive
                    ? $"Content/Items/MechLegs/{modPlayer.equippedParts[MechMod.legsIndex].ModItem.GetType().Name}RVisual"
                    : $"Content/Items/MechLegs/Pre{modPlayer.equippedParts[MechMod.legsIndex].ModItem.GetType().Name}RVisual";
                string legsL = modPlayer.powerCellActive
                    ? $"Content/Items/MechLegs/{modPlayer.equippedParts[MechMod.legsIndex].ModItem.GetType().Name}LVisual"
                    : $"Content/Items/MechLegs/Pre{modPlayer.equippedParts[MechMod.legsIndex].ModItem.GetType().Name}LVisual";
                visualPlayer.legsRTexture = Mod.Assets.Request<Texture2D>(legsR);
                visualPlayer.legsLTexture = Mod.Assets.Request<Texture2D>(legsL);
            }

            if (!modPlayer.equippedParts[MechMod.weaponIndex].IsAir)
            {
                // Fallback to the item’s texture if no custom visual exists
                visualPlayer.weaponTexture = TextureAssets.Item[modPlayer.equippedParts[MechMod.weaponIndex].type];
            }
            else
            {
                visualPlayer.weaponTexture = null;
            }
        }

        public override void Dismount(Player player, ref bool skipDust)
        {
            MechModPlayer modPlayer = player.GetModPlayer<MechModPlayer>();
            MechWeaponsPlayer weaponsPlayer = player.GetModPlayer<MechWeaponsPlayer>();

            player.ClearBuff(ModContent.BuffType<MechBuff>()); // Clear the mech buff

            SoundEngine.PlaySound(SoundID.Research, player.position); // Play Research sound when dismounting the mech

            modPlayer.mechDebuffDuration = 900;
            modPlayer.launchForce = -10;

            foreach (var part in player.GetModPlayer<MechModPlayer>().equippedParts)
            {
                if (part.ModItem is IMechModule mechModule)
                {
                    if (mechModule.MType == IMechModule.ModuleType.OnDismount)
                    {
                        mechModule.ModuleEffect(this, player, modPlayer, weaponsPlayer); // Apply the module effect on dismount
                    }
                }
            }

            // Debuff the Player
            int mechDebuff = ModContent.BuffType<MechDebuff>();
            int duration = (int)(modPlayer.mechDebuffDuration / // Debuff duration scales higher as the player's health gets lower relative to max health
                    ((float)player.statLife <= modPlayer.maxLife * 0.25 ? // If statement for making sure the debuff duration isn't too punishing at low health
                    (modPlayer.maxLife * 0.25) / modPlayer.maxLife
                    : (float)player.statLife / modPlayer.maxLife));
            player.AddBuff(mechDebuff, duration);

            player.opacityForAnimation = 1; // Make Player visible

            player.velocity.Y = modPlayer.launchForce; // Launch the Player upwards

            // Reset stat values
            modPlayer.lifeBonus = 0;
            modPlayer.armourBonus = 0;
        }

        public override void UpdateEffects(Player player)
        {
            MechModPlayer modPlayer = player.GetModPlayer<MechModPlayer>();
            MechVisualPlayer visualPlayer = player.GetModPlayer<MechVisualPlayer>();
            MechWeaponsPlayer weaponsPlayer = player.GetModPlayer<MechWeaponsPlayer>();

            if (player.mount._frameState == Mount.FrameFlying)
            {
                // Disable player's ability to hover while flying
                if (!modPlayer.allowDown)
                    player.controlDown = false;

                MountData.jumpSpeed = modPlayer.flightJumpSpeed;
                MountData.runSpeed = modPlayer.flightHorizontalSpeed;
                MountData.swimSpeed = modPlayer.flightHorizontalSpeed;
            }
            else
            {
                MountData.jumpSpeed = modPlayer.groundJumpSpeed;
                MountData.runSpeed = modPlayer.groundHorizontalSpeed;
                MountData.swimSpeed = modPlayer.groundHorizontalSpeed;
            }

            // Grant life bonus
            player.statLifeMax2 += modPlayer.lifeBonus;
            if (modPlayer.grantedLifeBonus == false)
            {
                player.statLife += modPlayer.lifeBonus; // Increase player's health to match new max health
                modPlayer.maxLife = player.statLifeMax2;
                modPlayer.grantedLifeBonus = true;
            }
            // Grant armour bonus
            player.statDefense += modPlayer.armourBonus;

            WeaponUseAnimationSetup(player, modPlayer, visualPlayer, weaponsPlayer);

            foreach (var part in player.GetModPlayer<MechModPlayer>().equippedParts)
            {
                if (part.ModItem is IMechModule mechModule)
                {
                    if (mechModule.MType == IMechModule.ModuleType.Persistent)
                    {
                        mechModule.ModuleEffect(this, player, modPlayer, weaponsPlayer); // Apply the module effect while mech is active
                    }
                }
            }

            Effects(player, modPlayer, visualPlayer);
        }

        private static void Effects(Player player, MechModPlayer modPlayer, MechVisualPlayer visualPlayer)
        {
            #region Booster

            DashPlayer dashPlayer = player.GetModPlayer<DashPlayer>();
            int boosterDuration = 30;

            if (!modPlayer.equippedParts[MechMod.boosterIndex].IsAir)
            {
                if (dashPlayer.dashActive || player.mount._frameState == Mount.FrameInAir || player.mount._frameState == Mount.FrameFlying)
                {
                    if (visualPlayer.boosterTimer < boosterDuration)
                        visualPlayer.boosterTimer++;
                    if (visualPlayer.boosterTimer >= boosterDuration)
                    {
                        if (dashPlayer.dashActive || player.mount._frameState == Mount.FrameFlying)
                        {
                            SoundEngine.PlaySound(SoundID.Item13, player.position); // Play Rocket Boots/Jetpack sound for Booster use
                            visualPlayer.boosterTimer = 0; // Reset the timer
                        }
                    }

                    float dustSpeedX;
                    float dustSpeedY;

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

                    float dustCenterXLeft = 2;
                    float dustCenterXRight = 8;

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
            }
            else
            {
                visualPlayer.boosterTimer = boosterDuration;
            }

            #endregion

            #region Step

            float stepSpeed = 26 / (player.velocity.Length() / 3);
            int positionRight = player.direction == -1 ? 6 : 2;
            int positionLeft = player.direction == -1 ? -14 : -20;

            if (visualPlayer.stepTimer < stepSpeed)
                visualPlayer.stepTimer++;
            if (visualPlayer.stepTimer >= stepSpeed && player.mount._frameState == Mount.FrameRunning)
            {
                SoundEngine.PlaySound(SoundID.NPCHit3, player.position); // Play Dig sound for step use
                if (!visualPlayer.changeposition)
                {
                    for (int i = 0; i < 15; i++)
                        Dust.NewDust(new Vector2(player.position.X + positionRight, player.position.Y + 80), 30, 1, DustID.Smoke, player.velocity.X * 0.2f, player.velocity.Y * 0.2f); // Create dust when running
                    visualPlayer.changeposition = true;
                }
                else
                {
                    for (int i = 0; i < 15; i++)
                        Dust.NewDust(new Vector2(player.position.X + positionLeft, player.position.Y + 80), 30, 1, DustID.Smoke, player.velocity.X * 0.2f, player.velocity.Y * 0.2f); // Create dust when running
                    visualPlayer.changeposition = false;
                }
                visualPlayer.stepTimer = 0;
            }

            #endregion
        }


        public override bool Draw(List<DrawData> playerDrawData, int drawType, Player drawPlayer, ref Texture2D texture, ref Texture2D glowTexture, ref Vector2 drawPosition, ref Rectangle frame, ref Color drawColor, ref Color glowColor, ref float rotation, ref SpriteEffects spriteEffects, ref Vector2 drawOrigin, ref float drawScale, float shadow)
        {
            if (drawType == 0)
            {
                //MechModPlayer modPlayer = drawPlayer.GetModPlayer<MechModPlayer>();
                MechVisualPlayer visualPlayer = drawPlayer.GetModPlayer<MechVisualPlayer>();

                // Get the default frame logic as a new rectangle
                Rectangle setArmRFrame = frame;
                Rectangle setArmLFrame = frame;
                if (visualPlayer.armRFrame >= 0) // If the arm frame is manually set,
                {
                    int frameHeight = visualPlayer.armsRTexture.Value.Height / visualPlayer.armRAnimationFrames; // Calculate the height of each frame based on the total height of the texture and the number of frames
                    setArmRFrame = new Rectangle(0, visualPlayer.armRFrame * frameHeight, visualPlayer.armsLTexture.Value.Width, frameHeight); // Change the set arm frame to a new rectangle based on the arm frame and the height of each frame
                }
                if (visualPlayer.armLFrame >= 0)
                {
                    int frameHeight = visualPlayer.armsLTexture.Value.Height / visualPlayer.armLAnimationFrames;
                    setArmLFrame = new Rectangle(0, visualPlayer.armLFrame * frameHeight, visualPlayer.armsLTexture.Value.Width, frameHeight);
                }

                int visualDirection = visualPlayer.useDirection != 0 ? visualPlayer.useDirection : drawPlayer.direction; // Use the use direction if it is not 0, otherwise use the player's direction
                spriteEffects = visualDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

                Vector2 groundOffset = new(0, -13); // Offset to position the mech above the ground

                // Draw right arm first
                if (visualPlayer.armsRTexture != null)
                    playerDrawData.Add(new DrawData(visualPlayer.armsRTexture.Value, drawPosition + groundOffset + new Vector2(visualPlayer.bodyOffsets[0].X * visualDirection, visualPlayer.bodyOffsets[0].Y), setArmRFrame, drawColor, rotation, drawOrigin, drawScale, spriteEffects));

                // Draw right leg
                if (visualPlayer.legsRTexture != null)
                    playerDrawData.Add(new DrawData(visualPlayer.legsRTexture.Value, drawPosition + groundOffset + new Vector2(visualPlayer.bodyOffsets[1].X * visualDirection, visualPlayer.bodyOffsets[1].Y), frame, drawColor, rotation, drawOrigin, drawScale, spriteEffects));

                // Draw body
                if (visualPlayer.bodyTexture != null)
                    playerDrawData.Add(new DrawData(visualPlayer.bodyTexture.Value, drawPosition + groundOffset, frame, drawColor, rotation, drawOrigin, drawScale, spriteEffects));

                // Draw left leg
                if (visualPlayer.legsLTexture != null)
                    playerDrawData.Add(new DrawData(visualPlayer.legsLTexture.Value, drawPosition + groundOffset + new Vector2(visualPlayer.bodyOffsets[2].X * visualDirection, visualPlayer.bodyOffsets[2].Y), frame, drawColor, rotation, drawOrigin, drawScale, spriteEffects));

                // Draw head
                if (visualPlayer.headTexture != null)
                    playerDrawData.Add(new DrawData(visualPlayer.headTexture.Value, drawPosition + groundOffset + new Vector2(visualPlayer.bodyOffsets[3].X * visualDirection, visualPlayer.bodyOffsets[3].Y), frame, drawColor, rotation, drawOrigin, drawScale, spriteEffects));

                // Draw weapon
                if (visualPlayer.weaponTexture != null)
                    playerDrawData.Add(new DrawData(visualPlayer.weaponTexture.Value, drawPosition + visualPlayer.weaponPosition + groundOffset, null, drawColor, visualPlayer.weaponRotation, visualPlayer.weaponOrigin, visualPlayer.weaponScale, visualPlayer.weaponSpriteEffects));

                // Draw left arm last
                if (visualPlayer.armsLTexture != null)
                    playerDrawData.Add(new DrawData(visualPlayer.armsLTexture.Value, drawPosition + groundOffset + new Vector2(visualPlayer.bodyOffsets[4].X * visualDirection, visualPlayer.bodyOffsets[4].Y), setArmLFrame, drawColor, rotation, drawOrigin, drawScale, spriteEffects));
            }

            return false;
        }

        public override void UseAbility(Player player, Vector2 mousePosition, bool toggleOn)
        {
            // Block weapon usage if mousing over any user interface element
            if (player.mouseInterface)
                return;

            MechModPlayer modPlayer = player.GetModPlayer<MechModPlayer>();
            MechVisualPlayer visualPlayer = player.GetModPlayer<MechVisualPlayer>();
            MechWeaponsPlayer weaponsPlayer = player.GetModPlayer<MechWeaponsPlayer>();

            if (modPlayer.equippedParts[MechMod.weaponIndex].ModItem is IMechWeapon weapon)
            {
                if (player.whoAmI == Main.myPlayer && Main.mouseLeft && weaponsPlayer.timer >= weaponsPlayer.attackRate) // Attack when ready
                {
                    weapon.UseAbility(player, weaponsPlayer, visualPlayer, mousePosition, toggleOn);
                    if (weaponsPlayer.canUse)
                    {
                        if (Main.MouseWorld.X > player.MountedCenter.X)
                            visualPlayer.useDirection = 1;
                        else
                            visualPlayer.useDirection = -1;
                        if (!player.controlLeft || !player.controlRight)
                            player.direction = visualPlayer.useDirection; // Set the player's direction to the last use direction if not controlling horizontal movement
                        weaponsPlayer.timer = 0;
                    }
                }
                if (weaponsPlayer.timer < weaponsPlayer.attackRate)
                    weaponsPlayer.timer++; // Increment the timer until it reaches the attack rate
            }
            else
            {
                return;
            }
        }

        private static void WeaponUseAnimationSetup(Player player, MechModPlayer modPlayer, MechVisualPlayer visualPlayer, MechWeaponsPlayer weaponsPlayer)
        {
            if (modPlayer.equippedParts[MechMod.weaponIndex].ModItem is IMechWeapon)
            {
                if (weaponsPlayer.canUse)
                {
                    if (visualPlayer.animationTimer > 0 || visualPlayer.animationProgress > 0) // Only run animations if timer or progress is active
                    {
                        if (weaponsPlayer.useType == MechWeaponsPlayer.UseType.Swing) // Constantly update animation
                        {
                            WeaponUseAnimation(player, visualPlayer, weaponsPlayer, MechWeaponsPlayer.UseType.Swing);
                        }
                        else if (weaponsPlayer.useType == MechWeaponsPlayer.UseType.Point) // Only animate for one frame
                        {
                            if (!visualPlayer.animateOnce)
                            {
                                WeaponUseAnimation(player, visualPlayer, weaponsPlayer, MechWeaponsPlayer.UseType.Point);
                                visualPlayer.animateOnce = true;
                            }
                        }
                    }
                    if (weaponsPlayer.timer >= weaponsPlayer.attackRate)
                    {
                        visualPlayer.animateOnce = false; // Reset the animate once bool when the weapon is ready to attack again
                    }
                }
                if (!Main.mouseLeft && visualPlayer.animationTimer <= 0 && visualPlayer.animationProgress <= 0)
                {
                    // Reset the arm frame to default
                    visualPlayer.armRFrame = -1;
                    visualPlayer.armLFrame = -1;
                    visualPlayer.weaponScale = 0f; // Hide the weapon when not in use
                    visualPlayer.useDirection = 0; // Reset last use direction
                }
            }
        }

        // Function for weapon use animations like pointing and swinging 
        private static void WeaponUseAnimation(Player player, MechVisualPlayer visualPlayer, MechWeaponsPlayer weaponsPlayer, MechWeaponsPlayer.UseType useType)
        {
            int weaponPositionX = 0; // X position of the weapon
            int weaponPositionY = 0; // Y position of the weapon

            int weaponOriginOffsetX = 0; // X offset of the weapon origin
            int weaponOriginOffsetY = 0; // Y offset of the weapon origin

            switch (useType)
            {
                case MechWeaponsPlayer.UseType.Point:

                    visualPlayer.weaponScale = 1f;

                    weaponPositionX = 0;
                    weaponPositionY = -22;

                    weaponOriginOffsetX = -38;
                    weaponOriginOffsetY = 0;

                    if (Main.mouseLeft) // Only rotate on fire
                    {
                        float pointAngle = (Main.MouseWorld - player.MountedCenter).ToRotation(); // Get the angle between the mouse position and the player mounted center
                        float pointAngleDeg = MathHelper.ToDegrees(pointAngle); // Convert the angle to degrees for easier calculations
                        visualPlayer.weaponRotation = pointAngle; // Set the weapon rotation to the angle between the mouse and the player

                        visualPlayer.armRFrame = 9; // Set the right arm frame to be by side

                        // Check if the mouse is at different angles relative to the player (so if mouse is pointing up, the arm will point up, if mouse is pointing down, the arm will point down, etc.)
                        if (pointAngleDeg >= -135 && pointAngleDeg <= -45)
                        {
                            visualPlayer.armLFrame = 12; // Pointing angled up
                        }
                        else if (pointAngleDeg >= -45 && pointAngleDeg <= 45)
                        {
                            visualPlayer.armLFrame = 11; // Pointing horizontal right
                        }
                        else if (pointAngleDeg <= -135 || pointAngleDeg >= 135)
                        {
                            visualPlayer.armLFrame = 11; // Pointing horizontal left
                        }
                        else if (pointAngleDeg <= 135 && pointAngleDeg >= 45)
                        {
                            visualPlayer.armLFrame = 10; // Pointing angled down
                        }
                    }
                    break;
                case MechWeaponsPlayer.UseType.Swing:

                    visualPlayer.weaponScale = 1f;

                    weaponPositionX = -10;
                    weaponPositionY = -12;

                    weaponOriginOffsetX = -62;
                    weaponOriginOffsetY = 44 * visualPlayer.useDirection;

                    if (player.whoAmI == Main.myPlayer) // Progress calculation should only be done on the client player
                    {
                        float progress = 1f - (visualPlayer.animationProgress / weaponsPlayer.attackRate); // Calculate the progress of the animation based on the animation progress (equal to projectile's life time) and attack rate

                        visualPlayer.armRFrame = 9; // Set the right arm frame to be by side

                        visualPlayer.armLFrame = (int)MathHelper.Lerp(14, 10, progress); // Lerp the arm through the up, angled up, horizontal and angled down frames respectively (11 is used as starting value to make the up frame last longer)

                        // Calculate the swing's starting and ending angle and lerp it between the angle
                        float startAngle = -2f * visualPlayer.useDirection;
                        float endAngle = 1.5f * visualPlayer.useDirection;
                        float swingAngle = MathHelper.Lerp(startAngle, endAngle, progress);

                        visualPlayer.weaponRotation = swingAngle + (visualPlayer.useDirection == -1 ? MathHelper.Pi : 0f); // Set the weapon rotation to the swing angle (that also changes depending on last use direction)
                    }
                    break;
            }
            visualPlayer.weaponPosition = new Vector2(weaponPositionX * visualPlayer.useDirection, weaponPositionY); // Setting a new position lets a weapon be futher out or further up from the player
            if (visualPlayer.weaponTexture != null)
                visualPlayer.weaponOrigin = new Vector2(visualPlayer.weaponTexture.Value.Width / 2 + weaponOriginOffsetX, visualPlayer.weaponTexture.Value.Height / 2 + weaponOriginOffsetY); // Setting a new origin gives the weapon a different rotation point
            visualPlayer.weaponSpriteEffects = visualPlayer.useDirection == -1 ? SpriteEffects.FlipVertically : SpriteEffects.None; // Flip the weapon sprite based on the player's direction
        }

        public void ApplyPartStats(Player player, Item equippedHead, Item equippedBody, Item equippedArms, Item equippedLegs, Item equippedBooster)
        {
            MechModPlayer modPlayer = player.GetModPlayer<MechModPlayer>();
            MechWeaponsPlayer weaponsPlayer = player.GetModPlayer<MechWeaponsPlayer>();

            // Reset weapon stats that are added/multiplied before applying new ones
            weaponsPlayer.partDamageBonus = 0f;
            weaponsPlayer.partCritChanceBonus = 0f;
            weaponsPlayer.partAttackSpeedBonus = 0f;
            weaponsPlayer.partKnockbackBonus = 0f;

            for (int i = 0; i < modPlayer.partEffectiveness.Length; i++)
            {
                modPlayer.partEffectiveness[i] = 1f; // Reset part effectiveness multipliers
            }
            // Apply body stats first as it increases and decreases the effectivness of other parts
            if (!equippedBody.IsAir)
                if (modPlayer.equippedParts[MechMod.bodyIndex].ModItem is IMechParts body)
                    body.ApplyStats(player, modPlayer, weaponsPlayer, this);

            if (!equippedArms.IsAir)
                if (modPlayer.equippedParts[MechMod.armsIndex].ModItem is IMechParts arms)
                    arms.ApplyStats(player, modPlayer, weaponsPlayer, this);
            if (!equippedLegs.IsAir)
                if (modPlayer.equippedParts[MechMod.legsIndex].ModItem is IMechParts legs)
                    legs.ApplyStats(player, modPlayer, weaponsPlayer, this);

            MountData.flightTimeMax = 0;
            modPlayer.flightHorizontalSpeed = 0f;
            modPlayer.flightJumpSpeed = 0f;
            player.GetModPlayer<DashPlayer>().ableToDash = false;
            player.GetModPlayer<DashPlayer>().dashCoolDown = 0;
            player.GetModPlayer<DashPlayer>().dashDuration = 0;
            player.GetModPlayer<DashPlayer>().dashVelo = 0f;
            if (!equippedBooster.IsAir)
            {
                if (modPlayer.equippedParts[MechMod.boosterIndex].ModItem is IMechParts booster)
                    booster.ApplyStats(player, modPlayer, weaponsPlayer, this);
            }
            else
            {
                // Unique as a player can go without a Booster, but the other Parts are required
                if (modPlayer.powerCellActive)
                    modPlayer.lifeBonus += 100;
            }

            // Apply head stats last as it can have multiplicative effects
            if (!equippedHead.IsAir)
                if (modPlayer.equippedParts[MechMod.headIndex].ModItem is IMechParts head)
                    head.ApplyStats(player, modPlayer, weaponsPlayer, this);
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
                        int dashDir;
                        if (Player.controlRight)
                            dashDir = 1;
                        else if (Player.controlLeft)
                            dashDir = -1;
                        else
                            dashDir = Player.direction;

                        float newVelo;
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
