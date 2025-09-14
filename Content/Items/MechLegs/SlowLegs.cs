using MechMod.Common.Players;
using MechMod.Content.Mounts;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MechMod.Content.Items.MechLegs
{
    public class SlowLegs : ModItem, IMechParts
    {
        public override void SetDefaults()
        {
            Item.value = Item.buyPrice(gold: 4);
            Item.rare = ItemRarityID.Green;
        }

        public void ApplyStats(Player player, MechModPlayer modPlayer, MechWeaponsPlayer weaponsPlayer, ModularMech mech)
        {
            modPlayer.lifeBonus += (int)(150 * modPlayer.partEffectiveness[MechMod.legsIndex]); // 150 health bonus

            // Ground stats
            mech.MountData.acceleration = 0.1f * modPlayer.partEffectiveness[MechMod.legsIndex]; // 0.1 acceleration
            modPlayer.groundHorizontalSpeed = 3f * modPlayer.partEffectiveness[MechMod.legsIndex]; // 3 horizontal speed

            // Jumping stats
            mech.MountData.jumpHeight = (int)(7 * modPlayer.partEffectiveness[MechMod.legsIndex]); // 7 jump height
            modPlayer.groundJumpSpeed = 4f * modPlayer.partEffectiveness[MechMod.legsIndex]; // 4 jump speed
        }

        public void BodyOffsets(MechVisualPlayer visualPlayer, string body)
        {
            switch (body)
            {
                case "BaseBody":
                    visualPlayer.bodyOffsets[1] = new Vector2(-2, 0);
                    visualPlayer.bodyOffsets[2] = new Vector2(-2, 0);
                    break;
                case "FastBody":
                    visualPlayer.bodyOffsets[1] = new Vector2(-3, 0);
                    visualPlayer.bodyOffsets[2] = new Vector2(-3, 0);
                    break;
                default:
                    visualPlayer.bodyOffsets[1] = new Vector2(0, 0); // Right
                    visualPlayer.bodyOffsets[2] = new Vector2(0, 0); // Left
                    break;
            }
        }
    }
}
