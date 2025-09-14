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

            // Dashing stats
            player.GetModPlayer<DashPlayer>().ableToDash = true; // Allow dashing
            player.GetModPlayer<DashPlayer>().dashVelo = 15f * modPlayer.partEffectiveness[MechMod.boosterIndex]; // 15 velocity
            player.GetModPlayer<DashPlayer>().dashCoolDown = 90; // 1.5 seconds of cooldown
            player.GetModPlayer<DashPlayer>().dashDuration = 60; // 1 second of dash duration
        }

        public void BodyOffsets(MechVisualPlayer visualPlayer, string body) { }
    }
}
