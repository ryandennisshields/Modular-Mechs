using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace MechMod.Common.UI
{
    /// <summary>
    /// System that actually shows the UI when the player uses the Mech Bench.
    /// <para/><see cref="MechBenchUI"/> only contains the details, this class is responsible for managing the UI state and actually making it exist.
    /// </summary>

    [Autoload(Side = ModSide.Client)] // Load the system only on the client side, as this system is only used for UI, the server shouldn't need to load it
    public class MechBenchUISystem : ModSystem
    {
        private UserInterface mechBenchInterface; // UserInterface works as the actual UI, MechBenchUI just contains the actual UI elements and logic
        internal MechBenchUI mechBenchUI; // Custom state that contains the UI elements and logic for the Mech Bench UI

        // Functions to set the state of the UI to show and hide
        public void ShowMyUI()
        {
            mechBenchUI.OnPlayerUse(); // Also run the OnPlayerUse function
            mechBenchInterface?.SetState(mechBenchUI);
        }
        public void HideMyUI()
        {
            mechBenchInterface?.SetState(null);
        }

        public override void Load()
        {
            // Create the interface that can swap between different states (using ShowMyUI and HideMyUI)
            mechBenchInterface = new UserInterface();
            // Create a custom state for the UI (so when the interface is switching states, it can use this custom state to show the Mech Bench stuff)
            mechBenchUI = new MechBenchUI();

            // Activate the custom state for use (runs intialisation code in MechBenchUI)
            mechBenchUI.Activate();
        }

        public override void UpdateUI(GameTime gameTime)
        {
            // If an interface state is set (MechBenchUI), update the interface and the associated state
            if (mechBenchInterface?.CurrentState != null)
                mechBenchInterface.Update(gameTime);
        }
        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            // Modify the interface layers to treat the Mech Bench UI to be part of a UI layer and draw it
            int mouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text")); // Use Mouse Text layer
            if (mouseTextIndex != -1)
            {
                // Insert the layer to the game using the interface information
                layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer(
                    "MechMod: MechBenchInterface",
                    delegate
                    {
                        if (mechBenchInterface?.CurrentState != null)
                        {
                            mechBenchInterface.Draw(Main.spriteBatch, new GameTime()); // Draw the current state of the interface (MechBenchUI) to be visible on the screen
                        }
                        return true;
                    },
                    InterfaceScaleType.UI));
            }
        }
    }
}
