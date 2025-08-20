using MechMod.Common.Players;
using MechMod.Content.Items.MechWeapons;
using MechMod.Content.Mounts;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MechMod.Content.Items.MechArms
{
    internal class FastArms : ModItem, IMechParts
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
            Weapons.partDamageBonus += 0.2f; // 20% damage bonus
            Weapons.partCritChanceBonus += 0.1f; // 10% more critical chance
            mech.lifeBonus -= 25; // 25 health penalty
        }

        public void BodyOffsets(Player player, string body)
        {
            MechModPlayer modPlayer = player.GetModPlayer<MechModPlayer>();

            switch (body)
            {
                case "BaseBody":
                    modPlayer.bodyOffsets[0] = new Vector2(1, 0);
                    modPlayer.bodyOffsets[4] = new Vector2(1, 0);
                    break;
                case "SlowBody":
                    modPlayer.bodyOffsets[0] = new Vector2(3, -4);
                    modPlayer.bodyOffsets[4] = new Vector2(3, -4);
                    break;
                default:
                    modPlayer.bodyOffsets[0] = new Vector2(0, 0); // Right
                    modPlayer.bodyOffsets[4] = new Vector2(0, 0); // Left
                    break;
            }
        }
    }
}
