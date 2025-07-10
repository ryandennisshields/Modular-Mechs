using MechMod.Content.Items.MechWeapons;
using MechMod.Content.Mechs;
using Terraria;
using Terraria.ModLoader;

namespace MechMod.Content.Items.MechLegs
{
    public class SlowLegs : ModItem, IMechParts
    {
        public override string Texture => "MechMod/Content/Items/MechLegs/BaseLegs";
        public override void SetDefaults()
        {
            Item.width = 20; // The width of the item's hitbox in pixels.
            Item.height = 20; // The height of the item's hitbox in pixels.
            Item.value = 10000; // The value of the item in copper coins.
            Item.rare = 2; // The rarity of the item.
        }

        public void ApplyStats(Player player, ModularMech mech)
        {
            mech.lifeBonus += 100; // 100 health bonus

            // Ground stats
            mech.MountData.acceleration = 0.1f;
            mech.groundHorizontalSpeed = 3f;

            // Jumping stats
            mech.MountData.jumpHeight = 7;
            mech.groundJumpSpeed = 4f;
        }
    }
}
