using MechMod.Content.Items.MechWeapons;
using MechMod.Content.Mounts;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static MechMod.Content.Mounts.IMechModule;

namespace MechMod.Content.Items.MechModules.Passive
{
    public class Spikes : ModItem, IMechModule
    {
        public override void SetDefaults()
        {
            Item.width = 20; // The width of the item's hitbox in pixels.
            Item.height = 20; // The height of the item's hitbox in pixels.
            Item.value = Item.buyPrice(gold: 4);
            Item.rare = 3; // The rarity of the item.
        }

        public ModuleSlot moduleSlot => ModuleSlot.Passive; // Passive slot
        public ModuleType moduleType => ModuleType.Persistent; // Persistent effect

        private DamageClass contactClass = DamageClass.Default; // Damage class for contact damage
        private int contactDamage = 10; // Damage dealt on contact with enemies
        private int contactKnockback = 20; // Knockback applied on contact
        //private Rectangle hitbox; // Hitbox for spikes

        private Dictionary<int, int> damageCooldown = new Dictionary<int, int>(); // Cooldown for each NPC

        public void InitialEffect(ModularMech mech, Player player) { }

        public void ModuleEffect(ModularMech mech, Player player)
        {
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                NPC.HitInfo hitInfo = new NPC.HitInfo
                {
                    Damage = Weapons.DamageCalc(contactDamage, player, contactClass),
                    Knockback = Weapons.KnockbackCalc(contactKnockback, player, contactClass),
                    HitDirection = npc.Center.X < player.Center.X ? -1 : 1, // Determine hit direction based on NPC position relative to player
                };
                if (!damageCooldown.TryGetValue(npc.whoAmI, out int cooldown) && npc.active && !npc.friendly && !npc.dontTakeDamage && npc.Hitbox.Intersects(player.getRect()))
                {
                    // Apply contact damage to the NPC
                    npc.StrikeNPC(hitInfo);
                    damageCooldown[npc.whoAmI] = 30; // Set a cooldown of 30 frames (0.5 seconds)
                    // Optionally, you can add a sound effect or visual effect here
                }
            }

            foreach (var key in damageCooldown.Keys.ToList())
            {
                damageCooldown[key]--;
                if (damageCooldown[key] <= 0)
                    damageCooldown.Remove(key); // Remove the cooldown if it has expired
            }
        }
    }
}
