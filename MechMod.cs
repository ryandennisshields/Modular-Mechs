using MechMod.Common.Players;
using MechMod.Common.UI;
using MechMod.Content.Buffs;
using MechMod.Content.Items.MechArms;
using MechMod.Content.Items.MechBodies;
using MechMod.Content.Items.MechBoosters;
using MechMod.Content.Items.MechHeads;
using MechMod.Content.Items.MechLegs;
using MechMod.Content.Items.MechWeapons;
using MechMod.Content.Items.MechModules.Passive;
using MechMod.Content.Items.MechModules.Active;
using MechMod.Content.Mounts;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;
using static MechMod.Common.Players.MechModPlayer;
using System.IO;
using MechMod.Content.NPCs;

namespace MechMod
{
    public struct MechPart(int itemType, string partType)
    {
        public int ItemType = itemType;
        public string PartType = partType;
    }

    public class MechMod : Mod
    {
        public static Dictionary<string, MechPart> MechParts;

        public static int headIndex = 0;
        public static int bodyIndex = 1;
        public static int armsIndex = 2;
        public static int legsIndex = 3;
        public static int boosterIndex = 4;
        public static int weaponIndex = 5;
        public static int passivemodule1Index = 6;
        public static int passivemodule2Index = 7;
        public static int activemoduleIndex = 8;

        public static ModKeybind MechDashKeybind;
        public static ModKeybind MechActivateModule;

        public override void Load()
        {
            MechParts = new Dictionary<string, MechPart>
            {
                // Mech Parts
                // Base Parts
                { "basehead", new MechPart(ModContent.ItemType<BaseHead>(), "head")},
                { "basebody", new MechPart(ModContent.ItemType<BaseBody>(), "body")},
                { "basearms", new MechPart(ModContent.ItemType<BaseArms>(), "arms")},
                { "baselegs", new MechPart(ModContent.ItemType<BaseLegs>(), "legs")},
                { "basebooster", new MechPart(ModContent.ItemType<BaseBooster>(), "booster")},
                // Slow Parts
                { "slowhead", new MechPart(ModContent.ItemType<SlowHead>(), "head")},
                { "slowbody", new MechPart(ModContent.ItemType<SlowBody>(), "body")},
                { "slowarms", new MechPart(ModContent.ItemType<SlowArms>(), "arms")},
                { "slowlegs", new MechPart(ModContent.ItemType<SlowLegs>(), "legs")},
                { "slowbooster", new MechPart(ModContent.ItemType<SlowBooster>(), "booster")},
                // Fast Parts
                { "fasthead", new MechPart(ModContent.ItemType<FastHead>(), "head")},
                { "fastbody", new MechPart(ModContent.ItemType<FastBody>(), "body")},
                { "fastarms", new MechPart(ModContent.ItemType<FastArms>(), "arms")},
                { "fastlegs", new MechPart(ModContent.ItemType<FastLegs>(), "legs")},
                { "fastbooster", new MechPart(ModContent.ItemType<FastBooster>(), "booster")},

                // Weapons
                { "basegun", new MechPart(ModContent.ItemType<BaseGun>(), "weapon")},
                { "basesword", new MechPart(ModContent.ItemType<BaseSword>(), "weapon")},
                { "lasergun", new MechPart(ModContent.ItemType<LaserGun>(), "weapon")},
                { "projsword", new MechPart(ModContent.ItemType<ProjSword>(), "weapon")},
                { "baselauncher", new MechPart(ModContent.ItemType<BaseLauncher>(), "weapon")},

                // Modules
                // Passive
                { "spikes", new MechPart(ModContent.ItemType<Spikes>(), "passivemodule")},
                { "nucleareject", new MechPart(ModContent.ItemType<NuclearEject>(), "passivemodule")},
                { "hover", new MechPart(ModContent.ItemType<Hover>(), "passivemodule")},
                { "brace", new MechPart(ModContent.ItemType<Brace>(), "passivemodule")},
                // Active
                { "missilelauncher", new MechPart(ModContent.ItemType<MissileLauncher>(), "activemodule")},
                { "repair", new MechPart(ModContent.ItemType<Repair>(), "activemodule")},
            };

            MechDashKeybind = KeybindLoader.RegisterKeybind(this, "MechDash", "V");

            MechActivateModule = KeybindLoader.RegisterKeybind(this, "MechActivateModule", "C");
        }

        public override void Unload()
        {
            MechParts = null;
            MechDashKeybind = null;
        }

        internal enum MessageType : byte
        {
            EquippedPartsAndLevelSync,
        }

        public override void HandlePacket(BinaryReader reader, int whoAmI)
        {
            MessageType msgType = (MessageType)reader.ReadByte();

            byte playerNumber = reader.ReadByte();
            MechModPlayer modPlayer = Main.player[playerNumber].GetModPlayer<MechModPlayer>();

            switch (msgType)
            {
                case MessageType.EquippedPartsAndLevelSync:
                    //byte playerNumber = reader.ReadByte();
                    //MechModPlayer modPlayer = Main.player[playerNumber].GetModPlayer<MechModPlayer>();
                    modPlayer.RecievePlayerSync(reader);

                    if (Main.netMode == NetmodeID.Server)
                    {
                        modPlayer.SyncPlayer(-1, whoAmI, false);
                    }
                    break;
                    //case MessageType.WeaponAnimationSync:
                    //    int armFrame = reader.ReadInt32();
                    //    float animationTimer = reader.ReadSingle();
                    //    int animationProgress = reader.ReadInt32();
                    //    int useDirection = reader.ReadInt32();

                    //    modPlayer.armFrame = armFrame;
                    //    modPlayer.animationTimer = animationTimer;
                    //    modPlayer.animationProgress = animationProgress;
                    //    modPlayer.useDirection = useDirection;

                    //    if (Main.netMode == NetmodeID.Server)
                    //    {
                    //        ModPacket packet = GetPacket();
                    //        packet.Write((byte)MessageType.WeaponAnimationSync);
                    //        packet.Write((byte)playerNumber);
                    //        packet.Write(armFrame);
                    //        packet.Write(animationTimer);
                    //        packet.Write(animationProgress);
                    //        packet.Write(useDirection);
                    //        packet.Send(-1, playerNumber);
                    //    }
                    //    break;
            }
        }

        #region Commands

        //public class EquipCommand : ModCommand
        //{
        //    public override CommandType Type => CommandType.Chat;

        //    public override string Command => "equipPart";

        //    public override string Usage => "/equipPart <itemName>";

        //    public override string Description => "Equips the specified Part to the mech.";

        //    public override void Action(CommandCaller caller, string input, string[] args)
        //    {
        //        if (args.Length != 1)
        //        {
        //            caller.Reply("Usage: /equipPart <itemName>");
        //            return;
        //        }

        //        string partName = args[0].ToLowerInvariant();
        //        Player player = caller.Player;
        //        MechModPlayer mechPlayer = player.GetModPlayer<MechModPlayer>();

        //        if (MechMod.MechParts.TryGetValue(partName, out MechPart mechPart))
        //        {
        //            Item item = new Item();
        //            item.SetDefaults(mechPart.ItemType);

        //            switch (mechPart.PartType)
        //            {
        //                case "Head":
        //                    mechPlayer.equippedParts[MechMod.headIndex] = item;
        //                    break;
        //                case "Body":
        //                    mechPlayer.equippedParts[MechMod.bodyIndex] = item;
        //                    break;
        //                case "Arms":
        //                    mechPlayer.equippedParts[MechMod.armsIndex] = item;
        //                    break;
        //                case "Legs":
        //                    mechPlayer.equippedParts[MechMod.legsIndex] = item;
        //                    break;
        //                case "Booster":
        //                    mechPlayer.equippedParts[MechMod.boosterIndex] = item;
        //                    break;
        //                case "Weapon":
        //                    mechPlayer.equippedParts[MechMod.weaponIndex] = item;
        //                    break;
        //                case "Passive Module":
        //                    if (mechPlayer.equippedParts[MechMod.passivemodule1Index].IsAir)
        //                        mechPlayer.equippedParts[MechMod.passivemodule1Index] = item;
        //                    else if (mechPlayer.equippedParts[MechMod.passivemodule2Index].IsAir)
        //                        mechPlayer.equippedParts[MechMod.passivemodule2Index] = item;
        //                    else
        //                        caller.Reply("Both passive module slots are occupied. Please unequip a module (Passive Module 1 or Passive Module 2) first.");
        //                    break;
        //                case "Active Module":
        //                    mechPlayer.equippedParts[MechMod.activemoduleIndex] = item;
        //                    break;
        //            }
        //            caller.Reply($"Equipped {partName} to the mech.");
        //        }
        //        else
        //        {
        //            caller.Reply($"Part {partName} not found.");
        //        }
        //    }
        //}

        //public class UnequipCommand : ModCommand
        //{
        //    public override CommandType Type => CommandType.Chat;

        //    public override string Command => "unequipPart";

        //    public override string Usage => "/unequipPart <partslotName>";

        //    public override string Description => "Unequips the Part equipped to the specified slot of the mech.";

        //    public override void Action(CommandCaller caller, string input, string[] args)
        //    {
        //        if (args.Length != 1)
        //        {
        //            caller.Reply("Usage: /unequipPart <partslotName>");
        //            return;
        //        }

        //        string partslotName = args[0];
        //        Player player = caller.Player;
        //        MechModPlayer mechPlayer = player.GetModPlayer<MechModPlayer>();

        //        switch (partslotName)
        //        {
        //            case "head":
        //                mechPlayer.equippedParts[MechMod.headIndex].TurnToAir();
        //                caller.Reply($"Unequipped Head from the mech.");
        //                break;
        //            case "body":
        //                mechPlayer.equippedParts[MechMod.bodyIndex].TurnToAir();
        //                caller.Reply($"Unequipped Body from the mech.");
        //                break;
        //            case "arms":
        //                mechPlayer.equippedParts[MechMod.armsIndex].TurnToAir();
        //                caller.Reply($"Unequipped Arms from the mech.");
        //                break;
        //            case "legs":
        //                mechPlayer.equippedParts[MechMod.legsIndex].TurnToAir();
        //                caller.Reply($"Unequipped Legs from the mech.");
        //                break;
        //            case "booster":
        //                mechPlayer.equippedParts[MechMod.boosterIndex].TurnToAir();
        //                caller.Reply($"Unequipped Booster from the mech.");
        //                break;
        //            case "weapon":
        //                mechPlayer.equippedParts[MechMod.weaponIndex].TurnToAir();
        //                caller.Reply($"Unequipped Weapon from the mech.");
        //                break;
        //            case "passivemodule1":
        //                mechPlayer.equippedParts[MechMod.passivemodule1Index].TurnToAir();
        //                caller.Reply($"Unequipped Passive Module 1 from the mech.");
        //                break;
        //            case "passivemodule2":
        //                mechPlayer.equippedParts[MechMod.passivemodule2Index].TurnToAir();
        //                caller.Reply($"Unequipped Passive Module 2 from the mech.");
        //                break;
        //            case "activemodule":
        //                mechPlayer.equippedParts[MechMod.activemoduleIndex].TurnToAir();
        //                caller.Reply($"Unequipped Active Module from the mech.");
        //                break;
        //            default:
        //                caller.Reply($"Slot {partslotName} not recognized.");
        //                break;
        //        }
        //    }
        //}

        //public class DebugMountStateCommand : ModCommand
        //{
        //    public override CommandType Type => CommandType.Chat;
        //    public override string Command => "debugMounts";
        //    public override string Description => "Outputs all players' mount state and textures.";

        //    public override void Action(CommandCaller caller, string input, string[] args)
        //    {
        //        foreach (Player p in Main.player)
        //        {
        //            if (p != null && p.active)
        //            {
        //                string msg = $"Player {p.whoAmI} ({p.name}): Mount.Active={p.mount.Active}, Mount.Type={p.mount.Type}";
        //                Main.NewText(msg, Color.Yellow);

        //                if (p.mount.Active)
        //                {
        //                    var mountData = p.mount._data;
        //                    string textureInfo = mountData != null
        //                        ? $"BackTexture={(mountData.backTexture != null ? mountData.backTexture.ToString() : "null")}, " +
        //                          $"FrontTexture={(mountData.frontTexture != null ? mountData.frontTexture.ToString() : "null")}"
        //                        : "MountData is null";
        //                    Main.NewText($"    {textureInfo}", Color.LightGreen);
        //                }

        //                var modPlayer = p.GetModPlayer<MechModPlayer>();
        //                string texturePath = "none";
        //                if (!modPlayer.equippedParts[MechMod.headIndex].IsAir)
        //                {
        //                    // This should match your draw logic
        //                    texturePath = $"Content/Items/MechHeads/BaseHeadVisual";
        //                }
        //                Main.NewText($"Player {p.whoAmI} ({p.name}): headTexture path = {texturePath}", Color.Orange);
        //            }
        //        }
        //    }
        //}

        //public class ForceMountCommand : ModCommand
        //{
        //    public override CommandType Type => CommandType.Chat;
        //    public override string Command => "forceMounts";
        //    public override string Description => "Force Mech Mount.";

        //    public override void Action(CommandCaller caller, string input, string[] args)
        //    {
        //        foreach (Player p in Main.player)
        //        {
        //            if (p != null && p.active)
        //            {
        //                p.mount.SetMount(ModContent.MountType<ModularMech>(), p);
        //            }
        //        }
        //    }
        //}

        //public override void PostSetupContent()
        //{
        //    int mechDebuff = ModContent.BuffType<MechDebuff>();
        //    Terraria.ID.BuffID.Sets.NurseCannotRemoveDebuff[mechDebuff] = true;
        //}

        #endregion
    }
}