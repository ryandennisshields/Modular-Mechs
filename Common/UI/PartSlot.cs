using Microsoft.Xna.Framework.Graphics;
using Terraria.UI;
using Terraria;
using Microsoft.Xna.Framework;
using MechMod.Common.Players;
using Terraria.ModLoader.UI;
using Terraria.Audio;
using Terraria.ID;

namespace MechMod.Common.UI
{
    /// <summary>
    /// Custom UI element that represents a slot for a Mech Part, storing the item in the slot, denoting the slot Part type and allowing the player to change Parts.
    /// </summary>

    public class PartSlot : UIElement
    {
        public Item item; // Stores the item stored in a slot
        private string slotPartType; // Stores the type of Part this slot is for

        // Function to create a new PartSlot with a specific Part type
        public PartSlot(string partType)
        {
            item = new Item();
            item.SetDefaults(0);
            slotPartType = partType;
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            var modPlayer = Main.LocalPlayer.GetModPlayer<MechModPlayer>();

            // Draw the slot with the appearance of a base game item slot
            base.DrawSelf(spriteBatch);
            ItemSlot.Draw(spriteBatch, ref item, ItemSlot.Context.InventoryCoin, GetDimensions().Position());

            // Display item information when hovering over the slot
            if (!item.IsAir)
            {
                if (IsMouseHovering)
                {
                    Main.HoverItem = item;
                    Main.hoverItemName = item.Name;
                }
            }
            else
            {
                // Unique tooltip for an empty Booster slot to indicate the empty Booster slot health bonus
                if (IsMouseHovering && slotPartType == "booster" && modPlayer.powerCellActive)
                {
                    UICommon.TooltipMouseText("Empty\n[c/30FFFF:+100 Health]");
                }
            }
        }

        // Function to check if the item is a valid Mech Part for the slot
        private bool isMechPart(Item item)
        {
            foreach (var mechPart in MechMod.MechParts.Values)
            {
                if (mechPart.ItemType == item.type)
                {
                    // Logic so the passive module slot 1 and 2 can accept the same Part type
                    if (mechPart.PartType == "passivemodule" && slotPartType == "passivemodule1" || slotPartType == "passivemodule2")
                        return true;
                    else
                        return mechPart.PartType == slotPartType;
                }
            }
            return false;
        }

        // Function to equip a Part to the slot when clicked
        public void EquipPart(UIMouseEvent evt, UIElement listeningElement)
        {
            var modPlayer = Main.LocalPlayer.GetModPlayer<MechModPlayer>();

            if (Main.mouseItem.IsAir) // If the player doesn't have an item to put in the slot,
            {
                if (item.IsAir) // If the slot is empty,
                {
                    return; // Do nothing
                }
                else // If the slot has an item,
                {
                    // Grab the item
                    Main.mouseItem = item.Clone();
                    item.TurnToAir();
                    SoundEngine.PlaySound(SoundID.Grab);
                    return;
                }
            }
            // Now that the slot is either empty or has an item that the player is trying to pick up, we can check for when the player is actually trying to equip a Part
            else if (isMechPart(Main.mouseItem)) // If the mouse has an item, check if it's a valid Mech Part first for the slot
            {
                // Prevent the player from equipping the same passive module in both slots
                if (
                    (slotPartType == "passivemodule1" || slotPartType == "passivemodule2") &&
                    (modPlayer.equippedParts[MechMod.passivemodule1Index].type == Main.mouseItem.type ||
                     modPlayer.equippedParts[MechMod.passivemodule2Index].type == Main.mouseItem.type)
                )
                {
                    Main.NewText("You already have this passive module equipped!", Color.Red); // Notify the player that they already have this passive module equipped
                }
                else if (!Main.mouseItem.IsAir && item.IsAir) // If the slot is empty and the mouse has a Mech Part,
                {
                    // Place the Part
                    item = Main.mouseItem.Clone();
                    Main.mouseItem.TurnToAir();
                    SoundEngine.PlaySound(SoundID.Grab);
                }
                else if (!Main.mouseItem.IsAir && !item.IsAir) // If both the slot and the mouse have Mech Parts, 
                {
                    // Swap the Parts
                    Item temp = item.Clone();
                    item = Main.mouseItem.Clone();
                    Main.mouseItem = temp;
                    SoundEngine.PlaySound(SoundID.Grab);
                }
            }
            else // If the Mech Part is invalid for the slot,
            {
                Main.NewText("Invalid Mech Part!", Color.Red); // Notify the player that the Part is invalid
            }
        }
    }
}
