using MechMod.Common.Players;
using MechMod.Content.Items.MechWeapons;
using MechMod.Content.Mounts;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MechMod.Content.Items.MechBodies
{
    internal class FastBody : ModItem, IMechParts
    {
        public override void SetDefaults()
        {
            Item.width = 20; // The width of the item's hitbox in pixels.
            Item.height = 20; // The height of the item's hitbox in pixels.
            Item.value = Item.buyPrice(gold: 2);
            Item.rare = ItemRarityID.Green; // The rarity of the item.
        }

        public void ApplyStats(Player player, MechModPlayer modPlayer, MechWeaponsPlayer weaponsPlayer, ModularMech mech)
        {
            modPlayer.lifeBonus += 25; // 25 health bonus
            //mech.MountData.acceleration *= 1.2f; // 20% faster acceleration
            //mech.MountData.runSpeed *= 1.2f; // 20% faster run speed
            //mech.MountData.swimSpeed *= 1.2f; // 20% faster swim speed

            modPlayer.partEffectiveness[MechMod.legsIndex] = 1.5f; // 1.5x effectiveness for legs
            modPlayer.partEffectiveness[MechMod.armsIndex] = 0.75f; // 0.75x effectiveness for arms
        }

        public void BodyOffsets(Player player, string body) { }
    }
}
