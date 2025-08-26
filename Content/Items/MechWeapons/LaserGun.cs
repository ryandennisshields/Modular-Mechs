using Terraria.ModLoader;
using Terraria;
using MechMod.Content.Mounts;
using Terraria.ID;
using Terraria.DataStructures;
using Microsoft.Xna.Framework;
using MechMod.Common.Players;
using rail;
using System.Net.Mail;
using Terraria.Audio;

namespace MechMod.Content.Items.MechWeapons
{
    public class LaserGun : ModItem, IMechWeapon
    {
        public override void SetDefaults()
        {
            Item.width = 20; // The width of the item's hitbox in pixels.
            Item.height = 20; // The height of the item's hitbox in pixels.
            Item.value = Item.buyPrice(gold: 4);
            Item.rare = 3; // The rarity of the item.
        }

        public void SetStats(Player player)
        {
            Weapons.DamageClass = DamageClass.Magic; // Set the damage class for ranged weapons
            Weapons.useType = Weapons.UseType.Point; // Set the use type for point weapons
        }

        public void UseAbility(Player player, Vector2 mousePosition, bool toggleOn)
        {
            int manaCost = 6; // Mana cost for use
            if (player.statMana > manaCost)
            {
                Weapons.canUse = true;

                int projectileType = ProjectileID.LaserMachinegunLaser;

                int damage = Weapons.DamageCalc(24, player);
                Weapons.CritChanceCalc(4, player);
                Weapons.attackRate = Weapons.AttackSpeedCalc(22, player);
                float knockback = Weapons.KnockbackCalc(4, player);

                float projSpeed = 12;
                int holdTime = 50; // Amount of time player holds out the weapon after ceasing to fire
                Vector2 offset = new Vector2(0, -38); // Offset to adjust the projectile's spawn position relative to the mech's center

                Vector2 direction = (Main.MouseWorld - player.Center) - offset; // new Vector2 corrects the offset to still make it go towards the cursor
                direction.Normalize(); // Normalize the direction vector to ensure it has a length of 1
                Vector2 velocity = direction * projSpeed;
                // Adjust the spawn position to be at the end of the muzzle
                Vector2 muzzleOffset = Vector2.Normalize(velocity) * 70f;
                if (Collision.CanHit(player.Center + offset, 0, 0, player.Center + offset + muzzleOffset, 0, 0))
                {
                    offset += muzzleOffset;
                }

                int projID = Projectile.NewProjectile(new EntitySource_Parent(player), player.Center + offset, velocity, projectileType, damage, knockback, player.whoAmI);
                player.CheckMana(manaCost, true);
                player.manaRegenDelay = 120;
                player.GetModPlayer<MechModPlayer>().animationTimer = holdTime;

                SoundEngine.PlaySound(SoundID.Item12, player.position); // Play Laser sound when the weapon is used
            }
            else
                Weapons.canUse = false;
        }
    }
}
