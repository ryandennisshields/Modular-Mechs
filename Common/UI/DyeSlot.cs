using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.UI;

namespace MechMod.Common.UI
{
    /// <summary>
    /// Stores the dyes that the apply to the visuals of the mech's parts.
    /// </summary>

    public class DyeSlot : UIElement
    {
        public Item slotItem; // Stores the dye in a slot

        public DyeSlot()
        {
            slotItem = new Item();
            slotItem.SetDefaults(0);
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            // Store the old inventory scale and set a new scale for drawing the slot
            float oldScale = Main.inventoryScale;
            Main.inventoryScale = 0.6f;

            // Draw the slot with the appearance of a dye slot
            base.DrawSelf(spriteBatch);
            ItemSlot.Draw(spriteBatch, ref slotItem, ItemSlot.Context.EquipDye, GetDimensions().Position());

            Main.inventoryScale = oldScale; // Reset the inventory scale back to the old scale (keeps it pinned at a specific scale)

            // Enforce only one stack of dye in the slot
            if (!slotItem.IsAir && slotItem.stack > 1)
                slotItem.stack = 1;

            // Display item information when hovering over the slot
            if (!slotItem.IsAir)
            {
                if (IsMouseHovering)
                {
                    Main.HoverItem = slotItem;
                    Main.hoverItemName = slotItem.Name;
                }
            }
        }

        // Function to check if the item is a dye
        static private bool IsDye(Item item)
        {
            return item.dye > 0;
        }

        // Function to store a dye to the slot when clicked
        public void DropEquipDye(UIMouseEvent evt, UIElement listeningElement)
        {
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
            // Now that the slot is either empty or has an item that the player is trying to pick up, we can check for when the player is actually trying to slot a dye
            else if (IsDye(Main.mouseItem)) // If the mouse has an item, check if it's a dye
            {
                if (!Main.mouseItem.IsAir && slotItem.IsAir) // If the slot is empty and the mouse has a dye,
                {
                    // Place the dye
                    slotItem = Main.mouseItem.Clone();
                    // Only allow one stack of dye
                    slotItem.stack = 1;
                    Main.mouseItem.stack--;
                    if (Main.mouseItem.stack <= 0)
                        Main.mouseItem.TurnToAir();
                    SoundEngine.PlaySound(SoundID.Grab);
                }
                else if (!Main.mouseItem.IsAir && !slotItem.IsAir && Main.mouseItem.stack == 1) // If both the slot and the mouse have dyes, and the attempted swap item is only one stack of an item,
                {
                    // Swap the dyes
                    Utils.Swap(ref slotItem, ref Main.mouseItem);
                    SoundEngine.PlaySound(SoundID.Grab);
                }
            }
        }
    }
}
