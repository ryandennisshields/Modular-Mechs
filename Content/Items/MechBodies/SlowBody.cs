using MechMod.Common.Players;
using MechMod.Content.Mounts;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MechMod.Content.Items.MechBodies
{
    public class SlowBody : ModItem, IMechParts
    {
        public override void SetDefaults()
        {
            Item.value = Item.buyPrice(gold: 4);
            Item.rare = ItemRarityID.Green;
        }

        public void ApplyStats(Player player, MechModPlayer modPlayer, MechWeaponsPlayer weaponsPlayer, ModularMech mech)
        {
            modPlayer.lifeBonus += 100; // 100 health bonus

            modPlayer.partEffectiveness[MechMod.headIndex] = 2f; // 2x effectiveness for head
            modPlayer.partEffectiveness[MechMod.legsIndex] = 0.75f; // 0.75x effectiveness for legs
        }

        public void BodyOffsets(MechVisualPlayer visualPlayer, string body) { }
    }
}
