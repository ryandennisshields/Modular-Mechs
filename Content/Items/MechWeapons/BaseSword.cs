using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria;
using MechMod.Content.Mechs;
using MechMod.Content.Items.MechWeapons;
using Terraria.ID;
using Terraria.DataStructures;
using Microsoft.Xna.Framework;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using MechMod.Common.Players;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Composition.Convention;

namespace MechMod.Content.Items.MechWeapons
{
    public class BaseSword : ModItem, IMechWeapon
    {
        public override void SetDefaults()
        {
            Item.width = 20; // The width of the item's hitbox in pixels.
            Item.height = 20; // The height of the item's hitbox in pixels.
            Item.value = 10000; // The value of the item in copper coins.
            Item.rare = 3; // The rarity of the item.
        }

        public float timer { get; set; } = 0f;
        public float attackRate { get; set; } = 0f;
        public Weapons.UseType useType => Weapons.UseType.Swing;

        public void UseAbility(Player player, Vector2 mousePosition, bool toggleOn)
        {
            int projectileType = ModContent.ProjectileType<BaseSwordSwing>();

            Weapons.damageClass = DamageClass.Melee;

            int damage = Weapons.DamageCalc(12, player);
            Weapons.CritChanceCalc(4, player);
            attackRate = Weapons.AttackSpeedCalc(100, player);
            float knockback = Weapons.KnockbackCalc(4, player);

            int owner = player.whoAmI;

            if (player.whoAmI == Main.myPlayer && Main.mouseLeft && timer >= attackRate)
            {
                int projID = Projectile.NewProjectile(new EntitySource_Parent(player), Main.LocalPlayer.MountedCenter, new Vector2(0,0), projectileType, damage, knockback, owner);
                if (Main.projectile.IndexInRange(projID) && Main.projectile[projID].ModProjectile is BaseSwordSwing swing)
                {
                    swing.swingDuration = attackRate;
                    Main.projectile[projID].timeLeft = (int)attackRate; // Ensure timeLeft matches
                }
            }
        }
    }

    public class BaseSwordSwing : ModProjectile
    {
        //private const float SwingArc = 2.5f;
        public float swingDuration;

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 80;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            //Projectile.timeLeft = (int)swingDuration;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.ownerHitCheck = true;

            AIType = ProjectileID.Bullet;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            MechModPlayer modPlayer = player.GetModPlayer<MechModPlayer>();

            float progress = 1f - (Projectile.timeLeft / swingDuration);

            //player.GetModPlayer<MechModPlayer>().animationTime = MathHelper.Lerp(3, 0, progress);
            //Main.NewText(player.GetModPlayer<MechModPlayer>().animationTime);

            if (progress <= 0.25)
            {
                modPlayer.animationProgress = 4;
            }
            else if (progress <= 0.5)
            {
                modPlayer.animationProgress = 3;
            }
            else if (progress <= 0.75)
            {
                modPlayer.animationProgress = 2;
            }
            else
            {
                modPlayer.animationProgress = 1;
            }
            //Main.NewText(progress);

            // Set the swing arc (from -45 to +45 degrees+, for example)
            float startAngle = -1.5f * modPlayer.lastUseDirection;
            float endAngle = 1.5f * modPlayer.lastUseDirection;
            float angle = MathHelper.Lerp(startAngle, endAngle, progress);

            // Set the distance from the player (how far the sword is held out)
            float distance = 60f; // Adjust to match your sprite's blade length

            // Offset for the swing's origin (raise/lower as needed)
            Vector2 swingOriginOffset = new Vector2(-10, -40);

            // Calculate the position of the sword's tip
            Vector2 swingOrigin = player.MountedCenter + swingOriginOffset;
            Vector2 offset = angle.ToRotationVector2() * distance;
            Projectile.Center = swingOrigin + offset;

            // Set rotation for drawing
            Projectile.rotation = angle + (modPlayer.lastUseDirection == 1 ? 0f : MathHelper.Pi);

            // Make sure the projectile follows the player's direction
            //Projectile.direction = player.direction;
            //Projectile.spriteDirection = player.direction;

            // Optional: kill the projectile if the player can't use items
            //if (!player.channel || player.noItems || player.CCed)
            //    Projectile.Kill();
        }

        public override void OnKill(int timeLeft)
        {
            Player player = Main.player[Projectile.owner];
            player.GetModPlayer<MechModPlayer>().animationProgress = 0;
        }


        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;

            // Use the mech's center as the origin for rotation
            // The projectile's position is already set to player.MountedCenter + offset in AI
            // So, draw at player.MountedCenter, with the origin at the base of the sword sprite (e.g., handle)
            Player player = Main.player[Projectile.owner];
            Vector2 mechCenter = player.MountedCenter - Main.screenPosition;

            // Set the origin to the base of the sword (e.g., middle of the left edge if the blade points right)
            // -60 is distance
            Vector2 swordOrigin = new Vector2(texture.Width / 2f - 60f, texture.Height); // Adjust as needed for your sprite

            Main.EntitySpriteDraw(
                texture,
                mechCenter + new Vector2(-10, -40), // -40 to make it higher offset
                null,
                lightColor,
                Projectile.rotation,
                swordOrigin,
                Projectile.scale,
                Projectile.spriteDirection == -1 ? SpriteEffects.FlipVertically : SpriteEffects.None,
                0
            );
            return false; // We handled drawing
        }
    }
}
