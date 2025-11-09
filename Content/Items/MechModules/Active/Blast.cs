using MechMod.Common.Players;
using MechMod.Content.Debuffs;
using MechMod.Content.Mounts;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static MechMod.Content.Mounts.IMechModule;

namespace MechMod.Content.Items.MechModules.Active
{
    /// <summary>
    /// Active Module that slows the Mech down to charge up for a duration, then releasing a powerful energy blast that damages and knocks back nearby enemies, ignoring armour.
    /// </summary>

    public class Blast : ModItem, IMechModule
    {
        public override void SetDefaults()
        {
            Item.value = Item.buyPrice(gold: 8);
            Item.rare = ItemRarityID.Orange;
        }

        public ModuleSlot MSlot => ModuleSlot.Active; // Active slot
        public ModuleType MType => ModuleType.Persistent; // Persistent effect

        private int cooldown = 1200; // Cooldown in frames (20 seconds)

        private bool activate; // Tracker for when the blast happens

        private int chargeTime = 120; // Charge time in frames (2 seconds)
        private float speedReduction = 0.25f; // 75% speed reduction during charge-up

        // Blast projectile properties
        private DamageClass blastClass = DamageClass.Default;
        private int blastDamage = 75;
        private int blastKnockback = 30;
        private int blastType = ModContent.ProjectileType<BlastProjectile>();

        private int chargeSoundTimer; // Timer to control charge sound playback
        private int chargeSoundRate = 25; // Rate at which charge sound plays

        public void ModuleEffect(ModularMech mech, Player player, MechModPlayer modPlayer, MechWeaponsPlayer weaponsPlayer, MechVisualPlayer visualPlayer)
        {
            if (player.whoAmI == Main.myPlayer && MechMod.MechActivateModule.JustPressed && !player.HasBuff(ModContent.BuffType<Cooldown>())) // If the player presses the "MechActivateModule" binding and the player is not on cooldown,
            {
                player.AddBuff(ModContent.BuffType<Cooldown>(), cooldown); // Add cooldown
                modPlayer.chargeTimer = chargeTime; // Reset charge timer
            }

            if (modPlayer.chargeTimer > 0) // If the charge timer is less than the charge time,
            {
                modPlayer.chargeTimer--; // Increment charge timer

                Dust.NewDust(new Vector2(player.position.X - 20, player.position.Y), player.width * 3, player.height, DustID.Electric, Alpha: 100); // Create charging dust effect

                if (chargeSoundTimer >= chargeSoundRate) // If the sound timer is equal to or greater than the sound rate,
                {
                    SoundEngine.PlaySound(SoundID.Item15, player.position); // Play charging sound while charging
                    chargeSoundTimer = 0; // Reset sound timer
                }

                if (!activate) // If the blast hasn't been activated yet,
                {
                    // Slow down the Mech during charge
                    modPlayer.groundHorizontalSpeed *= speedReduction;
                    modPlayer.groundJumpSpeed *= speedReduction;
                    modPlayer.flightHorizontalSpeed *= speedReduction;
                    modPlayer.flightJumpSpeed *= speedReduction;
                    activate = true;
                }
            }
            else if (modPlayer.chargeTimer <= 0 && activate) // If the charge timer has reached the charge time and the blast ready to be activated,
            {
                // Create blast projectile
                Projectile.NewProjectile(
                    new EntitySource_Parent(player),
                    player.MountedCenter,
                    Vector2.Zero,
                    blastType,
                    weaponsPlayer.DamageCalc(blastDamage, player, blastClass),
                    weaponsPlayer.KnockbackCalc(blastKnockback, player, blastClass),
                    player.whoAmI
                );

                // Reset stat changes
                modPlayer.groundJumpSpeed /= speedReduction;
                modPlayer.groundHorizontalSpeed /= speedReduction;
                modPlayer.flightJumpSpeed /= speedReduction;
                modPlayer.flightHorizontalSpeed /= speedReduction;
                activate = false;
            }

            if (chargeSoundTimer < chargeSoundRate)
                chargeSoundTimer++; // Increment sound timer
        }
    }

    public class BlastProjectile : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_954"; // Use a small spark texture

        public override void SetDefaults()
        {
            Projectile.width = 800;
            Projectile.height = 800;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 30; // 0.5 second duration
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.ArmorPenetration = 999; // Ignore armour
        }

        public override void OnSpawn(IEntitySource source)
        {
            // Play blast sounds
            SoundEngine.PlaySound(SoundID.Item14, Projectile.position);
            SoundEngine.PlaySound(SoundID.Item62, Projectile.position);
            SoundEngine.PlaySound(SoundID.Item94, Projectile.position);
        }

        public override void AI()
        {
            for (int i = 0; i < 500; i++)
            {
                // Blast dust
                int dust1 = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Electric, Alpha: 100, Scale: 1.5f);
                int dust2 = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke, Alpha: 100);
                Main.dust[dust1].noGravity = true;
                Main.dust[dust2].noGravity = true;
            }
        }
    }
}