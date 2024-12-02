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
            //DisplayName.SetDefault("Mech Power");
            //Description.SetDefault("You are controlling a mech!");
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = true;
            Main.debuff[Type] = false;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            // Any additional effects while the buff is active
            player.controlUseItem = false;
        }
    }
}
