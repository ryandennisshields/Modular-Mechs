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
        public override string Texture => "MechMod/Content/Items/MechBoosters/BaseBooster";
        public override void SetDefaults()
        {
            Item.width = 20; // The width of the item's hitbox in pixels.
            Item.height = 20; // The height of the item's hitbox in pixels.
            Item.value = Item.buyPrice(gold: 20);
            Item.rare = 2; // The rarity of the item.
        }

        public void ApplyStats(Player player, ModularMech mech)
        {
            mech.lifeBonus += 50; // 50 health bonus

            mech.MountData.flightTimeMax = 0;

            // Allow dashing
            player.GetModPlayer<DashPlayer>().ableToDash = true;
            player.GetModPlayer<DashPlayer>().dashVelo = 15f;
            player.GetModPlayer<DashPlayer>().dashCoolDown = 90; // 1.5 seconds of cooldown
            player.GetModPlayer<DashPlayer>().dashDuration = 60; // 1 second of dash duration
        }
    }
}
