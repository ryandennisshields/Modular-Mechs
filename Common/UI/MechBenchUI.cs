using MechMod.Common.Players;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;
using MechMod.Content.Items.MechMisc;
using Microsoft.Xna.Framework.Graphics;

namespace MechMod.Common.UI
{
    /// <summary>
    /// Contains the information for the Mech Bench UI, which allows players to equip Parts to their mech and upgrade it.
    /// <para/> Used with <see cref="MechBenchUISystem"/> to manage the UI state and existence, <see cref="PartSlot"/> to create the slots for Parts, and the <see cref="MechBench"/> item to access the UI in the game.
    /// </summary>

    public class MechBenchUI : UIState
    {
        private UIPanel mainPanel;
        private PartSlot[] slots;
        private UIText[] slotNames;

        private UIText upgradeCostText;
        private UIText upgradeButton;
        private UIText playerLevel;

        #region UI Intialisation

        public override void OnInitialize()
        {
            base.OnInitialize();

            mainPanel = new UIPanel();
            slots = new PartSlot[9];
            slotNames = new UIText[9];

            slots =
            [
            new PartSlot("head"),
            new PartSlot("body"),
            new PartSlot("arms"),
            new PartSlot("legs"),
            new PartSlot("booster"),
            new PartSlot("weapon"),
            new PartSlot("passivemodule1"),
            new PartSlot("passivemodule2"),
            new PartSlot("activemodule")
            ];

            slotNames =
            [
            new UIText("Head"),
            new UIText("Body"),
            new UIText("Arms"),
            new UIText("Legs"),
            new UIText("Booster"),
            new UIText("Weapon"),
            new UIText("Modules"),
            new UIText("Passive"),
            new UIText("Active")
            ];

            mainPanel.Left.Set(600f, 0f);
            mainPanel.Top.Set(200f, 0f);
            mainPanel.Width.Set(300f, 0f);
            mainPanel.Height.Set(350f, 0f);
            mainPanel.OnLeftMouseDown += DragStart;
            mainPanel.OnLeftMouseUp += DragEnd;
            Append(mainPanel);

            // Exit/Close Button
            UIImage closeButton = new(ModContent.Request<Texture2D>("MechMod/Common/UI/Close"));
            closeButton.Width.Set(20, 0);
            closeButton.Height.Set(20, 0);
            closeButton.HAlign = 1f;
            closeButton.OnLeftClick += OnCloseClick;
            mainPanel.Append(closeButton);

            // Head, Body, Arms and Legs Slots
            for (int i = 0; i < 4; i++)
            {
                slots[i].Width.Set(32.5f, 0);
                slots[i].Height.Set(32.5f, 0);
                slots[i].HAlign = 0.25f;
                slots[i].Top.Set(30 + (i * 40), 0);
                slots[i].OnLeftClick += slots[i].DropEquipPart;
                mainPanel.Append(slots[i]);

                slotNames[i].Width.Set(50, 0);
                slotNames[i].Height.Set(25, 0);
                slotNames[i].HAlign = 0.025f;
                slotNames[i].Top.Set(37.5f + (i * 40), 0);
                mainPanel.Append(slotNames[i]);
            }

            // Booster Slot
            slots[4].Width.Set(32.5f, 0);
            slots[4].Height.Set(32.5f, 0);
            slots[4].HAlign = 0.25f;
            slots[4].Top.Set(215, 0);
            slots[4].OnLeftClick += slots[4].DropEquipPart;
            mainPanel.Append(slots[4]);

            slotNames[4].Width.Set(50, 0);
            slotNames[4].Height.Set(25, 0);
            slotNames[4].HAlign = 0.205f;
            slotNames[4].Top.Set(195, 0);
            mainPanel.Append(slotNames[4]);

            // Weapon Slot
            slots[5].Width.Set(32.5f, 0);
            slots[5].Height.Set(32.5f, 0);
            slots[5].HAlign = 0.75f;
            slots[5].Top.Set(215, 0);
            slots[5].OnLeftClick += slots[5].DropEquipPart;
            mainPanel.Append(slots[5]);

            slotNames[5].Width.Set(50, 0);
            slotNames[5].Height.Set(25, 0);
            slotNames[5].HAlign = 0.7725f;
            slotNames[5].Top.Set(195, 0);
            mainPanel.Append(slotNames[5]);

            // Passive Module Slots
            for (int i = 6; i < 8; i++)
            {
                slots[i].Width.Set(32.5f, 0);
                slots[i].Height.Set(32.5f, 0);
                slots[i].HAlign = 0.175f + (i - 6) * 0.15f;
                slots[i].Top.Set(290, 0);
                slots[i].OnLeftClick += slots[i].DropEquipPart;
                mainPanel.Append(slots[i]);
            }

            // Active Module Slot
            slots[8].Width.Set(32.5f, 0);
            slots[8].Height.Set(32.5f, 0);
            slots[8].HAlign = 0.75f;
            slots[8].Top.Set(290, 0);
            slots[8].OnLeftClick += slots[8].DropEquipPart;
            mainPanel.Append(slots[8]);

            // Module Text
            slotNames[6].Width.Set(50, 0);
            slotNames[6].Height.Set(25, 0);
            slotNames[6].HAlign = 0.5f;
            slotNames[6].Top.Set(250, 0);
            mainPanel.Append(slotNames[6]);

            slotNames[7].Width.Set(50, 0);
            slotNames[7].Height.Set(25, 0);
            slotNames[7].HAlign = 0.215f;
            slotNames[7].Top.Set(270, 0);
            mainPanel.Append(slotNames[7]);

            slotNames[8].Width.Set(50, 0);
            slotNames[8].Height.Set(25, 0);
            slotNames[8].HAlign = 0.76f;
            slotNames[8].Top.Set(270, 0);
            mainPanel.Append(slotNames[8]);

            for (int i = 0; i < slots.Length; i++)
            {
                slots[i].OnLeftClick += OnPartSlotInteract; 
            }

            // Upgrade Section
            UIText upgradeText = new("Upgrade");
            upgradeText.Width.Set(50, 0);
            upgradeText.Height.Set(25, 0);
            upgradeText.Left.Set(105, 0);
            upgradeText.Top.Set(40, 0);
            mainPanel.Append(upgradeText);

            upgradeCostText = new UIText("");
            upgradeCostText.Width.Set(150, 0);
            upgradeCostText.Height.Set(25, 0);
            upgradeCostText.Left.Set(105, 0);
            upgradeCostText.Top.Set(80, 0);
            mainPanel.Append(upgradeCostText);

            upgradeButton = new UIText("Confirm");
            upgradeButton.Width.Set(25, 0);
            upgradeButton.Height.Set(25, 0);
            upgradeButton.Left.Set(105, 0);
            upgradeButton.Top.Set(140, 0);
            upgradeButton.OnLeftClick += OnUpgradeButtonClick;
            mainPanel.Append(upgradeButton);

            playerLevel = new UIText("");
            playerLevel.Width.Set(25, 0);
            playerLevel.Height.Set(25, 0);
            playerLevel.Left.Set(105, 0);
            playerLevel.Top.Set(160, 0);
            mainPanel.Append(playerLevel);

            // Debugging Reset Button
            //UIPanel resetButton = new();
            //resetButton.Width.Set(25, 0);
            //resetButton.Height.Set(25, 0);
            //resetButton.Left.Set(225, 0);
            //resetButton.Top.Set(140, 0);
            //resetButton.OnLeftClick += OnResetButtonClick;
            //mainPanel.Append(resetButton);
        }

        #endregion

        // Function for when the player opens the Mech Bench UI through the Mech Bench
        public void OnPlayerUse()
        {
            var player = Main.LocalPlayer;
            var modPlayer = Main.LocalPlayer.GetModPlayer<MechModPlayer>();

            player.mount.Dismount(player); // Dismount to not cause any problems with editing the mech while it's active
            modPlayer.disableMounts = true; // Disable mounts

            // Fill the Part slots with the player's equipped Parts
            for (int i = 0; i < slots.Length; i++)
            {
                if (!modPlayer.equippedParts[i].IsAir)
                {
                    slots[i].slotItem = modPlayer.equippedParts[i];
                }
            }

            UpdateUpgradeRequirements();
        }

        public override void Update(GameTime gameTime)
        {
            // Let the game know when the player is mousing over the UI
            if (ContainsPoint(Main.MouseScreen))
                Main.LocalPlayer.mouseInterface = true;

            // If dragging the UI, update its position with the mouse
            if (dragging)
            {
                mainPanel.Left.Set(Main.mouseX - offset.X, 0f);
                mainPanel.Top.Set(Main.mouseY - offset.Y, 0f);
                Recalculate();
            }

            // Change upgrade button color based on mouse hover to indicate interactivity
            if (upgradeButton.IsMouseHovering)
                upgradeButton.TextColor = Color.Red;
            else
                upgradeButton.TextColor = Color.Yellow;
        }

        // Function for when the player clicks the exit/close button
        private void OnCloseClick(UIMouseEvent evt, UIElement listeningElement)
        {
            // Hide the UI and play Menu Close sound
            ModContent.GetInstance<MechBenchUISystem>().HideMyUI();
            SoundEngine.PlaySound(SoundID.MenuClose);
        }

        #region Update Equipped Parts

        // Function for when the player interacts with a Part slot
        private void OnPartSlotInteract(UIMouseEvent evt, UIElement listeningElement)
        {
            var modPlayer = Main.LocalPlayer.GetModPlayer<MechModPlayer>();

            // Verify if the slot is empty or if the slot has a Part
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i].slotItem.IsAir)
                {
                    modPlayer.equippedParts[i].TurnToAir();
                }
                else
                {
                    modPlayer.equippedParts[i] = slots[i].slotItem;
                }
            }
        }

        // Function for when the player right-clicks a Part (so when right-click is used to equip/swap a Part, it updates the equipped Parts properly)
        public override void RightMouseDown(UIMouseEvent evt)
        {
            var modPlayer = Main.LocalPlayer.GetModPlayer<MechModPlayer>();

            // Verify if the slot is empty or if the slot has a Part
            for (int i = 0; i < slots.Length; i++)
            {
                slots[i].RightClickEquipPart(); // Call the right-click equip function for each slot
                if (slots[i].slotItem.IsAir)
                {
                    modPlayer.equippedParts[i].TurnToAir();
                }
                else
                {
                    modPlayer.equippedParts[i] = slots[i].slotItem;
                }
            }
        }

        #endregion

        #region Dragging UI

        private Vector2 offset; // Offset to keep track of where the mouse is relative to the panel
        private bool dragging;

        private void DragStart(UIMouseEvent evt, UIElement listeningElement)
        {
            // When the mouse button is pressed, calculate the offset based on the mouse position relative to the panel
            offset = new Vector2(evt.MousePosition.X - mainPanel.Left.Pixels, evt.MousePosition.Y - mainPanel.Top.Pixels); // Set the offset based on the mouse position relative to the panel
            dragging = true;
        }

        private void DragEnd(UIMouseEvent evt, UIElement listeningElement)
        {
            // When the mouse button is released, update the panel's position based on the final mouse position
            Vector2 endMousePosition = evt.MousePosition;
            dragging = false;

            mainPanel.Left.Set(endMousePosition.X - offset.X, 0f);
            mainPanel.Top.Set(endMousePosition.Y - offset.Y, 0f);

            Recalculate(); // Recalculate the panel's dimensions and position after dragging
        }

        #endregion

        public override void OnDeactivate()
        {
            // After closing, force dismount the player again and allow mounts again
            var player = Main.LocalPlayer;
            var modPlayer = Main.LocalPlayer.GetModPlayer<MechModPlayer>();

            player.mount.Dismount(player);

            modPlayer.disableMounts = false;
        }

        #region Upgrade System

        // Materials used for each upgrade level, including multiple options for materials that have variants
        private int[][] upgradeRequiredMaterial =
        [
            [ItemID.HellstoneBar],
            [ItemID.MythrilBar, ItemID.OrichalcumBar],
            [ItemID.HallowedBar],
            [ItemID.ChlorophyteBar],
            [ItemID.FragmentNebula, ItemID.FragmentSolar, ItemID.FragmentStardust, ItemID.FragmentVortex],
            [ItemID.LunarBar]
        ];
        private int upgradeCost = 20; // Cost of each resource required for each upgrade

        // Damage increases for each upgrade level (as a percentage increase, for example 0.70f means 70% increase)
        private float[] upgradeDamageValues =
        [
            0.20f,
            0.25f,
            0.50f,
            0.50f,
            3.50f,
            3.00f
        ];

        // Function for when the player clicks the upgrade button
        private void OnUpgradeButtonClick(UIMouseEvent evt, UIElement listeningElement)
        {
            var player = Main.LocalPlayer;
            var modPlayer = Main.LocalPlayer.GetModPlayer<MechModPlayer>();

            // Check if the player has enough resources
            if (HasUpgradeMaterial(player))
            {
                // Before increasing upgrade level, remove associated resources from the player
                DeductResources(player);

                // Increase the upgrade damage bonus, notify the player of the increase, increase the upgrade level, and play a sound
                modPlayer.upgradeDamageBonus += upgradeDamageValues[modPlayer.upgradeLevel]; 
                Main.NewText($"Upgrade Successful! Damage increased by {Math.Round(upgradeDamageValues[modPlayer.upgradeLevel] * 100)}%");
                modPlayer.upgradeLevel++;
                SoundEngine.PlaySound(SoundID.Research);
                
                // Update requirements for next upgrade
                UpdateUpgradeRequirements();
            }
            else if (modPlayer.upgradeLevel >= upgradeRequiredMaterial.Length) // If max level, tell the player
            {
                Main.NewText("Max Level");
            }
            else // If not enough resources, notify the player
            {
                Main.NewText($"Not enough resources for upgrade.");
            }
            
        }

        // Function to check if the player has enough resources to upgrade their level
        private bool HasUpgradeMaterial(Player player)
        {
            var modPlayer = Main.LocalPlayer.GetModPlayer<MechModPlayer>();

            if (modPlayer.upgradeLevel < upgradeRequiredMaterial.Length) // If the player's upgrade level is less than the maximum amount of upgrades available
            {
                foreach (int materialID in upgradeRequiredMaterial[modPlayer.upgradeLevel]) // Grab the materials required for the upgrade level
                {
                    if (player.CountItem(materialID) >= upgradeCost) // If the player has enough of the required material,
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        // Function to deduct the required resources from the player for the upgrade
        private void DeductResources(Player player)
        {
            var modPlayer = Main.LocalPlayer.GetModPlayer<MechModPlayer>();
            // Loop through all possible materials for the current upgrade level
            foreach (int materialID in upgradeRequiredMaterial[modPlayer.upgradeLevel])
            {
                // Deduct the required amount if the player has enough of this material
                if (player.CountItem(materialID) >= upgradeCost)
                {
                    for (int i = 0; i < upgradeCost; i++)
                    player.ConsumeItem(materialID);

                    Main.NewText($"{upgradeCost} {Lang.GetItemNameValue(materialID)} consumed for upgrade.");
                    break;
                }
            }
        }

        // Function to update the upgrade requirements text based on the player's current upgrade level
        private void UpdateUpgradeRequirements()
        {
            var modPlayer = Main.LocalPlayer.GetModPlayer<MechModPlayer>();

            if (modPlayer.upgradeLevel < upgradeRequiredMaterial.Length)
            {
                // Get the current upgrade materials
                int[] currentMaterials = upgradeRequiredMaterial[modPlayer.upgradeLevel];

                // Create a string for the requirement text
                string requirementText = $"{upgradeCost}x ";

                // Specific text for Fragments
                if (modPlayer.upgradeLevel == 4)
                {
                    requirementText += "any Lunar\nFragment";
                }
                else
                {
                    // Loop through materials and display them
                    for (int i = 0; i < currentMaterials.Length; i++)
                    {
                        int materialID = currentMaterials[i];
                        // Add material name to the requirement text
                        requirementText += Lang.GetItemNameValue(materialID);

                        if (i < currentMaterials.Length - 1) // If there is multiple upgrade materials for one upgrade,
                        {
                            requirementText += "\nor "; // Add "or" between them
                        }

                    }
                }
                upgradeCostText.SetText(requirementText); // Display the upgrade requirements
            }
            else // If the player has went to the maximum upgrade level,
            {
                upgradeCostText.SetText("Max Level"); // Display max level text
            }
            playerLevel.SetText($"Level {modPlayer.upgradeLevel + 1}"); // Display the player's current upgrade level
        }

        #endregion

        #region Debugging Reset Button

        //private void OnResetButtonClick(UIMouseEvent evt, UIElement listeningElement)
        //{
        //    var modPlayer = Main.LocalPlayer.GetModPlayer<MechModPlayer>();
        //    modPlayer.upgradeLevel = 0;
        //    modPlayer.upgradeDamageBonus = 0.0f;
        //    Main.NewText("Player Reset");
        //}

        #endregion
    }
}
