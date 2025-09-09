using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MechMod.Content.Buffs
{
    /// <summary>
    /// Debuff that acts as a cooldown for using active modules, preventing them from being used again until the debuff expires.
    /// Using a debuff makes the cooldown visible to the player.
    /// </summary>

    public class Cooldown : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.buffDoubleApply[Type] = false;
            BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
        }
    }
}
