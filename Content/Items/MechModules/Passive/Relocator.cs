using MechMod.Common.Players;
using MechMod.Content.Mounts;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static MechMod.Content.Mounts.IMechModule;

namespace MechMod.Content.Items.MechModules.Passive
{
    /// <summary>
    /// Passive Module that returns the player to the position where they initially mounted the Mech after leaving it.
    /// </summary>

    public class Relocator : ModItem, IMechModule
    {
        public override void SetDefaults()
        {
            Item.value = Item.buyPrice(gold: 4);
            Item.rare = ItemRarityID.Orange;
        }

        public ModuleSlot MSlot => ModuleSlot.Passive; // Passive slot
        public ModuleType MType => ModuleType.OnMount; // Mount effect (BUT has visuals of dismount Module, as the actual effect happens when dismounting)

        public void ModuleEffect(ModularMech mech, Player player, MechModPlayer modPlayer, MechWeaponsPlayer weaponsPlayer, MechVisualPlayer visualPlayer)
        {
            modPlayer.relocatorPosition = player.position; // Store the player's position when mounting the mech (to be used when dismounting)
        }
    }
}
