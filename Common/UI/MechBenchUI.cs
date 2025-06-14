using MechMod.Common.Players;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Stubble.Core.Classes;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.UI;

namespace MechMod.Common.UI
{
    public class MechBenchUI : UIState
    {

        private UIPanel mainPanel;
        private PartSlot[] slots;
        private UIText[] slotNames;

        private UIText upgradeCostText;
        private UIText upgradeButton;
        private UIText playerLevel;

        public bool playerLoaded;

        public override void OnInitialize()
        {
            base.OnInitialize();

            mainPanel = new UIPanel();
            slots = new PartSlot[6];
            slotNames = new UIText[6];

            slots =
            [
            new PartSlot("Head"),
            new PartSlot("Body"),
            new PartSlot("Arms"),
            new PartSlot("Legs"),
            new PartSlot("Booster"),
            new PartSlot("Weapon")
            ];

            slotNames =
            [
            new UIText("Head"),
            new UIText("Body"),
            new UIText("Arms"),
            new UIText("Legs"),
            new UIText("Booster"),
            new UIText("Weapon")
            ];

            mainPanel.Left.Set(600f, 0f);
            mainPanel.Top.Set(200f, 0f);
            mainPanel.Width.Set(300f, 0f);
            mainPanel.Height.Set(300f, 0f);
            mainPanel.OnLeftMouseDown += DragStart;
            mainPanel.OnLeftMouseUp += DragEnd;
            Append(mainPanel);

            UIPanel button = new UIPanel();   // 1
            button.Width.Set(25, 0);
            button.Height.Set(25, 0);
            button.HAlign = 1f;
            button.OnLeftClick += OnButtonClick;  // 3
            mainPanel.Append(button);

            // Head, Body, Arms and Legs Slots
            for (int i = 0; i < slots.Length - 2; i++)
            {
                slots[i].Width.Set(32.5f, 0);
                slots[i].Height.Set(32.5f, 0);
                slots[i].Left.Set(50, 0);
                slots[i].Top.Set(40 + (i * 40), 0);
                slots[i].OnLeftClick += slots[i].EquipPart;
                mainPanel.Append(slots[i]);

                slotNames[i].Width.Set(50, 0);
                slotNames[i].Height.Set(25, 0);
                slotNames[i].Left.Set(-5, 0);
                slotNames[i].Top.Set(47.5f + (i * 40), 0);
                mainPanel.Append(slotNames[i]);
            }

            // Booster Slot
            slots[4].Width.Set(32.5f, 0);
            slots[4].Height.Set(32.5f, 0);
            slots[4].Left.Set(50, 0);
            slots[4].Top.Set(225, 0);
            slots[4].OnLeftClick += slots[4].EquipPart;
            mainPanel.Append(slots[4]);

            slotNames[4].Width.Set(50, 0);
            slotNames[4].Height.Set(25, 0);
            slotNames[4].Left.Set(33, 0);
            slotNames[4].Top.Set(200f, 0);
            mainPanel.Append(slotNames[4]);

            // Weapon Slot
            slots[5].Width.Set(32.5f, 0);
            slots[5].Height.Set(32.5f, 0);
            slots[5].Left.Set(185, 0);
            slots[5].Top.Set(225, 0);
            slots[5].OnLeftClick += slots[5].EquipPart;
            mainPanel.Append(slots[5]);

            slotNames[5].Width.Set(50, 0);
            slotNames[5].Height.Set(25, 0);
            slotNames[5].Left.Set(169, 0);
            slotNames[5].Top.Set(200f, 0);
            mainPanel.Append(slotNames[5]);

            playerLoaded = false;

            UIText upgradeText = new UIText("Upgrade");
            upgradeText.Width.Set(50, 0);
            upgradeText.Height.Set(25, 0);
            upgradeText.Left.Set(105, 0);
            upgradeText.Top.Set(50, 0);
            mainPanel.Append(upgradeText);

            upgradeCostText = new UIText("");
            upgradeCostText.Width.Set(150, 0);
            upgradeCostText.Height.Set(25, 0);
            upgradeCostText.Left.Set(105, 0);
            upgradeCostText.Top.Set(90, 0);
            mainPanel.Append(upgradeCostText);

            upgradeButton = new UIText("Confirm");
            upgradeButton.Width.Set(25, 0);
            upgradeButton.Height.Set(25, 0);
            upgradeButton.Left.Set(105, 0);
            upgradeButton.Top.Set(150, 0);
            upgradeButton.OnLeftClick += OnUpgradeButtonClick;
            mainPanel.Append(upgradeButton);

            playerLevel = new UIText("");
            playerLevel.Width.Set(25, 0);
            playerLevel.Height.Set(25, 0);
            playerLevel.Left.Set(105, 0);
            playerLevel.Top.Set(170, 0);
            mainPanel.Append(playerLevel);

            //Dev Button
            UIPanel resetButton = new UIPanel();
            resetButton.Width.Set(25, 0);
            resetButton.Height.Set(25, 0);
            resetButton.Left.Set(225, 0);
            resetButton.Top.Set(150, 0);
            resetButton.OnLeftClick += OnResetButtonClick;
            mainPanel.Append(resetButton);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (ContainsPoint(Main.MouseScreen))
                Main.LocalPlayer.mouseInterface = true;

            if (dragging)
            {
                mainPanel.Left.Set(Main.mouseX - offset.X, 0f);
                mainPanel.Top.Set(Main.mouseY - offset.Y, 0f);
                Recalculate();
            }

            if (upgradeButton.IsMouseHovering)
                upgradeButton.TextColor = Color.Red;
            else
                upgradeButton.TextColor = Color.Yellow;
        }

        private void OnButtonClick(UIMouseEvent evt, UIElement listeningElement)
        {
            ModContent.GetInstance<MechBenchUISystem>().HideMyUI();
        }

        private Vector2 offset;
        private bool dragging;

        private void DragStart(UIMouseEvent evt, UIElement listeningElement)
        {
            offset = new Vector2(evt.MousePosition.X - mainPanel.Left.Pixels, evt.MousePosition.Y - mainPanel.Top.Pixels);
            dragging = true;
        }

        private void DragEnd(UIMouseEvent evt, UIElement listeningElement)
        {
            Vector2 endMousePosition = evt.MousePosition;
            dragging = false;

            mainPanel.Left.Set(endMousePosition.X - offset.X, 0f);
            mainPanel.Top.Set(endMousePosition.Y - offset.Y, 0f);

            Recalculate();
        }

        public void OnPlayerUse()
        {
            var player = Main.LocalPlayer;
            var modPlayer = Main.LocalPlayer.GetModPlayer<MechModPlayer>();

            player.mount.Dismount(player);
            for (int i = 0; i < slots.Length; i++)
            {
                if (!modPlayer.equippedParts[i].IsAir)
                {
                    slots[i].item = modPlayer.equippedParts[i];
                }
            }

            UpdateUpgradeRequirements();
        }

        public override void OnDeactivate()
        {
            base.OnDeactivate();

            var player = Main.LocalPlayer;
            var modPlayer = Main.LocalPlayer.GetModPlayer<MechModPlayer>();

            player.mount.Dismount(player);
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i].item.IsAir)
                {
                    modPlayer.equippedParts[i].TurnToAir();
                }
                else
                {
                    modPlayer.equippedParts[i] = slots[i].item;
                }

            }
        }

        private int[][] upgradeRequiredMaterial =
        [
            [ItemID.DemoniteBar, ItemID.CrimtaneBar],
            [ItemID.HellstoneBar],
            [ItemID.MythrilBar, ItemID.OrichalcumBar],
            [ItemID.HallowedBar],
            [ItemID.ChlorophyteBar],
            [ItemID.LunarBar]
        ];
        private int upgradeCost = 20;

        private float[] upgradeDamageValues =
        [
            0.70f,
            1.20f,
            1.60f,
            1.60f,
            2.65f,
            18.80f
        ];

        private void OnUpgradeButtonClick(UIMouseEvent evt, UIElement listeningElement)
        {
            var player = Main.LocalPlayer;
            var modPlayer = Main.LocalPlayer.GetModPlayer<MechModPlayer>();

            // Check if the player has enough resources
            if (HasUpgradeMaterial(player))
            {
                // Before increasing Upgrade Level
                DeductResources(player);

                modPlayer.upgradeDamageBonus += upgradeDamageValues[modPlayer.upgradeLevel]; 
                Main.NewText($"Upgrade Successful! Damage increased by {Math.Round(upgradeDamageValues[modPlayer.upgradeLevel] * 100)}%");

                modPlayer.upgradeLevel++;
                
                // After increasing Upgrade Level
                UpdateUpgradeRequirements();
            }
            else if (modPlayer.upgradeLevel >= upgradeRequiredMaterial.Length)
            {
                Main.NewText("Max Level");
            }
            else
            {
                Main.NewText($"Not enough resources for upgrade.");
            }
            
        }

        private bool HasUpgradeMaterial(Player player)
        {
            var modPlayer = Main.LocalPlayer.GetModPlayer<MechModPlayer>();

            if (modPlayer.upgradeLevel < upgradeRequiredMaterial.Length)
            {
                foreach (int materialID in upgradeRequiredMaterial[modPlayer.upgradeLevel])
                {
                    if (player.CountItem(materialID) >= upgradeCost)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

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

        private void UpdateUpgradeRequirements()
        {
            var modPlayer = Main.LocalPlayer.GetModPlayer<MechModPlayer>();

            if (modPlayer.upgradeLevel < upgradeRequiredMaterial.Length)
            {
                // Get the current upgrade materials
                int[] currentMaterials = upgradeRequiredMaterial[modPlayer.upgradeLevel];

                // Create a string for the requirement text
                string requirementText = $"{upgradeCost}x ";

                // Loop through materials and display them
                for (int i = 0; i < currentMaterials.Length; i++)
                {
                    int materialID = currentMaterials[i];

                    // Add material name to the requirement text
                    requirementText += Lang.GetItemNameValue(materialID);

                    // If there is multiple upgrade materials for one upgrade, add "or" between them
                    if (i < currentMaterials.Length - 1)
                    {
                        requirementText += "\nor ";
                    }
                }

                upgradeCostText.SetText(requirementText);
            }
            else
            {
                upgradeCostText.SetText("Max Level");
            }
            playerLevel.SetText($"Level {modPlayer.upgradeLevel + 1}");
        }

        private void OnResetButtonClick(UIMouseEvent evt, UIElement listeningElement)
        {
            var modPlayer = Main.LocalPlayer.GetModPlayer<MechModPlayer>();
            modPlayer.upgradeLevel = 0;
            modPlayer.upgradeDamageBonus = 0.0f;
            Main.NewText("Player Reset");
        }
    }
}
