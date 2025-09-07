using Microsoft.Xna.Framework;
using MechMod.Content.Mounts;
using Terraria;
using Terraria.ModLoader;
using static MechMod.Content.Mounts.IMechModule;
using Terraria.DataStructures;
using Terraria.ID;
using MechMod.Content.Items.MechWeapons;
using Microsoft.VisualBasic;

namespace MechMod.Content.Items.MechModules.Passive
{
    public class Hover : ModItem, IMechModule
    {
        public override void SetDefaults()
        {
            Item.width = 20; // The width of the item's hitbox in pixels.
            Item.height = 20; // The height of the item's hitbox in pixels.
            Item.value = Item.buyPrice(gold: 4);
            Item.rare = ItemRarityID.Orange; // The rarity of the item.
        }

        public ModuleSlot MSlot => ModuleSlot.Passive; // Passive slot
        public ModuleType MType => ModuleType.Persistent; // Persistent effect

        public void ModuleEffect(ModularMech mech, Player player)
        {
            if (player.mount._frameState == Mount.FrameFlying) // If player is flying,
            {
                mech.allowDown = true; // Allow hovering
                if (player.controlDown) // If player is hovering,
                {
                    Weapons.partDamageBonus += 0.1f; // 10% damage bonus
                }
            }
        }
    }
}
