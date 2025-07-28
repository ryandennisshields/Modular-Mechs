using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MechMod.Content.Dusts
{
    public class BoosterDust : ModDust
    {

        // This class is for a custom Dust effect for the Mech's booster, with code to make it look like the dust is behind the Mech.

        public override string Texture => null;

        public override void OnSpawn(Terraria.Dust dust)
        {
            // Use vanilla dust texture
            int desiredVanillaDustTexture = DustID.Flare;
            int frameX = desiredVanillaDustTexture * 10 % 1000;
            int frameY = desiredVanillaDustTexture * 10 / 1000 * 30 + Main.rand.Next(3) * 10;
            dust.frame = new Rectangle(frameX, frameY, 8, 8);

            dust.noGravity = true; // No gravity for dust
            dust.alpha = 255; // Fully transparent at spawn
        }

        public override bool Update(Terraria.Dust dust)
        {
            Lighting.AddLight(dust.position, 1f, 0.5f, 0f);

            if (dust.customData is Player player)
            {
                Rectangle[] boxes =
                [
                    new Rectangle( // Head
                        (int)(player.position.X + (player.direction == -1 ? -2 : -10)),
                        (int)(player.position.Y - 45),
                        32,
                        36
                    ),
                    new Rectangle( // Body
                        (int)(player.position.X + (player.direction == -1 ? -18 : -28)),
                        (int)(player.position.Y - 16),
                        66,
                        53
                    ),
                    new Rectangle( // Legs
                        (int)(player.position.X + (player.direction == -1 ? -8 : -16)),
                        (int)(player.position.Y + 28),
                        44,
                        53
                    )
                ];

                bool insideAnyBox = false;
                foreach (Rectangle box in boxes)
                {
                    if (box.Contains(dust.position.ToPoint()))
                    {
                        insideAnyBox = true;
                        break;
                    }
                }
                dust.alpha = insideAnyBox ? 255 : 0;
            }
            return true;
        }
    }
}
