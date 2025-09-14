using MechMod.Common.Players;
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
            Item.value = Item.buyPrice(gold: 2);
            Item.rare = ItemRarityID.Green;
        }

        public void ApplyStats(Player player, MechModPlayer modPlayer, MechWeaponsPlayer weaponsPlayer, ModularMech mech)
        {
            modPlayer.lifeBonus += (int)(50 * modPlayer.partEffectiveness[MechMod.legsIndex]); // 50 health bonus

            // Ground stats
            mech.MountData.acceleration = 0.2f * modPlayer.partEffectiveness[MechMod.legsIndex]; // 0.2 acceleration
            modPlayer.groundHorizontalSpeed = 6f * modPlayer.partEffectiveness[MechMod.legsIndex]; // 6 horizontal speed

            // Jumping stats
            mech.MountData.jumpHeight = (int)(10 * modPlayer.partEffectiveness[MechMod.legsIndex]); // 10 jump height
            modPlayer.groundJumpSpeed = 12f * modPlayer.partEffectiveness[MechMod.legsIndex]; // 12 jump speed
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
