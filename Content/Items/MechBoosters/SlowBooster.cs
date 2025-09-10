using MechMod.Common.Players;
using MechMod.Content.Items.MechWeapons;
using MechMod.Content.Mounts;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static MechMod.Content.Mounts.ModularMech;

namespace MechMod.Content.Items.MechBoosters
{
    internal class SlowBooster : ModItem, IMechParts
    {
        public override void SetDefaults()
        {
            Item.width = 20; // The width of the item's hitbox in pixels.
            Item.height = 20; // The height of the item's hitbox in pixels.
            Item.value = Item.buyPrice(gold: 10);
            Item.rare = ItemRarityID.Green; // The rarity of the item.
        }

        public void ApplyStats(Player player, MechModPlayer modPlayer, MechWeaponsPlayer weaponsPlayer, ModularMech mech)
        {
            modPlayer.lifeBonus += 50; // 50 health bonus

            // Allow dashing
            player.GetModPlayer<DashPlayer>().ableToDash = true;
            player.GetModPlayer<DashPlayer>().dashVelo = 15f * modPlayer.partEffectiveness[MechMod.boosterIndex];
            player.GetModPlayer<DashPlayer>().dashCoolDown = 90; // 1.5 seconds of cooldown
            player.GetModPlayer<DashPlayer>().dashDuration = 60; // 1 second of dash duration
        }

        public void BodyOffsets(MechVisualPlayer visualPlayer, string body) { }
    }
}
