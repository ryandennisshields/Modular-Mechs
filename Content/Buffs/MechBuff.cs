using MechMod.Content.Mechs;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace MechMod.Content.Buffs
{
    public class MechBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = true;
            Main.buffNoSave[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.mount.SetMount(ModContent.MountType<ModularMech>(), player);
            player.buffTime[buffIndex] = 10;
            player.controlUseItem = false; // Disable item use
        }
    }
}
