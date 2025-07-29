using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.UI;
using Terraria;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;
using MechMod.Common.Players;
using Terraria.ModLoader.UI;
using Terraria.Audio;
using Terraria.ID;

namespace MechMod.Common.UI
{
    internal class PartSlot : UIElement
    {
        public Item item;
        private string slotPartType;

        public PartSlot(string partType)
        {
            item = new Item();
            item.SetDefaults(0);
            slotPartType = partType;
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            var modPlayer = Main.LocalPlayer.GetModPlayer<MechModPlayer>();

            base.DrawSelf(spriteBatch);
            ItemSlot.Draw(spriteBatch, ref item, ItemSlot.Context.InventoryCoin, GetDimensions().Position());
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
                if (IsMouseHovering && slotPartType == "booster" && modPlayer.powerCellActive)
                {
                    UICommon.TooltipMouseText("Empty\n[c/30FFFF:+100 Health]");
                }
            }
        }

        private bool isMechPart(Item item)
        {
            // Check if the part type matches the required part type for the slot
            foreach (var mechPart in MechMod.MechParts.Values)
            {
                if (mechPart.ItemType == item.type)
                {
                    // Logic for handling passive modules
                    if (mechPart.PartType == "passivemodule" && slotPartType == "passivemodule1" || slotPartType == "passivemodule2")
                        return true;
                    else
                        return mechPart.PartType == slotPartType;
                }
            }
            return false;
        }

        public void EquipPart(UIMouseEvent evt, UIElement listeningElement)
        {
            var modPlayer = Main.LocalPlayer.GetModPlayer<MechModPlayer>();

            if (Main.mouseItem.IsAir && item.IsAir)
            {
                return;
            }
            else if (Main.mouseItem.IsAir && !item.IsAir)
            {
                // If the slot has an item and the mouse is empty, pick up the item
                Main.mouseItem = item.Clone();
                item.TurnToAir();
                SoundEngine.PlaySound(SoundID.Grab);
                return;
            }
            if (isMechPart(Main.mouseItem))
            {
                if (
                    (slotPartType == "passivemodule1" || slotPartType == "passivemodule2") &&
                    (modPlayer.equippedParts[MechMod.passivemodule1Index].type == Main.mouseItem.type ||
                     modPlayer.equippedParts[MechMod.passivemodule2Index].type == Main.mouseItem.type)
                )
                {
                    Main.NewText("You already have this passive module equipped!", Color.Red);
                    return;
                }
                else if (!Main.mouseItem.IsAir && item.IsAir)
                {
                    // If the slot is empty and the mouse has an item, place the item
                    item = Main.mouseItem.Clone();
                    Main.mouseItem.TurnToAir();
                    SoundEngine.PlaySound(SoundID.Grab);
                }
                else if (!Main.mouseItem.IsAir && !item.IsAir)
                {
                    // If both the slot and the mouse have items, swap them
                    Item temp = item.Clone();
                    item = Main.mouseItem.Clone();
                    Main.mouseItem = temp;
                    SoundEngine.PlaySound(SoundID.Grab);
                }
            }
            else
            {
                Main.NewText("Invalid Mech Part!", Color.Red);
            }
        }
    }
}
