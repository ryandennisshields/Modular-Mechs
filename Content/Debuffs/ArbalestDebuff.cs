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
            Main.pvpBuff[Type] = true; // Allow the debuff to be applied in PvP
        }

        public override bool ReApply(NPC npc, int time, int buffIndex)
        {
            npc.GetGlobalNPC<GlobalDebuffEffect>().arbalestFrame++; // Increment the frame counter each time the debuff is reapplied
            return true;
        }
    }
}
