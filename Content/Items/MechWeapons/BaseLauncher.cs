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
    /// <summary>
    /// Weapon that uses rockets as ammo and fires them towards the cursor.
    /// </summary>

    public class BaseLauncher : ModItem, IMechWeapon
    {
        public override void SetDefaults()
        {
            Item.value = Item.buyPrice(gold: 8);
            Item.rare = ItemRarityID.Orange;

            Item.useAmmo = AmmoID.Rocket; // Make the weapon use ammo
            AmmoID.Sets.SpecificLauncherAmmoProjectileFallback[Item.type] = ItemID.RocketLauncher; // As this is a launcher, they need specific values from a dictionary to dictate what projectile will be used, this code makes it use the same Rockets as the Rocket Launcher 
        }

        public void SetStats(MechWeaponsPlayer weaponsPlayer)
        {
            weaponsPlayer.DamageClass = DamageClass.Ranged; // Set DamageClass to Ranged
            weaponsPlayer.useType = MechWeaponsPlayer.UseType.Point; // Set use type to Point
        }

        public void UseAbility(Player player, MechWeaponsPlayer weaponsPlayer, MechVisualPlayer visualPlayer, Vector2 mousePosition, bool toggleOn)
        {
            player.PickAmmo(Item, out int projectileType, out float _, out int _, out float _, out int usedAmmo); // Set the projectile type to use corresponding ammo and get the ammo item ID
            Item ammoItem = new();
            ammoItem.SetDefaults(usedAmmo); // Create an instance of the ammo item to be able to be consumed
            // Consume ammo, disable weapon use if out of ammo
            if (player.CountItem(usedAmmo) > 0)
            {
                weaponsPlayer.canUse = true;
                if (ammoItem.maxStack > 1) // Only consume if the item isn't an "endless" ammo type
                    player.ConsumeItem(usedAmmo);
            }
            else
            {
                weaponsPlayer.canUse = false;
                return;
            }

            // Calculate projectile properties
            int damage = weaponsPlayer.DamageCalc(102, player);
            weaponsPlayer.CritChanceCalc(10, player);
            weaponsPlayer.attackRate = weaponsPlayer.AttackSpeedCalc(55, player);
            float knockback = weaponsPlayer.KnockbackCalc(6, player);
            float projSpeed = 8;

            // Get the direction and velocity towards the mouse cursor, adjusting for the offset
            Vector2 offset = new(0, -38); // Offset to adjust the projectile's spawn position relative to the mech's center
            Vector2 direction = (Main.MouseWorld - player.Center) - offset;
            direction.Normalize();
            Vector2 velocity = direction * projSpeed;

            // Adjust the spawn position to be at the end of the muzzle
            Vector2 muzzleOffset = Vector2.Normalize(velocity) * 70f;
            if (Collision.CanHit(player.Center + offset, 0, 0, player.Center + offset + muzzleOffset, 0, 0))
            {
                offset += muzzleOffset;
            }

            // Create projectile
            Projectile.NewProjectile(new EntitySource_Parent(player), player.Center + offset, velocity, projectileType, damage, knockback, player.whoAmI);
            
            int holdTime = 50; // Amount of time player holds out the weapon after ceasing to use
            visualPlayer.animationTimer = holdTime; // Set the animation timer to hold the weapon out
            SoundEngine.PlaySound(SoundID.Item61, player.position); // Play Grenade Launcher sound when the weapon is used
        }
    }
}
