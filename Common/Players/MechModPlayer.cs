using MechMod.Common.UI;
using MechMod.Content.Buffs;
using MechMod.Content.Items.MechSpawner;
using MechMod.Content.Mechs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.Chat;
using Terraria.DataStructures;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace MechMod.Common.Players
{
    public class MechModPlayer : ModPlayer
    {
        public bool disableMounts; // Flag to control whether mounts are disabled

        public Item[] equippedParts;
        public int upgradeLevel;
        public float upgradeDamageBonus;

        public float animationTime; // Used for animation logic behind mech weapon usage
                                    // (For example, for swinging, it changes how fast the weapon swings and how the arm moves with it, and for pointing it changes how long the arm and weapon remain out)

        public override void Initialize()
        {
            equippedParts = new Item[6];
            upgradeLevel = 0;
            upgradeDamageBonus = 0;
        }

        public override void SaveData(TagCompound tag)
        {
            if (!equippedParts[MechMod.headIndex].IsAir)
                tag["equippedHead"] = ItemIO.Save(equippedParts[MechMod.headIndex]);
            if (!equippedParts[MechMod.bodyIndex].IsAir)
                tag["equippedBody"] = ItemIO.Save(equippedParts[MechMod.bodyIndex]);
            if (!equippedParts[MechMod.armsIndex].IsAir)
                tag["equippedArms"] = ItemIO.Save(equippedParts[MechMod.armsIndex]);
            if (!equippedParts[MechMod.legsIndex].IsAir)
                tag["equippedLegs"] = ItemIO.Save(equippedParts[MechMod.legsIndex]);
            if (!equippedParts[MechMod.boosterIndex].IsAir)
                tag["equippedBooster"] = ItemIO.Save(equippedParts[MechMod.boosterIndex]);
            if (!equippedParts[MechMod.weaponIndex].IsAir)
                tag["equippedWeapon"] = ItemIO.Save(equippedParts[MechMod.weaponIndex]);
            tag["upgradeLevel"] = upgradeLevel;
            tag["upgradeDamageBonus"] = upgradeDamageBonus;
        }
        public override void LoadData(TagCompound tag)
        {
            if (tag.ContainsKey("equippedHead"))
                equippedParts[MechMod.headIndex] = ItemIO.Load(tag.GetCompound("equippedHead"));
            if (tag.ContainsKey("equippedBody"))
                equippedParts[MechMod.bodyIndex] = ItemIO.Load(tag.GetCompound("equippedBody"));
            if (tag.ContainsKey("equippedArms"))
                equippedParts[MechMod.armsIndex] = ItemIO.Load(tag.GetCompound("equippedArms"));
            if (tag.ContainsKey("equippedLegs"))
                equippedParts[MechMod.legsIndex] = ItemIO.Load(tag.GetCompound("equippedLegs"));
            if (tag.ContainsKey("equippedBooster"))
                equippedParts[MechMod.boosterIndex] = ItemIO.Load(tag.GetCompound("equippedBooster"));
            if (tag.ContainsKey("equippedWeapon"))
                equippedParts[MechMod.weaponIndex] = ItemIO.Load(tag.GetCompound("equippedWeapon"));
            if (tag.ContainsKey("upgradeLevel"))
                upgradeLevel = tag.GetInt("upgradeLevel");
            if (tag.ContainsKey("upgradeDamageBonus"))
                upgradeDamageBonus = tag.GetFloat("upgradeDamageBonus");
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
                    Player.statLife = (int)(PlayerHealth *= 0.5f);
                    Player.immuneTime = 60;
                    Player.mount.Dismount(Player);
                    // Should be set to 7200 ticks
                    Player.AddBuff(ModContent.BuffType<MechDebuff>(), 60);
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
            int mountBuffID = Player.mount.BuffType;

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

                drawInfo.drawPlayer.invis = true;
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

            if (animationTime > 0)
                animationTime--;
        }

        public override bool CanUseItem(Item item)
        {
            if (disableMounts)
            {
                return false;
            }
            return true;
        }
    }
}
