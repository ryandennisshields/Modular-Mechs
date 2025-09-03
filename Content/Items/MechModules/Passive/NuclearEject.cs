using Microsoft.Xna.Framework;
using MechMod.Content.Mounts;
using Terraria;
using Terraria.ModLoader;
using static MechMod.Content.Mounts.IMechModule;
using Terraria.DataStructures;
using Terraria.ID;
using MechMod.Content.Items.MechWeapons;

namespace MechMod.Content.Items.MechModules.Passive
{
    public class NuclearEject : ModItem, IMechModule
    {
        public override void SetDefaults()
        {
            Item.width = 20; // The width of the item's hitbox in pixels.
            Item.height = 20; // The height of the item's hitbox in pixels.
            Item.value = Item.buyPrice(gold: 10);
            Item.rare = 3; // The rarity of the item.
        }

        public ModuleSlot moduleSlot => ModuleSlot.Passive; // Passive slot
        public ModuleType moduleType => ModuleType.OnDismount; // Dismount effect

        private DamageClass explosionClass = DamageClass.Default;
        private int explosionDamage = 100;
        private int explosionKnockback = 30;
        private int explosionType = ModContent.ProjectileType<NuclearEjectProjectile>();

        public void InitialEffect(ModularMech mech, Player player) { }

        public void ModuleEffect(ModularMech mech, Player player)
        {
            if (Main.myPlayer == player.whoAmI)
            {
                Projectile.NewProjectile(
                    new EntitySource_Parent(player),
                    player.MountedCenter,
                    Vector2.Zero,
                    explosionType,
                    Weapons.DamageCalc(explosionDamage, player, explosionClass),
                    Weapons.KnockbackCalc(explosionKnockback, player, explosionClass),
                    player.whoAmI
                    );
            }
            mech.mechDebuffDuration = (int)(mech.mechDebuffDuration * 1.5f); // Increase debuff duration by 50%
            mech.launchForce *= 2; // Double the launch force
        }
    }

    public class NuclearEjectProjectile : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_954";

        public override void SetDefaults()
        {
            Projectile.width = 800;
            Projectile.height = 100;
            Projectile.friendly = true; // Can damage enemies
            Projectile.penetrate = -1; // Infinite penetration
            Projectile.timeLeft = 30; // Duration of the explosion
            Projectile.tileCollide = false; // Ignore tile collisions
            Projectile.ignoreWater = true; // Ignore water
        }

        //public override void OnSpawn(IEntitySource source)
        //{
        //    for (int i = 0; i < 2500; i++)
        //    {
        //        // Explosion dust
        //        Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Torch, 0f, 0f, 100, default, 1f);
        //        Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke, 0f, 0f, 100, default, 0.5f);
        //    }
        //}

        public override void AI()
        {
            for (int i = 0; i < 1000; i++)
            {
                // Explosion dust
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Torch, 0f, 0f, 100, default, 1f);
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke, 0f, 0f, 100, default, 0.5f);
            }
        }
    }
}
