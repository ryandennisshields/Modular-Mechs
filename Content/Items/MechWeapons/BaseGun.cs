using Terraria.ModLoader;
using Terraria;
using MechMod.Content.Mounts;
using Terraria.ID;
using Terraria.DataStructures;
using Microsoft.Xna.Framework;
using MechMod.Common.Players;
using Terraria.Audio;

namespace MechMod.Content.Items.MechWeapons
{
    public class BaseGun : ModItem, IMechWeapon
    {
        public override void SetDefaults()
        {
            Item.width = 20; // The width of the item's hitbox in pixels.
            Item.height = 20; // The height of the item's hitbox in pixels.
            Item.value = Item.buyPrice(gold: 2);
            Item.rare = 3; // The rarity of the item.
        }

        public void SetStats(Player player)
        {
            Weapons.DamageClass = DamageClass.Ranged; // Set the damage class for ranged weapons
            Weapons.useType = Weapons.UseType.Point; // Set the use type for point weapons
        }

        public void UseAbility(Player player, Vector2 mousePosition, bool toggleOn)
        {
            int projectileType = ProjectileID.Bullet;

            int damage = Weapons.DamageCalc(9, player);
            Weapons.CritChanceCalc(7, player);
            Weapons.attackRate = Weapons.AttackSpeedCalc(13, player);
            float knockback = Weapons.KnockbackCalc(4, player);

            float projSpeed = 10;
            int holdTime = 50; // Amount of time player holds out the weapon after ceasing to fire
            Vector2 offset = new Vector2(0, -46); // Offset to adjust the projectile's spawn position relative to the mech's center

            Vector2 direction = (Main.MouseWorld - player.Center) - offset; // new Vector2 corrects the offset to still make it go towards the cursor
            direction.Normalize(); // Normalize the direction vector to ensure it has a length of 1
            Vector2 velocity = direction * projSpeed;

            int projID = Projectile.NewProjectile(new EntitySource_Parent(player), player.Center + offset, velocity, projectileType, damage, knockback, player.whoAmI);
            player.GetModPlayer<MechModPlayer>().animationTimer = holdTime;

            SoundEngine.PlaySound(SoundID.Item11, player.position); // Play Gun sound when the weapon is used
        }
    }
}
