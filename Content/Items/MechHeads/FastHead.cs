using MechMod.Common.Players;
using MechMod.Content.Mounts;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MechMod.Content.Items.MechHeads
{
    public class FastHead : ModItem, IMechParts
    {
        public override void SetDefaults()
        {
            Item.value = Item.buyPrice(gold: 2);
            Item.rare = ItemRarityID.Green;
        }

        public void ApplyStats(Player player, MechModPlayer modPlayer, MechWeaponsPlayer weaponsPlayer, ModularMech mech)
        {
            weaponsPlayer.partCritChanceBonus *= 2f * modPlayer.partEffectiveness[MechMod.headIndex]; // 2x critical chance
        }

        public void BodyOffsets(MechVisualPlayer visualPlayer, string body)
        {
            switch (body)
            {
                case "BaseBody":
                    visualPlayer.bodyOffsets[3] = new Vector2(-1, 0);
                    break;
                case "SlowBody":
                    visualPlayer.bodyOffsets[3] = new Vector2(1, -6);
                    break;
                default:
                    visualPlayer.bodyOffsets[3] = new Vector2(0, 0);
                    break;
            }
        }
    }
}
