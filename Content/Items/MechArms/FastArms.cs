using MechMod.Common.Players;
using MechMod.Content.Items.MechWeapons;
using MechMod.Content.Mounts;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MechMod.Content.Items.MechArms
{
    internal class FastArms : ModItem, IMechParts
    {
        public override void SetDefaults()
        {
            Item.width = 20; // The width of the item's hitbox in pixels.
            Item.height = 20; // The height of the item's hitbox in pixels.
            Item.value = Item.buyPrice(gold: 2);
            Item.rare = ItemRarityID.Green; // The rarity of the item.
        }

        public void ApplyStats(Player player, MechModPlayer modPlayer, MechWeaponsPlayer weaponsPlayer, ModularMech mech)
        {
            weaponsPlayer.partDamageBonus += 0.2f * modPlayer.partEffectiveness[MechMod.armsIndex]; // 20% damage bonus
            weaponsPlayer.partCritChanceBonus += 0.1f * modPlayer.partEffectiveness[MechMod.armsIndex]; // 10% more critical chance
            modPlayer.lifeBonus -= (int)(25 / modPlayer.partEffectiveness[MechMod.armsIndex]); // 25 health penalty
        }

        public void BodyOffsets(Player player, string body)
        {
            MechModPlayer modPlayer = player.GetModPlayer<MechModPlayer>();

            switch (body)
            {
                case "BaseBody":
                    modPlayer.bodyOffsets[0] = new Vector2(1, 0);
                    modPlayer.bodyOffsets[4] = new Vector2(1, 0);
                    break;
                case "SlowBody":
                    modPlayer.bodyOffsets[0] = new Vector2(3, -4);
                    modPlayer.bodyOffsets[4] = new Vector2(3, -4);
                    break;
                default:
                    modPlayer.bodyOffsets[0] = new Vector2(0, 0); // Right
                    modPlayer.bodyOffsets[4] = new Vector2(0, 0); // Left
                    break;
            }
        }
    }
}
