using MechMod.Content.Items.MechWeapons;
using MechMod.Content.Mechs;
using Terraria;
using Terraria.ModLoader;

namespace MechMod.Content.Items.MechLegs
{
    public class FastLegs : ModItem, IMechParts
    {
        public override string Texture => "MechMod/Content/Items/MechLegs/BaseLegs";
        public override void SetDefaults()
        {
            Item.width = 20; // The width of the item's hitbox in pixels.
            Item.height = 20; // The height of the item's hitbox in pixels.
            Item.value = 10000; // The value of the item in copper coins.
            Item.rare = 2; // The rarity of the item.
        }

        public void ApplyStats(ModularMech mech)
        {
            mech.lifeBonus += 25; // 25 health bonus

            // Ground stats
            mech.MountData.acceleration = 0.2f;
            mech.MountData.runSpeed = 6f;
            mech.MountData.swimSpeed = 6f;

            // Jumping stats
            mech.MountData.jumpHeight = 10;
            mech.MountData.jumpSpeed = 12f;
        }
    }
}
