using MechMod.Common.Players;
using MechMod.Content.Buffs;
using MechMod.Content.Debuffs;
using MechMod.Content.Dusts;
using MechMod.Content.Items.MechWeapons;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ID;

using Terraria.ModLoader;

namespace MechMod.Content.Mounts
{
    /// These interfaces are used to define the required methods for Mech Parts, Weapons, and Modules.
    /// Interfaces create a contract, the other class provides implemention for the methods, and then that method is ran somewhere in ModularMech.

    /// <summary>
    /// Interface for Mech Parts (head, body, arms, legs, booster).
    /// </summary>
    public interface IMechParts
    {
        void ApplyStats(Player player, MechModPlayer modPlayer, MechWeaponsPlayer weaponsPlayer, ModularMech mech); // Apply part stats to the player and mech

        void BodyOffsets(MechVisualPlayer visualPlayer, string body); // Set body offsets for other parts while drawing the mech
    }

    /// <summary>
    /// Interface for Mech Weapons.
    /// </summary>
    public interface IMechWeapon
    {
        void SetStats(MechWeaponsPlayer weaponsPlayer); // Apply the weapon's stats (mainly DamageClass and use type)

        void UseAbility(Player player, MechWeaponsPlayer weaponsPlayer, MechVisualPlayer visualPlayer, Vector2 mousePosition, bool toggleOn); // Activates on attempting to use the weapon

        void UpdateAbility(Player player, MechWeaponsPlayer weaponsPlayer, MechVisualPlayer visualPlayer); // Activates every tick while the mech is active
    }

    /// <summary>
    /// Interface for Mech Modules.
    /// </summary>
    public interface IMechModule
    {
        public enum ModuleSlot // Defines which slot the module can be equipped in
        {
            Passive,
            Active
        }
        public ModuleSlot MSlot { get; }

        public enum ModuleType // Defines when the module effect is applied
        {
            Persistent,
            OnMount,
            OnDismount
        }
        public ModuleType MType { get; }

        void ModuleEffect(ModularMech mech, Player player, MechModPlayer modPlayer, MechWeaponsPlayer weaponsPlayer, MechVisualPlayer visualPlayer); // Execute effect of the module
    }

    /// <summary>
    /// Mount that represents the Modular Mech, which can be customized with different parts and weapons.
    /// <para/> Basically the "core" of the mod.
    /// </summary>

    public class ModularMech : ModMount
    {
        public override void SetStaticDefaults()
        {
            // Misc
            MountData.fallDamage = 0;
            MountData.constantJump = true;
            MountData.blockExtraJumps = true;
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
            MountData.swimFrameStart = MountData.inAirFrameStart;

            // Set movement stats to 0, as they will be modified in code based on the equipped parts
            MountData.acceleration = 0f;
            MountData.runSpeed = 0f;
            MountData.swimSpeed = 0f;

            MountData.jumpHeight = 0;
            MountData.jumpSpeed = 0f;

            MountData.dashSpeed = 0f;

            MountData.flightTimeMax = 0;

            // Set the mount's texture size
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

            modPlayer.mechBuffDuration = modPlayer.powerCellActive ? 5400 : 2700; // Set mech buff duration based on if power cell is active

            ApplyParts(player, modPlayer, visualPlayer, weaponsPlayer); // Apply the stats and textures of the equipped parts

            modPlayer.grantedBonuses = false; // Reset the bonuses tracker

            // Reset Module variables
            modPlayer.allowHover = false;
            modPlayer.manaShield = false;
            modPlayer.relocatorPosition = default;
            // Check for any OnMount modules and apply their effects
            foreach (var part in player.GetModPlayer<MechModPlayer>().equippedParts)
            {
                if (part.ModItem is IMechModule mechModule)
                {
                    if (mechModule.MType == IMechModule.ModuleType.OnMount)
                    {
                        mechModule.ModuleEffect(this, player, modPlayer, weaponsPlayer, visualPlayer); // Apply the module effect on mount
                    }
                }
            }

            player.AddBuff(ModContent.BuffType<MechBuff>(), modPlayer.mechBuffDuration);

            player.opacityForAnimation = 0; // Make Player invisible

            SoundEngine.PlaySound(SoundID.Research, player.position); // Play Research sound when mounting the mech
        }

        // Function that applies the stats and textures of the equipped parts
        private void ApplyParts(Player player, MechModPlayer modPlayer, MechVisualPlayer visualPlayer, MechWeaponsPlayer weaponsPlayer)
        {
            // Apply Part Stats
            ApplyPartStats(player, modPlayer.equippedParts[MechMod.headIndex], modPlayer.equippedParts[MechMod.bodyIndex], modPlayer.equippedParts[MechMod.armsIndex], modPlayer.equippedParts[MechMod.legsIndex], modPlayer.equippedParts[MechMod.boosterIndex]);

            // Apply Weapon Stats (denotes a weapon's use type and damage class)
            weaponsPlayer.DamageClass = DamageClass.Default; // Reset the damage class to default before applying the weapon's stats
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

            // For each Part texture:
            // 1. Check if the Part is not air (i.e. a part is equipped)
            // 2. Grab the texture's spritesheet using the Part's name and the appropriate path
            // 3. If the power cell is active, also grab the light texture using the same method
            // 4. Set the texture variable(s) to the grabbed texture(s)
            if (!modPlayer.equippedParts[MechMod.headIndex].IsAir)
            {
                string headPath = $"Content/Items/MechHeads/{modPlayer.equippedParts[MechMod.headIndex].ModItem.GetType().Name}Visual";
                visualPlayer.headTexture = Mod.Assets.Request<Texture2D>(headPath);
                if (modPlayer.powerCellActive)
                {
                    string headLightPath = $"Content/Items/MechHeads/{modPlayer.equippedParts[MechMod.headIndex].ModItem.GetType().Name}Light";
                    visualPlayer.headLightTexture = Mod.Assets.Request<Texture2D>(headLightPath);
                }
            }

            if (!modPlayer.equippedParts[MechMod.bodyIndex].IsAir)
            {
                string bodyPath = $"Content/Items/MechBodies/{modPlayer.equippedParts[MechMod.bodyIndex].ModItem.GetType().Name}Visual";
                visualPlayer.bodyTexture = Mod.Assets.Request<Texture2D>(bodyPath);
                if (modPlayer.powerCellActive)
                {
                    string bodyLightPath = $"Content/Items/MechBodies/{modPlayer.equippedParts[MechMod.bodyIndex].ModItem.GetType().Name}Light";
                    if (ModContent.RequestIfExists<Texture2D>(Mod.Name + "/" + bodyLightPath, out _)) // Check if the light texture exists before trying to load it
                        visualPlayer.bodyLightTexture = Mod.Assets.Request<Texture2D>(bodyLightPath);
                    else
                        visualPlayer.bodyLightTexture = null;
                }
            }

            if (!modPlayer.equippedParts[MechMod.armsIndex].IsAir)
            {
                string armsR = $"Content/Items/MechArms/{modPlayer.equippedParts[MechMod.armsIndex].ModItem.GetType().Name}RVisual";
                string armsL = $"Content/Items/MechArms/{modPlayer.equippedParts[MechMod.armsIndex].ModItem.GetType().Name}LVisual";
                visualPlayer.armsRTexture = Mod.Assets.Request<Texture2D>(armsR);
                visualPlayer.armsLTexture = Mod.Assets.Request<Texture2D>(armsL);
                if (modPlayer.powerCellActive)
                {
                    string armsRLight = $"Content/Items/MechArms/{modPlayer.equippedParts[MechMod.armsIndex].ModItem.GetType().Name}RLight";
                    string armsLLight = $"Content/Items/MechArms/{modPlayer.equippedParts[MechMod.armsIndex].ModItem.GetType().Name}LLight";
                    visualPlayer.armsRLightTexture = Mod.Assets.Request<Texture2D>(armsRLight);
                    visualPlayer.armsLLightTexture = Mod.Assets.Request<Texture2D>(armsLLight);
                }
            }

            if (!modPlayer.equippedParts[MechMod.legsIndex].IsAir)
            {
                string legsR = $"Content/Items/MechLegs/{modPlayer.equippedParts[MechMod.legsIndex].ModItem.GetType().Name}RVisual";
                string legsL = $"Content/Items/MechLegs/{modPlayer.equippedParts[MechMod.legsIndex].ModItem.GetType().Name}LVisual";
                visualPlayer.legsRTexture = Mod.Assets.Request<Texture2D>(legsR);
                visualPlayer.legsLTexture = Mod.Assets.Request<Texture2D>(legsL);
                if (modPlayer.powerCellActive)
                {
                    string legsRLight = $"Content/Items/MechLegs/{modPlayer.equippedParts[MechMod.legsIndex].ModItem.GetType().Name}RLight";
                    string legsLLight = $"Content/Items/MechLegs/{modPlayer.equippedParts[MechMod.legsIndex].ModItem.GetType().Name}LLight";
                    visualPlayer.legsRLightTexture = Mod.Assets.Request<Texture2D>(legsRLight);
                    visualPlayer.legsLLightTexture = Mod.Assets.Request<Texture2D>(legsLLight);
                }
            }

            // For weapons, grab the item texture directly from the item if the weapon Part is not air
            if (!modPlayer.equippedParts[MechMod.weaponIndex].IsAir)
            {
                visualPlayer.weaponTexture = TextureAssets.Item[modPlayer.equippedParts[MechMod.weaponIndex].type];
            }
            else
            {
                visualPlayer.weaponTexture = null;
            }

            visualPlayer.mechColour = Color.White; // Reset mech colour to white
            // Grab the equipped dyes and get their shader IDs for dyeing the mech
            for (int i = 0; i < visualPlayer.dyes.Length; i++)
            {
                visualPlayer.dyeShaders[i] = GameShaders.Armor.GetShaderIdFromItemId(visualPlayer.dyes[i].type);
            }
        }

        public override void Dismount(Player player, ref bool skipDust)
        {
            MechModPlayer modPlayer = player.GetModPlayer<MechModPlayer>();
            MechWeaponsPlayer weaponsPlayer = player.GetModPlayer<MechWeaponsPlayer>();
            MechVisualPlayer visualPlayer = player.GetModPlayer<MechVisualPlayer>();

            player.ClearBuff(ModContent.BuffType<MechBuff>()); // Clear the mech buff
            player.ClearBuff(ModContent.BuffType<Cooldown>()); // Clear active module cooldown

            // Clear Summon buffs
            player.ClearBuff(ModContent.BuffType<DroneBuff>());

            SoundEngine.PlaySound(SoundID.Research, player.position); // Play Research sound when dismounting the mech

            modPlayer.mechDebuffDuration = 900; // Reset to base debuff duration
            modPlayer.launchForce = -10; // Reset to base launch force

            // Check for any OnDismount modules and apply their effects
            foreach (var part in player.GetModPlayer<MechModPlayer>().equippedParts)
            {
                if (part.ModItem is IMechModule mechModule)
                {
                    if (mechModule.MType == IMechModule.ModuleType.OnDismount)
                    {
                        mechModule.ModuleEffect(this, player, modPlayer, weaponsPlayer, visualPlayer); // Apply the module effect on dismount
                    }
                }
            }
            if (modPlayer.relocatorPosition != default) // If Relocator position is set,
            {
                player.Teleport(modPlayer.relocatorPosition, TeleportationStyleID.TeleporterTile); // Teleport the player to the set position
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
            modPlayer.lifeBonus = 100;
            modPlayer.armourBonus = default;
        }

        public override void UpdateEffects(Player player)
        {
            MechModPlayer modPlayer = player.GetModPlayer<MechModPlayer>();
            MechVisualPlayer visualPlayer = player.GetModPlayer<MechVisualPlayer>();
            MechWeaponsPlayer weaponsPlayer = player.GetModPlayer<MechWeaponsPlayer>();

            // Check for any Persistent modules and apply their effects
            foreach (var part in player.GetModPlayer<MechModPlayer>().equippedParts)
            {
                if (part.ModItem is IMechModule mechModule)
                {
                    if (mechModule.MType == IMechModule.ModuleType.Persistent)
                    {
                        mechModule.ModuleEffect(this, player, modPlayer, weaponsPlayer, visualPlayer); // Apply the module effect while mech is active
                    }
                }
            }

            if (player.mount._frameState == Mount.FrameFlying)
            {
                // Disable player's ability to hover while flying
                if (!modPlayer.allowHover)
                    player.controlDown = false;

                // Use flight stats from Booster for speed
                MountData.jumpSpeed = modPlayer.flightJumpSpeed;
                MountData.runSpeed = modPlayer.flightHorizontalSpeed;
                MountData.swimSpeed = modPlayer.flightHorizontalSpeed;
            }
            else
            {
                // Use ground stats from Legs for speed
                MountData.jumpSpeed = modPlayer.groundJumpSpeed;
                MountData.runSpeed = modPlayer.groundHorizontalSpeed;
                MountData.swimSpeed = modPlayer.groundHorizontalSpeed;
            }

            // Grant life bonus
            player.statLifeMax2 += modPlayer.lifeBonus;
            if (modPlayer.grantedBonuses == false)
            {
                player.statLife += modPlayer.lifeBonus; // Increase player's health to match new max health
                modPlayer.maxLife = player.statLifeMax2;
                modPlayer.grantedBonuses = true;
            }
            player.statDefense += modPlayer.armourBonus; // Grant armour bonus

            if (modPlayer.equippedParts[MechMod.weaponIndex].ModItem is IMechWeapon weapon) // If a weapon is equipped,
            {
                if (player.whoAmI == Main.myPlayer)
                {
                    weapon.UpdateAbility(player, weaponsPlayer, visualPlayer); // Update the weapon's ability
                }
            }

            if (weaponsPlayer.updateTimer < weaponsPlayer.updateRate)
                weaponsPlayer.updateTimer++; // Increment the update timer until it reaches the update rate

            WeaponUseAnimationSetup(player, modPlayer, visualPlayer, weaponsPlayer); // Setup weapon use animation and position

            Effects(player, modPlayer, visualPlayer); // Apply visual and sound effects

            if (!player.HasBuff(ModContent.BuffType<MechBuff>())) // If the player does not have the Mech buff,
            {
                player.mount.Dismount(player); // Dismount the mech
            }
        }

        // Function for any visual or/and sound effects
        private static void Effects(Player player, MechModPlayer modPlayer, MechVisualPlayer visualPlayer)
        {
            #region Duration Warning

            if (player.HasBuff(ModContent.BuffType<MechBuff>())) // If the player has the mech buff,
            {
                int buffTime = player.buffTime[player.FindBuffIndex(ModContent.BuffType<MechBuff>())]; // Get the remaining buff time
                var warnSound = SoundID.MenuTick with { Volume = 2 }; // Sound to play for warning
                if (buffTime <= 360 && buffTime % 60 == 0) // If the buff time is less than or equal to 6 seconds and is a multiple of 60 (1 second intervals),
                {
                    warnSound.Pitch = -0.5f + (buffTime / 300f); // Change pitch based on remaining time
                    SoundEngine.PlaySound(warnSound, player.position); // Play warning sound
                }
            }

            #endregion

            #region Jumping

            if ((player.mount._frameState == Mount.FrameRunning || player.mount._frameState == Mount.FrameStanding) && player.controlJump && player.velocity.Y == 0) // If the player is running and presses jump while on the ground,
            {
                SoundEngine.PlaySound(SoundID.NPCHit11, player.position); // Play jump sound
            }

            #endregion

            #region Booster

            DashPlayer dashPlayer = player.GetModPlayer<DashPlayer>();
            int boosterDuration = 30; // Delay between booster sounds

            if (!modPlayer.equippedParts[MechMod.boosterIndex].IsAir) // If a Booster is equipped,
            {
                if (dashPlayer.dashActive || player.mount._frameState == Mount.FrameInAir || player.mount._frameState == Mount.FrameFlying) // If player is dashing, in air, or flying,
                {
                    if (visualPlayer.boosterTimer < boosterDuration) // Increment the timer until it reaches the duration
                        visualPlayer.boosterTimer++;
                    if (visualPlayer.boosterTimer >= boosterDuration) // Once the timer reaches the duration,
                    {
                        if (dashPlayer.dashActive || player.mount._frameState == Mount.FrameFlying) // If dashing or flying,
                        {
                            SoundEngine.PlaySound(SoundID.Item13, player.position); // Play Rocket Boots/Jetpack sound for Booster use
                            visualPlayer.boosterTimer = default; // Reset the timer
                        }
                    }

                    // Change dust velocity and direction based on if dashing, in air, or flying
                    float dustSpeedX;
                    float dustSpeedY;
                    if (dashPlayer.dashActive) // If dashing,
                    {
                        // Create horizontal dust
                        dustSpeedX = 5;
                        dustSpeedY = 0;
                    }
                    else if (player.mount._frameState == Mount.FrameInAir || player.mount._frameState == Mount.FrameFlying) // If in air or flying,
                    {
                        // Create vertical dust (more force if flying)
                        dustSpeedX = 0;
                        dustSpeedY = player.controlJump ? 14 : 8;
                    }
                    else
                    {
                        dustSpeedX = 0;
                        dustSpeedY = 0;
                    }

                    float dustCenterXLeft = 2; // X position while facing left
                    float dustCenterXRight = 8; // X position while facing right

                    float dustOffsetX = 0; // Offset to vary the X position of the dust

                    for (int i = 0; i < 20; i++)
                    {
                        // Change dust position based on the index
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
                        // Create the dust at the appropriate position based on the player's direction
                        float posX = player.direction == -1 ? player.position.X + dustCenterXLeft + dustOffsetX : player.position.X + dustCenterXRight + dustOffsetX;
                        float posY = (i < 10) ? player.position.Y - 5 : player.position.Y + 15;
                        int dust = Dust.NewDust(new Vector2(posX, posY), 1, 1, ModContent.DustType<BoosterDust>(), dustSpeedX, dustSpeedY);
                        Main.dust[dust].customData = player; // Use custom data to hide dust if behind Mech
                    }
                }
            }
            else
            {
                visualPlayer.boosterTimer = boosterDuration; // Reset the timer if no Booster is equipped
            }

            #endregion

            int directionOffset = player.direction == -1 ? -10 : 0; // Offset to position the dust correctly based on the player's direction

            #region Step

            if (player.velocity.X != 0) // If the player is moving,
            {
                float stepSpeed = 26 / (player.velocity.Length() / 3); // Speed of steps based on the player's velocity (faster velocity = faster steps)

                if (visualPlayer.stepTimer < stepSpeed) // Increment the timer until it reaches the step speed
                    visualPlayer.stepTimer++;
                if (visualPlayer.stepTimer >= stepSpeed && player.mount._frameState == Mount.FrameRunning) // Once the timer reaches the step speed and the player is running,
                {
                    SoundEngine.PlaySound(SoundID.NPCHit3, player.position); // Play hit sound for step use
                    for (int i = 0; i < 15; i++)
                        Dust.NewDust(new Vector2(player.position.X + directionOffset, player.position.Y + 80), 30, 1, DustID.Smoke, player.velocity.X * 0.2f, player.velocity.Y * 0.2f); // Create dust when running
                    visualPlayer.stepTimer = default; // Reset the timer
                }
            }

            #endregion

            #region Landing

            if (player.mount._frameState == Mount.FrameFlying || player.mount._frameState == Mount.FrameInAir) // If the player is in air,
            {
                visualPlayer.airTime++; // Increment the air time
                visualPlayer.airVelocity = player.velocity.Y; // Store the Y velocity while in air
            }

            if ((player.mount._frameState == Mount.FrameRunning || player.mount._frameState == Mount.FrameStanding) && visualPlayer.airTime >= 45 && visualPlayer.airVelocity >= 8) // If the player is grounded, air time is greater than 45 frames (0.75 seconds), and the stored Y velocity is greater than or equal to 8,
            {
                // Play landing sounds
                SoundEngine.PlaySound(SoundID.NPCHit3, player.position);
                SoundEngine.PlaySound(SoundID.Dig, player.position);
                for (int i = 0; i < 50; i++)
                    Dust.NewDust(new Vector2(player.position.X - 20 + directionOffset, player.position.Y + 80), 80, 1, DustID.Smoke, player.velocity.X * 0.2f, player.velocity.Y * 0.2f); // Create dust when landing
                visualPlayer.airTime = default; // Reset the air time
                visualPlayer.airVelocity = default; // Reset the stored Y velocity
            }

            #endregion
        }

        public override bool Draw(List<DrawData> playerDrawData, int drawType, Player drawPlayer, ref Texture2D texture, ref Texture2D glowTexture, ref Vector2 drawPosition, ref Rectangle frame, ref Color drawColor, ref Color glowColor, ref float rotation, ref SpriteEffects spriteEffects, ref Vector2 drawOrigin, ref float drawScale, float shadow)
        {
            if (drawType == 0)
            {
                MechVisualPlayer visualPlayer = drawPlayer.GetModPlayer<MechVisualPlayer>();

                // Get the default frame logic as a new rectangle
                Rectangle setArmRFrame = frame;
                Rectangle setArmLFrame = frame;
                if (visualPlayer.armRFrame >= 0) // If the arm frame is manually set,
                {
                    int frameHeight = visualPlayer.armsRTexture.Value.Height / visualPlayer.armRAnimationFrames; // Calculate the height of each frame based on the total height of the texture and the number of frames
                    setArmRFrame = new Rectangle(0, visualPlayer.armRFrame * frameHeight, visualPlayer.armsLTexture.Value.Width, frameHeight); // Change the set arm frame to a new rectangle based on the arm frame and the height of each frame
                }
                // Do the same as above for the left arm
                if (visualPlayer.armLFrame >= 0)
                {
                    int frameHeight = visualPlayer.armsLTexture.Value.Height / visualPlayer.armLAnimationFrames;
                    setArmLFrame = new Rectangle(0, visualPlayer.armLFrame * frameHeight, visualPlayer.armsLTexture.Value.Width, frameHeight);
                }

                int visualDirection = visualPlayer.useDirection != 0 ? visualPlayer.useDirection : drawPlayer.direction; // Use the use direction if it is not 0, otherwise use the player's direction
                spriteEffects = visualDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None; // Flip the sprite based on the visual direction

                Vector2 groundOffset = new(0, -13); // Offset to position the mech above the ground

                // Get and store dyes
                int headDye = visualPlayer.dyeShaders[0];
                int bodyDye = visualPlayer.dyeShaders[1];
                int armsDye = visualPlayer.dyeShaders[2];
                int legsDye = visualPlayer.dyeShaders[3];
                int lightsDye = visualPlayer.dyeShaders[4];

                #region Drawing Parts

                // Right arm
                if (visualPlayer.armsRTexture != null)
                {
                    var drawData = new DrawData(
                        visualPlayer.armsRTexture.Value,
                        drawPosition + groundOffset + new Vector2(visualPlayer.bodyOffsets[0].X * visualDirection, visualPlayer.bodyOffsets[0].Y),
                        setArmRFrame, visualPlayer.mechColour != Color.White ? visualPlayer.mechColour : drawColor, rotation, drawOrigin, drawScale, spriteEffects);
                    if (armsDye > 0) drawData.shader = armsDye;
                    playerDrawData.Add(drawData);
                    if (visualPlayer.armsRLightTexture != null)
                    {
                        var lightDrawData = new DrawData(
                            visualPlayer.armsRLightTexture.Value,
                            drawPosition + groundOffset + new Vector2(visualPlayer.bodyOffsets[0].X * visualDirection, visualPlayer.bodyOffsets[0].Y),
                            setArmRFrame, Color.White, rotation, drawOrigin, drawScale, spriteEffects);
                        if (lightsDye > 0) lightDrawData.shader = lightsDye;
                        playerDrawData.Add(lightDrawData);
                    }
                }

                // Right leg
                if (visualPlayer.legsRTexture != null)
                {
                    var drawData = new DrawData(
                        visualPlayer.legsRTexture.Value,
                        drawPosition + groundOffset + new Vector2(visualPlayer.bodyOffsets[1].X * visualDirection, visualPlayer.bodyOffsets[1].Y),
                        frame, visualPlayer.mechColour != Color.White ? visualPlayer.mechColour : drawColor, rotation, drawOrigin, drawScale, spriteEffects);
                    if (legsDye > 0) drawData.shader = legsDye;
                    playerDrawData.Add(drawData);
                    if (visualPlayer.legsRLightTexture != null)
                    {
                        var lightDrawData = new DrawData(
                            visualPlayer.legsRLightTexture.Value,
                            drawPosition + groundOffset + new Vector2(visualPlayer.bodyOffsets[1].X * visualDirection, visualPlayer.bodyOffsets[1].Y),
                            frame, Color.White, rotation, drawOrigin, drawScale, spriteEffects);
                        if (lightsDye > 0) lightDrawData.shader = lightsDye;
                        playerDrawData.Add(lightDrawData);
                    }
                }

                // Body
                if (visualPlayer.bodyTexture != null)
                {
                    var drawData = new DrawData(
                        visualPlayer.bodyTexture.Value,
                        drawPosition + groundOffset,
                        frame, visualPlayer.mechColour != Color.White ? visualPlayer.mechColour : drawColor, rotation, drawOrigin, drawScale, spriteEffects);
                    if (bodyDye > 0) drawData.shader = bodyDye;
                    playerDrawData.Add(drawData);
                    if (visualPlayer.bodyLightTexture != null)
                    {
                        var lightDrawData = new DrawData(
                            visualPlayer.bodyLightTexture.Value,
                            drawPosition + groundOffset,
                            frame, Color.White, rotation, drawOrigin, drawScale, spriteEffects);
                        if (lightsDye > 0) lightDrawData.shader = lightsDye;
                        playerDrawData.Add(lightDrawData);
                    }
                }

                // Left leg
                if (visualPlayer.legsLTexture != null)
                {
                    var drawData = new DrawData(
                        visualPlayer.legsLTexture.Value,
                        drawPosition + groundOffset + new Vector2(visualPlayer.bodyOffsets[2].X * visualDirection, visualPlayer.bodyOffsets[2].Y),
                        frame, visualPlayer.mechColour != Color.White ? visualPlayer.mechColour : drawColor, rotation, drawOrigin, drawScale, spriteEffects);
                    if (legsDye > 0) drawData.shader = legsDye;
                    playerDrawData.Add(drawData);
                    if (visualPlayer.legsLLightTexture != null)
                    {
                        var lightDrawData = new DrawData(
                            visualPlayer.legsLLightTexture.Value,
                            drawPosition + groundOffset + new Vector2(visualPlayer.bodyOffsets[2].X * visualDirection, visualPlayer.bodyOffsets[2].Y),
                            frame, Color.White, rotation, drawOrigin, drawScale, spriteEffects);
                        if (lightsDye > 0) lightDrawData.shader = lightsDye;
                        playerDrawData.Add(lightDrawData);
                    }
                }

                // Head
                if (visualPlayer.headTexture != null)
                {
                    var drawData = new DrawData(
                        visualPlayer.headTexture.Value,
                        drawPosition + groundOffset + new Vector2(visualPlayer.bodyOffsets[3].X * visualDirection, visualPlayer.bodyOffsets[3].Y),
                        frame, visualPlayer.mechColour != Color.White ? visualPlayer.mechColour : drawColor, rotation, drawOrigin, drawScale, spriteEffects);
                    if (headDye > 0) drawData.shader = headDye;
                    playerDrawData.Add(drawData);
                    if (visualPlayer.headLightTexture != null)
                    {
                        var lightDrawData = new DrawData(
                            visualPlayer.headLightTexture.Value,
                            drawPosition + groundOffset + new Vector2(visualPlayer.bodyOffsets[3].X * visualDirection, visualPlayer.bodyOffsets[3].Y),
                            frame, Color.White, rotation, drawOrigin, drawScale, spriteEffects);
                        if (lightsDye > 0) lightDrawData.shader = lightsDye;
                        playerDrawData.Add(lightDrawData);
                    }
                }

                // Weapon
                if (visualPlayer.weaponTexture != null)
                    playerDrawData.Add(new DrawData(visualPlayer.weaponTexture.Value, drawPosition + visualPlayer.weaponPosition + groundOffset, null, visualPlayer.weaponColour != Color.White ? visualPlayer.weaponColour : drawColor, visualPlayer.weaponRotation, visualPlayer.weaponOrigin, visualPlayer.weaponScale, visualPlayer.weaponSpriteEffects));

                // Left arm
                if (visualPlayer.armsLTexture != null)
                {
                    var drawData = new DrawData(
                        visualPlayer.armsLTexture.Value,
                        drawPosition + groundOffset + new Vector2(visualPlayer.bodyOffsets[4].X * visualDirection, visualPlayer.bodyOffsets[4].Y),
                        setArmLFrame, visualPlayer.mechColour != Color.White ? visualPlayer.mechColour : drawColor, rotation, drawOrigin, drawScale, spriteEffects);
                    if (armsDye > 0) drawData.shader = armsDye;
                    playerDrawData.Add(drawData);
                    if (visualPlayer.armsLLightTexture != null)
                    {
                        var lightDrawData = new DrawData(
                            visualPlayer.armsLLightTexture.Value,
                            drawPosition + groundOffset + new Vector2(visualPlayer.bodyOffsets[4].X * visualDirection, visualPlayer.bodyOffsets[4].Y),
                            setArmLFrame, Color.White, rotation, drawOrigin, drawScale, spriteEffects);
                        if (lightsDye > 0) lightDrawData.shader = lightsDye;
                        playerDrawData.Add(lightDrawData);
                    }
                }

                #endregion
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

            if (modPlayer.equippedParts[MechMod.weaponIndex].ModItem is IMechWeapon weapon) // If a weapon is equipped,
            {
                if (player.whoAmI == Main.myPlayer && Main.mouseLeft && weaponsPlayer.useTimer >= weaponsPlayer.useRate && weaponsPlayer.updateTimer >= weaponsPlayer.updateRate) // If player is the client player, holding left click, and the use and update timer has reached the use and update rate,
                {
                    weaponsPlayer.canUse = true; // Set the weapon to be usable
                    weapon.UseAbility(player, weaponsPlayer, visualPlayer, mousePosition, toggleOn); // Create the weapon's projectile(s) and activate any visuals
                    if (weaponsPlayer.canUse) // If the weapon can be used (found out after the weapon has had a chance to run it's functionality),
                    {
                        // Set the last use direction based on the mouse position relative to the player
                        if (Main.MouseWorld.X > player.MountedCenter.X)
                            visualPlayer.useDirection = 1;
                        else
                            visualPlayer.useDirection = -1;
                        if (!player.controlLeft || !player.controlRight)
                            player.direction = visualPlayer.useDirection; // Set the player's direction to the last use direction if not controlling horizontal movement
                        weaponsPlayer.useTimer = 0; // Reset the use timer
                    }
                }
                if (weaponsPlayer.useTimer < weaponsPlayer.useRate)
                    weaponsPlayer.useTimer++; // Increment the use timer until it reaches the use rate
            }
            else // Otherwise,
            {
                return; // Do nothing
            }
        }

        // Function that sets up the weapon use animation depending on the weapon's use type
        private static void WeaponUseAnimationSetup(Player player, MechModPlayer modPlayer, MechVisualPlayer visualPlayer, MechWeaponsPlayer weaponsPlayer)
        {
            if (modPlayer.equippedParts[MechMod.weaponIndex].ModItem is IMechWeapon) // If a weapon is equipped,
            {
                if (weaponsPlayer.canUse) // If the weapon can be used,
                {
                    if (visualPlayer.animationTimer > 0 || visualPlayer.animationProgress > 0) // Only run animations if timer or progress is active
                    {
                        if (weaponsPlayer.useType == MechWeaponsPlayer.UseType.Swing) // If the weapon is a swinging type,
                        {
                            WeaponUseAnimation(player, visualPlayer, weaponsPlayer, MechWeaponsPlayer.UseType.Swing); // Run the swing animation
                        }
                        else if (!visualPlayer.animateOnce) // If the animation relies on animating once per use,
                        {
                            if (weaponsPlayer.useType == MechWeaponsPlayer.UseType.Point) // If the weapon is a pointing type,
                            {
                                WeaponUseAnimation(player, visualPlayer, weaponsPlayer, MechWeaponsPlayer.UseType.Point); // Run the point animation
                                visualPlayer.animateOnce = true;
                            }
                            if (weaponsPlayer.useType == MechWeaponsPlayer.UseType.HoldUp) // If the weapon is a hold up type,
                            {
                                WeaponUseAnimation(player, visualPlayer, weaponsPlayer, MechWeaponsPlayer.UseType.HoldUp); // Run the hold up animation
                                visualPlayer.animateOnce = true;
                            }
                        }
                    }
                    if (weaponsPlayer.useTimer >= weaponsPlayer.useRate || weaponsPlayer.useTimer >= weaponsPlayer.useRate) // If the use or update timer has reached the use or update rate,
                    {
                        visualPlayer.animateOnce = false; // Reset the animation once tracker so animation can be run again
                    }
                }
                if ((!Main.mouseLeft || visualPlayer.animateOnce == true) && visualPlayer.animationTimer <= 0 && visualPlayer.animationProgress <= 0) // If the player is not using the weapon or the animation should not be occuring, and there is no animation timer or progress,
                {
                    // Reset the arm frame to default
                    visualPlayer.armRFrame = -1;
                    visualPlayer.armLFrame = -1;
                    visualPlayer.weaponScale = default; // Hide the weapon when not in use
                    visualPlayer.useDirection = default; // Reset last use direction
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

            switch (useType) // Switch between different use types
            {
                case MechWeaponsPlayer.UseType.Point: // For pointing weapons,

                    visualPlayer.weaponScale = 1f; // Make weapon visible

                    // Set position and origin offsets
                    weaponPositionX = 0;
                    weaponPositionY = -22;
                    weaponOriginOffsetX = -38;
                    weaponOriginOffsetY = 0;

                    if (Main.mouseLeft) // Only run the animation if the player is holding left click
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
                case MechWeaponsPlayer.UseType.Swing: // For swinging weapons,

                    visualPlayer.weaponScale = 1f; // Make weapon visible

                    // Set position and origin offsets
                    weaponPositionX = -10;
                    weaponPositionY = -12;
                    weaponOriginOffsetX = -62;
                    weaponOriginOffsetY = 44 * visualPlayer.useDirection;

                    if (player.whoAmI == Main.myPlayer) // Progress calculation should only be done on the client player
                    {
                        float progress = 1f - (visualPlayer.animationProgress / weaponsPlayer.useRate); // Calculate the progress of the animation based on the animation progress (equal to projectile's life time) and attack rate

                        visualPlayer.armRFrame = 9; // Set the right arm frame to be by side

                        visualPlayer.armLFrame = (int)MathHelper.Lerp(14, 10, progress); // Lerp the arm through the up, angled up, horizontal and angled down frames respectively (11 is used as starting value to make the up frame last longer)

                        // Calculate the swing's starting and ending angle and lerp it between the angle
                        float startAngle = -2f * visualPlayer.useDirection;
                        float endAngle = 1.5f * visualPlayer.useDirection;
                        float swingAngle = MathHelper.Lerp(startAngle, endAngle, progress);

                        visualPlayer.weaponRotation = swingAngle + (visualPlayer.useDirection == -1 ? MathHelper.Pi : 0f); // Set the weapon rotation to the swing angle (that also changes depending on last use direction)
                    }
                    break;
                case MechWeaponsPlayer.UseType.HoldUp: // For hold up weapons,

                    visualPlayer.weaponScale = 1f; // Make weapon visible

                    // Set position and origin offsets
                    weaponPositionX = 16;
                    weaponPositionY = -44;
                    weaponOriginOffsetX = 0;
                    weaponOriginOffsetY = 0;
                    visualPlayer.weaponRotation = visualPlayer.useDirection == -1 ? MathHelper.Pi : 0; // Set the weapon rotation to be straight up

                    visualPlayer.armRFrame = 9; // Set the right arm frame to be by side
                    visualPlayer.armLFrame = 12; // Set the left arm frame to be holding up
                    break;
            }
            visualPlayer.weaponPosition = new Vector2(weaponPositionX * visualPlayer.useDirection, weaponPositionY); // Set the position of the weapon based on the weapon position values
            if (visualPlayer.weaponTexture != null)
                visualPlayer.weaponOrigin = new Vector2(visualPlayer.weaponTexture.Value.Width / 2 + weaponOriginOffsetX, visualPlayer.weaponTexture.Value.Height / 2 + weaponOriginOffsetY); // Set the origin of the weapon based on the weapon origin offset values (point of rotation)
            visualPlayer.weaponSpriteEffects = visualPlayer.useDirection == -1 ? SpriteEffects.FlipVertically : SpriteEffects.None; // Flip the weapon sprite based on the player's direction
        }

        // Function that applies the stats of each equipped part
        public void ApplyPartStats(Player player, Item equippedHead, Item equippedBody, Item equippedArms, Item equippedLegs, Item equippedBooster)
        {
            MechModPlayer modPlayer = player.GetModPlayer<MechModPlayer>();
            MechWeaponsPlayer weaponsPlayer = player.GetModPlayer<MechWeaponsPlayer>();

            // Reset weapon stats that are added/multiplied before applying new ones
            weaponsPlayer.partDamageBonus = default;
            weaponsPlayer.partCritChanceBonus = default;
            weaponsPlayer.partAttackSpeedBonus = default;
            weaponsPlayer.partKnockbackBonus = default;
            weaponsPlayer.finalDamageModifier = 1f;

            for (int i = 0; i < modPlayer.partEffectiveness.Length; i++)
            {
                modPlayer.partEffectiveness[i] = 1f; // Reset part effectiveness multipliers
            }
            // Apply body stats first as it increases and decreases the effectivness of other parts
            if (!equippedBody.IsAir)
                if (modPlayer.equippedParts[MechMod.bodyIndex].ModItem is IMechParts body)
                    body.ApplyStats(player, modPlayer, weaponsPlayer, this); // Apply Body stats

            if (!equippedArms.IsAir)
                if (modPlayer.equippedParts[MechMod.armsIndex].ModItem is IMechParts arms)
                    arms.ApplyStats(player, modPlayer, weaponsPlayer, this); // Apply Arm stats
            if (!equippedLegs.IsAir)
                if (modPlayer.equippedParts[MechMod.legsIndex].ModItem is IMechParts legs)
                    legs.ApplyStats(player, modPlayer, weaponsPlayer, this); // Apply Leg stats

            // Reset flight stats before applying new ones
            MountData.flightTimeMax = default;
            modPlayer.flightHorizontalSpeed = default;
            modPlayer.flightJumpSpeed = default;
            player.GetModPlayer<DashPlayer>().ableToDash = false;
            player.GetModPlayer<DashPlayer>().dashCoolDown = default;
            player.GetModPlayer<DashPlayer>().dashDuration = default;
            player.GetModPlayer<DashPlayer>().dashVelo = default;

            if (!equippedBooster.IsAir) // If a Booster is equipped,
            {
                if (modPlayer.equippedParts[MechMod.boosterIndex].ModItem is IMechParts booster)
                    booster.ApplyStats(player, modPlayer, weaponsPlayer, this); // Apply Booster stats
            }
            else
            {
                // Unique as a player can go without a Booster, but the other Parts are required
                if (modPlayer.powerCellActive)
                    modPlayer.lifeBonus += 100; // Give extra life bonus if no Booster is equipped with power cell active
            }

            // Apply head stats last as it can have multiplicative effects
            if (!equippedHead.IsAir)
                if (modPlayer.equippedParts[MechMod.headIndex].ModItem is IMechParts head)
                    head.ApplyStats(player, modPlayer, weaponsPlayer, this);
        }

        /// <summary>
        /// Handles dashing ability when a Booster is equipped to the Mech.
        /// </summary>

        public class DashPlayer : ModPlayer
        {
            public bool ableToDash; // Check if the player can dash at all
            public float dashVelo; // Velocity of the dash
            public int dashCoolDown; // Time between dashes
            public int dashDuration; // Time the dash lasts

            public bool dashActive; // Check if the player is currently dashing

            private int dashDelay = 0; // Delay between dashes
            private int dashTimer = 0; // Keeps track of how long the dash has been active

            public override void PreUpdateMovement()
            {
                if (ableToDash && Player.mount.Active && Player.mount.Type == ModContent.MountType<ModularMech>()) // If able to dash and the player is mounted on the mech,
                {
                    if (MechMod.MechDashKeybind.JustPressed && dashDelay == 0) // If the player presses the dash keybind and the dash is not on cooldown,
                    {
                        // Set the dash direction based on player input
                        int dashDir;
                        if (Player.controlRight)
                            dashDir = 1;
                        else if (Player.controlLeft)
                            dashDir = -1;
                        else
                            dashDir = Player.direction;

                        // Get the dash velocity based on the dash direction and dash velocity stat
                        float newVelo;
                        newVelo = dashDir * dashVelo;

                        // Set the dash cooldown and timer
                        dashDelay = dashCoolDown;
                        dashTimer = dashDuration;

                        Player.velocity.X = newVelo; // Apply the dash velocity to the player
                    }

                    if (dashDelay > 0) // If the dash is on cooldown,
                        dashDelay--; // Decrement the delay
                }

                if (dashTimer > 0) // If dash is currently active,
                {
                    dashTimer--; // Decrement the dash timer
                    dashActive = true; // Set dash active to true
                    if (dashTimer <= 0) // If the dash timer has run out,
                    {
                        dashActive = false; // Set dash active to false
                        if (Player.direction == 1 ? Player.velocity.X > Player.mount.RunSpeed : Player.velocity.X < -Player.mount.RunSpeed)
                            Player.velocity.X -= Player.mount.RunSpeed * Player.direction; // Stop the dash velocity increase when the timer runs out
                    }
                }
            }
        }
    }
}
