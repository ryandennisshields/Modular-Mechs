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
        public static DamageClass DamageClass { get; set; } = DamageClass.Default;

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

        public static int DamageCalc(int baseDamage, Player player, DamageClass damageClass = null, bool total = true, bool mult = true, bool flat = true)
        {
            var modPlayer = player.GetModPlayer<MechModPlayer>();

            float genericAddDamageBonus = player.GetDamage(DamageClass.Generic).Additive;
            float classAddDamageBonus = player.GetDamage(damageClass != null ? damageClass : DamageClass).Additive;
            float genericMultDamageBonus = player.GetDamage(DamageClass.Generic).Multiplicative;
            float classMultDamageBonus = player.GetDamage(damageClass != null ? damageClass : DamageClass).Multiplicative;
            float genericFlatDamageBonus = player.GetDamage(DamageClass.Generic).Flat;
            float classFlatDamageBonus = player.GetDamage(damageClass != null ? damageClass : DamageClass).Flat;

            //switch (totalOrmultOrflat)
            //{
            //    case "total":
            //        float addDamageBonus = genericAddDamageBonus + classAddDamageBonus - 1f;
            //        float totalDamageBonus = addDamageBonus + modPlayer.upgradeDamageBonus + partDamageBonus;
            //        Main.NewText($"{DamageClass}, {addDamageBonus}, {modPlayer.upgradeDamageBonus}, {partDamageBonus}");
            //        return totalDamageBonus;
            //    case "mult":
            //        float multDamageBonus = genericMultDamageBonus + classMultDamageBonus - 1f;
            //        return multDamageBonus;
            //    case "flat":
            //        float flatDamageBonus = genericFlatDamageBonus + classFlatDamageBonus;
            //        return flatDamageBonus;

            //}

            float totalDamageBonus = 0f;
            float multDamageBonus = 0f;
            float flatDamageBonus = 0f;

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

            baseDamage = (int)((((baseDamage * totalDamageBonus) * multDamageBonus) + flatDamageBonus));

            return baseDamage;
        }

        public static int CritChanceCalc(int baseCritChance, Player player, DamageClass damageClass = null)
        {
            var modPlayer = player.GetModPlayer<MechModPlayer>();

            float genericCritChanceBonus = player.GetCritChance(DamageClass.Generic);
            float classCritChanceBonus = player.GetCritChance(damageClass != null ? damageClass : DamageClass);

            float critChanceBonus = genericCritChanceBonus + classCritChanceBonus + partCritChanceBonus;

            // 4 is taken away from the Weapon's Crit Chance to take into account the base Crit Chance
            baseCritChance = (int)((baseCritChance - 4) + critChanceBonus);

            return baseCritChance;

            // MIGHT NEED TO RETURN A VALUE, INCLUDING OTHER CALCS? (Would be better to have it be added to the player's crit chance maybe, but
            // it might be better to have these values be something exclusive to the mech as a seperate thing instead of effecting what
            // the player's stats read as)
            //player.GetCritChance(DamageClass.Generic) += critChanceBonus;
        }

        public static int AttackSpeedCalc(int baseAttackSpeed, Player player, DamageClass damageClass = null)
        {
            var modPlayer = player.GetModPlayer<MechModPlayer>();

            float genericAttackSpeedBonus = player.GetAttackSpeed(DamageClass.Generic);
            float classAttackSpeedBonus = player.GetAttackSpeed(damageClass != null ? damageClass : DamageClass);

            float attackSpeedBonus = genericAttackSpeedBonus + classAttackSpeedBonus + partAttackSpeedBonus - 1f;

            baseAttackSpeed = (int)(baseAttackSpeed / attackSpeedBonus);

            return baseAttackSpeed;
        }

        public static int KnockbackCalc(int baseKnockback, Player player, DamageClass damageClass = null, bool total = true, bool mult = true, bool flat = true)
        {
            var modPlayer = player.GetModPlayer<MechModPlayer>();

            float genericAddKnockbackBonus = player.GetKnockback(DamageClass.Generic).Additive;
            float classAddKnockbackBonus = player.GetKnockback(damageClass != null ? damageClass : DamageClass).Additive;
            float genericMultKnockbackBonus = player.GetKnockback(DamageClass.Generic).Multiplicative;
            float classMultKnockbackBonus = player.GetKnockback(damageClass != null ? damageClass : DamageClass).Multiplicative;
            float genericFlatKnockbackBonus = player.GetKnockback(DamageClass.Generic).Flat;
            float classFlatKnockbackBonus = player.GetKnockback(damageClass != null ? damageClass : DamageClass).Flat;

            //float addKnockbackBonus = genericAddKnockbackBonus + classAddKnockbackBonus - 1f;
            //float multKnockbackBonus = genericMultKnockbackBonus + classMultKnockbackBonus - 1f;
            //float flatKnockbackBonus = genericFlatKnockbackBonus + classFlatKnockbackBonus;

            //float totalKnockbackBonus = addKnockbackBonus + partKnockbackBonus;

            //switch (totalOrmultOrflat)
            //{
            //    case "total":
            //        return totalKnockbackBonus;
            //    case "mult":
            //        return multKnockbackBonus;
            //    case "flat":
            //        return flatKnockbackBonus;

            //}

            float totalKnockbackBonus = 0f;
            float multKnockbackBonus = 0f;
            float flatKnockbackBonus = 0f;

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

            baseKnockback = (int)((((baseKnockback * totalKnockbackBonus) * multKnockbackBonus) + flatKnockbackBonus));

            //player.GetKnockback(DamageClass.Generic).Flat += flatKnockbackBonus;
            //player.GetKnockback(DamageClass.Generic) *= totalKnockbackBonus;

            return baseKnockback;
        }
    }
}
