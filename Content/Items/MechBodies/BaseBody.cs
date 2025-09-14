using MechMod.Common.Players;
using MechMod.Content.Mounts;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MechMod.Content.Items.MechBodies
{
    public class BaseBody : ModItem, IMechParts
    {
        public override void SetDefaults()
        {
            Item.value = Item.buyPrice(gold: 1);
            Item.rare = ItemRarityID.Green;
        }

        public void ApplyStats(Player player, MechModPlayer modPlayer, MechWeaponsPlayer weaponsPlayer, ModularMech mech)
        {
            modPlayer.lifeBonus += 50; // 50 health bonus

            modPlayer.partEffectiveness[MechMod.headIndex] = 1.25f; // 1.25x effectiveness for head
            modPlayer.partEffectiveness[MechMod.armsIndex] = 1.25f; // 1.25x effectiveness for arms
            modPlayer.partEffectiveness[MechMod.legsIndex] = 1.25f; // 1.25x effectiveness for legs
        }

        public void BodyOffsets(MechVisualPlayer visualPlayer, string body) { }
    }
}
