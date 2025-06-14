using MechMod.Content.Items.MechWeapons;
using MechMod.Content.Mechs;
using Terraria;
using Terraria.ModLoader;
using static MechMod.Content.Mechs.ModularMech;

namespace MechMod.Content.Items.MechBoosters
{
    internal class BaseBooster : ModItem, IMechParts
    {
        public override void SetDefaults()
        {
            Item.width = 20; // The width of the item's hitbox in pixels.
            Item.height = 20; // The height of the item's hitbox in pixels.
            Item.value = 10000; // The value of the item in copper coins.
            Item.rare = 2; // The rarity of the item.
        }

        public void ApplyStats(ModularMech mech)
        {
            var player = Main.LocalPlayer;

            mech.lifeBonus = 0;

            mech.MountData.flightTimeMax = 60; // 1 second of flight time
            mech.MountData.usesHover = false;

            player.GetModPlayer<DashPlayer>().ableToDash = false;
            player.GetModPlayer<DashPlayer>().dashVelo = 0f;
            player.GetModPlayer<DashPlayer>().dashCoolDown = 0;
            player.GetModPlayer<DashPlayer>().dashDuration = 0;
        }
    }
}
