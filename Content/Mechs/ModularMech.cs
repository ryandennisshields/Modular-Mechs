using Humanizer;
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
using rail;
using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
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
            MountData.totalFrames = 4; // Amount of animation frames for the mount
            MountData.playerYOffsets = Enumerable.Repeat(20, MountData.totalFrames).ToArray(); // Fills an array with values for less repeating code
            MountData.xOffset = 13;
            MountData.yOffset = -12;
            MountData.playerHeadOffset = 22;
            MountData.bodyFrame = 3;
            // Standing
            MountData.standingFrameCount = 4;
            MountData.standingFrameDelay = 12;
            MountData.standingFrameStart = 0;
            // Running
            MountData.runningFrameCount = 4;
            MountData.runningFrameDelay = 12;
            MountData.runningFrameStart = 0;
            // Flying
            MountData.flyingFrameCount = 0;
            MountData.flyingFrameDelay = 0;
            MountData.flyingFrameStart = 0;
            // In-air
            MountData.inAirFrameCount = 1;
            MountData.inAirFrameDelay = 12;
            MountData.inAirFrameStart = 0;
            // Idle
            MountData.idleFrameCount = 4;
            MountData.idleFrameDelay = 12;
            MountData.idleFrameStart = 0;
            MountData.idleFrameLoop = true;
            // Swim
            MountData.swimFrameCount = MountData.inAirFrameCount;
            MountData.swimFrameDelay = MountData.inAirFrameDelay;
            MountData.swimFrameStart = MountData.inAirFrameStart;

            if (!Main.dedServ)
            {
                MountData.textureWidth = MountData.backTexture.Width() + 20;
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

        private Texture2D headTexture;
        private Texture2D bodyTexture;
        private Texture2D armsTexture;
        private Texture2D legsTexture;

        public override void SetMount(Player player, ref bool skipDust)
        {
            var modPlayer = player.GetModPlayer<MechModPlayer>();
            
            // Hide the Player
            player.opacityForAnimation = 0;
            // Make the Player's hitbox slightly larger
            player.width = 26;

            // Apply visuals to the Mech
            for (int i = 0; i < MechMod.boosterIndex; i++)
            {
                if (!modPlayer.equippedParts[i].IsAir)
                {
                    headTexture = Mod.Assets.Request<Texture2D>($"Content/Items/MechHeads/{modPlayer.equippedParts[MechMod.headIndex].ModItem.GetType().Name}").Value;
                    bodyTexture = Mod.Assets.Request<Texture2D>($"Content/Items/MechBodies/{modPlayer.equippedParts[MechMod.bodyIndex].ModItem.GetType().Name}").Value;
                    armsTexture = Mod.Assets.Request<Texture2D>($"Content/Items/MechArms/{modPlayer.equippedParts[MechMod.armsIndex].ModItem.GetType().Name}").Value;
                    legsTexture = Mod.Assets.Request<Texture2D>($"Content/Items/MechLegs/{modPlayer.equippedParts[MechMod.legsIndex].ModItem.GetType().Name}").Value;
                }
            }

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

        private void UpdatePartTextures(Player player)
        {
            var modPlayer = player.GetModPlayer<MechModPlayer>();

            // Change the texture based on the equipped parts
            headTexture = Mod.Assets.Request<Texture2D>($"Content/Items/MechHeads/{modPlayer.equippedParts[MechMod.headIndex].ModItem.GetType().Name}").Value;
            bodyTexture = Mod.Assets.Request<Texture2D>($"Content/Items/MechBodies/{modPlayer.equippedParts[MechMod.bodyIndex].ModItem.GetType().Name}").Value;
            armsTexture = Mod.Assets.Request<Texture2D>($"Content/Items/MechArms/{modPlayer.equippedParts[MechMod.armsIndex].ModItem.GetType().Name}").Value;
            legsTexture = Mod.Assets.Request<Texture2D>($"Content/Items/MechLegs/{modPlayer.equippedParts[MechMod.legsIndex].ModItem.GetType().Name}").Value;
        }

        public override bool Draw(List<DrawData> playerDrawData, int drawType, Player drawPlayer, ref Texture2D texture, ref Texture2D glowTexture, ref Vector2 drawPosition, ref Rectangle frame, ref Color drawColor, ref Color glowColor, ref float rotation, ref SpriteEffects spriteEffects, ref Vector2 drawOrigin, ref float drawScale, float shadow)
        {
            if (drawType == 0)
            {
                // Draw body first
                playerDrawData.Add(new DrawData(bodyTexture, drawPosition + new Vector2(drawPlayer.direction, 0), frame, drawColor, rotation, drawOrigin, drawScale, spriteEffects));

                // Draw head
                playerDrawData.Add(new DrawData(headTexture, drawPosition + new Vector2(drawPlayer.direction, 0), frame, drawColor, rotation, drawOrigin, drawScale, spriteEffects));

                // Draw arms
                playerDrawData.Add(new DrawData(armsTexture, drawPosition + new Vector2(drawPlayer.direction, 0), frame, drawColor, rotation, drawOrigin, drawScale, spriteEffects));

                // Draw legs
                playerDrawData.Add(new DrawData(legsTexture, drawPosition + new Vector2(drawPlayer.direction, 0), frame, drawColor, rotation, drawOrigin, drawScale, spriteEffects));
            }
            return false;
        }

        public override void UseAbility(Player player, Vector2 mousePosition, bool toggleOn)
        {
            var modPlayer = player.GetModPlayer<MechModPlayer>();

            if (modPlayer.equippedParts[MechMod.weaponIndex].ModItem is IMechWeapon weapon)
            {
                weapon.UseAbility(player, mousePosition, toggleOn);
            }
            else
            {
                return;
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
