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
using MechMod.Common.Players;

namespace MechMod.Content.Items.MechWeapons
{
    public class BaseGun : ModItem, IMechWeapon
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
        public Weapons.UseType useType => Weapons.UseType.Point;

        public void UseAbility(Player player, Vector2 mousePosition, bool toggleOn)
        {
            int projectileType = ProjectileID.Bullet;

            Weapons.damageClass = DamageClass.Ranged;

            int damage = Weapons.DamageCalc(12, player);
            Weapons.CritChanceCalc(4, player);
            attackRate = Weapons.AttackSpeedCalc(20, player);
            float knockback = Weapons.KnockbackCalc(4, player);
            float projSpeed = 10;

            int owner = player.whoAmI;
            Vector2 direction = (Main.MouseWorld - Main.LocalPlayer.MountedCenter) - new Vector2(0, -42); // new Vector2 corrects the offset to still make it go towards the cursor
            direction.Normalize(); // Normalize the direction vector to ensure it has a length of 1
            Vector2 velocity = direction * projSpeed;

            if (player.whoAmI == Main.myPlayer && Main.mouseLeft && timer >= attackRate)
            {
                int projID = Projectile.NewProjectile(new EntitySource_Parent(player), Main.LocalPlayer.MountedCenter + new Vector2(0, -42), velocity, projectileType, damage, knockback, owner);
                player.GetModPlayer<MechModPlayer>().animationTime = 20;
                timer = 0;
            }

            if (timer < attackRate)
                timer++;
        }
    }
}
