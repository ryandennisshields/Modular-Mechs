using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MechMod.Content.Buffs
{
    /// <summary>
    /// Debuff that applies after dismounting the mech, disabling mounts (mech), reducing max health and damage for a short duration to simulate the player needing to recover from being in the mech.
    /// </summary>

    public class MechDebuff : ModBuff
    {
        public const float PlayerHealthReduction = 0.25f; // 25% reduction in max health
        public const float PlayerDamageModifier = 0.75f; // 25% reduction in damage

        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.buffDoubleApply[Type] = false;
            BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            if (player.mount.Active)
                player.mount.Dismount(player); // Make sure the player is counted as dismounted
            // Apply the health and damage reductions
            float PlayerHealth = player.statLifeMax;
            int PlayerHealthModifier = (int)(PlayerHealth *= PlayerHealthReduction);
            player.statLifeMax2 -= PlayerHealthModifier;
            player.GetDamage<GenericDamageClass>() *= PlayerDamageModifier;
        }
    }
}
