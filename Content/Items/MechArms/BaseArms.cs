using MechMod.Common.Players;
using MechMod.Content.Items.MechWeapons;
using MechMod.Content.Mounts;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MechMod.Content.Items.MechArms
{
    internal class BaseArms : ModItem, IMechParts
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
            weaponsPlayer.partDamageBonus += 0.2f * modPlayer.partEffectiveness[MechMod.armsIndex]; // 20% damage bonus
        }

        public void BodyOffsets(Player player, string body)
        {
            MechModPlayer modPlayer = player.GetModPlayer<MechModPlayer>();

            switch (body)
            {
                case "FastBody":
                    modPlayer.bodyOffsets[0] = new Vector2(-5, 0);
                    modPlayer.bodyOffsets[4] = new Vector2(5, 0);
                    break;
                case "SlowBody":
                    modPlayer.bodyOffsets[0] = new Vector2(0, -6);
                    modPlayer.bodyOffsets[4] = new Vector2(0, -6);
                    break;
                default:
                    modPlayer.bodyOffsets[0] = new Vector2(0, 0); // Right
                    modPlayer.bodyOffsets[4] = new Vector2(0, 0); // Left
                    break;
            }
        }
    }
}
