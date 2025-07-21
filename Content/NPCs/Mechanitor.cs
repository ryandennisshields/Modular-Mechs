using MechMod;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.GameContent.Personalities;
using Terraria.GameContent.UI;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.Utilities;
using static Terraria.GameContent.Bestiary.IL_BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions;

namespace MechMod.Content.NPCs
{
    [AutoloadHead]
    public class Mechanitor : ModNPC
    {
        public const string shopName = "Shop";
        public int numberOfTimesTalkedTo = 0;

        private static int shimmerHeadIndex;
        private static Profiles.StackedNPCProfile NPCProfile;

        public override void Load()
        {
            shimmerHeadIndex = Mod.AddNPCHeadTexture(Type, Texture + "_Shimmer_Head");
        }

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 25;

            NPCID.Sets.ExtraFramesCount[Type] = 9;
            NPCID.Sets.AttackFrameCount[Type] = 4;
            NPCID.Sets.DangerDetectRange[Type] = 700;
            NPCID.Sets.AttackType[Type] = 1;
            NPCID.Sets.AttackTime[Type] = 90;
            NPCID.Sets.AttackAverageChance[Type] = 30;
            NPCID.Sets.HatOffsetY[Type] = 4;
            NPCID.Sets.ShimmerTownTransform[NPC.type] = true;

            NPCID.Sets.ShimmerTownTransform[Type] = true;

            //NPCID.Sets.FaceEmote[Type] = ModContent.EmoteBubbleType<ExamplePersonEmote>();

            NPCID.Sets.NPCBestiaryDrawModifiers drawModifiers = new NPCID.Sets.NPCBestiaryDrawModifiers()
            {
                Velocity = 1f,
                Direction = 1
            };

            NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, drawModifiers);

            NPC.Happiness
                .SetBiomeAffection<ForestBiome>(AffectionLevel.Like)
                .SetBiomeAffection<SnowBiome>(AffectionLevel.Dislike)
                //.SetBiomeAffection<ExampleSurfaceBiome>(AffectionLevel.Love)
                .SetNPCAffection(NPCID.Dryad, AffectionLevel.Love)
                .SetNPCAffection(NPCID.Guide, AffectionLevel.Like)
                .SetNPCAffection(NPCID.Merchant, AffectionLevel.Dislike)
                .SetNPCAffection(NPCID.Demolitionist, AffectionLevel.Hate);

            NPCProfile = new Profiles.StackedNPCProfile(
                new Profiles.DefaultNPCProfile(Texture, NPCHeadLoader.GetHeadSlot(HeadTexture), Texture + "_Party"),
                new Profiles.DefaultNPCProfile(Texture + "_Shimmer", shimmerHeadIndex, Texture + "_Shimmer_Party")
            );
        }

        public override void SetDefaults()
        {
            NPC.townNPC = true;
            NPC.friendly = true;
            NPC.width = 18;
            NPC.height = 40;
            NPC.aiStyle = 7;
            NPC.damage = 10;
            NPC.defense = 15;
            NPC.lifeMax = 250;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.knockBackResist = 0.5f;

            AnimationType = NPCID.Guide;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Surface,

				new FlavorTextBestiaryInfoElement("Hailing from a mysterious greyscale cube world, the Example Person is here to help you understand everything about tModLoader."),
            });
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            if (Main.netMode != NetmodeID.Server && NPC.life <= 0)
            {
                string variant = "";
                if (NPC.IsShimmerVariant) variant += "_Shimmer";
                if (NPC.altTexture == 1) variant += "_Party";
                int hatGore = NPC.GetPartyHatGore();
                int headGore = Mod.Find<ModGore>($"{Name}_Gore{variant}_Head").Type;
                int armGore = Mod.Find<ModGore>($"{Name}_Gore{variant}_Arm").Type;
                int legGore = Mod.Find<ModGore>($"{Name}_Gore{variant}_Leg").Type;

                if (hatGore > 0)
                {
                    Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, hatGore);
                }
                Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, headGore, 1f);
                Gore.NewGore(NPC.GetSource_Death(), NPC.position + new Vector2(0, 20), NPC.velocity, armGore);
                Gore.NewGore(NPC.GetSource_Death(), NPC.position + new Vector2(0, 20), NPC.velocity, armGore);
                Gore.NewGore(NPC.GetSource_Death(), NPC.position + new Vector2(0, 34), NPC.velocity, legGore);
                Gore.NewGore(NPC.GetSource_Death(), NPC.position + new Vector2(0, 34), NPC.velocity, legGore);
            }
        }

        public override bool CanTownNPCSpawn(int numTownNPCs)
        {
            //for (int k = 0; k < Main.maxPlayers; k++)
            //{
            //    Player player = Main.player[k];
            //    if (!player.active)
            //    {
            //        continue;
            //    }

            //    // Player has to have either an ExampleItem or an ExampleBlock in order for the NPC to spawn
            //    if (player.inventory.Any(item => item.type == ModContent.ItemType<ExampleItem>() || item.type == ModContent.ItemType<Items.Placeable.ExampleBlock>()))
            //    {
            //        return true;
            //    }
            //}

            return false;
        }

        public override ITownNPCProfile TownNPCProfile()
        {
            return NPCProfile;
        }

        public override List<string> SetNPCNameList()
        {
            return new List<string>() {
                "Someone",
                "Somebody",
                "Blocky",
                "Colorless"
            };
        }

        public override void FindFrame(int frameHeight)
        {
            /*npc.frame.Width = 40;
			if (((int)Main.time / 10) % 2 == 0)
			{
				npc.frame.X = 40;
			}
			else
			{
				npc.frame.X = 0;
			}*/
        }

        public override string GetChat()
        {
            WeightedRandom<string> chat = new WeightedRandom<string>();

            int partyGirl = NPC.FindFirstNPC(NPCID.PartyGirl);
            if (partyGirl >= 0 && Main.rand.NextBool(4))
            {
                chat.Add(Language.GetTextValue("Mods.MechMod.NPCs.Mechanitor.Dialogue.PartyGirlDialogue", Main.npc[partyGirl].GivenName));
            }
            chat.Add(Language.GetTextValue("Mods.MechMod.NPCs.Mechanitor.Dialogue.StandardDialogue1"));
            chat.Add(Language.GetTextValue("Mods.MechMod.NPCs.Mechanitor.Dialogue.StandardDialogue2"));
            chat.Add(Language.GetTextValue("Mods.MechMod.NPCs.Mechanitor.Dialogue.StandardDialogue3"));
            chat.Add(Language.GetTextValue("Mods.MechMod.NPCs.Mechanitor.Dialogue.StandardDialogue4"));
            chat.Add(Language.GetTextValue("Mods.MechMod.NPCs.Mechanitor.Dialogue.CommonDialogue"), 5.0);
            chat.Add(Language.GetTextValue("Mods.MechMod.NPCs.Mechanitor.Dialogue.RareDialogue"), 0.1);

            numberOfTimesTalkedTo++;
            if (numberOfTimesTalkedTo >= 10)
            {
                chat.Add(Language.GetTextValue("Mods.MechMod.NPCs.Mechanitor.Dialogue.TalkALot"));
            }

            string chosenChat = chat;

            //if (chosenChat == Language.GetTextValue("Mods.MechMod.NPCs.Mechanitor.Dialogue.StandardDialogue4"))
            //{
            //    Main.npcChatCornerItem = ItemID.HiveBackpack;
            //}

            return chosenChat;
        }

        public override void SetChatButtons(ref string button, ref string button2)
        { 
            button = Language.GetTextValue("LegacyInterface.28");
            button2 = "Tips";
        }

        private int tipIndex = 0;
        private int tipCount = 8;

        public override void OnChatButtonClicked(bool firstButton, ref string shop)
        {
            if (firstButton) // Shop button
            { 
                shop = shopName;
            }
            if (!firstButton) // Tips button
            {
                tipIndex++;
                Main.npcChatText = Language.GetTextValue($"Mods.MechMod.NPCs.Mechanitor.Dialogue.Tip{tipIndex}");
                if (tipIndex >= tipCount)
                {
                    tipIndex = 0;
                }
            }
        }

        public override void AddShops()
        {
            Condition condition1 = Condition.DownedEowOrBoc;
            Condition condition2 = Condition.Hardmode;
            Condition condition3 = Condition.DownedMechBossAll;
            Condition condition4 = Condition.DownedGolem;

            var npcShop = new NPCShop(Type, shopName)
                    // Base (Parts = 1 Gold, Weapons = 2 Gold)
                    .Add<Items.MechSpawner.MechSpawner>()
                    .Add<Items.MechBench.MechBench>()
                    .Add<Items.MechHeads.BaseHead>()
                    .Add<Items.MechBodies.BaseBody>()
                    .Add<Items.MechArms.BaseArms>()
                    .Add<Items.MechLegs.BaseLegs>()
                    .Add<Items.MechWeapons.BaseGun>()
                    .Add<Items.MechWeapons.BaseSword>()
                    // Condition 1 (Parts = 2 Gold, Weapons = 4 Gold)
                    .Add<Items.MechHeads.FastHead>(condition1)
                    .Add<Items.MechBodies.FastBody>(condition1)
                    .Add<Items.MechArms.FastArms>(condition1)
                    .Add<Items.MechLegs.FastLegs>(condition1)
                    .Add<Items.MechWeapons.LaserGun>(condition1)
                    // Condition 2 (Parts = 4 Gold, Boosters = 20 Gold, Weapons = 8 Gold)
                    .Add<Items.MechHeads.SlowHead>(condition2)
                    .Add<Items.MechBodies.SlowBody>(condition2)
                    .Add<Items.MechArms.SlowArms>(condition2)
                    .Add<Items.MechLegs.SlowLegs>(condition2)
                    .Add<Items.MechBoosters.BaseBooster>(condition2)
                    .Add<Items.MechBoosters.FastBooster>(condition2)
                    .Add<Items.MechBoosters.SlowBooster>(condition2)
                    .Add<Items.MechWeapons.BaseLauncher>(condition2)
                    .Add<Items.MechWeapons.ProjSword>(condition2)
                    // Condition 3 (Modules = 10 Gold)
                    .Add<Items.MechModules.Passive.Brace>(condition3)
                    .Add<Items.MechModules.Passive.Hover>(condition3)
                    .Add<Items.MechModules.Passive.NuclearEject>(condition3)
                    .Add<Items.MechModules.Passive.Spikes>(condition3)
                    // Condition 4 (Modules = 20 Gold)
                    .Add<Items.MechModules.Active.MissileLauncher>(condition4)
                    .Add<Items.MechModules.Active.Repair>(condition4)
                    ;
            npcShop.Register();
        }

        //public override void ModifyActiveShop(string shopName, Item[] items)
        //{
        //    foreach (Item item in items)
        //    {
        //        // Skip 'air' items and null items.
        //        if (item == null || item.type == ItemID.None)
        //        {
        //            continue;
        //        }

        //        // If NPC is shimmered then reduce all prices by 50%.
        //        if (NPC.IsShimmerVariant)
        //        {
        //            int value = item.shopCustomPrice ?? item.value;
        //            item.shopCustomPrice = value / 2;
        //        }
        //    }
        //}

        //public override void ModifyNPCLoot(NPCLoot npcLoot)
        //{
        //    npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<ExampleCostume>()));
        //}

        public override bool CanGoToStatue(bool toKingStatue) => true;

        public override void TownNPCAttackStrength(ref int damage, ref float knockback)
        {
            damage = 20;
            knockback = 4f;
        }

        public override void TownNPCAttackCooldown(ref int cooldown, ref int randExtraCooldown)
        {
            cooldown = 30;
            randExtraCooldown = 30;
        }

        public override void TownNPCAttackProj(ref int projType, ref int attackDelay)
        {
            projType = ProjectileID.Bullet;
            attackDelay = 1;
        }

        public override void TownNPCAttackProjSpeed(ref float multiplier, ref float gravityCorrection, ref float randomOffset)
        {
            multiplier = 12f;
            randomOffset = 2f;
        }

        public override void LoadData(TagCompound tag)
        {
            numberOfTimesTalkedTo = tag.GetInt("numberOfTimesTalkedTo");
        }

        public override void SaveData(TagCompound tag)
        {
            tag["numberOfTimesTalkedTo"] = numberOfTimesTalkedTo;
        }

        // Let the NPC "talk about" minion boss
        //public override int? PickEmote(Player closestPlayer, List<int> emoteList, WorldUIAnchor otherAnchor)
        //{
        //    // By default this NPC will have a chance to use the Minion Boss Emote even if Minion Boss is not downed yet
        //    int type = ModContent.EmoteBubbleType<MinionBossEmote>();
        //    // If the NPC is talking to the Demolitionist, it will be more likely to react with angry emote
        //    if (otherAnchor.entity is NPC { type: NPCID.Demolitionist })
        //    {
        //        type = EmoteID.EmotionAnger;
        //    }

        //    // Make the selection more likely by adding it to the list multiple times
        //    for (int i = 0; i < 4; i++)
        //    {
        //        emoteList.Add(type);
        //    }

        //    // Use this or return null if you don't want to override the emote selection totally
        //    return base.PickEmote(closestPlayer, emoteList, otherAnchor);
        //}
    }
}