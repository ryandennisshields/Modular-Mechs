using Microsoft.Xna.Framework;
using MechMod.Content.Mounts;
using Terraria;
using Terraria.ModLoader;
using static MechMod.Content.Mounts.IMechModule;
using Terraria.DataStructures;
using Terraria.ID;
using MechMod.Common.Players;
using Terraria.Audio;

namespace MechMod.Content.Items.MechModules.Passive
{
    /// <summary>
    /// Passive Module that causes a nuclear explosion when the player dismounts, launching them upwards with increased force and increasing the duration of the Mech Debuff.
    /// </summary>

    public class NuclearEject : ModItem, IMechModule
    {
        public override void SetDefaults()
        {
            Item.value = Item.buyPrice(gold: 4);
            Item.rare = ItemRarityID.Orange;
        }

        public ModuleSlot MSlot => ModuleSlot.Passive; // Passive slot
        public ModuleType MType => ModuleType.OnDismount; // Dismount effect

        // Explosion projectile properties
        private DamageClass explosionClass = DamageClass.Default;
        private int explosionDamage = 100;
        private int explosionKnockback = 30;
        private int explosionType = ModContent.ProjectileType<NuclearEjectProjectile>();

        public void ModuleEffect(ModularMech mech, Player player, MechModPlayer modPlayer, MechWeaponsPlayer weaponsPlayer)
        {
            if (Main.myPlayer == player.whoAmI)
            {
                // Create explosion projectile
                Projectile.NewProjectile(
                    new EntitySource_Parent(player),
                    player.MountedCenter,
                    Vector2.Zero,
                    explosionType,
                    weaponsPlayer.DamageCalc(explosionDamage, player, explosionClass),
                    weaponsPlayer.KnockbackCalc(explosionKnockback, player, explosionClass),
                    player.whoAmI
                    );
            }
            modPlayer.mechDebuffDuration = (int)(modPlayer.mechDebuffDuration * 1.5f); // Increase debuff duration by 50%
            modPlayer.launchForce *= 2; // Double the launch force
        }
    }

    public class NuclearEjectProjectile : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_954"; // Use a small spark texture

        public override void SetDefaults()
        {
            Projectile.width = 800; // Hitbox width
            Projectile.height = 100; // Hitbox height
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 30; // 0.5 second duration
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }

        public override void OnSpawn(IEntitySource source)
        {
            // Play explosion sounds
            SoundEngine.PlaySound(SoundID.Item14, Projectile.position);
            SoundEngine.PlaySound(SoundID.Item62, Projectile.position);
        }

        public override void AI()
        {
            for (int i = 0; i < 250; i++)
            {
                // Explosion dust
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Torch, 0f, 0f, 100, default, 1.5f);
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke, 0f, 0f, 100, default, 1f);
            }
        }
    }
}
