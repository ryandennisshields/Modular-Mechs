using MechMod.Content.Items.MechWeapons;
using MechMod.Content.Mounts;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MechMod.Content.Items.MechArms
{
    internal class FastArms : ModItem, IMechParts
    {
        public override string Texture => "MechMod/Content/Items/MechArms/BaseArms";
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
    }
}
