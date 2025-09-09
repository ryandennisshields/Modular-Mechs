using MechMod.Common.Players;
using MechMod.Content.Mounts;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MechMod.Content.Items.MechBodies
{
    internal class BaseBody : ModItem, IMechParts
    {
        public override void SetDefaults()
        {
            Item.width = 20; // The width of the item's hitbox in pixels.
            Item.height = 20; // The height of the item's hitbox in pixels.
            Item.value = Item.buyPrice(gold: 1);
            Item.rare = ItemRarityID.Green; // The rarity of the item.
        }

        public void ApplyStats(Player player, MechModPlayer modPlayer, MechWeaponsPlayer weaponsPlayer, ModularMech mech)
        {
            modPlayer.lifeBonus += 50; // 50 health bonus

            modPlayer.partEffectiveness[MechMod.headIndex] = 1.25f; // 1.25x effectiveness for head
            modPlayer.partEffectiveness[MechMod.armsIndex] = 1.25f; // 1.25x effectiveness for arms
            modPlayer.partEffectiveness[MechMod.legsIndex] = 1.25f; // 1.25x effectiveness for legs
        }

        public void BodyOffsets(Player player, string body) { }
    }
}
