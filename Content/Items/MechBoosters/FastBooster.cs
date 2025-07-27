using MechMod.Content.Items.MechWeapons;
using MechMod.Content.Mounts;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static MechMod.Content.Mounts.ModularMech;

namespace MechMod.Content.Items.MechBoosters
{
    internal class FastBooster : ModItem, IMechParts
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
            mech.lifeBonus -= 50; // 50 health penalty

            // Flight stats
            mech.MountData.flightTimeMax = 105; // 1.75 seconds of flight time
            mech.flightHorizontalSpeed = 8f;
            mech.flightJumpSpeed = 8f;

            // Allow dashing
            player.GetModPlayer<DashPlayer>().ableToDash = true;
            player.GetModPlayer<DashPlayer>().dashVelo = 15f;
            player.GetModPlayer<DashPlayer>().dashCoolDown = 60; // 1 second of cooldown
            player.GetModPlayer<DashPlayer>().dashDuration = 30; // 0.5 seconds of dash duration
        }
    }
}
