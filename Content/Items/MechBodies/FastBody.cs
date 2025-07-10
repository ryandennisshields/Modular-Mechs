using MechMod.Content.Items.MechWeapons;
using MechMod.Content.Mechs;
using Terraria;
using Terraria.ModLoader;

namespace MechMod.Content.Items.MechBodies
{
    internal class FastBody : ModItem, IMechParts
    {
        public override string Texture => "MechMod/Content/Items/MechBodies/BaseBody";
        public override void SetDefaults()
        {
            Item.width = 20; // The width of the item's hitbox in pixels.
            Item.height = 20; // The height of the item's hitbox in pixels.
            Item.value = 10000; // The value of the item in copper coins.
            Item.rare = 2; // The rarity of the item.
        }

        public void ApplyStats(Player player, ModularMech mech)
        {
            mech.lifeBonus += 50; // 50 health bonus
            mech.MountData.acceleration *= 1.2f; // 20% faster acceleration
            mech.MountData.runSpeed *= 1.2f; // 20% faster run speed
            mech.MountData.swimSpeed *= 1.2f; // 20% faster swim speed
        }
    }
}
