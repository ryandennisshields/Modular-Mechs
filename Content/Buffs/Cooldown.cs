using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MechMod.Content.Buffs
{
    public class Cooldown : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
        }
    }
}
