using MechMod.Common.Players;
using MechMod.Content.Mounts;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static MechMod.Content.Mounts.ModularMech;

namespace MechMod.Content.Items.MechBoosters
{
    public class FastBooster : ModItem, IMechParts
    {
        public override void SetDefaults()
        {
            Item.value = Item.buyPrice(gold: 10);
            Item.rare = ItemRarityID.Green;
        }

        public void ApplyStats(Player player, MechModPlayer modPlayer, MechWeaponsPlayer weaponsPlayer, ModularMech mech)
        {
            modPlayer.lifeBonus -= 50; // 50 health penalty

            // Flight stats
            mech.MountData.flightTimeMax = (int)(105 * modPlayer.partEffectiveness[MechMod.boosterIndex]); // 1.75 seconds of flight time
            modPlayer.flightHorizontalSpeed = 8f * modPlayer.partEffectiveness[MechMod.boosterIndex]; // 8 horizontal speed
            modPlayer.flightJumpSpeed = 8f * modPlayer.partEffectiveness[MechMod.boosterIndex]; // 8 jump speed

            DashPlayer dashPlayer = player.GetModPlayer<DashPlayer>();

            // Dashing stats
            dashPlayer.ableToDash = true; // Allow dashing
            dashPlayer.dashVelo = 15f * modPlayer.partEffectiveness[MechMod.boosterIndex]; // 15 velocity
            dashPlayer.dashCoolDown = 60; // 1 second of cooldown
            dashPlayer.dashDuration = 30; // 0.5 seconds of dash duration
            dashPlayer.upwardDashes = 3; // 3 upward dashes
        }

        public void BodyOffsets(MechVisualPlayer visualPlayer, string body) { }
    }
}
