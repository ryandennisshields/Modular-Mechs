using MechMod.Common.Players;
using MechMod.Content.Mounts;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static MechMod.Content.Mounts.IMechModule;

namespace MechMod.Content.Items.MechModules.Passive
{
    /// <summary>
    /// Passive Module that will allow Mana to absorb some damage taken. Having a magic weapon equipped makes the shield more effective. Having Mana Sickness temporarily disables the shield.
    /// </summary>

    public class ManaShield : ModItem, IMechModule
    {
        public override void SetDefaults()
        {
            Item.value = Item.buyPrice(gold: 4);
            Item.rare = ItemRarityID.Orange;
        }

        public ModuleSlot MSlot => ModuleSlot.Passive; // Passive slot
        public ModuleType MType => ModuleType.Persistent; // Persistent effect

        public void ModuleEffect(ModularMech mech, Player player, MechModPlayer modPlayer, MechWeaponsPlayer weaponsPlayer, MechVisualPlayer visualPlayer)
        {
            modPlayer.manaShield = true; // Enable mana shield (has to be handled in MechModPlayer.OnHurt)
        }
    }
}
