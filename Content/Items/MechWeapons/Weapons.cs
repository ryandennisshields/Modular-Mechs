using MechMod.Common.Players;
using MechMod.Content.Mechs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace MechMod.Content.Items.MechWeapons
{
    public static class Weapons
    {

        // These functions allow easy grabbing of Damage, Critical Chance, Attack Speed and Knockback bonuses for when the Mech parts change them (or for Upgrades)

        // DamageClass sets the type of damage the weapon does, which is used to calculate what bonuses the weapon recieves
        // e.g. Melee weapons will recieve bonuses from Melee Damage, Ranged weapons will recieve bonuses from Ranged Damage, etc.
        public static DamageClass damageClass;

        // UseType sets the kind of action the Mech executes to use the weapon
        // e.g. Swing weapons will swing the weapon (for something like a sword), Point weapons will point the weapon (for something like a gun)
        public enum UseType
        {
            Swing,
            Point
        }
        public static UseType useType;

        public static float timer; // Timer for weapon attack rate
        public static float attackRate; // Attack rate for the weapon, used to determine how fast the weapon can be used
        public static bool canUse; // Determines if a weapon can be used, for example, disabling use if weapon is out of mana or ammo

        public static float partDamageBonus;
        public static float partCritChanceBonus;
        public static float partAttackSpeedBonus;
        public static float partKnockbackBonus;

        public static float BonusDamage(Player player, string totalOrmultOrflat)
        {
            var modPlayer = player.GetModPlayer<MechModPlayer>();

            float genericAddDamageBonus = player.GetDamage(DamageClass.Generic).Additive;
            float classAddDamageBonus = player.GetDamage(damageClass).Additive;
            float genericMultDamageBonus = player.GetDamage(DamageClass.Generic).Multiplicative;
            float classMultDamageBonus = player.GetDamage(damageClass).Multiplicative;
            float genericFlatDamageBonus = player.GetDamage(DamageClass.Generic).Flat;
            float classFlatDamageBonus = player.GetDamage(damageClass).Flat;
            
            float addDamageBonus = genericAddDamageBonus + classAddDamageBonus - 1f;
            float multDamageBonus = genericMultDamageBonus + classMultDamageBonus - 1f;
            float flatDamageBonus = genericFlatDamageBonus + classFlatDamageBonus;

            float totalDamageBonus = addDamageBonus + modPlayer.upgradeDamageBonus + partDamageBonus;

            switch (totalOrmultOrflat)
            {
                case "total":
                    return totalDamageBonus;
                case "mult":
                    return multDamageBonus;
                case "flat":
                    return flatDamageBonus;

            }
            return 0f;
        }

        public static float BonusCritChance(Player player)
        {
            var modPlayer = player.GetModPlayer<MechModPlayer>();

            float genericCritChanceBonus = player.GetCritChance(DamageClass.Generic);
            float classCritChanceBonus = player.GetCritChance(damageClass);

            float critChanceBonus = genericCritChanceBonus + classCritChanceBonus + partCritChanceBonus;

            return critChanceBonus;
        }

        public static float BonusAttackSpeed(Player player)
        {
            var modPlayer = player.GetModPlayer<MechModPlayer>();

            float genericAttackSpeedBonus = player.GetAttackSpeed(DamageClass.Generic);
            float classAttackSpeedBonus = player.GetAttackSpeed(damageClass);

            float attackSpeedBonus = genericAttackSpeedBonus + classAttackSpeedBonus + partAttackSpeedBonus - 1f;

            return attackSpeedBonus;
        }

        public static float BonusKnockback(Player player, string totalOrmultOrflat)
        {
            var modPlayer = player.GetModPlayer<MechModPlayer>();

            float genericAddKnockbackBonus = player.GetKnockback(DamageClass.Generic).Additive;
            float classAddKnockbackBonus = player.GetKnockback(damageClass).Additive;
            float genericMultKnockbackBonus = player.GetKnockback(DamageClass.Generic).Multiplicative;
            float classMultKnockbackBonus = player.GetKnockback(damageClass).Multiplicative;
            float genericFlatKnockbackBonus = player.GetKnockback(DamageClass.Generic).Flat;
            float classFlatKnockbackBonus = player.GetKnockback(damageClass).Flat;

            float addKnockbackBonus = genericAddKnockbackBonus + classAddKnockbackBonus - 1f;
            float multKnockbackBonus = genericMultKnockbackBonus + classMultKnockbackBonus - 1f;
            float flatKnockbackBonus = genericFlatKnockbackBonus + classFlatKnockbackBonus;

            float totalKnockbackBonus = addKnockbackBonus + partKnockbackBonus;

            switch (totalOrmultOrflat)
            {
                case "total":
                    return totalKnockbackBonus;
                case "mult":
                    return multKnockbackBonus;
                case "flat":
                    return flatKnockbackBonus;

            }
            return 0f;
        }

        public static int DamageCalc(int damage, Player player)
        {
            float totalDamageBonus = BonusDamage(player, "total");
            float flatDamageBonus = BonusDamage(player, "flat");
            float multDamageBonus = BonusDamage(player, "mult");

            damage = (int)((((damage * totalDamageBonus) * multDamageBonus) + flatDamageBonus));

            player.GetDamage(DamageClass.Generic).Flat += flatDamageBonus;
            player.GetDamage(DamageClass.Generic) *= totalDamageBonus;

            return damage;

        }

        // Returning a value is not required as Crit Chance bonuses work fine being increased with GetCritChance
        public static void CritChanceCalc(int critChance, Player player)
        {
            float critChanceBonus = BonusCritChance(player);

            // 4 is taken away from the Weapon's Crit Chance to take into account the base Crit Chance
            critChance = (int)((critChance - 4) + critChanceBonus);

            player.GetCritChance(DamageClass.Generic) += critChanceBonus;
        }

        public static int AttackSpeedCalc(int attackSpeed, Player player)
        {
            float attackSpeedBonus = BonusAttackSpeed(player);

            attackSpeed = (int)(attackSpeed / attackSpeedBonus);

            player.GetAttackSpeed(DamageClass.Generic) += attackSpeedBonus;

            return attackSpeed;
        }

        public static int KnockbackCalc(int knockback, Player player)
        {
            float totalKnockbackBonus = BonusKnockback(player, "total");
            float flatKnockbackBonus = BonusKnockback(player, "flat");
            float multKnockbackBonus = BonusKnockback(player, "mult");

            knockback = (int)((((knockback * totalKnockbackBonus) * multKnockbackBonus) + flatKnockbackBonus));

            player.GetKnockback(DamageClass.Generic).Flat += flatKnockbackBonus;
            player.GetKnockback(DamageClass.Generic) *= totalKnockbackBonus;

            return knockback;

        }
    }
}
