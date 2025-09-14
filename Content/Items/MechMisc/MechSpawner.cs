using MechMod.Content.Mounts;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MechMod.Content.Items.MechMisc
{
    /// <summary>
    /// Item that acts as the way for the player to mount their Mech.
    /// </summary>

    public class MechSpawner : ModItem
    {
        public override void SetDefaults()
        {
            Item.autoReuse = false;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useAnimation = 15;
            Item.useTime = 15;
            Item.noMelee = true;
            Item.rare = ItemRarityID.Cyan;
            Item.value = Item.buyPrice(silver: 50);
            Item.mountType = ModContent.MountType<ModularMech>();
        }
    }
}