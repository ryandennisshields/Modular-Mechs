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
            Item.value = Item.buyPrice(gold: 8);
            Item.rare = ItemRarityID.Orange;
        }

        public void SetStats(MechWeaponsPlayer weaponsPlayer)
        {
            weaponsPlayer.DamageClass = DamageClass.Melee; // Set DamageClass to Melee
            weaponsPlayer.useType = MechWeaponsPlayer.UseType.Swing; // Set use type to Swing
        }

        public void UseAbility(Player player, MechWeaponsPlayer weaponsPlayer, MechVisualPlayer visualPlayer, Vector2 mousePosition, bool toggleOn)
        {
            weaponsPlayer.canUse = true; // Always allow use for this weapon

            int projectileType = ModContent.ProjectileType<ProjSwordProj>(); // Use a custom projectile for the sword swing

            // Calculate projectile properties
            int damage = weaponsPlayer.DamageCalc(32, player);
            weaponsPlayer.CritChanceCalc(6, player);
            weaponsPlayer.attackRate = weaponsPlayer.AttackSpeedCalc(20, player);
            float knockback = weaponsPlayer.KnockbackCalc(6, player);

            // Create swing projectile
            int projID = Projectile.NewProjectile(new EntitySource_Parent(player), player.Center, new Vector2(0, 0), projectileType, damage, knockback, player.whoAmI);
            if (Main.projectile.IndexInRange(projID) && Main.projectile[projID].ModProjectile is BaseSwordProj proj) // Grab the active projectile instance
            {
                // Allow the swing speed to be modified by attack rate
                proj.swingDuration = weaponsPlayer.attackRate;
                Main.projectile[projID].timeLeft = (int)weaponsPlayer.attackRate;
            }

            float projSpeed = 10;

            // Get the direction and velocity towards the mouse cursor, adjusting for the offset
            Vector2 offset = new(0, -42); // Offset to adjust the projectile's spawn position relative to the mech's center
            Vector2 direction = (Main.MouseWorld - player.Center) - offset;
            direction.Normalize();
            Vector2 velocity = direction * projSpeed;

            // Create beam projectile
            Projectile.NewProjectile(new EntitySource_Parent(player), player.Center + offset, velocity, ProjectileID.SwordBeam, damage, knockback, player.whoAmI);

            SoundEngine.PlaySound(SoundID.Item1, player.position); // Play Swing sound when the weapon is used
            SoundEngine.PlaySound(SoundID.Item8, player.position); // Play Projectile sound when the weapon is used
        }
    }

    public class ProjSwordProj : ModProjectile
    {
        Player player;
        MechVisualPlayer visualPlayer;

        public float swingDuration = 0f; // Duration of sword swing

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
            // Grab player and visual player instances
            player = Main.player[Projectile.owner];
            visualPlayer = player.GetModPlayer<MechVisualPlayer>();

            visualPlayer.animationProgress = Projectile.timeLeft; // Set the animation progress to the time left of the projectile
            float progress = 1f - (Projectile.timeLeft / swingDuration); // Progress goes from 1 to 0 as the projectile time decreases

            // Change the position of the hitbox as it goes through the swing
            Vector2 position;
            if (progress <= 0.33)
                position = new(-30 * visualPlayer.useDirection, -130);
            else if (progress <= 0.66)
                position = new(70 * visualPlayer.useDirection, -100);
            else
                position = new(70 * visualPlayer.useDirection, 0);

            Projectile.Center = player.Center + position; // Set the position of the projectile relative to the player

            // If the player is no longer mounted, kill the projectile
            if (!player.mount.Active)
                Projectile.Kill();
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.MagicPixel.Value; // Texture is not needed, visuals are handled in ModularMech code

            // Override the draw code to prevent the default white pixel from being drawn
            Main.EntitySpriteDraw(
                texture,
                Projectile.Center,
                null,
                lightColor,
                Projectile.rotation,
                new Vector2(0, 0),
                0,
                SpriteEffects.None,
                0
            );
            return false;
        }

        public override void OnKill(int timeLeft)
        {
            visualPlayer.animationProgress = 0; // Reset the animation progress
        }
    }
}
