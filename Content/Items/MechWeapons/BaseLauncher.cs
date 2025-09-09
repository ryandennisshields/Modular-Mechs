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
    public class BaseLauncher : ModItem, IMechWeapon
    {
        public override void SetDefaults()
        {
            Item.width = 20; // The width of the item's hitbox in pixels.
            Item.height = 20; // The height of the item's hitbox in pixels.
            Item.value = Item.buyPrice(gold: 8);
            Item.rare = ItemRarityID.Orange; // The rarity of the item.

            Item.useAmmo = AmmoID.Rocket; // Make the weapon use ammo
            AmmoID.Sets.SpecificLauncherAmmoProjectileFallback[Item.type] = ItemID.RocketLauncher; // As this is a launcher, they need specific values from a dictionary to dictate what projectile will be used, this code makes it use the same Rockets as the Rocket Launcher 
        }

        public void SetStats(MechWeaponsPlayer weaponsPlayer)
        {
            weaponsPlayer.DamageClass = DamageClass.Ranged; // Set the damage class for ranged weapons
            weaponsPlayer.useType = MechWeaponsPlayer.UseType.Point; // Set the use type for point weapons
        }

        public void UseAbility(Player player, MechWeaponsPlayer weaponsPlayer, Vector2 mousePosition, bool toggleOn)
        {
            player.PickAmmo(Item, out int projectileType, out float _, out int _, out float _, out int usedAmmo); // Set the projectile type to use corresponding ammo and get the ammo item ID
            // Consume ammo, disable weapon use if out of ammo
            if (player.CountItem(usedAmmo) > 0)
            {
                weaponsPlayer.canUse = true;
                player.ConsumeItem(usedAmmo);
            }
            else
            {
                weaponsPlayer.canUse = false;
                return;
            }

            int damage = weaponsPlayer.DamageCalc(102, player);
            weaponsPlayer.CritChanceCalc(10, player);
            weaponsPlayer.attackRate = weaponsPlayer.AttackSpeedCalc(55, player);
            float knockback = weaponsPlayer.KnockbackCalc(6, player);

            float projSpeed = 8;
            int holdTime = 50; // Amount of time player holds out the weapon after ceasing to fire
            Vector2 offset = new(0, -38); // Offset to adjust the projectile's spawn position relative to the mech's center

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
            player.GetModPlayer<MechModPlayer>().animationTimer = holdTime;

            SoundEngine.PlaySound(SoundID.Item61, player.position); // Play Grenade Launcher sound when the weapon is used
        }
    }
}
