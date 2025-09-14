using MechMod.Common.Players;
using MechMod.Content.Mounts;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MechMod.Content.Items.MechArms
{
    public class BaseArms : ModItem, IMechParts
    {
        public override void SetDefaults()
        {
            Item.value = Item.buyPrice(gold: 1);
            Item.rare = ItemRarityID.Green;
        }

        public void ApplyStats(Player player, MechModPlayer modPlayer, MechWeaponsPlayer weaponsPlayer, ModularMech mech)
        {
            weaponsPlayer.partDamageBonus += 0.2f * modPlayer.partEffectiveness[MechMod.armsIndex]; // 20% damage bonus
        }

        public void BodyOffsets(MechVisualPlayer visualPlayer, string body)
        {
            switch (body)
            {
                case "FastBody":
                    visualPlayer.bodyOffsets[0] = new Vector2(-5, 0);
                    visualPlayer.bodyOffsets[4] = new Vector2(5, 0);
                    break;
                case "SlowBody":
                    visualPlayer.bodyOffsets[0] = new Vector2(0, -6);
                    visualPlayer.bodyOffsets[4] = new Vector2(0, -6);
                    break;
                default:
                    visualPlayer.bodyOffsets[0] = new Vector2(0, 0); // Right
                    visualPlayer.bodyOffsets[4] = new Vector2(0, 0); // Left
                    break;
            }
        }
    }
}
