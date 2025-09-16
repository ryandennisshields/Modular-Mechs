using MechMod.Common.Players;
using MechMod.Content.Mounts;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace MechMod.Content.Items.MechWeapons
{
    /// <summary>
    /// Weapon that swings a sword in an arc in front of the Mech.
    /// </summary>

    public class BaseSword : ModItem, IMechWeapon
    {
        public override void SetDefaults()
        {
            Item.value = Item.buyPrice(gold: 2);
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

            int projectileType = ModContent.ProjectileType<BaseSwordProj>(); // Use a custom projectile for the sword swing

            // Calculate projectile properties
            int damage = weaponsPlayer.DamageCalc(100, player);
            weaponsPlayer.CritChanceCalc(4, player);
            weaponsPlayer.attackRate = weaponsPlayer.AttackSpeedCalc(30, player);
            float knockback = weaponsPlayer.KnockbackCalc(7, player);

            // Create projectile
            int projID = Projectile.NewProjectile(new EntitySource_Parent(player), player.Center, new Vector2(0, 0), projectileType, damage, knockback, player.whoAmI);
            if (Main.projectile.IndexInRange(projID) && Main.projectile[projID].ModProjectile is BaseSwordProj proj) // Grab the active projectile instance
            {
                // Allow the swing speed to be modified by attack rate
                proj.swingDuration = weaponsPlayer.attackRate;
                Main.projectile[projID].timeLeft = (int)weaponsPlayer.attackRate;
            }

            SoundEngine.PlaySound(SoundID.Item1, player.position); // Play Swing sound when the weapon is used
        }
    }

    public class BaseSwordProj : ModProjectile
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
                new Vector2(0,0),
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
