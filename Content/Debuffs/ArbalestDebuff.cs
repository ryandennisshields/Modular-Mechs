using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MechMod.Content.Items.MechWeapons;
using MechMod.Common.Global;

namespace MechMod.Content.Debuffs
{
    /// <summary>
    /// Debuff for the <see cref="Arbalest"/> effect. Visuals and logic are handled in <see cref="GlobalDebuffEffect"/>.
    /// </summary>

    public class ArbalestDebuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true; // Mark as a debuff
            Main.pvpBuff[Type] = true; // Allow the debuff to be applied in PvP
            BuffID.Sets.NurseCannotRemoveDebuff[Type] = true; // Prevent nurse from removing the debuff
        }

        public override bool ReApply(NPC npc, int time, int buffIndex)
        {
            npc.GetGlobalNPC<GlobalDebuffEffect>().arbalestFrame++; // Increment the frame counter each time the debuff is reapplied
            return true;
        }
    }
}
