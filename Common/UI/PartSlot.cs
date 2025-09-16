using MechMod.Common.Players;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader.UI;
using Terraria.UI;

namespace MechMod.Common.UI
{
    /// <summary>
    /// Custom UI element that represents a slot for a Mech Part, storing the item in the slot, denoting the slot Part type and allowing the player to change Parts.
    /// </summary>

    public class PartSlot : UIElement
    {
        public Item slotItem; // Stores the item stored in a slot
        private string slotPartType; // Stores the type of Part this slot is for

        // Function to create a new PartSlot with a specific Part type
        public PartSlot(string partType)
        {
            slotItem = new Item();
            slotItem.SetDefaults(0);
            slotPartType = partType;
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            var modPlayer = Main.LocalPlayer.GetModPlayer<MechModPlayer>();

            // Store the old inventory scale and set a new scale for drawing the slot
            float oldScale = Main.inventoryScale;
            Main.inventoryScale = 0.6f;

            // Draw the slot with the appearance of a base game item slot
            base.DrawSelf(spriteBatch);
            ItemSlot.Draw(spriteBatch, ref slotItem, ItemSlot.Context.InventoryCoin, GetDimensions().Position());

            Main.inventoryScale = oldScale; // Reset the inventory scale back to the old scale (keeps it pinned at a specific scale)

            // Display item information when hovering over the slot
            if (!slotItem.IsAir)
            {
                if (IsMouseHovering)
                {
                    Main.HoverItem = slotItem;
                    Main.hoverItemName = slotItem.Name;
                }
            }
            else
            {
                // Unique tooltip for an empty Booster slot to indicate the empty Booster slot health bonus
                if (IsMouseHovering && slotPartType == "booster" && modPlayer.powerCellActive)
                {
                    UICommon.TooltipMouseText("Empty\n[c/3030FF:+100 Health]");
                }
            }
        }

        #region Equip Helper Functions

        // Function to check if the item is a valid Mech Part for the slot
        private bool IsMechPart(Item item)
        {
            foreach (var mechPart in MechMod.MechParts.Values)
            {
                if (mechPart.ItemType == item.type)
                {
                    // Logic so the passive module slot 1 and 2 can accept the same Part type
                    if (mechPart.PartType == "passivemodule")
                    {
                        if (slotPartType == "passivemodule1" || slotPartType == "passivemodule2")
                            return true;
                    }
                    else
                        return mechPart.PartType == slotPartType;
                }
            }
            return false;
        }

        // Function to prevent equipping the same passive module in both slots
        private bool DupeCheckPassiveModule(MechModPlayer modPlayer, Item item)
        {
            if (slotPartType == "passivemodule1" || slotPartType == "passivemodule2")
            {
                int passivemodule1 = modPlayer.equippedParts[MechMod.passivemodule1Index].type;
                int passivemodule2 = modPlayer.equippedParts[MechMod.passivemodule2Index].type;

                // Prevent the player from equipping the same passive module in both slots
                if (passivemodule1 == item.type || passivemodule2 == item.type)
                {
                    if (slotPartType == "passivemodule1")
                        Main.NewText("You already have this passive module equipped!", Color.Red); // Notify the player that they already have this passive module equipped
                    return false;
                }
            }
            return true;
        }

        // Function to get the index of the inventory slot currently being hovered over by the mouse
        public static int GetHoveredInventorySlot()
        {
            float slotSize = 43.5f;
            float slotGap = 4f;
            float startX = 20.5f;
            float startY = 20;

            // For the inventory, there are 5 rows of 10 slots
            for (int row = 0; row < 5; row++)
            {
                for (int col = 0; col < 10; col++)
                {
                    int index = row * 10 + col; // Get the index of the slot in the inventory array
                    // Calculate the positioning of the inventory slots
                    float x = startX + col * (slotSize + slotGap);
                    float y = startY + row * (slotSize + slotGap);
                    Rectangle slotRect = new((int)x, (int)y, (int)slotSize, (int)slotSize);

                    if (slotRect.Contains(Main.mouseX, Main.mouseY)) // If the mouse is hovering over a slot,
                    {
                        int slotIndex = index;
                        return slotIndex; // Return the index of the hovered slot
                    }
                }
            }
            return -1;
        }

        #endregion

        #region Equip Functions

        // Function to equip a Part to the slot when clicked
        public void DropEquipPart(UIMouseEvent evt, UIElement listeningElement)
        {
            var modPlayer = Main.LocalPlayer.GetModPlayer<MechModPlayer>();

            if (Main.mouseItem.IsAir) // If the player doesn't have an item to put in the slot,
            {
                if (slotItem.IsAir) // If the slot is empty,
                {
                    return; // Do nothing
                }
                else // If the slot has an item,
                {
                    // Grab the item
                    Main.mouseItem = slotItem.Clone();
                    slotItem.TurnToAir();
                    SoundEngine.PlaySound(SoundID.Grab);
                    return;
                }
            }
            // Now that the slot is either empty or has an item that the player is trying to pick up, we can check for when the player is actually trying to equip a Part
            else if (IsMechPart(Main.mouseItem)) // If the mouse has an item, check if it's a valid Mech Part first for the slot
            {
                if (!DupeCheckPassiveModule(modPlayer, Main.mouseItem))
                {
                    return; // Prevent equipping the same passive module in both slots
                }
                else if (!Main.mouseItem.IsAir && slotItem.IsAir) // If the slot is empty and the mouse has a Mech Part,
                {
                    // Place the Part
                    slotItem = Main.mouseItem.Clone();
                    Main.mouseItem.TurnToAir();
                    SoundEngine.PlaySound(SoundID.Grab);
                }
                else if (!Main.mouseItem.IsAir && !slotItem.IsAir) // If both the slot and the mouse have Mech Parts, 
                {
                    // Swap the Parts
                    Utils.Swap(ref slotItem, ref Main.mouseItem);
                    SoundEngine.PlaySound(SoundID.Grab);
                }
            }
            else // If the Mech Part is invalid for the slot,
            {
                Main.NewText("Invalid Mech Part!", Color.Red); // Notify the player that the Part is invalid
            }
        }

        // Function to equip a Part to the slot when right-clicked
        public void RightClickEquipPart()
        {
            int hoveredSlot = GetHoveredInventorySlot(); // Get the index of the inventory slot currently being hovered over by the mouse
            if (hoveredSlot >= 0 && hoveredSlot < Main.LocalPlayer.inventory.Length) // If the hovered slot index is valid,
            {
                var modPlayer = Main.LocalPlayer.GetModPlayer<MechModPlayer>();

                Item item = Main.LocalPlayer.inventory[hoveredSlot]; // Get the item in the hovered inventory slot

                if (!item.IsAir)
                {
                    if (IsMechPart(item))
                    {
                        if (!DupeCheckPassiveModule(modPlayer, item))
                        {
                            return; // Prevent equipping the same passive module in both slots
                        }
                        else if (slotItem.IsAir) // If the corresponding part slot is empty,
                        {
                            // Equip the Part into the corresponding part slot
                            slotItem = item.Clone();
                            Main.LocalPlayer.inventory[hoveredSlot].TurnToAir();
                            SoundEngine.PlaySound(SoundID.Grab);
                            return;
                        }
                        else
                        {
                            // Swap the Parts from the inventory and the corresponding part slot
                            Utils.Swap(ref slotItem, ref Main.LocalPlayer.inventory[hoveredSlot]);
                            SoundEngine.PlaySound(SoundID.Grab);
                            return;
                        }
                    }
                }
            }
        }

        #endregion
    }
}
