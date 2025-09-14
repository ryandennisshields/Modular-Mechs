using MechMod.Common.Players;
using MechMod.Content.Mounts;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MechMod.Content.Items.MechBodies
{
    public class FastBody : ModItem, IMechParts
    {
        public override void SetDefaults()
        {
            Item.value = Item.buyPrice(gold: 2);
            Item.rare = ItemRarityID.Green;
        }

        public void ApplyStats(Player player, MechModPlayer modPlayer, MechWeaponsPlayer weaponsPlayer, ModularMech mech)
        {
            modPlayer.lifeBonus += 25; // 25 health bonus

            modPlayer.partEffectiveness[MechMod.legsIndex] = 1.5f; // 1.5x effectiveness for legs
            modPlayer.partEffectiveness[MechMod.armsIndex] = 0.75f; // 0.75x effectiveness for arms
        }

        public void BodyOffsets(MechVisualPlayer visualPlayer, string body) { }
    }
}
