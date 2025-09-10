using MechMod.Common.Players;
using MechMod.Content.Items.MechWeapons;
using MechMod.Content.Mounts;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MechMod.Content.Items.MechLegs
{
    public class FastLegs : ModItem, IMechParts
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
            modPlayer.lifeBonus += (int)(50 * modPlayer.partEffectiveness[MechMod.legsIndex]); // 50 health bonus

            // Ground stats
            mech.MountData.acceleration = 0.2f * modPlayer.partEffectiveness[MechMod.legsIndex];
            modPlayer.groundHorizontalSpeed = 6f * modPlayer.partEffectiveness[MechMod.legsIndex];

            // Jumping stats
            mech.MountData.jumpHeight = (int)(10 * modPlayer.partEffectiveness[MechMod.legsIndex]);
            modPlayer.groundJumpSpeed = 12f * modPlayer.partEffectiveness[MechMod.legsIndex];
        }

        public void BodyOffsets(MechVisualPlayer visualPlayer, string body)
        {
            switch (body)
            {
                case "BaseBody":
                    visualPlayer.bodyOffsets[1] = new Vector2(1, 0);
                    visualPlayer.bodyOffsets[2] = new Vector2(1, 0);
                    break;
                case "SlowBody":
                    visualPlayer.bodyOffsets[1] = new Vector2(1, 0);
                    visualPlayer.bodyOffsets[2] = new Vector2(1, 0);
                    break;
                default:
                    visualPlayer.bodyOffsets[1] = new Vector2(0, 0); // Right
                    visualPlayer.bodyOffsets[2] = new Vector2(0, 0); // Left
                    break;
            }
        }
    }
}
