using MechMod.Common.Players;
using MechMod.Content.Items.MechWeapons;
using MechMod.Content.Mounts;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MechMod.Content.Items.MechArms
{
    internal class SlowArms : ModItem, IMechParts
    {
        public override void SetDefaults()
        {
            Item.width = 20; // The width of the item's hitbox in pixels.
            Item.height = 20; // The height of the item's hitbox in pixels.
            Item.value = Item.buyPrice(gold: 4);
            Item.rare = 2; // The rarity of the item.
        }

        public void ApplyStats(Player player, MechModPlayer modPlayer, ModularMech mech)
        {
            Weapons.partDamageBonus += 0.3f * modPlayer.partEffectiveness[MechMod.armsIndex]; // 30% damage bonus
            Weapons.partAttackSpeedBonus -= 0.1f / modPlayer.partEffectiveness[MechMod.armsIndex]; // 10% slower attack speed
            Weapons.partCritChanceBonus -= 0.1f / modPlayer.partEffectiveness[MechMod.armsIndex]; // 10% less critical chance
        }

        public void BodyOffsets(Player player, string body)
        {
            MechModPlayer modPlayer = player.GetModPlayer<MechModPlayer>();

            switch (body)
            {
                case "BaseBody":
                    modPlayer.bodyOffsets[0] = new Vector2(-2, 0);
                    modPlayer.bodyOffsets[4] = new Vector2(0, 0);
                    break;
                case "FastBody":
                    modPlayer.bodyOffsets[0] = new Vector2(-5, 0);
                    modPlayer.bodyOffsets[4] = new Vector2(1, 0);
                    break;
                default:
                    modPlayer.bodyOffsets[0] = new Vector2(0, 0); // Right
                    modPlayer.bodyOffsets[4] = new Vector2(0, 0); // Left
                    break;
            }
        }
    }
}
