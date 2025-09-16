using MechMod.Content.Mounts;
using Terraria;
using Terraria.ModLoader;
using static MechMod.Content.Mounts.IMechModule;
using Terraria.ID;
using MechMod.Common.Players;

namespace MechMod.Content.Items.MechModules.Passive
{
    /// <summary>
    /// Passive Module that allows the player to hover with Boosters, granting a small damage bonus when hovering.
    /// </summary>

    public class Hover : ModItem, IMechModule
    {
        public override void SetDefaults()
        {
            Item.value = Item.buyPrice(gold: 10);
            Item.rare = ItemRarityID.Orange;
        }

        public ModuleSlot MSlot => ModuleSlot.Passive; // Passive slot
        public ModuleType MType => ModuleType.Persistent; // Persistent effect

        public void ModuleEffect(ModularMech mech, Player player, MechModPlayer modPlayer, MechWeaponsPlayer weaponsPlayer)
        {
            if (player.mount._frameState == Mount.FrameFlying) // If player is flying,
            {
                modPlayer.allowDown = true; // Allow hovering
                if (player.controlDown) // If player is hovering,
                {
                    weaponsPlayer.partDamageBonus += 0.1f; // 10% damage bonus
                }
            }
        }
    }
}
