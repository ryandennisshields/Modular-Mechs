using MechMod.Common.Players;
using MechMod.Content.Items.MechWeapons;
using MechMod.Content.Mounts;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static MechMod.Content.Mounts.ModularMech;

namespace MechMod.Content.Items.MechBoosters
{
    internal class BaseBooster : ModItem, IMechParts
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
            //mech.lifeBonus = 0;

            // Flight stats
            mech.MountData.flightTimeMax = (int)(150 * modPlayer.partEffectiveness[MechMod.boosterIndex]); // 2.5 seconds of flight time
            modPlayer.flightHorizontalSpeed = 6f * modPlayer.partEffectiveness[MechMod.boosterIndex];
            modPlayer.flightJumpSpeed = 6f * modPlayer.partEffectiveness[MechMod.boosterIndex];
        }

        public void BodyOffsets(Player player, string body) { }
    }
}
