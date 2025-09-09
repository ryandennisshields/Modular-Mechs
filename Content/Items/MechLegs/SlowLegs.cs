using MechMod.Common.Players;
using MechMod.Content.Items.MechWeapons;
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
            Item.width = 20; // The width of the item's hitbox in pixels.
            Item.height = 20; // The height of the item's hitbox in pixels.
            Item.value = Item.buyPrice(gold: 4);
            Item.rare = ItemRarityID.Green; // The rarity of the item.
        }

        public void ApplyStats(Player player, MechModPlayer modPlayer, MechWeaponsPlayer weaponsPlayer, ModularMech mech)
        {
            modPlayer.lifeBonus += (int)(150 * modPlayer.partEffectiveness[MechMod.legsIndex]); // 150 health bonus

            // Ground stats
            mech.MountData.acceleration = 0.1f * modPlayer.partEffectiveness[MechMod.legsIndex];
            modPlayer.groundHorizontalSpeed = 3f * modPlayer.partEffectiveness[MechMod.legsIndex];

            // Jumping stats
            mech.MountData.jumpHeight = (int)(7 * modPlayer.partEffectiveness[MechMod.legsIndex]);
            modPlayer.groundJumpSpeed = 4f * modPlayer.partEffectiveness[MechMod.legsIndex];
        }

        public void BodyOffsets(Player player, string body)
        {
            MechModPlayer modPlayer = player.GetModPlayer<MechModPlayer>();

            switch (body)
            {
                case "BaseBody":
                    modPlayer.bodyOffsets[1] = new Vector2(-2, 0);
                    modPlayer.bodyOffsets[2] = new Vector2(-2, 0);
                    break;
                case "FastBody":
                    modPlayer.bodyOffsets[1] = new Vector2(-3, 0);
                    modPlayer.bodyOffsets[2] = new Vector2(-3, 0);
                    break;
                default:
                    modPlayer.bodyOffsets[1] = new Vector2(0, 0); // Right
                    modPlayer.bodyOffsets[2] = new Vector2(0, 0); // Left
                    break;
            }
        }
    }
}
