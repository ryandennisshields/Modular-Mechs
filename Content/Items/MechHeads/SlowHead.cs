using MechMod.Common.Players;
using MechMod.Content.Items.MechWeapons;
using MechMod.Content.Mounts;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MechMod.Content.Items.MechHeads
{
    internal class SlowHead : ModItem, IMechParts
    {
        public override void SetDefaults()
        {
            Item.width = 20; // The width of the item's hitbox in pixels.
            Item.height = 20; // The height of the item's hitbox in pixels.
            Item.value = Item.buyPrice(gold: 4);
            Item.rare = ItemRarityID.Green; // The rarity of the item.
        }

        public void ApplyStats(Player player, MechModPlayer modPlayer, MechWeaponsPlayer weaponsPlayer, ModularMech mech)
        {
            modPlayer.armourBonus = (player.statDefense / 2) * modPlayer.partEffectiveness[MechMod.headIndex]; // 1.5x the armour
        }

        public void BodyOffsets(Player player, string body)
        {
            MechModPlayer modPlayer = player.GetModPlayer<MechModPlayer>();

            switch (body)
            {
                case "BaseBody":
                    modPlayer.bodyOffsets[3] = new Vector2(0, 8);
                    break;
                case "FastBody":
                    modPlayer.bodyOffsets[3] = new Vector2(-1, 8);
                    break;
                default:
                    modPlayer.bodyOffsets[3] = new Vector2(0, 0);
                    break;
            }
        }
    }
}
