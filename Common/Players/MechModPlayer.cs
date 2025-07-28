using MechMod.Common.UI;
using MechMod.Content.Buffs;
using MechMod.Content.Items.MechMisc;
using MechMod.Content.Mounts;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Linq;
using Steamworks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.Chat;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.Utilities.Terraria.Utilities;

namespace MechMod.Common.Players
{
    public class MechModPlayer : ModPlayer
    {
        public bool disableMounts; // Flag to control whether mounts are disabled

        public Item[] equippedParts;
        public int upgradeLevel;
        public float upgradeDamageBonus;
        public bool powerCellActive = false;

        public float animationTimer; // Timer for mech weapon animation logic (constantly ticks down)
        public int animationProgress; // Progress for mech weapon animation logic (needs to be manually incremented and decremented)

        public int lastUseDirection; // Stores the last weapon use direction

        // Variables beyond here used to be in ModMount for the Mech, but were changed over to ModPlayer so the variables can be unique per player while ModMount's variables are the same for every player
        // CHECK WHAT STUFF NEEDS TO BE HERE OR IS STATIC AND CAN REMAIN IN ModMount

        public Texture2D headTexture;
        public Texture2D bodyTexture;
        public Texture2D armsTexture;
        public Texture2D legsTexture;
        public Texture2D weaponTexture; // Used so the equipped weapon can be drawn while in use

        public bool animateOnce = false;

        public int armFrame = -1; // Used for controlling the current arm frame
        public int armAnimationFrames = 11; // Total number of frames that the arm texture has (to include the many arm rotations/positions for weapon animation)

        public Vector2 weaponPosition = Vector2.Zero; // Used for positioning the weapon when it is drawn
        public float weaponRotation = 0f; // Used for rotating the weapon when it is drawn
        public Vector2 weaponOrigin = Vector2.Zero; // Used so a different origin can be set for rotation
        public float weaponScale = 1f; // Used so the weapon can be hidden when needed
        public SpriteEffects weaponSpriteEffects = SpriteEffects.None; // Used so the weapon's sprite can be flipped when needed


        public override void Initialize()
        {
            equippedParts = new Item[9];
            for (int i = 0; i < equippedParts.Length; i++)
                equippedParts[i] = new Item();
            upgradeLevel = 0;
            upgradeDamageBonus = 0;
        }

        public override void SaveData(TagCompound tag)
        {
            if (!equippedParts[MechMod.headIndex].IsAir)
                tag["equippedHead"] = ItemIO.Save(equippedParts[MechMod.headIndex]);
            else
                equippedParts[MechMod.headIndex] = new Item();
            if (!equippedParts[MechMod.bodyIndex].IsAir)
                tag["equippedBody"] = ItemIO.Save(equippedParts[MechMod.bodyIndex]);
            else
                equippedParts[MechMod.bodyIndex] = new Item();
            if (!equippedParts[MechMod.armsIndex].IsAir)
                tag["equippedArms"] = ItemIO.Save(equippedParts[MechMod.armsIndex]);
            else
                equippedParts[MechMod.armsIndex] = new Item();
            if (!equippedParts[MechMod.legsIndex].IsAir)
                tag["equippedLegs"] = ItemIO.Save(equippedParts[MechMod.legsIndex]);
            else
                equippedParts[MechMod.legsIndex] = new Item();
            if (!equippedParts[MechMod.boosterIndex].IsAir)
                tag["equippedBooster"] = ItemIO.Save(equippedParts[MechMod.boosterIndex]);
            else
                equippedParts[MechMod.boosterIndex] = new Item();
            if (!equippedParts[MechMod.weaponIndex].IsAir)
                tag["equippedWeapon"] = ItemIO.Save(equippedParts[MechMod.weaponIndex]);
            else
                equippedParts[MechMod.weaponIndex] = new Item();
            if (!equippedParts[MechMod.passivemodule1Index].IsAir)
                tag["equippedPassiveModule1"] = ItemIO.Save(equippedParts[MechMod.passivemodule1Index]);
            else
                equippedParts[MechMod.passivemodule1Index] = new Item();
            if (!equippedParts[MechMod.passivemodule2Index].IsAir)
                tag["equippedPassiveModule2"] = ItemIO.Save(equippedParts[MechMod.passivemodule2Index]);
            else
                equippedParts[MechMod.passivemodule2Index] = new Item();
            if (!equippedParts[MechMod.activemoduleIndex].IsAir)
                tag["equippedActiveModule"] = ItemIO.Save(equippedParts[MechMod.activemoduleIndex]);
            else
                equippedParts[MechMod.activemoduleIndex] = new Item();

            tag["upgradeLevel"] = upgradeLevel;
            tag["upgradeDamageBonus"] = upgradeDamageBonus;
            tag["powerCellActive"] = powerCellActive;
        }

        public override void LoadData(TagCompound tag)
        {
            if (tag.ContainsKey("equippedHead"))
                equippedParts[MechMod.headIndex] = ItemIO.Load(tag.GetCompound("equippedHead"));
            else
                equippedParts[MechMod.headIndex] = new Item();
            if (tag.ContainsKey("equippedBody"))
                equippedParts[MechMod.bodyIndex] = ItemIO.Load(tag.GetCompound("equippedBody"));
            else
                equippedParts[MechMod.bodyIndex] = new Item();
            if (tag.ContainsKey("equippedArms"))
                equippedParts[MechMod.armsIndex] = ItemIO.Load(tag.GetCompound("equippedArms"));
            else
                equippedParts[MechMod.armsIndex] = new Item();
            if (tag.ContainsKey("equippedLegs"))
                equippedParts[MechMod.legsIndex] = ItemIO.Load(tag.GetCompound("equippedLegs"));
            else
                equippedParts[MechMod.legsIndex] = new Item();
            if (tag.ContainsKey("equippedBooster"))
                equippedParts[MechMod.boosterIndex] = ItemIO.Load(tag.GetCompound("equippedBooster"));
            else
                equippedParts[MechMod.boosterIndex] = new Item();
            if (tag.ContainsKey("equippedWeapon"))
                equippedParts[MechMod.weaponIndex] = ItemIO.Load(tag.GetCompound("equippedWeapon"));
            else
                equippedParts[MechMod.weaponIndex] = new Item();
            if (tag.ContainsKey("equippedPassiveModule1"))
                equippedParts[MechMod.passivemodule1Index] = ItemIO.Load(tag.GetCompound("equippedPassiveModule1"));
            else
                equippedParts[MechMod.passivemodule1Index] = new Item();
            if (tag.ContainsKey("equippedPassiveModule2"))
                equippedParts[MechMod.passivemodule2Index] = ItemIO.Load(tag.GetCompound("equippedPassiveModule2"));
            else
                equippedParts[MechMod.passivemodule2Index] = new Item();
            if (tag.ContainsKey("equippedActiveModule"))
                equippedParts[MechMod.activemoduleIndex] = ItemIO.Load(tag.GetCompound("equippedActiveModule"));
            else
                equippedParts[MechMod.activemoduleIndex] = new Item();

            if (tag.ContainsKey("upgradeLevel"))
                upgradeLevel = tag.GetInt("upgradeLevel");
            if (tag.ContainsKey("upgradeDamageBonus"))
                upgradeDamageBonus = tag.GetFloat("upgradeDamageBonus");
            if (tag.ContainsKey("powerCellActive"))
                powerCellActive = tag.GetBool("powerCellActive");
        }

        // Send out changes to server and other clients
        public override void SyncPlayer(int toWho, int fromWho, bool newPlayer)
        {
            ModPacket packet = Mod.GetPacket();
            packet.Write((byte)MechMod.MessageType.EquippedPartsAndLevelSync);
            packet.Write((byte)Player.whoAmI);
            for (int i = 0; i < equippedParts.Length; i++)
                packet.Write(equippedParts[i].type);

            // Add animation state
            packet.Write(animationTimer);
            packet.Write(animationProgress);
            packet.Write(lastUseDirection);
            packet.Write(armFrame);
            packet.Write(weaponPosition.X);
            packet.Write(weaponPosition.Y);
            packet.Write(weaponRotation);
            packet.Write(weaponOrigin.X);
            packet.Write(weaponOrigin.Y);
            packet.Write(weaponScale);
            packet.Write((int)weaponSpriteEffects);
            // Add more as needed (weaponRotation, weaponScale, etc.)

            packet.Send(toWho, fromWho);
        }

        // Receive changes from server and other clients
        public void RecievePlayerSync(BinaryReader reader)
        {
            for (int i = 0; i < equippedParts.Length; i++)
                equippedParts[i].SetDefaults(reader.ReadInt32());

            headTexture = Mod.Assets.Request<Texture2D>($"Content/Items/MechHeads/{equippedParts[MechMod.headIndex].ModItem.GetType().Name}Visual").Value;
            bodyTexture = Mod.Assets.Request<Texture2D>($"Content/Items/MechBodies/{equippedParts[MechMod.bodyIndex].ModItem.GetType().Name}Visual").Value;
            armsTexture = Mod.Assets.Request<Texture2D>($"Content/Items/MechArms/{equippedParts[MechMod.armsIndex].ModItem.GetType().Name}Visual").Value;
            legsTexture = Mod.Assets.Request<Texture2D>($"Content/Items/MechLegs/{equippedParts[MechMod.legsIndex].ModItem.GetType().Name}Visual").Value;
            weaponTexture = Mod.Assets.Request<Texture2D>($"Content/Items/MechWeapons/{equippedParts[MechMod.weaponIndex].ModItem.GetType().Name}").Value;
            // Read animation state
            animationTimer = reader.ReadSingle();
            animationProgress = reader.ReadInt32();
            lastUseDirection = reader.ReadInt32();
            armFrame = reader.ReadInt32();
            //weaponPosition = reader.ReadVector2();
            weaponPosition.X = reader.ReadInt32();
            weaponPosition.Y = reader.ReadInt32();
            weaponRotation = reader.ReadSingle();
            //weaponOrigin = reader.ReadVector2();
            weaponOrigin.X = reader.ReadInt32();
            weaponOrigin.Y = reader.ReadInt32();
            weaponScale = reader.ReadSingle();
            weaponSpriteEffects = (SpriteEffects)reader.ReadInt32();
            // Read more as needed
        }

        public override void CopyClientState(ModPlayer targetCopy)
        {
            var clone = (MechModPlayer)targetCopy;

            // Check clients against this client to watch for changes
            // If a change is detected, SendClientChanges will sync the changes

            for (int i = 0; i < equippedParts.Length; i++)
                clone.equippedParts[i].type = equippedParts[i].type;

            clone.headTexture = headTexture;
            clone.bodyTexture = bodyTexture;
            clone.armsTexture = armsTexture;
            clone.legsTexture = legsTexture;
            clone.weaponTexture = weaponTexture;
            // Copy animation state
            clone.animationTimer = animationTimer;
            clone.animationProgress = animationProgress;
            clone.lastUseDirection = lastUseDirection;
            clone.armFrame = armFrame;
            clone.weaponPosition = weaponPosition;
            clone.weaponRotation = weaponRotation;
            clone.weaponOrigin = weaponOrigin;
            clone.weaponScale = weaponScale;
            clone.weaponSpriteEffects = weaponSpriteEffects;
            // Copy more as needed
        }
        public override void SendClientChanges(ModPlayer clientPlayer)
        {
            var clone = (MechModPlayer)clientPlayer;
            bool needsSync = false;

            for (int i = 0; i < equippedParts.Length; i++)
            {
                if (equippedParts[i].type != clone.equippedParts[i].type)
                {
                    needsSync = true;
                    break;
                }
            }

            // Check animation state
            if (headTexture != clone.headTexture ||
                bodyTexture != clone.bodyTexture ||
                armsTexture != clone.armsTexture ||
                legsTexture != clone.legsTexture ||
                weaponTexture != clone.weaponTexture ||
                animationTimer != clone.animationTimer ||
                animationProgress != clone.animationProgress ||
                lastUseDirection != clone.lastUseDirection ||
                armFrame != clone.armFrame ||
                weaponPosition != clone.weaponPosition ||
                weaponRotation != clone.weaponRotation ||
                weaponOrigin != clone.weaponOrigin ||
                weaponScale != clone.weaponScale ||
                weaponSpriteEffects != clone.weaponSpriteEffects
                )
            {
                needsSync = true;
            }

            if (needsSync) // Only sync clients if a variable changes
                SyncPlayer(-1, Main.myPlayer, false);
        }

        public override void ResetEffects()
        {
            disableMounts = false; // Reset the flag when effects are recalculated
        }

        public override void UpdateDead()
        {
            disableMounts = false; // Reset the flag when the player dies
        }

        public override void OnHurt(Player.HurtInfo info)
        {
            if (Player.mount.Active && Player.mount.Type == ModContent.MountType<ModularMech>())
            {
                if (info.Damage >= Player.statLife || Player.statLife < 1)
                {
                    float PlayerHealth = Player.statLifeMax;
                    Player.mount.Dismount(Player);
                    Player.statLife = (int)(PlayerHealth *= 0.5f);
                    Player.immuneTime = 60;
                }
            }
        }

        public override void UpdateEquips()
        {
            if (Player.mount.Active && Player.mount.Type == ModContent.MountType<ModularMech>())
            {
                DisablePlayerEffects();
            }
        }

        // These disable the Player's Buffs, specific Debuffs, and any visual effects (For example, set bonus visual effects)
        #region Disabling Player Effects

        public void DisablePlayerEffects()
        {
            // Debuffs
            // Removed to make the Mech more realistic and disabling effects that dismount the Player
            // General
            Player.noKnockback = true;
            Player.poisoned = false;
            Player.bleed = false;
            Player.silence = false;
            Player.moonLeech = false;
            Player.rabid = false;
            Player.burned = false;
            Player.windPushed = false;

            // Dismounts
            Player.stoned = false;
            Player.cursed = false;
            Player.tongued = false;
            Player.frozen = false;

            // Buffs
            int mountBuffID = ModContent.BuffType<MechBuff>();

            for (int i = 0; i < Player.buffType.Length; i++)
            {
                int buffType = Player.buffType[i];

                if (buffType != 0 && buffType != mountBuffID && !Main.debuff[buffType])
                {
                    Player.DelBuff(i);
                }
            }
        }

        // Visual Effects
        public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo)
        {
            if (Player.mount.Active && Player.mount.Type == ModContent.MountType<ModularMech>())
            {
                drawInfo.drawPlayer.head = 0;
                drawInfo.drawPlayer.body = 0;
                drawInfo.drawPlayer.legs = 0;

                if (lastUseDirection != 0)
                    drawInfo.drawPlayer.direction = lastUseDirection; // Force player's direction to be the last use direction
            }
        }

        #endregion

        public override void PreUpdate()
        {
            MechSpawner mechSpawnerItem = ModContent.GetInstance<MechSpawner>();
            // Check for your debuff and set the disableMounts flag accordingly
            if (Player.HasBuff(ModContent.BuffType<MechDebuff>()))
            {
                disableMounts = true;
                CanUseItem(mechSpawnerItem.Item);
            }
            if (equippedParts[MechMod.headIndex].IsAir ||
                equippedParts[MechMod.bodyIndex].IsAir ||
                equippedParts[MechMod.armsIndex].IsAir ||
                equippedParts[MechMod.legsIndex].IsAir)
            {
                disableMounts = true;
                CanUseItem(mechSpawnerItem.Item);
            }

            if (animationTimer > 0)
                animationTimer--;
        }

        public override bool CanUseItem(Item item)
        {
            if (disableMounts)
            {
                return false;
            }
            return true;
        }

        //public override void DrawEffects(PlayerDrawSet drawInfo, ref float r, ref float g, ref float b, ref float a, ref bool fullBright)
        //{
        //    Rectangle[] boxes =
        //        [
        //            new Rectangle( // Head
        //                (int)(Player.position.X + (Player.direction == -1 ? -2 : -10) - Main.screenPosition.X),
        //                (int)(Player.position.Y - 41 - Main.screenPosition.Y),
        //                32,
        //                36
        //            ),
        //            new Rectangle( // Body
        //                (int)(Player.position.X + (Player.direction == -1 ? -18 : -28) - Main.screenPosition.X),
        //                (int)(Player.position.Y - 12 - Main.screenPosition.Y),
        //                66,
        //                53
        //            ),
        //            new Rectangle( // Legs
        //                (int)(Player.position.X + (Player.direction == -1 ? -8 : -16) - Main.screenPosition.X),
        //                (int)(Player.position.Y + 32 - Main.screenPosition.Y),
        //                44,
        //                53
        //            )
        //        ];

        //    foreach (Rectangle box in boxes)
        //    {
        //        DrawDebugRectangle(box, Color.Red);
        //    }
        //}

        //private void DrawDebugRectangle(Rectangle rect, Color color)
        //{
        //    // Use a 1x1 white pixel texture (built-in)
        //    Texture2D pixel = Terraria.GameContent.TextureAssets.MagicPixel.Value;
        //    // Top
        //    Main.spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Y, rect.Width, 2), color);
        //    // Bottom
        //    Main.spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Y + rect.Height - 2, rect.Width, 2), color);
        //    // Left
        //    Main.spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Y, 2, rect.Height), color);
        //    // Right
        //    Main.spriteBatch.Draw(pixel, new Rectangle(rect.X + rect.Width - 2, rect.Y, 2, rect.Height), color);
        //}
    }
}
