using MechMod.Content.Buffs;
using MechMod.Content.Items.MechMisc;
using MechMod.Content.Mounts;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace MechMod.Common.Players
{
    /// <summary>
    /// Stores general player-related data and specific player-related logic.
    /// <para/> Things like equipped Parts and upgrades are stored here to be accessible through the code but to also be synced between clients when the mod is used in multiplayer.
    /// <para/> It also contains logic for things like disabling mounts under certain conditions, saving the player when they die in the mech, and disabling certain player effects and buffs/debuffs when the mech is active.
    /// </summary>

    public class MechModPlayer : ModPlayer
    {
        public bool disableMounts; // Flag to control whether mounts are disabled

        /// Parts and upgrades
        public Item[] equippedParts;
        public int upgradeLevel;
        public float upgradeDamageBonus;
        public bool powerCellActive;

        /// Part effects
        public int lifeBonus;
        public int armourBonus;

        public float[] partEffectiveness = new float[9]; // Array to store the effectiveness of each Part (changed by Body Parts)

        // Seperate variables for the mount's jump and run speeds for ground and flight states
        public float groundJumpSpeed = 0f;
        public float groundHorizontalSpeed = 0f;
        public float flightJumpSpeed = 0f;
        public float flightHorizontalSpeed = 0f;

        /// General Mech variables
        public bool allowDown; // Controls whether the player can hold down to hover with Booster Parts
        public int mechDebuffDuration; // Duration of the Mech Debuff applied when dismounting the Mech
        public int launchForce; // Force applied to the player when dismounting the Mech

        /// Trackers
        public bool grantedLifeBonus; // Tracks if the player has received the life bonus given from Parts
        public float maxLife; // Saves the maximum life of players for the debuff duration calculation

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

        // Function to send out changes to server and other clients as a packet that gets collected in MechMod.HandlePacket
        public override void SyncPlayer(int toWho, int fromWho, bool newPlayer)
        {
            ModPacket packet = Mod.GetPacket();
            packet.Write((byte)MechMod.MessageType.PartsSync);
            packet.Write((byte)Player.whoAmI);
            packet.Write(powerCellActive);
            for (int i = 0; i < equippedParts.Length; i++)
                packet.Write(equippedParts[i].type);

            packet.Send(toWho, fromWho);
        }

        // Function that receives changes from server and other clients from packets that get received in MechMod.HandlePacket
        public void RecievePlayerSync(BinaryReader reader)
        {
            powerCellActive = reader.ReadBoolean();
            for (int i = 0; i < equippedParts.Length; i++)
                equippedParts[i].SetDefaults(reader.ReadInt32());
        }

        // These functions work together to detect changes in the player data and sync them between clients,
        // removing the need to manually call SyncPlayer whenever a change in data/variables is made

        // Function that copies other client's data to then be checked against this client in SendClientChanges
        public override void CopyClientState(ModPlayer targetCopy)
        {
            var clone = (MechModPlayer)targetCopy;

            clone.powerCellActive = powerCellActive;
            for (int i = 0; i < equippedParts.Length; i++)
                clone.equippedParts[i].type = equippedParts[i].type;
        }

        // Function that runs if CopyClientState detects a change, syncing the changes between clients
        public override void SendClientChanges(ModPlayer clientPlayer)
        {
            var clone = (MechModPlayer)clientPlayer;
            bool syncPlayer = false;
            if (powerCellActive != clone.powerCellActive)
            {
                syncPlayer = true;
            }
            for (int i = 0; i < equippedParts.Length; i++)
            {
                if (equippedParts[i].type != clone.equippedParts[i].type)
                {
                    syncPlayer = true;
                }
            }

            if (syncPlayer) // Only sync clients if needed
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
                SoundStyle MetalPipe = new("MechMod/Content/Assets/Sounds/MetalPipe", SoundType.Sound) // Get the custom metal pipe sound
                {
                    Volume = 0.25f, // Lower the volume (it's fucking loud)
                };
                if (Main.rand.NextBool(500)) // 1 in 500 chance
                    SoundEngine.PlaySound(MetalPipe, Player.position); // Play metal pipe sound the mech is hurt
                else
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
        }

        public override bool CanUseItem(Item item)
        {
            if (disableMounts) // If mounts are disabled,
            {
                return false; // Prevent the use of the item (Mech Spawner)
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

        //    float slotSize = 43.5f;
        //    float slotGap = 4f;
        //    float startX = 20.5f;
        //    float startY = 20;

        //    for (int row = 0; row < 5; row++)
        //    {
        //        for (int col = 0; col < 10; col++)
        //        {
        //            int index = row * 10 + col;
        //            float x = startX + col * (slotSize + slotGap);
        //            float y = startY + row * (slotSize + slotGap);
        //            Rectangle slotRect = new((int)x, (int)y, (int)slotSize, (int)slotSize);

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
