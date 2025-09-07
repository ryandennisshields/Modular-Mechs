using MechMod.Content.Buffs;
using MechMod.Content.Items.MechMisc;
using MechMod.Content.Mounts;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace MechMod.Common.Players
{
    /// <summary>
    /// Stores player-related data and specific player-related logic.
    /// <para/> Things like equipped Parts, upgrades, and visual information are stored here to be accessible through many Parts of the code but to also be synced between clients when the mod is used in multiplayer.
    /// <para/> It also contains logic for things like disabling mounts under certain conditions, saving the player when they die in the mech, and disabling certain player effects and buffs/debuffs when the mech is active.
    /// </summary>

    public class MechModPlayer : ModPlayer
    {
        public bool disableMounts; // Flag to control whether mounts are disabled

        public Item[] equippedParts;
        public int upgradeLevel;
        public float upgradeDamageBonus;
        public bool powerCellActive = false;

        public float[] partEffectiveness = new float[9];

        public int lifeBonus;
        public int armourBonus;

        public float animationTimer; // Timer for mech weapon animation logic (constantly ticks down)
        public int animationProgress; // Progress for mech weapon animation logic (needs to be manually incremented and decremented)

        public Asset<Texture2D> headTexture;
        public Asset<Texture2D> bodyTexture;
        public Asset<Texture2D> armsRTexture;
        public Asset<Texture2D> armsLTexture;
        public Asset<Texture2D> legsRTexture;
        public Asset<Texture2D> legsLTexture;
        public Asset<Texture2D> weaponTexture; // Used so the equipped weapon can be drawn while in use

        public Vector2[] bodyOffsets = new Vector2[5];

        public bool animateOnce = false; // Used to control whether the mech weapon animation should only play once or loop

        // Used for controlling the current arm frame
        public int armRFrame = -1;
        public int armLFrame = -1;
        // Total number of frames that the arm texture has (to include the many arm rotations/positions for weapon animation)
        public int armRAnimationFrames = 10;
        public int armLAnimationFrames = 14;

        public int useDirection; // Stores the last weapon use direction

        public Vector2 weaponPosition = Vector2.Zero; // Used for positioning the weapon when it is drawn
        public float weaponRotation = 0f; // Used for rotating the weapon when it is drawn
        public Vector2 weaponOrigin = Vector2.Zero; // Used so a different origin can be set for rotation
        public float weaponScale = 1f; // Used so the weapon can be hidden when needed
        public SpriteEffects weaponSpriteEffects = SpriteEffects.None; // Used so the weapon's sprite can be flipped when needed


        public override void Initialize()
        {
            // Fill equipped Parts with empty items while intialising
            equippedParts = new Item[9];
            for (int i = 0; i < equippedParts.Length; i++)
                equippedParts[i] = new Item();
        }

        #region Saving and Loading Data

        public override void SaveData(TagCompound tag)
        {
            // Save equipped Parts, upgrade level, damage bonus, and power cell state
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
            // Load equipped Parts, upgrade level, damage bonus, and power cell state
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

        #endregion

        #region Syncing Player Data (Networking)

        // Send out changes to server and other clients
        public override void SyncPlayer(int toWho, int fromWho, bool newPlayer)
        {
            // Sync up the equipped Parts and animation details between clients
            ModPacket packet = Mod.GetPacket();
            packet.Write((byte)MechMod.MessageType.EquippedPartsAndLevelSync);
            packet.Write((byte)Player.whoAmI);
            for (int i = 0; i < equippedParts.Length; i++)
                packet.Write(equippedParts[i].type);
            packet.Write(animationTimer);
            packet.Write(animationProgress);
            packet.Write(useDirection);
            packet.Write(armRFrame);
            packet.Write(armLFrame);
            packet.Write(weaponPosition.X);
            packet.Write(weaponPosition.Y);
            packet.Write(weaponRotation);
            packet.Write(weaponOrigin.X);
            packet.Write(weaponOrigin.Y);
            packet.Write(weaponScale);
            packet.Write((int)weaponSpriteEffects);

            packet.Send(toWho, fromWho);
        }

        // Receive changes from server and other clients
        public void RecievePlayerSync(BinaryReader reader)
        {
            for (int i = 0; i < equippedParts.Length; i++)
                equippedParts[i].SetDefaults(reader.ReadInt32());
            headTexture = Mod.Assets.Request<Texture2D>($"Content/Items/MechHeads/{equippedParts[MechMod.headIndex].ModItem.GetType().Name}Visual");
            bodyTexture = Mod.Assets.Request<Texture2D>($"Content/Items/MechBodies/{equippedParts[MechMod.bodyIndex].ModItem.GetType().Name}Visual");
            armsRTexture = Mod.Assets.Request<Texture2D>($"Content/Items/MechArms/{equippedParts[MechMod.armsIndex].ModItem.GetType().Name}RVisual");
            armsLTexture = Mod.Assets.Request<Texture2D>($"Content/Items/MechArms/{equippedParts[MechMod.armsIndex].ModItem.GetType().Name}LVisual");
            legsRTexture = Mod.Assets.Request<Texture2D>($"Content/Items/MechLegs/{equippedParts[MechMod.legsIndex].ModItem.GetType().Name}RVisual");
            legsLTexture = Mod.Assets.Request<Texture2D>($"Content/Items/MechLegs/{equippedParts[MechMod.armsIndex].ModItem.GetType().Name}LVisual");
            weaponTexture = Mod.Assets.Request<Texture2D>($"Content/Items/MechWeapons/{equippedParts[MechMod.weaponIndex].ModItem.GetType().Name}");
            animationTimer = reader.ReadSingle();
            animationProgress = reader.ReadInt32();
            useDirection = reader.ReadInt32();
            armRFrame = reader.ReadInt32();
            armLFrame = reader.ReadInt32();
            weaponPosition.X = reader.ReadInt32();
            weaponPosition.Y = reader.ReadInt32();
            weaponRotation = reader.ReadSingle();
            weaponOrigin.X = reader.ReadInt32();
            weaponOrigin.Y = reader.ReadInt32();
            weaponScale = reader.ReadSingle();
            weaponSpriteEffects = (SpriteEffects)reader.ReadInt32();
        }

        // Check clients against this client to watch for changes
        // If a change is detected, SendClientChanges will sync the changes
        public override void CopyClientState(ModPlayer targetCopy)
        {
            var clone = (MechModPlayer)targetCopy;

            for (int i = 0; i < equippedParts.Length; i++)
                clone.equippedParts[i].type = equippedParts[i].type;
            clone.headTexture = headTexture;
            clone.bodyTexture = bodyTexture;
            clone.armsRTexture = armsRTexture;
            clone.armsLTexture = armsLTexture;
            clone.legsRTexture = legsRTexture;
            clone.legsLTexture = legsLTexture;
            clone.weaponTexture = weaponTexture;
            clone.animationTimer = animationTimer;
            clone.animationProgress = animationProgress;
            clone.useDirection = useDirection;
            clone.armRFrame = armRFrame;
            clone.armLFrame = armLFrame;
            clone.weaponPosition = weaponPosition;
            clone.weaponRotation = weaponRotation;
            clone.weaponOrigin = weaponOrigin;
            clone.weaponScale = weaponScale;
            clone.weaponSpriteEffects = weaponSpriteEffects;
        }

        // If CopyClientState detects a change, this method will be called to sync the changes
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
            if (headTexture != clone.headTexture ||
                bodyTexture != clone.bodyTexture ||
                armsRTexture != clone.armsRTexture ||
                armsLTexture != clone.armsLTexture ||
                legsRTexture != clone.legsRTexture ||
                legsLTexture != clone.legsLTexture ||
                weaponTexture != clone.weaponTexture ||
                animationTimer != clone.animationTimer ||
                animationProgress != clone.animationProgress ||
                useDirection != clone.useDirection ||
                armRFrame != clone.armRFrame ||
                armLFrame != clone.armLFrame ||
                weaponPosition != clone.weaponPosition ||
                weaponRotation != clone.weaponRotation ||
                weaponOrigin != clone.weaponOrigin ||
                weaponScale != clone.weaponScale ||
                weaponSpriteEffects != clone.weaponSpriteEffects
                )
            {
                needsSync = true;
            }

            if (needsSync) // Only sync clients if needed
                SyncPlayer(-1, Main.myPlayer, false);
        }

        #endregion

        public override void ResetEffects()
        {
            disableMounts = false; // Reset disabling mounts when effects are recalculated
        }

        public override void UpdateDead()
        {
            disableMounts = false; // Reset disabling mounts when the player dies
        }

        public override void OnHurt(Player.HurtInfo info)
        {
            if (Player.mount.Active && Player.mount.Type == ModContent.MountType<ModularMech>())
            {
                SoundEngine.PlaySound(SoundID.NPCHit4, Player.position); // Play metal hit sound when the mech is hurt
                // Save the player if they die in the mech
                if (info.Damage >= Player.statLife || Player.statLife < 1) // If the player dies in the mech,
                {
                    Player.mount.Dismount(Player); // Dismount the player
                    float PlayerHealth = Player.statLifeMax; // Get the player's max health
                    Player.statLife = (int)(PlayerHealth *= 0.5f); // Set the player's health to 50% of their max health
                    Player.immuneTime = 80; // Give the player 1.33 seconds of invincibility
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

        // These disable the Player's Set Bonus, specific Debuffs, and any visual effects (For example, set bonus visual effects)
        public void DisablePlayerEffects()
        {
            // Armor Set Bonuses
            Player.head = 0;
            Player.body = 0;
            Player.legs = 0;

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
        }

        public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo)
        {
            if (Player.mount.Active && Player.mount.Type == ModContent.MountType<ModularMech>())
            {
                //if (useDirection != 0)
                //    Player.direction = useDirection; // Force player's direction to be the last use direction
            }
        }

        public override void PreUpdate()
        {
            // Disable spawning the mech in different cases, like having the Mech debuff or not having certain equipped Parts
            MechSpawner mechSpawnerItem = ModContent.GetInstance<MechSpawner>();
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
                animationTimer--; // Count down the animation timer
        }

        // Disable the use of the Mech spawner if mounts are disabled
        public override bool CanUseItem(Item item)
        {
            if (disableMounts)
            {
                return false;
            }
            return true;
        }

        #region Debugging Drawing

        //public override void DrawEffects(PlayerDrawSet drawInfo, ref float r, ref float g, ref float b, ref float a, ref bool fullBright)
        //{
        //    /// Booster Dust hiding area rectangle
        //    Rectangle box = new Rectangle(
        //                (int)(Player.position.X - (Player.direction == -1 ? 22 : 30) - Main.screenPosition.X),
        //                (int)(Player.position.Y - 29 - Main.screenPosition.Y),
        //                72,
        //                106
        //                );
        //    DrawDebugRectangle(box, Color.Red);

        //    /// Inventory Slot rectangles
        //    float slotSize = 75f * Main.inventoryScale;
        //    float slotGap = 4f * Main.inventoryScale;
        //    float startX = 20.5f;
        //    float startY = 20;

        //    for (int row = 0; row < 5; row++)
        //    {
        //        for (int col = 0; col < 10; col++)
        //        {
        //            int index = row * 10 + col;
        //            float x = startX + col * (slotSize + slotGap);
        //            float y = startY + row * (slotSize + slotGap);
        //            Rectangle slotRect = new Rectangle((int)x, (int)y, (int)slotSize, (int)slotSize);

        //            Color color = slotRect.Contains(Main.mouseX, Main.mouseY) ? Color.Red : Color.White;
        //            Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, slotRect, color * 0.5f);
        //        }
        //    }
        //}

        //private void DrawDebugRectangle(Rectangle rect, Color color)
        //{
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

        #endregion
    }
}
