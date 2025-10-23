using MechMod.Common.Players;
using MechMod.Content.Mounts;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static MechMod.Content.Mounts.IMechModule;

namespace MechMod.Content.Items.MechModules.Passive
{
    /// <summary>
    /// Passive Module that allows the player to pan the camera further out when holding right-click with a ranged weapon (similar to Terraria's scope accessories), gaining a small damage bonus but reducing movement speed.
    /// </summary>

    public class Brace : ModItem, IMechModule
    {
        public override void SetDefaults()
        {
            Item.value = Item.buyPrice(gold: 4);
            Item.rare = ItemRarityID.Orange;
        }

        public ModuleSlot MSlot => ModuleSlot.Passive; // Passive slot
        public ModuleType MType => ModuleType.Persistent; // Persistent effect

        private bool changed = false; // Tracker for if stats have been changed
        private CameraPan cameraPanInstance = ModContent.GetInstance<CameraPan>(); // Reference to CameraPan instance

        private const float damageBonus = 0.1f; // 10% damage bonus
        private const float speedReduction = 0.9f; // 10% speed reduction

        public void ModuleEffect(ModularMech mech, Player player, MechModPlayer modPlayer, MechWeaponsPlayer weaponsPlayer, MechVisualPlayer visualPlayer)
        {
            if (weaponsPlayer.DamageClass == DamageClass.Ranged) // If the player's weapon is ranged,
            {
                if (weaponsPlayer.activateRightClick) // If player is holding right-click,
                {
                    if (cameraPanInstance.lerp < 1f) // If camera pan lerp is less than 1,
                        cameraPanInstance.lerp += 0.075f; // Increase lerp value to pan camera out
                    if (!changed) // If stats haven't been changed yet,
                    {
                        // Apply stat changes
                        weaponsPlayer.partDamageBonus += damageBonus;
                        modPlayer.groundHorizontalSpeed *= speedReduction;
                        modPlayer.groundJumpSpeed *= speedReduction;
                        modPlayer.flightHorizontalSpeed *= speedReduction;
                        modPlayer.flightJumpSpeed *= speedReduction;
                        changed = true;
                    }
                }
                else // Otherwise,
                {
                    if (cameraPanInstance.lerp > 0f) // If camera pan lerp is greater than 0,
                        cameraPanInstance.lerp -= 0.075f; // Decrease lerp value to pan camera back in
                    if (changed) // If stats have been changed,
                    {
                        // Reset stat changes
                        weaponsPlayer.partDamageBonus -= damageBonus;
                        modPlayer.groundJumpSpeed /= speedReduction;
                        modPlayer.groundHorizontalSpeed /= speedReduction;
                        modPlayer.flightJumpSpeed /= speedReduction;
                        modPlayer.flightHorizontalSpeed /= speedReduction;
                        changed = false;
                    }
                }
            }
        }
    }

    public class CameraPan : ModSystem
    {
        public float lerp; // Lerp value for camera panning

        public override void ModifyScreenPosition()
        {
            Player player = Main.LocalPlayer; // Get local player
            if (player.active && lerp > 0f) // If player is active and camera pan is active,
            {
                Vector2 mouseWorld = Main.MouseWorld; // Get mouse world position
                Vector2 screenCenter = Main.screenPosition + new Vector2(Main.screenWidth / 2, Main.screenHeight / 2); // Get screen center position
                Vector2 offset = (mouseWorld - screenCenter) * 0.75f; // Calculate offset (75% of the distance from screen center to mouse position)
                Main.screenPosition = Vector2.Lerp(Main.screenPosition, Main.screenPosition + offset, lerp); // Lerp screen position towards the offset position
            }
        }
    }
}
