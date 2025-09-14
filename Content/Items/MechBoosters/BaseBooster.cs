using MechMod.Common.Players;
using MechMod.Content.Mounts;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MechMod.Content.Items.MechBoosters
{
    public class BaseBooster : ModItem, IMechParts
    {
        public override void SetDefaults()
        {
            Item.value = Item.buyPrice(gold: 10);
            Item.rare = ItemRarityID.Green;
        }

        public void ApplyStats(Player player, MechModPlayer modPlayer, MechWeaponsPlayer weaponsPlayer, ModularMech mech)
        {
            // Flight stats
            mech.MountData.flightTimeMax = (int)(150 * modPlayer.partEffectiveness[MechMod.boosterIndex]); // 2.5 seconds of flight time
            modPlayer.flightHorizontalSpeed = 6f * modPlayer.partEffectiveness[MechMod.boosterIndex]; // 6 horizontal speed
            modPlayer.flightJumpSpeed = 6f * modPlayer.partEffectiveness[MechMod.boosterIndex]; // 6 jump speed
        }

        public void BodyOffsets(MechVisualPlayer visualPlayer, string body) { }
    }
}
