using MechMod.Common.Players;
using MechMod.Content.Mounts;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static MechMod.Content.Mounts.IMechModule;

namespace MechMod.Content.Items.MechModules.Passive
{
    /// <summary>
    /// Passive Module that gives the Mech contact damage.
    /// </summary>

    public class Spikes : ModItem, IMechModule
    {
        public override void SetDefaults()
        {
            Item.value = Item.buyPrice(gold: 4);
            Item.rare = ItemRarityID.Orange;
        }

        public ModuleSlot MSlot => ModuleSlot.Passive; // Passive slot
        public ModuleType MType => ModuleType.Persistent; // Persistent effect

        // Contact damage properties
        private DamageClass contactClass = DamageClass.Default;
        private int contactDamage = 10;
        private int contactKnockback = 20;

        private Dictionary<int, int> damageCooldown = []; // Cooldown for each NPC

        public void ModuleEffect(ModularMech mech, Player player, MechModPlayer modPlayer, MechWeaponsPlayer weaponsPlayer)
        {
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                // Get the NPC and prepare hit info
                NPC npc = Main.npc[i];
                NPC.HitInfo hitInfo = new()
                {
                    Damage = weaponsPlayer.DamageCalc(contactDamage, player, contactClass),
                    Knockback = weaponsPlayer.KnockbackCalc(contactKnockback, player, contactClass),
                    HitDirection = npc.Center.X < player.Center.X ? -1 : 1, // Determine hit direction based on NPC position relative to player
                };
                if (!damageCooldown.TryGetValue(npc.whoAmI, out _) && npc.active && !npc.friendly && !npc.dontTakeDamage && npc.Hitbox.Intersects(player.getRect())) // If the NPC is active, hostile, can take damage, is touching the player, and not on cooldown,
                {
                    // Apply contact damage to the NPC
                    npc.StrikeNPC(hitInfo);
                    damageCooldown[npc.whoAmI] = 30; // Set a cooldown of 30 frames (0.5 seconds)
                }
            }

            foreach (var key in damageCooldown.Keys.ToList()) // For each NPC in the cooldown dictionary,
            {
                // Decrement the cooldown
                damageCooldown[key]--;
                if (damageCooldown[key] <= 0) // If the cooldown has expired,
                    damageCooldown.Remove(key); // Remove the cooldown
            }
        }
    }
}
