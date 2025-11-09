using MechMod.Common.Players;
using MechMod.Content.Buffs;
using MechMod.Content.Debuffs;
using MechMod.Content.Mounts;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static MechMod.Content.Mounts.IMechModule;

namespace MechMod.Content.Items.MechModules.Active
{
    /// <summary>
    /// Active Module that gives double damage for a duration, but cuts down the Mech's duration and hurts the player.
    /// </summary>

    public class Overclock : ModItem, IMechModule
    {
        public override void SetDefaults()
        {
            Item.value = Item.buyPrice(gold: 8);
            Item.rare = ItemRarityID.Orange;
        }

        public ModuleSlot MSlot => ModuleSlot.Active; // Active slot
        public ModuleType MType => ModuleType.Persistent; // Persistent effect

        private int cooldown = 300; // Cooldown in frames (5 seconds)

        private int mechDurationReduction = 1200; // Mech duration reduction in frames (20 seconds)
        private int buffDuration = 600; // Buff duration in frames (10 seconds)

        public void ModuleEffect(ModularMech mech, Player player, MechModPlayer modPlayer, MechWeaponsPlayer weaponsPlayer, MechVisualPlayer visualPlayer)
        {
            if (player.whoAmI == Main.myPlayer && MechMod.MechActivateModule.JustPressed && !player.HasBuff(ModContent.BuffType<Cooldown>())) // If the player presses the "MechActivateModule" binding and the player is not on cooldown,
            {
                ref int mechBuffTime = ref player.buffTime[player.FindBuffIndex(ModContent.BuffType<MechBuff>())]; // Get the remaining Mech buff time
                if (mechBuffTime > mechDurationReduction) // If the Mech has enough duration to use the module,
                {
                    player.AddBuff(ModContent.BuffType<Cooldown>(), cooldown); // Add cooldown
                    mechBuffTime -= mechDurationReduction; // Remove Mech duration
                    player.Hurt(PlayerDeathReason.ByCustomReason(NetworkText.FromLiteral($"{player.name} couldn't handle the heat.")), (int)(player.statLifeMax2 * 0.10f), 0, armorPenetration: 999); // Hurt the player for 10% of their max health
                    player.AddBuff(ModContent.BuffType<OverclockBuff>(), buffDuration); // Add Overclock buff
                    weaponsPlayer.finalDamageModifier *= 2f; // Double all damage
                    SoundEngine.PlaySound(SoundID.NPCHit57, player.position); // Play aggressive sound
                }
            }
        }
    }

    public class OverclockBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            MechWeaponsPlayer weaponsPlayer = player.GetModPlayer<MechWeaponsPlayer>();
            MechVisualPlayer visualPlayer = player.GetModPlayer<MechVisualPlayer>();

            if (player.mount.Active) // If the player is mounted,
            {
                for (int i = 0; i < 10; i++) // Create heat dust around the player
                {
                    int dustIndex = Dust.NewDust(new Vector2(player.position.X - 20, player.position.Y), player.width * 3, player.height, DustID.Torch, Scale: 1.5f);
                    Main.dust[dustIndex].velocity *= 2f;
                    Main.dust[dustIndex].noGravity = true;
                }
                visualPlayer.mechColour = Color.Red; // Change mech colour to indicate overclocking
            }

            if (player.buffTime[buffIndex] <= 0 || !player.mount.Active) // If the buff time runs out or the player is no longer mounted,
            {
                buffIndex--;
                weaponsPlayer.finalDamageModifier /= 2f; // Revert damage modifier
                visualPlayer.mechColour = Color.White; // Revert mech colour
            }
        }
    }
}
