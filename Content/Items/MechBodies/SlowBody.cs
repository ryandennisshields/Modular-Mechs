using MechMod.Common.Players;
using MechMod.Content.Items.MechWeapons;
using MechMod.Content.Mounts;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MechMod.Content.Items.MechBodies
{
    internal class SlowBody : ModItem, IMechParts
    {
        public override void SetDefaults()
        {
            Item.width = 20; // The width of the item's hitbox in pixels.
            Item.height = 20; // The height of the item's hitbox in pixels.
            Item.value = Item.buyPrice(gold: 4);
            Item.rare = ItemRarityID.Green; // The rarity of the item.
        }

        public void ApplyStats(Player player, MechModPlayer modPlayer, MechWeaponsPlayer weaponsPlayer, ModularMech mech)
        {
            modPlayer.lifeBonus += 100; // 100 health bonus
            //Weapons.partDamageBonus -= 0.1f; // 10% damage reduction

            modPlayer.partEffectiveness[MechMod.headIndex] = 2f; // 2x effectiveness for head
            modPlayer.partEffectiveness[MechMod.legsIndex] = 0.75f; // 0.75x effectiveness for legs
        }

        public void BodyOffsets(MechVisualPlayer visualPlayer, string body) { }
    }
}
