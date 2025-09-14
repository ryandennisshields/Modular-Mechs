using MechMod.Common.Players;
using MechMod.Content.Mounts;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MechMod.Content.Items.MechArms
{
    public class FastArms : ModItem, IMechParts
    {
        public override void SetDefaults()
        {
            Item.value = Item.buyPrice(gold: 2);
            Item.rare = ItemRarityID.Green;
        }

        public void ApplyStats(Player player, MechModPlayer modPlayer, MechWeaponsPlayer weaponsPlayer, ModularMech mech)
        {
            weaponsPlayer.partDamageBonus += 0.2f * modPlayer.partEffectiveness[MechMod.armsIndex]; // 20% damage bonus
            weaponsPlayer.partCritChanceBonus += 0.1f * modPlayer.partEffectiveness[MechMod.armsIndex]; // 10% more critical chance
            modPlayer.lifeBonus -= (int)(25 / modPlayer.partEffectiveness[MechMod.armsIndex]); // 25 health penalty
        }

        public void BodyOffsets(MechVisualPlayer visualPlayer, string body)
        {
            switch (body)
            {
                case "BaseBody":
                    visualPlayer.bodyOffsets[0] = new Vector2(1, 0);
                    visualPlayer.bodyOffsets[4] = new Vector2(1, 0);
                    break;
                case "SlowBody":
                    visualPlayer.bodyOffsets[0] = new Vector2(3, -4);
                    visualPlayer.bodyOffsets[4] = new Vector2(3, -4);
                    break;
                default:
                    visualPlayer.bodyOffsets[0] = new Vector2(0, 0); // Right
                    visualPlayer.bodyOffsets[4] = new Vector2(0, 0); // Left
                    break;
            }
        }
    }
}
