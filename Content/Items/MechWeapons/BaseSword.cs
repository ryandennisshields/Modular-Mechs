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
            int projectileType = ModContent.ProjectileType<BaseSwordProj>();

            Weapons.damageClass = DamageClass.Melee;

            int damage = Weapons.DamageCalc(12, player);
            Weapons.CritChanceCalc(4, player);
            attackRate = Weapons.AttackSpeedCalc(50, player);
            float knockback = Weapons.KnockbackCalc(30, player);

            int owner = player.whoAmI;

            if (player.whoAmI == Main.myPlayer)
            {
                int projID = Projectile.NewProjectile(new EntitySource_Parent(player), Main.LocalPlayer.MountedCenter, new Vector2(0,0), projectileType, damage, knockback, owner);
                if (Main.projectile.IndexInRange(projID) && Main.projectile[projID].ModProjectile is BaseSwordProj proj)
                {
                    // Allow the swing speed to be modified by attack rate
                    proj.swingDuration = attackRate; // Fixed number
                    Main.projectile[projID].timeLeft = (int)attackRate; // Decreases each tick
                    
                }
            }
        }
    }

    public class BaseSwordProj : ModProjectile
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
        }

        public override void AI()
        {
            player = Main.player[Projectile.owner];
            modPlayer = player.GetModPlayer<MechModPlayer>();

            modPlayer.animationProgress = Projectile.timeLeft; // Set the animation progress to the time left of the projectile
            float progress = 1f - (Projectile.timeLeft / swingDuration); // Progress goes from 1 to 0 as the projectile time decreases

            // Change the position of the hitbox as it goes through the swing
            Vector2 position = new Vector2(0,0);

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

        private HashSet<int> hitNPCs = new HashSet<int>(); // Track the NPCs that have been hit

        public override bool? CanHitNPC(NPC target)
        {
            return !hitNPCs.Contains(target.whoAmI); // Allow hitting the NPC if it hasn't been hit yet
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            hitNPCs.Add(target.whoAmI); // Mark the NPC as hit (prevents one NPC from being hit multiple times in one swing)
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            modifiers.HitDirectionOverride = modPlayer.lastUseDirection; // Set the hit direction based on the player's last use direction
        }
        public override void OnKill(int timeLeft)
        {
            modPlayer.animationProgress = 0; // Reset the animation progress
        }
    }
}
