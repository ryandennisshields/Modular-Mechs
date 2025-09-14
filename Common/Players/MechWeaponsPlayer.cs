using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;

namespace MechMod.Common.Players
{
    /// <summary>
    /// Stores functions that allow easy grabbing of Damage, Critical Chance, Attack Speed and Knockback bonuses and calculations for when the Mech Parts, upgrades or weapons use/change them.
    /// </summary>

    public class MechWeaponsPlayer : ModPlayer
    {
        // DamageClass sets the type of damage the weapon does, which is used to calculate what bonuses the weapon recieves
        // e.g. Melee weapons will recieve bonuses from Melee Damage, Ranged weapons will recieve bonuses from Ranged Damage, etc.
        public DamageClass DamageClass { get; set; } = DamageClass.Default;

        // UseType sets the kind of action the Mech executes to use the weapon
        // e.g. Swing weapons will swing the weapon (for something like a sword), Point weapons will point the weapon (for something like a gun)
        public enum UseType
        {
            Swing,
            Point
        }
        public UseType useType;

        public float timer; // Timer for weapon attack rate
        public float attackRate; // Used to determine how fast the weapon can be swung/fired/cast/used
        public bool canUse; // Determines if a weapon can be used, for example, disabling use if weapon is out of mana or ammo

        // Part bonuses that are added to the player's bonuses when calculating the final values for the weapon
        public float partDamageBonus;
        public float partCritChanceBonus;
        public float partAttackSpeedBonus;
        public float partKnockbackBonus;

        // Functions for calculating the final values for the weapon, taking into account the player's bonuses and the part bonuses
        // Parameters can be input to use a custom DamageClass instead of the one set for the weapon, and same can choose which types of modifying values to include in the calculation

        // Function for calculating damage of a weapon
        public int DamageCalc(int baseDamage, Player player, DamageClass damageClass = null, bool total = true, bool mult = true, bool flat = true)
        {
            MechModPlayer modPlayer = player.GetModPlayer<MechModPlayer>();

            // Get the player's damage bonuses for both Generic and the weapon's DamageClass (or the manually inputted one)
            float genericAddDamageBonus = player.GetDamage(DamageClass.Generic).Additive;
            float classAddDamageBonus = player.GetDamage(damageClass ?? DamageClass).Additive;
            float genericMultDamageBonus = player.GetDamage(DamageClass.Generic).Multiplicative;
            float classMultDamageBonus = player.GetDamage(damageClass ?? DamageClass).Multiplicative;
            float genericFlatDamageBonus = player.GetDamage(DamageClass.Generic).Flat;
            float classFlatDamageBonus = player.GetDamage(damageClass ?? DamageClass).Flat;

            float totalDamageBonus = 0f;
            float multDamageBonus = 0f;
            float flatDamageBonus = 0f;

            // Calculate the total, multiplicative and flat damage bonuses
            if (total)
            {
                float addDamageBonus = genericAddDamageBonus + classAddDamageBonus - 1f;
                totalDamageBonus = addDamageBonus + modPlayer.upgradeDamageBonus + partDamageBonus;
            }
            if (mult)
            {
                multDamageBonus = genericMultDamageBonus + classMultDamageBonus - 1f;
            }
            if (flat)
            {
                flatDamageBonus = genericFlatDamageBonus + classFlatDamageBonus;
            }

            // Collect the other damage bonuses to create the final damage value
            baseDamage = (int)(baseDamage * totalDamageBonus * multDamageBonus + flatDamageBonus);

            return baseDamage;
        }

        // Function for calculating critical strike chance of a weapon
        public int CritChanceCalc(int baseCritChance, Player player, DamageClass damageClass = null)
        {
            // Get the player's crit chance bonuses for both Generic and the weapon's DamageClass (or the manually inputted one)
            float genericCritChanceBonus = player.GetCritChance(DamageClass.Generic);
            float classCritChanceBonus = player.GetCritChance(damageClass ?? DamageClass);

            float critChanceBonus = genericCritChanceBonus + classCritChanceBonus + partCritChanceBonus; // Calculate the total crit chance bonus

            // Collect the other crit chance bonuses to create the final crit chance value
            // 4 is taken away from the weapon's crit chance to take into account the base crit chance
            baseCritChance = (int)(baseCritChance - 4 + critChanceBonus);

            return baseCritChance;
        }

        // Function for calculating attack speed of a weapon
        public int AttackSpeedCalc(int baseAttackSpeed, Player player, DamageClass damageClass = null)
        {
            // Get the player's attack speed bonuses for both Generic and the weapon's DamageClass (or the manually inputted one)
            float genericAttackSpeedBonus = player.GetAttackSpeed(DamageClass.Generic);
            float classAttackSpeedBonus = player.GetAttackSpeed(damageClass ?? DamageClass);

            float attackSpeedBonus = genericAttackSpeedBonus + classAttackSpeedBonus + partAttackSpeedBonus - 1f; // Calculate the total attack speed bonus

            baseAttackSpeed = (int)(baseAttackSpeed / attackSpeedBonus); // Calculate the final attack speed value

            return baseAttackSpeed;
        }

        public int KnockbackCalc(int baseKnockback, Player player, DamageClass damageClass = null, bool total = true, bool mult = true, bool flat = true)
        {
            // Get the player's knockback bonuses for both Generic and the weapon's DamageClass (or the manually inputted one)
            float genericAddKnockbackBonus = player.GetKnockback(DamageClass.Generic).Additive;
            float classAddKnockbackBonus = player.GetKnockback(damageClass ?? DamageClass).Additive;
            float genericMultKnockbackBonus = player.GetKnockback(DamageClass.Generic).Multiplicative;
            float classMultKnockbackBonus = player.GetKnockback(damageClass ?? DamageClass).Multiplicative;
            float genericFlatKnockbackBonus = player.GetKnockback(DamageClass.Generic).Flat;
            float classFlatKnockbackBonus = player.GetKnockback(damageClass ?? DamageClass).Flat;

            float totalKnockbackBonus = 0f;
            float multKnockbackBonus = 0f;
            float flatKnockbackBonus = 0f;

            // Calculate the total, multiplicative and flat knockback bonuses
            if (total)
            {
                float addKnockbackBonus = genericAddKnockbackBonus + classAddKnockbackBonus - 1f;
                totalKnockbackBonus = addKnockbackBonus + partKnockbackBonus;
            }
            if (mult)
            {
                multKnockbackBonus = genericMultKnockbackBonus + classMultKnockbackBonus - 1f;
            }
            if (flat)
            {
                flatKnockbackBonus = genericFlatKnockbackBonus + classFlatKnockbackBonus;
            }

            // Collect the other knockback bonuses to create the final knockback value
            baseKnockback = (int)(baseKnockback * totalKnockbackBonus * multKnockbackBonus + flatKnockbackBonus);

            return baseKnockback;
        }
    }
}
