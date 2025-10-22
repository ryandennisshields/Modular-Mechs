using MechMod.Content.Mounts;
using Terraria;
using Terraria.ModLoader;
using static MechMod.Content.Mounts.IMechModule;
using Terraria.ID;
using MechMod.Common.Players;

namespace MechMod.Content.Items.MechModules.Passive
{
    /// <summary>
    /// Passive Module that doubles the duration of the Mech but reduces final damage by 25%.
    /// </summary>

    public class EnergyExtension : ModItem, IMechModule
    {
        public override void SetDefaults()
        {
            Item.value = Item.buyPrice(gold: 10);
            Item.rare = ItemRarityID.Orange;
        }

        public ModuleSlot MSlot => ModuleSlot.Passive; // Passive slot
        public ModuleType MType => ModuleType.OnMount; // Mount effect

        public void ModuleEffect(ModularMech mech, Player player, MechModPlayer modPlayer, MechWeaponsPlayer weaponsPlayer)
        {
            modPlayer.mechBuffDuration *= 2; // Double the mech buff duration
            weaponsPlayer.finalDamageModifier = 0.75f; // 25% final damage reduction
        }
    }
}
