using MechMod.Common.Global;
using MechMod.Common.Players;
using MechMod.Content.Mounts;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace MechMod.Content.Items.MechWeapons
{
    /// <summary>
    /// Weapon that uses arrows as ammo and fires them towards the cursor.
    /// <para>If the player hits the same enemy four times in succession, an explosion occurs on the enemy.</para>
    /// </summary>

    public class Arbalest : ModItem, IMechWeapon
    {
        public override void SetDefaults()
        {
            Item.value = Item.buyPrice(gold: 2);
            Item.rare = ItemRarityID.Orange;

            Item.useAmmo = AmmoID.Arrow; // Make the weapon use Arrow ammo
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
            int damage = weaponsPlayer.DamageCalc(40, player);
            weaponsPlayer.CritChanceCalc(7, player);
            weaponsPlayer.useRate = weaponsPlayer.AttackSpeedCalc(24, player);
            float knockback = weaponsPlayer.KnockbackCalc(4, player);
            float projSpeed = 30;

            // Get the direction and velocity towards the mouse cursor, adjusting for the offset
            Vector2 offset = new(0, -38); // Offset to adjust the projectile's spawn position relative to the mech's center
            Vector2 direction = (Main.MouseWorld - player.Center) - offset;
            direction.Normalize();
            Vector2 velocity = direction * projSpeed;

            // Adjust the spawn position to be at the end of the muzzle
            Vector2 muzzleOffset = Vector2.Normalize(velocity) * 50f;
            if (Collision.CanHit(player.Center + offset, 0, 0, player.Center + offset + muzzleOffset, 0, 0))
            {
                offset += muzzleOffset;
            }

            // Create projectile
            int projID = Projectile.NewProjectile(new EntitySource_Parent(player), player.Center + offset, velocity, projectileType, damage, knockback, player.whoAmI);
            ArbalestProj firedProj = Main.projectile[projID].GetGlobalProjectile<ArbalestProj>(); // Grab the active projectile instance from global projectiles
            firedProj.isArbalestProj = true; // Denote that the projectile was fired from the Arbalest

            int holdTime = 50; // Amount of time player holds out the weapon after ceasing to use
            visualPlayer.animationTimer = holdTime; // Set the animation timer to hold the weapon out
            SoundEngine.PlaySound(SoundID.Item11, player.position); // Play Gun sound when the weapon is used
        }

        public void UpdateAbility(Player player, MechWeaponsPlayer weaponsPlayer, MechVisualPlayer visualPlayer) { }
    }

    public class ArbalestProj : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public bool isArbalestProj = false;

        private int duration = 120; // 2 seconds

        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (isArbalestProj) // If the projectile is the correct projectile (fired from the Arbalest),
                target.AddBuff(ModContent.BuffType<ArbalestDebuff>(), duration); // Apply the debuff to the target for the specified duration
        }
    }

    public class ArbalestDebuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
        }

        public override bool ReApply(NPC npc, int time, int buffIndex)
        {
            npc.GetGlobalNPC<GlobalDebuffEffect>().arbalestFrame++; // Increment the frame counter each time the debuff is reapplied
            return true;
        }
    }
}
