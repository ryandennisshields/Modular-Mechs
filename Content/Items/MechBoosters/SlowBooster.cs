using MechMod.Common.Players;
using MechMod.Content.Mounts;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static MechMod.Content.Mounts.ModularMech;

namespace MechMod.Content.Items.MechBoosters
{
    public class SlowBooster : ModItem, IMechParts
    {
        public override void SetDefaults()
        {
            Item.value = Item.buyPrice(gold: 10);
            Item.rare = ItemRarityID.Green;
        }

        public void ApplyStats(Player player, MechModPlayer modPlayer, MechWeaponsPlayer weaponsPlayer, ModularMech mech)
        {
            modPlayer.lifeBonus += 50; // 50 health bonus

            DashPlayer dashPlayer = player.GetModPlayer<DashPlayer>();

            // Dashing stats
            dashPlayer.ableToDash = true; // Allow dashing
            dashPlayer.dashVelo = 15f * modPlayer.partEffectiveness[MechMod.boosterIndex]; // 15 velocity
            dashPlayer.dashCoolDown = 90; // 1.5 seconds of cooldown
            dashPlayer.dashDuration = 60; // 1 second of dash duration
            dashPlayer.upwardDashes = 1; // 1 upward dash
        }

        public void BodyOffsets(MechVisualPlayer visualPlayer, string body) { }
    }
}
