using MechMod.Common.Players;
using MechMod.Content.Items.MechWeapons;
using MechMod.Content.Mounts;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MechMod.Content.Items.MechHeads
{
    internal class BaseHead : ModItem, IMechParts
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
            modPlayer.lifeBonus += (int)(50 * modPlayer.partEffectiveness[MechMod.headIndex]); // 50 health bonus
            weaponsPlayer.partDamageBonus += 0.10f * modPlayer.partEffectiveness[MechMod.headIndex]; // 10% damage bonus
        }

        public void BodyOffsets(MechVisualPlayer visualPlayer, string body)
        {
            switch (body)
            {
                case "FastBody":
                    visualPlayer.bodyOffsets[3] = new Vector2(-1, 0);
                    break;
                case "SlowBody":
                    visualPlayer.bodyOffsets[3] = new Vector2(2, -6);
                    break;
                default:
                    visualPlayer.bodyOffsets[3] = new Vector2(0, 0);
                    break;
            }
        }
    }
}
