using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria;
using MechMod.Content.Mechs;
using Terraria.ID;
using MechMod.Content.Items.MechWeapons;
using Terraria.DataStructures;
using Microsoft.Xna.Framework;

namespace MechMod.Content.Items.MechWeapons
{
    public class BaseSword : ModItem, IMechWeapon
    {
        public override void SetDefaults()
        {
            Item.width = 20; // The width of the item's hitbox in pixels.
            Item.height = 20; // The height of the item's hitbox in pixels.
            Item.value = 10000; // The value of the item in copper coins.
            Item.rare = 2; // The rarity of the item.
        }

        private int timer;

        public Weapons.UseType useType => Weapons.UseType.Swing;

        public void UseAbility(Player player, Vector2 mousePosition, bool toggleOn)
        {
            int projectileType = ProjectileID.SwordBeam;

            Weapons.damageClass = DamageClass.Melee;

        int damage = Weapons.DamageCalc(12, player);
            Weapons.CritChanceCalc(4, player);
            int attackSpeed = Weapons.AttackSpeedCalc(20, player);
            float knockback = Weapons.KnockbackCalc(4, player);
            float projSpeed = 10;

            int owner = player.whoAmI;
            mousePosition = Main.MouseWorld;
            Vector2 direction = mousePosition - player.Center;
            direction.Normalize();
            Vector2 velocity = direction * projSpeed;

            if (player.whoAmI == Main.myPlayer && Main.mouseLeft && timer >= attackSpeed)
            {
                int projID = Projectile.NewProjectile(new EntitySource_Parent(player), player.MountedCenter, velocity, projectileType, damage, knockback, owner);
                timer = 0;
            }

            if (timer < attackSpeed)
                timer++;
        }
    }
}
