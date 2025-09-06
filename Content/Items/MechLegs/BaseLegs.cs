using MechMod.Common.Players;
using MechMod.Content.Items.MechWeapons;
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
            Item.width = 20; // The width of the item's hitbox in pixels.
            Item.height = 20; // The height of the item's hitbox in pixels.
            Item.value = Item.buyPrice(gold: 1);
            Item.rare = 2; // The rarity of the item.
        }

        public void ApplyStats(Player player, MechModPlayer modPlayer, ModularMech mech)
        {
            modPlayer.lifeBonus += (int)(100 * modPlayer.partEffectiveness[MechMod.legsIndex]); // 100 health bonus

            // Ground stats
            mech.MountData.acceleration = 0.1f * modPlayer.partEffectiveness[MechMod.legsIndex];
            mech.groundHorizontalSpeed = 4f * modPlayer.partEffectiveness[MechMod.legsIndex];

            // Jumping stats
            mech.MountData.jumpHeight = (int)(10 * modPlayer.partEffectiveness[MechMod.legsIndex]);
            mech.groundJumpSpeed = 8f * modPlayer.partEffectiveness[MechMod.legsIndex];
        }

        public void BodyOffsets(Player player, string body)
        {
            MechModPlayer modPlayer = player.GetModPlayer<MechModPlayer>();

            switch (body)
            {
                case "FastBody":
                    modPlayer.bodyOffsets[1] = new Vector2(1, 0);
                    modPlayer.bodyOffsets[2] = new Vector2(1, 0);
                    break;
                case "SlowBody":
                    modPlayer.bodyOffsets[1] = new Vector2(0, 0);
                    modPlayer.bodyOffsets[2] = new Vector2(0, 0);
                    break;
                default:
                    modPlayer.bodyOffsets[1] = new Vector2(0, 0); // Right
                    modPlayer.bodyOffsets[2] = new Vector2(0, 0); // Left
                    break;
            }
        }
    }
}
