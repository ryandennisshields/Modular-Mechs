using MechMod.Common.Players;
using MechMod.Common.UI;
using MechMod.Content.Buffs;
using MechMod.Content.Items.MechArms;
using MechMod.Content.Items.MechBodies;
using MechMod.Content.Items.MechBoosters;
using MechMod.Content.Items.MechHeads;
using MechMod.Content.Items.MechLegs;
using MechMod.Content.Items.MechWeapons;
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

        public override void Load()
        {
            MechParts = new Dictionary<string, MechPart>
            {
                { "BaseHead", new MechPart(ModContent.ItemType<BaseHead>(), "Head")},
                { "BaseBody", new MechPart(ModContent.ItemType<BaseBody>(), "Body")},
                { "BaseArms", new MechPart(ModContent.ItemType<BaseArms>(), "Arms")},
                { "BaseLegs", new MechPart(ModContent.ItemType<BaseLegs>(), "Legs")},
                { "BaseBooster", new MechPart(ModContent.ItemType<BaseBooster>(), "Booster")},
                { "BaseGun", new MechPart(ModContent.ItemType<BaseGun>(), "Weapon")},
                { "BaseSword", new MechPart(ModContent.ItemType<BaseSword>(), "Weapon")}
            };
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

                string partName = args[0];
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
                    case "Head":
                        mechPlayer.equippedParts[MechMod.headIndex].TurnToAir();
                        caller.Reply($"Unequipped Head from the mech.");
                        break;
                    case "Body":
                        mechPlayer.equippedParts[MechMod.bodyIndex].TurnToAir();
                        caller.Reply($"Unequipped Body from the mech.");
                        break;
                    case "Arms":
                        mechPlayer.equippedParts[MechMod.armsIndex].TurnToAir();
                        caller.Reply($"Unequipped Arms from the mech.");
                        break;
                    case "Legs":
                        mechPlayer.equippedParts[MechMod.legsIndex].TurnToAir();
                        caller.Reply($"Unequipped Legs from the mech.");
                        break;
                    case "Booster":
                        mechPlayer.equippedParts[MechMod.boosterIndex].TurnToAir();
                        caller.Reply($"Unequipped Booster from the mech.");
                        break;
                    case "Weapon":
                        mechPlayer.equippedParts[MechMod.weaponIndex].TurnToAir();
                        caller.Reply($"Unequipped Weapon from the mech.");
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