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
            Item.rare = 2; // The rarity of the item.
        }

        public void ApplyStats(Player player, ModularMech mech)
        {
            MechModPlayer modPlayer = player.GetModPlayer<MechModPlayer>();

            modPlayer.lifeBonus += 25; // 25 health bonus

            // Ground stats
            mech.MountData.acceleration = 0.2f;
            mech.groundHorizontalSpeed = 6f;

            // Jumping stats
            mech.MountData.jumpHeight = 10;
            mech.groundJumpSpeed = 12f;
        }

        public void BodyOffsets(Player player, string body)
        {
            MechModPlayer modPlayer = player.GetModPlayer<MechModPlayer>();

            switch (body)
            {
                case "BaseBody":
                    modPlayer.bodyOffsets[1] = new Vector2(1, 0);
                    modPlayer.bodyOffsets[2] = new Vector2(1, 0);
                    break;
                case "SlowBody":
                    modPlayer.bodyOffsets[1] = new Vector2(1, 0);
                    modPlayer.bodyOffsets[2] = new Vector2(1, 0);
                    break;
                default:
                    modPlayer.bodyOffsets[1] = new Vector2(0, 0); // Right
                    modPlayer.bodyOffsets[2] = new Vector2(0, 0); // Left
                    break;
            }
        }
    }
}
