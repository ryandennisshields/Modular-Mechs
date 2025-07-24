using MechMod.Content.Items;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MechMod.Content.Buffs
{
    public class MechDebuff : ModBuff
    {
        public const float PlayerHealthReduction = 0.25f;
        public const float PlayerDamageModifier = 0.75f;

        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            float PlayerHealth = player.statLifeMax;
            int PlayerHealthModifier = (int)(PlayerHealth *= PlayerHealthReduction);
            player.statLifeMax2 -= PlayerHealthModifier;
            player.GetDamage<GenericDamageClass>() *= PlayerDamageModifier;
        }
    }
}
