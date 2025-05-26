using MechMod.Content.Items.MechWeapons;
using MechMod.Content.Mechs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Biomes.CaveHouse;
using Terraria.ID;
using Terraria.ModLoader;

namespace MechMod.Content.Projectiles
{
    public class MechPoint : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.friendly = false;
            Projectile.timeLeft = 5;
            Projectile.aiStyle = 1;

            AIType = ProjectileID.Bullet;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            // rotation that points towards the mouse
            //Vector2 mousePosition = Main.MouseWorld;
            //Vector2 direction = mousePosition - Projectile.Center;
            //direction.Normalize();
            //Projectile.rotation = (float)Math.Atan2(direction.Y, direction.X) + MathHelper.PiOver2;
            Main.EntitySpriteDraw(
                texture,
                Projectile.Center,
                null,
                lightColor, 
                Projectile.rotation,
                new Vector2(texture.Width / 2f, texture.Height / 2f),
                Projectile.scale,
                SpriteEffects.None,
                0
            );
            return true;
        }
    }
}
