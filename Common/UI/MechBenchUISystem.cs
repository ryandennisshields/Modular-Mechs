using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace MechMod.Common.UI
{
    internal class MechBenchUISystem : ModSystem
    {
        internal UserInterface MechBenchInterface;
        internal MechBenchUI MechBenchUI;

        private GameTime lastUpdateUiGameTime;

        public override void Load()
        {
            if (!Main.dedServ)
            {
                MechBenchInterface = new UserInterface();

                MechBenchUI = new MechBenchUI();
                MechBenchUI.Activate();
            }
        }

        public override void UpdateUI(GameTime gameTime)
        {
            lastUpdateUiGameTime = gameTime;
            if (MechBenchInterface?.CurrentState != null)
                MechBenchInterface.Update(gameTime);
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int mouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
            if (mouseTextIndex != -1)
            {
                layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer(
                    "MechMod: MechBenchInterface",
                    delegate
                    {
                        if (lastUpdateUiGameTime != null && MechBenchInterface?.CurrentState != null)
                        {
                            MechBenchInterface.Draw(Main.spriteBatch, lastUpdateUiGameTime);
                        }
                        return true;
                    },
                    InterfaceScaleType.UI));
            }
        }

        internal void ShowMyUI()
        {
            MechBenchUI.OnPlayerUse();
            MechBenchInterface?.SetState(MechBenchUI);
        }

        internal void HideMyUI()
        {
            MechBenchInterface?.SetState(null);
        }
    }
}
