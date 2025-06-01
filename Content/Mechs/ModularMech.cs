using Humanizer;
using log4net.Appender;
using MechMod.Common.Players;
using MechMod.Content.Buffs;
using MechMod.Content.Items.MechArms;
using MechMod.Content.Items.MechBodies;
using MechMod.Content.Items.MechBoosters;
using MechMod.Content.Items.MechHeads;
using MechMod.Content.Items.MechLegs;
using MechMod.Content.Items.MechWeapons;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using rail;
using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Net.Mail;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Chat;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI.Chat;

namespace MechMod.Content.Mechs
{
    public interface IMechParts
    {
        void ApplyStats(ModularMech mech);
    }
    public interface IMechWeapon
    {
        void UseAbility(Player player, Vector2 mousePosition, bool toggleOn);
        Weapons.UseType useType { get; }

        public float timer { get; set; }
        public float attackRate { get; set; }
    }

    public class ModularMech : ModMount
    {
        public override void SetStaticDefaults()
        {
            // Misc
            MountData.fallDamage = 0;
            MountData.heightBoost = 20; // Height between the mount and the ground (player's hitbox position)
            MountData.constantJump = true;
            MountData.blockExtraJumps = true;
            MountData.buff = ModContent.BuffType<MechBuff>();
            // Effects
            MountData.spawnDust = DustID.Smoke;
            // Frame data and player offsets
            MountData.totalFrames = 4; // Although the actual frame count is different, animations breaks if this value is any higher or lower
            MountData.playerYOffsets = Enumerable.Repeat(20, MountData.totalFrames).ToArray(); // Fills an array with values for less repeating code
            // Standing
            // All set to 0 as there is no standing animation
            MountData.standingFrameCount = 0;
            MountData.standingFrameDelay = 0;
            MountData.standingFrameStart = 0;
            // Running
            MountData.runningFrameCount = 4;
            MountData.runningFrameDelay = 12;
            MountData.runningFrameStart = 0;
            // Flying
            MountData.flyingFrameCount = 1;
            MountData.flyingFrameDelay = 12;
            MountData.flyingFrameStart = 4;
            // In-air
            MountData.inAirFrameCount = 1;
            MountData.inAirFrameDelay = 12;
            MountData.inAirFrameStart = 5;
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

            if (!Main.dedServ)
            {
                MountData.textureWidth = MountData.backTexture.Width();
                MountData.textureHeight = MountData.backTexture.Height();
            }

            MountData.acceleration = 0f;
            MountData.runSpeed = 0f;
            MountData.swimSpeed = 0f;
            MountData.dashSpeed = 0f;

            MountData.jumpHeight = 0;
            MountData.jumpSpeed = 0f;
        }

        public int lifeBonus;
        private bool grantedLifeBonus;

        public override void SetMount(Player player, ref bool skipDust)
        {
            var modPlayer = player.GetModPlayer<MechModPlayer>();

            // Hide the Player
            player.opacityForAnimation = 0;
            // Make the Player's hitbox slightly larger
            player.width = 26;

            // Apply Part Stats
            ApplyPartStats(modPlayer, modPlayer.equippedParts[MechMod.headIndex], modPlayer.equippedParts[MechMod.bodyIndex], modPlayer.equippedParts[MechMod.armsIndex], modPlayer.equippedParts[MechMod.legsIndex], modPlayer.equippedParts[MechMod.boosterIndex]);
        }

        public override void Dismount(Player player, ref bool skipDust)
        {
            // Debuff the Player
            int mechDebuff = ModContent.BuffType<MechDebuff>();
            //Should be set to 3600 ticks
            player.AddBuff(mechDebuff, 60);

            // Make Player visible
            player.opacityForAnimation = 1;
            // Reset Player's hitbox to default
            player.width = 20;

            // Reset life bonus, granted life bonus, part damage bonus, part attack speed bonus values
            lifeBonus = 0;
            grantedLifeBonus = false;
            Weapons.partDamageBonus = 0;
            Weapons.partAttackSpeedBonus = 0;
        }

        public override void UpdateEffects(Player player)
        {
            if (player.mount.Type == ModContent.MountType<ModularMech>())
            {
                // These are here to make sure the Player does not recieve buffs like set bonus buffs
                player.head = 0;
                player.body = 0;
                player.legs = 0;
            }

            // Grant life bonus
            player.statLifeMax2 += lifeBonus;
            if (grantedLifeBonus == false)
            {
                player.statLife += lifeBonus;
                grantedLifeBonus = true;
            }
        }

        private Texture2D headTexture;
        private Texture2D bodyTexture;
        private Texture2D armsTexture;
        private Texture2D legsTexture;
        private Texture2D weaponTexture; // Used so the equipped weapon can be drawn while in use

        private Vector2 armPosition = Vector2.Zero;
        private int armFrame = -1; // Used for controlling the current arm frame
        private int armAnimationFrames = 11; // Total number of frames that the arm texture has

        private Vector2 weaponPosition = Vector2.Zero; // Used for positioning the weapon when it is drawn (depending on use type)
        private float weaponRotation = 0f; // Used for rotating the weapon when it is drawn (depending on use type)
        private Vector2 weaponOrigin = Vector2.Zero; // Used so a different origin can be set for rotation (used for the swing use type)
        private float weaponScale = 1f; // Used so the weapon can be hidden when needed

        public float swingProgress = -1f; // Used specifically so swing animation can be tied to arm movement

        public override bool Draw(List<DrawData> playerDrawData, int drawType, Player drawPlayer, ref Texture2D texture, ref Texture2D glowTexture, ref Vector2 drawPosition, ref Rectangle frame, ref Color drawColor, ref Color glowColor, ref float rotation, ref SpriteEffects spriteEffects, ref Vector2 drawOrigin, ref float drawScale, float shadow)
        {
            // Apply visuals to the Mech
            //if (!modPlayer.equippedParts[MechMod.headIndex].IsAir)
            //    headTexture = Mod.Assets.Request<Texture2D>($"Content/Items/MechHeads/{modPlayer.equippedParts[MechMod.headIndex].ModItem.GetType().Name}").Value;
            //if (!modPlayer.equippedParts[MechMod.bodyIndex].IsAir)
            //    bodyTexture = Mod.Assets.Request<Texture2D>($"Content/Items/MechBodies/{modPlayer.equippedParts[MechMod.bodyIndex].ModItem.GetType().Name}").Value;
            //if (!modPlayer.equippedParts[MechMod.armsIndex].IsAir)
            //    armsTexture = Mod.Assets.Request<Texture2D>($"Content/Items/MechArms/{modPlayer.equippedParts[MechMod.armsIndex].ModItem.GetType().Name}").Value;
            //if (!modPlayer.equippedParts[MechMod.legsIndex].IsAir)
            //    legsTexture = Mod.Assets.Request<Texture2D>($"Content/Items/MechLegs/{modPlayer.equippedParts[MechMod.legsIndex].ModItem.GetType().Name}").Value;
            //if (!modPlayer.equippedParts[MechMod.weaponIndex].IsAir)
            //    weaponTexture = Mod.Assets.Request<Texture2D>($"Content/Items/MechWeapons/{modPlayer.equippedParts[MechMod.weaponIndex].ModItem.GetType().Name}").Value;

            // Apply visuals to the Mech (TEMPORARY TO MANUALLY SET TEXTURES FOR TESTING)
            var modPlayer = drawPlayer.GetModPlayer<MechModPlayer>();
            if (!modPlayer.equippedParts[MechMod.headIndex].IsAir)
                headTexture = Mod.Assets.Request<Texture2D>($"Content/Items/MechHeads/TestHeadAnim").Value;
            if (!modPlayer.equippedParts[MechMod.bodyIndex].IsAir)
                bodyTexture = Mod.Assets.Request<Texture2D>($"Content/Items/MechBodies/TestBodyAnim").Value;
            if (!modPlayer.equippedParts[MechMod.armsIndex].IsAir)
                armsTexture = Mod.Assets.Request<Texture2D>($"Content/Items/MechArms/TestArmsAnim").Value;
            if (!modPlayer.equippedParts[MechMod.legsIndex].IsAir)
                legsTexture = Mod.Assets.Request<Texture2D>($"Content/Items/MechLegs/TestLegsAnim").Value;
            if (!modPlayer.equippedParts[MechMod.weaponIndex].IsAir)
                weaponTexture = Mod.Assets.Request<Texture2D>($"Content/Items/MechWeapons/{modPlayer.equippedParts[MechMod.weaponIndex].ModItem.GetType().Name}").Value;

            //armPosition = new Vector2(-22 * drawPlayer.direction, -32);
            weaponPosition = new Vector2(3 * drawPlayer.direction, -44); // 3 = offset from player center as base value is a bit too far to the right, -44 = y offset to match arm position
            weaponOrigin = new Vector2(weaponTexture.Width / 2 - 26, weaponTexture.Height / 2); // width = getting the middle point of the weapon sprite combined with an offset to make it point outwards more from the mech body, height = middle point of sprite (offsetting this does weird stuff)
            SpriteEffects weaponSpriteEffects = drawPlayer.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipVertically; // Flip the weapon sprite based on the player's direction

            Rectangle setArmFrame = frame; // Get the default frame logic as a new rectangle
            if (armFrame >= 0) // If the arm frame is manually set,
            {
                int frameHeight = armsTexture.Height / armAnimationFrames; // Calculate the height of each frame based on the total height of the texture and the number of frames
                setArmFrame = new Rectangle(0, armFrame * frameHeight, armsTexture.Width, frameHeight); // Change the set arm frame to a new rectangle based on the arm frame and the height of each frame
            }

            if (drawType == 0)
            {
                // Draw left arm first
                playerDrawData.Add(new DrawData(armsTexture, drawPosition + new Vector2(24 * drawPlayer.direction, -46), frame, drawColor, rotation, drawOrigin, drawScale, spriteEffects));

                // Draw legs
                playerDrawData.Add(new DrawData(legsTexture, drawPosition + new Vector2(drawPlayer.direction, 13), frame, drawColor, rotation, drawOrigin, drawScale, spriteEffects));

                // Draw body
                playerDrawData.Add(new DrawData(bodyTexture, drawPosition + new Vector2(drawPlayer.direction, -32), frame, drawColor, rotation, drawOrigin, drawScale, spriteEffects));
                
                // Draw head
                playerDrawData.Add(new DrawData(headTexture, drawPosition + new Vector2(drawPlayer.direction, -71), frame, drawColor, rotation, drawOrigin, drawScale, spriteEffects));

                // Draw weapon
                playerDrawData.Add(new DrawData(weaponTexture, drawPosition + weaponPosition, null, drawColor, weaponRotation, weaponOrigin, weaponScale, weaponSpriteEffects));

                // Draw right arm last
                playerDrawData.Add(new DrawData(armsTexture, drawPosition + new Vector2(-22 * drawPlayer.direction, -46), setArmFrame, drawColor, rotation, drawOrigin, drawScale, spriteEffects));
            }
            return false;
        }

        public override void UseAbility(Player player, Vector2 mousePosition, bool toggleOn)
        {
            var modPlayer = player.GetModPlayer<MechModPlayer>();

            if (modPlayer.equippedParts[MechMod.weaponIndex].ModItem is IMechWeapon weapon)
            {
                if (player.whoAmI == Main.myPlayer && Main.mouseLeft) // Only if the player is holding left mouse
                {
                    if (Main.MouseWorld.X > Main.LocalPlayer.MountedCenter.X)
                        Main.LocalPlayer.direction = 1;
                    else
                        Main.LocalPlayer.direction = -1;
                }
                if (player.whoAmI == Main.myPlayer && Main.mouseLeft && weapon.timer >= weapon.attackRate)
                {
                    WeaponUseAnimation(weapon.useType);
                }
                //
                // THIS SHOULD ALSO NOT ACTIVATE IF THE ANIMATION TIME IS STILL ACTIVE
                //
                else if (player.whoAmI == Main.myPlayer && !Main.mouseLeft) // Only if the player has let go of left mouse
                {
                    armFrame = -1; // Reset the arm frame to default
                    weaponScale = 0f; // Hide the weapon when not in use
                }
                weapon.UseAbility(player, mousePosition, toggleOn);
            }
            else
            {
                return;
            }
        }

        // Function for weapon use animations like pointing and swinging 
        private void WeaponUseAnimation(Weapons.UseType useType)
        {
            // change arm sprite so the shoulder of the arm is in the middle (to act as the "hinge" point)

            // OLD METHOD:
            // add all the arm positions to player draw data so we can change when they are visible or not by modifying their scale

            // NEW METHOD:
            // new sprites will be added to the arm's animation sprite sheet with code that will change between the required frames

            // arm will have 5 sprites for 5 positions: "Up", "Angled Up", "Horizontal", "Angled Down", "Down"
            // these sprites should overwrite the base arm's texture and not animate like the regular arm
            // For "Point" use type:
            // when the player is firing in specific directions ("Angled Down", "Horizontal", "Angled Up"), change the sprite of the arm to match the direction, and have the mech hold the weapon pointing towards the mouse
            // For "Swing" use type:
            // move the arm between the "Up", "Angled Up" and "Horizontal" positions/sprites for each swing (mimicking a sword swing), with the weapon swinging in a downward arc along with the arm motion
            // also make sure that the entire mech is facing the direction of attack
            // weapons that swing will need to have timers like the time it takes to move between the arm positions, the overall "animation time" for how fast the weapon and arm moves, plus all the smooth rotation stuff with the swing animation
            // for point weapons, just make the texture constantly rotating to point towards the mouse

            // animation time variable might be needed for everything, for example, like how slower firing guns in the game remain out before the weapon can be fired again or after a set duration
            // animation time can be added to the IMechWeapon interface, so that each weapon can have its own animation time, or it can be set to a default value in the ModularMech class
            // For Point weapons, animation time basically dictates how long the arm stays in the pointing position, while for Swing weapons, it dictates how long the arm takes to swing down (which technically also controls the speed of the swing)

            // Swing weapons create a projectile then that projectile is modified with Main.projectiles (to change position actively)

            float angle = (Main.MouseWorld - Main.LocalPlayer.MountedCenter).ToRotation(); // Get the angle between the mouse position and the player mounted center
            int direction = Main.MouseWorld.X > Main.LocalPlayer.MountedCenter.X ? 1 : -1; // Determine the direction based on the mouse position relative to the player
            float angleDeg = MathHelper.ToDegrees(angle); // Convert the angle to degrees for easier calculations
            switch (useType)
            {
                case Weapons.UseType.Point:
                    weaponRotation = angle; // Set the weapon rotation to the angle between the mouse and the player
                    weaponScale = 1f; // Set the weapon scale to 1 so it is visible when pointing
                    // Check if the mouse is at different angles relative to the player (so if mouse is pointing up, the arm will point up, if mouse is pointing down, the arm will point down, etc.)
                    if (angleDeg >= -135 && angleDeg <= -45)
                    {
                        armFrame = 9; // Pointing angled up
                    }
                    else if (angleDeg >= -45 && angleDeg <= 45)
                    {
                        armFrame = 8; // Pointing horizontal right
                    }
                    else if (angleDeg <= -135 || angleDeg >= 135)
                    {
                        armFrame = 8; // Pointing horizontal left
                    }
                    else if (angleDeg <= 135 && angleDeg >= 45)
                    {
                        armFrame = 7; // Pointing angled down
                    }
                    //else
                        //armFrame = -1;
                    break;
                case Weapons.UseType.Swing:
                    if (swingProgress < 1f / 3f)
                    {
                        Main.NewText("run up");
                        armFrame = 10; // Up
                    }
                    else if (swingProgress < 2f / 3f)
                    {
                        armFrame = 9; // Angled up
                    }
                    else
                    {
                        armFrame = 8; // Horizontal
                    }
                    break;
                default:
                    armFrame = -1;
                    break;
            }
        }

        public void ApplyPartStats(MechModPlayer modPlayer, Item equippedHead, Item equippedBody, Item equippedArms, Item equippedLegs, Item equippedBooster)
        {
            if (!equippedHead.IsAir)
                if (modPlayer.equippedParts[MechMod.headIndex].ModItem is IMechParts head)
                    head.ApplyStats(this);
            if (!equippedBody.IsAir)
                if (modPlayer.equippedParts[MechMod.bodyIndex].ModItem is IMechParts body)
                    body.ApplyStats(this);
            if (!equippedArms.IsAir)
                if (modPlayer.equippedParts[MechMod.armsIndex].ModItem is IMechParts arms)
                    arms.ApplyStats(this);
            if (!equippedLegs.IsAir)
                if (modPlayer.equippedParts[MechMod.legsIndex].ModItem is IMechParts legs)
                    legs.ApplyStats(this);
            if (!equippedBooster.IsAir)
            {
                if (modPlayer.equippedParts[MechMod.boosterIndex].ModItem is IMechParts booster)
                    booster.ApplyStats(this);
            }
            else
            {
                // Unique as a player can go without a Booster, but the other parts are required

                MountData.flightTimeMax = 0;
                MountData.fatigueMax = 0;
                MountData.usesHover = false;

                Player player = Main.LocalPlayer;
                player.GetModPlayer<DashPlayer>().ableToDash = false;
                player.GetModPlayer<DashPlayer>().dashCoolDown = 0;
                player.GetModPlayer<DashPlayer>().dashDuration = 0;
                player.GetModPlayer<DashPlayer>().dashVelo = 0f;

                lifeBonus += 200;
            }
        }

        public class DashPlayer : ModPlayer
        {
            public bool ableToDash;

            public int dashCoolDown;
            public int dashDuration;

            public float dashVelo;

            private const int dashRight = 0;
            private const int dashLeft = 1;

            private int dashDir = -1;

            private int dashDelay = 0;
            private int dashTimer = 0; // CAN BE USED LATER FOR EFFECTS MID-DASH (See ExampleMod's Shield Accessory)

            public override void ResetEffects()
            {
                if (ableToDash && Player.mount.Active && Player.mount.Type == ModContent.MountType<ModularMech>())
                {
                    if (Player.controlRight && Player.releaseRight && Player.doubleTapCardinalTimer[dashRight] < 15)
                    {
                        dashDir = dashRight;
                    }
                    else if (Player.controlLeft && Player.releaseLeft && Player.doubleTapCardinalTimer[dashLeft] < 15)
                    {
                        dashDir = dashLeft;
                    }
                    else
                    {
                        dashDir = -1;
                    }
                }
            }

            public override void PreUpdateMovement()
            {
                if (ableToDash && Player.mount.Active && Player.mount.Type == ModContent.MountType<ModularMech>())
                {
                    if (dashDir != -1 && dashDelay == 0)
                    {
                        Vector2 newVelo = Player.velocity;

                        switch (dashDir)
                        {
                            case dashLeft when Player.velocity.X > -dashVelo:
                            case dashRight when Player.velocity.X < dashVelo:
                                {
                                    float dashDirection = dashDir == dashRight ? 1 : -1;
                                    newVelo.X = dashDirection * dashVelo;
                                    break;
                                }
                            default: return;
                        }

                        dashDelay = dashCoolDown;
                        dashTimer = dashDuration;
                        Player.velocity = newVelo;
                    }

                    if (dashDelay > 0)
                        dashDelay--;
                }
            }
        }
    }
}
