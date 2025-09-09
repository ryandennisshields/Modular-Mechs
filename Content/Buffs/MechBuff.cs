using MechMod.Common.Players;
using MechMod.Content.Mounts;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MechMod.Content.Buffs
{
    /// <summary>
    /// Acts as the buff for the Modular Mech mount, applying the mount to the player while active and doing things like disabling item use and NPC interaction.
    /// Although mount buffs are usually unlimited duration buffs that are applied while the mount is active, because the mech has a limited duration, this buff is applied with a timer that counts down while the mech is active.
    /// </summary>

    public class MechBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            // Although not a debuff, as this counts as the mount buff it should not be removable besides dismounting the mech
            Main.debuff[Type] = true;
            Main.buffDoubleApply[Type] = false;
            BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            if (player.buffTime[buffIndex] <= 0)
            {
                buffIndex--;
                player.mount.Dismount(player); // Dismount the mech
            }
            player.mount.SetMount(ModContent.MountType<ModularMech>(), player);
            player.controlUseItem = false; // Disable item use
            player.SetTalkNPC(-1); // Disable NPC interaction
        }    
    }
}
