using MechMod.Common.Players;
using MechMod.Content.Mounts;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MechMod.Content.Items.MechHeads
{
    public class SlowHead : ModItem, IMechParts
    {
        public override void SetDefaults()
        {
            Item.value = Item.buyPrice(gold: 4);
            Item.rare = ItemRarityID.Green;
        }

        public void ApplyStats(Player player, MechModPlayer modPlayer, MechWeaponsPlayer weaponsPlayer, ModularMech mech)
        {
            modPlayer.armourBonus = (player.statDefense / 2) * modPlayer.partEffectiveness[MechMod.headIndex]; // 1.5x armour
        }

        public void BodyOffsets(MechVisualPlayer visualPlayer, string body)
        {
            switch (body)
            {
                case "BaseBody":
                    visualPlayer.bodyOffsets[3] = new Vector2(0, 8);
                    break;
                case "FastBody":
                    visualPlayer.bodyOffsets[3] = new Vector2(-1, 8);
                    break;
                default:
                    visualPlayer.bodyOffsets[3] = new Vector2(0, 0);
                    break;
            }
        }
    }
}
