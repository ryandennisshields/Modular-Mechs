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
using MechMod.Content.Mechs;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;
using static MechMod.Common.Players.MechModPlayer;

namespace MechMod
{
    public struct MechPart
    {
        public int ItemType;
        public string PartType;

        public MechPart(int itemType, string partType)
        {
            ItemType = itemType;
            PartType = partType;
        }
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
        public static int pasModule1Index = 6;
        public static int pasModule2Index = 7;
        public static int actModuleIndex = 8;

        public static ModKeybind MechDashKeybind;
        public static ModKeybind MechActivateModule;

        public override void Load()
        {
            MechParts = new Dictionary<string, MechPart>
            {
                // Mech Parts
                // Base Parts
                { "basehead", new MechPart(ModContent.ItemType<BaseHead>(), "Head")},
                { "basebody", new MechPart(ModContent.ItemType<BaseBody>(), "Body")},
                { "basearms", new MechPart(ModContent.ItemType<BaseArms>(), "Arms")},
                { "baselegs", new MechPart(ModContent.ItemType<BaseLegs>(), "Legs")},
                { "basebooster", new MechPart(ModContent.ItemType<BaseBooster>(), "Booster")},
                // Slow Parts
                { "slowhead", new MechPart(ModContent.ItemType<SlowHead>(), "Head")},
                { "slowbody", new MechPart(ModContent.ItemType<SlowBody>(), "Body")},
                { "slowarms", new MechPart(ModContent.ItemType<SlowArms>(), "Arms")},
                { "slowlegs", new MechPart(ModContent.ItemType<SlowLegs>(), "Legs")},
                { "slowbooster", new MechPart(ModContent.ItemType<SlowBooster>(), "Booster")},
                // Fast Parts
                { "fasthead", new MechPart(ModContent.ItemType<FastHead>(), "Head")},
                { "fastbody", new MechPart(ModContent.ItemType<FastBody>(), "Body")},
                { "fastarms", new MechPart(ModContent.ItemType<FastArms>(), "Arms")},
                { "fastlegs", new MechPart(ModContent.ItemType<FastLegs>(), "Legs")},
                { "fastbooster", new MechPart(ModContent.ItemType<FastBooster>(), "Booster")},

                // Weapons
                { "basegun", new MechPart(ModContent.ItemType<BaseGun>(), "Weapon")},
                { "sasesword", new MechPart(ModContent.ItemType<BaseSword>(), "Weapon")},
                { "lasergun", new MechPart(ModContent.ItemType<LaserGun>(), "Weapon")},

                // Modules
                // Passive
                { "spikes", new MechPart(ModContent.ItemType<Spikes>(), "Passive Module")},
                { "nucleareject", new MechPart(ModContent.ItemType<NuclearEject>(), "Passive Module")},
                { "hover", new MechPart(ModContent.ItemType<Hover>(), "Passive Module")},
                // Active
                { "missilelauncher", new MechPart(ModContent.ItemType<MissileLauncher>(), "Active Module")},
            };

            MechDashKeybind = KeybindLoader.RegisterKeybind(this, "MechDash", "V");

            MechActivateModule = KeybindLoader.RegisterKeybind(this, "MechActivateModule", "C");
        }

        public override void Unload()
        {
            MechParts = null;
            MechDashKeybind = null;
        }

        public class EquipCommand : ModCommand
        {
            public override CommandType Type => CommandType.Chat;

            public override string Command => "equipPart";

            public override string Usage => "/equipPart <itemName>";

            public override string Description => "Equips the specified part to the mech.";

            public override void Action(CommandCaller caller, string input, string[] args)
            {
                if (args.Length != 1)
                {
                    caller.Reply("Usage: /equipPart <itemName>");
                    return;
                }

                string partName = args[0].ToLowerInvariant();
                Player player = caller.Player;
                MechModPlayer mechPlayer = player.GetModPlayer<MechModPlayer>();

                if (MechMod.MechParts.TryGetValue(partName, out MechPart mechPart))
                {
                    Item item = new Item();
                    item.SetDefaults(mechPart.ItemType);

                    switch (mechPart.PartType)
                    {
                        case "Head":
                            mechPlayer.equippedParts[MechMod.headIndex] = item;
                            break;
                        case "Body":
                            mechPlayer.equippedParts[MechMod.bodyIndex] = item;
                            break;
                        case "Arms":
                            mechPlayer.equippedParts[MechMod.armsIndex] = item;
                            break;
                        case "Legs":
                            mechPlayer.equippedParts[MechMod.legsIndex] = item;
                            break;
                        case "Booster":
                            mechPlayer.equippedParts[MechMod.boosterIndex] = item;
                            break;
                        case "Weapon":
                            mechPlayer.equippedParts[MechMod.weaponIndex] = item;
                            break;
                        case "Passive Module":
                            if (mechPlayer.equippedParts[MechMod.pasModule1Index].IsAir)
                                mechPlayer.equippedParts[MechMod.pasModule1Index] = item;
                            else if (mechPlayer.equippedParts[MechMod.pasModule2Index].IsAir)
                                mechPlayer.equippedParts[MechMod.pasModule2Index] = item;
                            else
                                caller.Reply("Both passive module slots are occupied. Please unequip a module (Passive Module 1 or Passive Module 2) first.");
                            break;
                        case "Active Module":
                            mechPlayer.equippedParts[MechMod.actModuleIndex] = item;
                            break;
                    }
                    caller.Reply($"Equipped {partName} to the mech.");
                }
                else
                {
                    caller.Reply($"Part {partName} not found.");
                }
            }
        }

        public class UnequipCommand : ModCommand
        {
            public override CommandType Type => CommandType.Chat;

            public override string Command => "unequipPart";

            public override string Usage => "/unequipPart <partslotName>";

            public override string Description => "Unequips the part equipped to the specified slot of the mech.";

            public override void Action(CommandCaller caller, string input, string[] args)
            {
                if (args.Length != 1)
                {
                    caller.Reply("Usage: /unequipPart <partslotName>");
                    return;
                }

                string partslotName = args[0];
                Player player = caller.Player;
                MechModPlayer mechPlayer = player.GetModPlayer<MechModPlayer>();

                switch (partslotName)
                {
                    case "head":
                        mechPlayer.equippedParts[MechMod.headIndex].TurnToAir();
                        caller.Reply($"Unequipped Head from the mech.");
                        break;
                    case "body":
                        mechPlayer.equippedParts[MechMod.bodyIndex].TurnToAir();
                        caller.Reply($"Unequipped Body from the mech.");
                        break;
                    case "arms":
                        mechPlayer.equippedParts[MechMod.armsIndex].TurnToAir();
                        caller.Reply($"Unequipped Arms from the mech.");
                        break;
                    case "legs":
                        mechPlayer.equippedParts[MechMod.legsIndex].TurnToAir();
                        caller.Reply($"Unequipped Legs from the mech.");
                        break;
                    case "booster":
                        mechPlayer.equippedParts[MechMod.boosterIndex].TurnToAir();
                        caller.Reply($"Unequipped Booster from the mech.");
                        break;
                    case "weapon":
                        mechPlayer.equippedParts[MechMod.weaponIndex].TurnToAir();
                        caller.Reply($"Unequipped Weapon from the mech.");
                        break;
                    case "passivemodule1":
                        mechPlayer.equippedParts[MechMod.pasModule1Index].TurnToAir();
                        caller.Reply($"Unequipped Passive Module 1 from the mech.");
                        break;
                    case "passivemodule2":
                        mechPlayer.equippedParts[MechMod.pasModule2Index].TurnToAir();
                        caller.Reply($"Unequipped Passive Module 2 from the mech.");
                        break;
                    case "activemodule":
                        mechPlayer.equippedParts[MechMod.actModuleIndex].TurnToAir();
                        caller.Reply($"Unequipped Active Module from the mech.");
                        break;
                    default:
                        caller.Reply($"Slot {partslotName} not recognized.");
                        break;
                }
            }
        }

        public override void PostSetupContent()
        {
            int mechDebuff = ModContent.BuffType<MechDebuff>();
            Terraria.ID.BuffID.Sets.NurseCannotRemoveDebuff[mechDebuff] = true;
        }
    }
}