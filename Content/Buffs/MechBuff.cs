using MechMod.Content.Mechs;
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
    public class MechBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            // Although not a debuff, as this counts as the mount buff it should not be removable besides dismounting the mech
            Main.debuff[Type] = true;
            BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            if (player.buffTime[buffIndex] <= 0)
            {
                player.mount.Dismount(player); // Dismount the mech
            }
            player.mount.SetMount(ModContent.MountType<ModularMech>(), player);
            player.controlUseItem = false; // Disable item use
            player.SetTalkNPC(-1); // Disable NPC interaction
        }    
    }
}
