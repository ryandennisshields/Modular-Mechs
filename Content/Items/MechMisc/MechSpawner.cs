using MechMod.Common.Players;
using MechMod.Content.Buffs;
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

        public override bool CanUseItem(Player player)
        {
            MechModPlayer modPlayer = player.GetModPlayer<MechModPlayer>();

            // Disable the mount if the player has the MechDebuff or if the Head, Body, Arms, or Legs Parts are not equipped
            if (player.HasBuff(ModContent.BuffType<MechDebuff>()) ||
                modPlayer.equippedParts[MechMod.headIndex].IsAir ||
                modPlayer.equippedParts[MechMod.bodyIndex].IsAir ||
                modPlayer.equippedParts[MechMod.armsIndex].IsAir ||
                modPlayer.equippedParts[MechMod.legsIndex].IsAir)
            {
                if (!FindIfEquipped(player)) // Specifically if the player doesn't have the spawner equipped in their mount slot,
                    Item.mountType = -1; // Set the mount type to -1 (fully disables use of the item at the time of use, preventing issues with mounting in inventory/hotbar vs. quick mount binding)
                return false; // Prevent the player from using the item
            }
            else // Otherwise,
                return true; // Allow the player to use the item
        }

        public override void UpdateInventory(Player player)
        {
            if (Item.mountType == -1) // If the mount is disabled,
                Item.mountType = ModContent.MountType<ModularMech>(); // Reset it back to normal (so it can be put into the mount slot after preventing disabled inventory/hotbar use)
        }

        // Function to check if the player has the item equipped in their mount slot
        private bool FindIfEquipped(Player player)
        {
            for (int i = 0; i < player.miscEquips.Length; i++)
            {
                Item item = player.miscEquips[i]; // Grab the item from misc slots
                if (!item.IsAir && item.mountType > 0 && item.type == Item.type) // If the item is not air, has a mount type, and is the same type as this item,
                    return true; // Return true (the item is equipped)
            }
            return false; // Otherwise, return false (the item is not equipped)
        }
    }
}