using MechMod.Common.Players;
using MechMod.Content.Items.MechWeapons;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace MechMod.Common.Global
{
    /// <summary>
    /// Global NPC class used to handle custom debuff effects.
    /// </summary>

    internal class GlobalDebuffEffect : GlobalNPC
    {
        public override bool InstancePerEntity => true; // Ensure each NPC has its own instance of this class, preventing shared state between NPCs

        // Arbalest debuff effect variables
        public int arbalestFrame = 0; // Current frame of the texture
        private int arbalestMaxFrame = 3; // Max frames in the texture
        private static Asset<Texture2D> arbalestDebuffTexture = ModContent.Request<Texture2D>("MechMod/Content/Items/MechWeapons/ArbalestDebuffEffect");

        public override void AI(NPC npc)
        {
            if (npc.HasBuff(ModContent.BuffType<ArbalestDebuff>())) // If the NPC has the Arbalest debuff,
            {
                if (arbalestFrame == arbalestMaxFrame) // If on the last frame,
                {
                    Player player = Main.player[npc.lastInteraction];
                    MechWeaponsPlayer weaponsPlayer = Main.player[npc.lastInteraction].GetModPlayer<MechWeaponsPlayer>();

                    // Calculate explosion damage and knockback
                    int damage = weaponsPlayer.DamageCalc(70, player);
                    float knockback = weaponsPlayer.KnockbackCalc(7, player);

                    // Create explosion effect and delete debuff
                    int projectileType = ProjectileID.GrenadeIII; // Use grenade explosion for the explosion effect
                    Projectile.NewProjectile(new EntitySource_Parent(npc), npc.Center, Vector2.Zero, projectileType, damage, knockback, Main.myPlayer);

                    arbalestFrame = default; // Reset frame counter
                    npc.DelBuff(npc.FindBuffIndex(ModContent.BuffType<ArbalestDebuff>())); // Remove the debuff
                }
            }
            else if (arbalestFrame > 0) // If the NPC no longer has the debuff but the frame counter is above 0,
                arbalestFrame = default; // Reset frame counter
        }

        public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (npc.HasBuff(ModContent.BuffType<ArbalestDebuff>())) // If the NPC has the Arbalest debuff,
            {
                // Draw the debuff effect on the NPC
                Vector2 position = npc.Center - screenPos + new Vector2(0, npc.gfxOffY);
                int frameHeight = arbalestDebuffTexture.Height() / arbalestMaxFrame;
                Rectangle sourceRect = new(0, arbalestFrame * frameHeight, arbalestDebuffTexture.Width(), frameHeight);
                Vector2 origin = new(arbalestDebuffTexture.Width() / 2, frameHeight / 2);
                float scale = npc.scale * 2;
                spriteBatch.Draw(arbalestDebuffTexture.Value, position, sourceRect, Color.White, npc.rotation, origin, scale, SpriteEffects.None, 0f);
            }
        }
    }
}
