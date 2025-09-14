using MechMod.Common.Players;
using MechMod.Content.Mounts;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MechMod.Content.Items.MechLegs
{
    public class BaseLegs : ModItem, IMechParts
    {
        public override void SetDefaults()
        {
            Item.value = Item.buyPrice(gold: 1);
            Item.rare = ItemRarityID.Green;
        }

        public void ApplyStats(Player player, MechModPlayer modPlayer, MechWeaponsPlayer weaponsPlayer, ModularMech mech)
        {
            modPlayer.lifeBonus += (int)(100 * modPlayer.partEffectiveness[MechMod.legsIndex]); // 100 health bonus

            // Ground stats
            mech.MountData.acceleration = 0.1f * modPlayer.partEffectiveness[MechMod.legsIndex]; // 0.1 acceleration
            modPlayer.groundHorizontalSpeed = 4f * modPlayer.partEffectiveness[MechMod.legsIndex]; // 4 horizontal speed

            // Jumping stats
            mech.MountData.jumpHeight = (int)(10 * modPlayer.partEffectiveness[MechMod.legsIndex]); // 10 jump height
            modPlayer.groundJumpSpeed = 8f * modPlayer.partEffectiveness[MechMod.legsIndex]; // 8 jump speed
        }

        public void BodyOffsets(MechVisualPlayer visualPlayer, string body)
        {
            switch (body)
            {
                case "FastBody":
                    visualPlayer.bodyOffsets[1] = new Vector2(1, 0);
                    visualPlayer.bodyOffsets[2] = new Vector2(1, 0);
                    break;
                case "SlowBody":
                    visualPlayer.bodyOffsets[1] = new Vector2(0, 0);
                    visualPlayer.bodyOffsets[2] = new Vector2(0, 0);
                    break;
                default:
                    visualPlayer.bodyOffsets[1] = new Vector2(0, 0); // Right
                    visualPlayer.bodyOffsets[2] = new Vector2(0, 0); // Left
                    break;
            }
        }
    }
}
