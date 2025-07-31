using System.Collections.Generic;
using Terraria.ModLoader;
using Terraria;
using MechMod.Content.Mounts;
using Terraria.DataStructures;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using MechMod.Common.Players;
using Terraria.ID;
using Terraria.Audio;

namespace MechMod.Content.Items.MechWeapons
{
    public class ProjSword : ModItem, IMechWeapon
    {
        public override void SetDefaults()
        {
            Item.width = 20; // The width of the item's hitbox in pixels.
            Item.height = 20; // The height of the item's hitbox in pixels.
            Item.value = Item.buyPrice(gold: 8);
            Item.rare = 3; // The rarity of the item.
        }

        public void SetStats(Player player)
        {
            Weapons.DamageClass = DamageClass.Melee; // Set the damage class for ranged weapons
            Weapons.useType = Weapons.UseType.Swing; // Set the use type for point weapons
        }

        public void UseAbility(Player player, Vector2 mousePosition, bool toggleOn)
        {
            int projectileType = ModContent.ProjectileType<BaseSwordProj>();

            int damage = Weapons.DamageCalc(8, player);
            Weapons.CritChanceCalc(6, player);
            Weapons.attackRate = Weapons.AttackSpeedCalc(20, player);
            float knockback = Weapons.KnockbackCalc(6, player);

            int projID = Projectile.NewProjectile(new EntitySource_Parent(player), player.MountedCenter, new Vector2(0, 0), projectileType, damage, knockback, player.whoAmI);
            if (Main.projectile.IndexInRange(projID) && Main.projectile[projID].ModProjectile is BaseSwordProj proj)
            {
                // Allow the swing speed to be modified by attack rate
                proj.swingDuration = Weapons.attackRate; // Fixed number
                Main.projectile[projID].timeLeft = (int)Weapons.attackRate; // Decreases each tick
            }
            float projSpeed = 10;
            Vector2 offset = new Vector2(0, -42); // Offset to adjust the projectile's spawn position relative to the mech's center
            Vector2 direction = (Main.MouseWorld - player.MountedCenter) - offset; // new Vector2 corrects the offset to still make it go towards the cursor
            direction.Normalize(); // Normalize the direction vector to ensure it has a length of 1
            Vector2 velocity = direction * projSpeed;
            Projectile.NewProjectile(new EntitySource_Parent(player), player.MountedCenter + offset, velocity, ProjectileID.SwordBeam, damage, knockback, player.whoAmI);

            SoundEngine.PlaySound(SoundID.Item1, player.position); // Play Swing sound when the weapon is used
            SoundEngine.PlaySound(SoundID.Item8, player.position); // Play Projectile sound when the weapon is used
        }
    }

    public class ProjSwordProj : ModProjectile
    {
        Player player;
        MechModPlayer modPlayer;

        public float swingDuration = 0f;

        public override string Texture => "Terraria/Images/MagicPixel"; // Texture is not needed, visuals are handled in ModularMech code

        public override void SetDefaults()
        {
            Projectile.width = 90;
            Projectile.height = 90;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.ownerHitCheck = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.DamageType = DamageClass.Melee; // Inherit bonuses like magma stone effect but not damage class damage scaling
        }

        public override void AI()
        {
            player = Main.player[Projectile.owner];
            modPlayer = player.GetModPlayer<MechModPlayer>();

            modPlayer.animationProgress = Projectile.timeLeft; // Set the animation progress to the time left of the projectile
            float progress = 1f - (Projectile.timeLeft / swingDuration); // Progress goes from 1 to 0 as the projectile time decreases

            // Changed the position of the hitbox as it goes through the swing
            Vector2 position = new Vector2(0, 0);

            if (progress <= 0.33)
                position = new Vector2(-30 * modPlayer.lastUseDirection, -130);
            else if (progress <= 0.66)
                position = new Vector2(70 * modPlayer.lastUseDirection, -100);
            else
                position = new Vector2(70 * modPlayer.lastUseDirection, 0);

            Projectile.Center = player.Center + position;

            if (!player.mount.Active)
                Projectile.Kill();
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.MagicPixel.Value; // Texture is not needed, visuals are handled in ModularMech code

            Main.EntitySpriteDraw(
                texture,
                Projectile.Center,
                null,
                lightColor,
                Projectile.rotation,
                new Vector2(0,0),
                0,
                SpriteEffects.None,
                0
            );
            return false;
        }

        public override void OnKill(int timeLeft)
        {
            modPlayer.animationProgress = 0; // Reset the animation progress
        }
    }
}
