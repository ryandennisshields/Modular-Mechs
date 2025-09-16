using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.Events;
using Terraria.GameContent.Personalities;
using Terraria.GameContent.UI;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace MechMod.Content.NPCs
{
    /// <summary>
    /// NPC that sells everything related to Mechs, including parts, weapons, modules and more.
    /// <para/> Also provides tips on how to use Mechs.
    /// </summary>

    [AutoloadHead]
    public class Mechanist : ModNPC
    {
        public int numberOfTimesTalkedTo = 0; // Track number of times talked to for dialogue purposes

        private static int shimmerHeadIndex; // Index for shimmer head texture
        private static Profiles.StackedNPCProfile NPCProfile; // Stores the NPC profiles for normal and shimmer variants for visuals

        public override void Load()
        {
            shimmerHeadIndex = Mod.AddNPCHeadTexture(Type, Texture + "_Shimmer_Head"); // Register the shimmer head texture
        }

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 24;

            NPCID.Sets.ExtraFramesCount[Type] = 8;
            NPCID.Sets.AttackFrameCount[Type] = 3;
            NPCID.Sets.DangerDetectRange[Type] = 700;
            NPCID.Sets.AttackType[Type] = 1;
            NPCID.Sets.AttackTime[Type] = 8;
            NPCID.Sets.AttackAverageChance[Type] = 1;
            NPCID.Sets.HatOffsetY[Type] = 4;
            NPCID.Sets.ShimmerTownTransform[NPC.type] = true;
            NPCID.Sets.ShimmerTownTransform[Type] = true;
            NPCID.Sets.FaceEmote[Type] = ModContent.EmoteBubbleType<MechanistEmote>();

            NPCID.Sets.NPCBestiaryDrawModifiers drawModifiers = new()
            {
                Velocity = 1f, // Make NPC walk to the right in bestiary
            };

            NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, drawModifiers);

            NPC.Happiness
                .SetBiomeAffection<UndergroundBiome>(AffectionLevel.Like)
                .SetBiomeAffection<SnowBiome>(AffectionLevel.Like)
                .SetBiomeAffection<DesertBiome>(AffectionLevel.Hate)
                .SetNPCAffection(NPCID.Cyborg, AffectionLevel.Love)
                .SetNPCAffection(NPCID.Steampunker, AffectionLevel.Love)
                .SetNPCAffection(NPCID.GoblinTinkerer, AffectionLevel.Like)
                .SetNPCAffection(NPCID.Mechanic, AffectionLevel.Like)
                .SetNPCAffection(NPCID.Dryad , AffectionLevel.Dislike)
                .SetNPCAffection(NPCID.WitchDoctor, AffectionLevel.Hate)
                ;

            // Set up the NPC profiles for normal and shimmer variants for visuals
            NPCProfile = new Profiles.StackedNPCProfile(
                new Profiles.DefaultNPCProfile(Texture, NPCHeadLoader.GetHeadSlot(HeadTexture)),
                new Profiles.DefaultNPCProfile(Texture + "_Shimmer", shimmerHeadIndex)
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
            bestiaryEntry.Info.AddRange([
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Surface, // NPC spawns on surface

				new FlavorTextBestiaryInfoElement("Having spent many years perfecting his craft, the Mechanitor provides his technology to others with a sense of pride."), // Bestiary flavor text
            ]);
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            if (Main.netMode != NetmodeID.Server && NPC.life <= 0) // If the NPC is killed,
            {
                // Get gore pieces
                string variant = "";
                if (NPC.IsShimmerVariant) variant += "_Shimmer";
                int hatGore = NPC.GetPartyHatGore();
                int headGore = Mod.Find<ModGore>($"{Name}_Gore{variant}_Head").Type;
                int armGore = Mod.Find<ModGore>($"{Name}_Gore{variant}_Arm").Type;
                int legGore = Mod.Find<ModGore>($"{Name}_Gore{variant}_Leg").Type;

                // Spawn gore pieces
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
            for (int k = 0; k < Main.maxPlayers; k++)
            {
                // Only spawn if there is an active player
                Player player = Main.player[k];
                if (!player.active)
                {
                    continue;
                }

                if (NPC.downedBoss2) // If Brain of Cthulhu or Eater of Worlds has been defeated,
                {
                    return true; // Allow the NPC to spawn
                }
            }
            return false;
        }

        public override ITownNPCProfile TownNPCProfile()
        {
            return NPCProfile;
        }

        public override List<string> SetNPCNameList()
        {
            return [
                "Ryan",
                "Austin",
                "Geoff",
                "Geoffrey",
                "Micheal",
                "Jimothy",
                "Jensen",
                "Caleb",
                "Cooper",
                "Michigan"
            ];
        }

        public override string GetChat()
        {
            WeightedRandom<string> chat = new(); // Create a weighted random for chat options

            // Proceed to go through dialogue options
            // Each dialogue option, if the condition is met, is added to the weighted random
            int witchDoctor = NPC.FindFirstNPC(NPCID.WitchDoctor); // Check and store the Witch Doctor's NPC index
            Condition golemCondition = Condition.DownedGolem; // Condition for Golem being defeated
            if (witchDoctor >= 0) // If the Witch Doctor is present,
            {
                if (golemCondition.IsMet()) // If Golem has been defeated,
                    chat.Add(Language.GetTextValue("Mods.MechMod.NPCs.Mechanist.Dialogue.WitchDoctorAfterGolem", Main.npc[witchDoctor].GivenName)); // Add "Witch Doctor after Golem" dialogue
                if (NPC.GivenName == "Ryan" && Main.rand.NextBool(30)) // If the Mechanist's name is Ryan and with a 1 in 30 chance,
                chat.Add(Language.GetTextValue("Mods.MechMod.NPCs.Mechanist.Dialogue.TheFunny", Main.npc[witchDoctor].GivenName)); // Add "The Funny" dialogue
            }
            Condition mechCondition = Condition.DownedMechBossAny; // Condition for any Mechanical Boss being defeated
            if (mechCondition.IsMet()) // If any Mechanical Boss has been defeated,
                chat.Add(Language.GetTextValue("Mods.MechMod.NPCs.Mechanist.Dialogue.AfterMechBoss")); // Add "After Mech Boss" dialogue
            else // If no Mechanical Boss has been defeated,
                chat.Add(Language.GetTextValue("Mods.MechMod.NPCs.Mechanist.Dialogue.BeforeMechBoss")); // Add "Before Mech Boss" dialogue
            if (Main.raining) // If it is raining,
                chat.Add(Language.GetTextValue("Mods.MechMod.NPCs.Mechanist.Dialogue.Raining")); // Add "Raining" dialogue
            if (Main.IsItStorming) // If it is storming,
                chat.Add(Language.GetTextValue("Mods.MechMod.NPCs.Mechanist.Dialogue.Storming")); // Add "Storming" dialogue
            if (Main.bloodMoon) // If it is a Blood Moon,
            {
                chat.Add(Language.GetTextValue("Mods.MechMod.NPCs.Mechanist.Dialogue.BloodMoon1")); // Add "Blood Moon 1" dialogue
                chat.Add(Language.GetTextValue("Mods.MechMod.NPCs.Mechanist.Dialogue.BloodMoon2")); // Add "Blood Moon 2" dialogue
            }
            if (BirthdayParty.PartyIsUp) // If there is a party,
                chat.Add(Language.GetTextValue("Mods.MechMod.NPCs.Mechanist.Dialogue.Party")); // Add "Party" dialogue
            if (Main.LocalPlayer.ZoneGraveyard) // If the player is in a graveyard,
                chat.Add(Language.GetTextValue("Mods.MechMod.NPCs.Mechanist.Dialogue.Graveyard")); // Add "Graveyard" dialogue
            Condition hardmodeCondition = Condition.Hardmode; // Condition for Hardmode
            if (hardmodeCondition.IsMet()) // If Hardmode has been reached,
                chat.Add(Language.GetTextValue("Mods.MechMod.NPCs.Mechanist.Dialogue.Hardmode")); // Add "Hardmode" dialogue
            else // If Hardmode has not been reached,
                chat.Add(Language.GetTextValue("Mods.MechMod.NPCs.Mechanist.Dialogue.PreHardmode")); // Add "Pre-Hardmode" dialogue
            numberOfTimesTalkedTo++; // Increment the number of times talked to
            if (numberOfTimesTalkedTo >= 10) // If the player has talked to the NPC 10 or more times,
                chat.Add(Language.GetTextValue("Mods.MechMod.NPCs.Mechanist.Dialogue.TalkALot")); // Add "Talk A Lot" dialogue
            // Add standard dialogue
            chat.Add(Language.GetTextValue("Mods.MechMod.NPCs.Mechanist.Dialogue.StandardDialogue1"));
            chat.Add(Language.GetTextValue("Mods.MechMod.NPCs.Mechanist.Dialogue.StandardDialogue2"));
            chat.Add(Language.GetTextValue("Mods.MechMod.NPCs.Mechanist.Dialogue.StandardDialogue3"));

            string chosenChat = chat; // Now that all possible options have been added to the weighted random, choose a random dialogue option from the weighted random

            return chosenChat; // Return the chosen dialogue option
        }

        public override void SetChatButtons(ref string button, ref string button2)
        { 
            button = Language.GetTextValue("LegacyInterface.28"); // Shop button
            button2 = "Tips";
        }

        private int tipIndex = 0; // Current tip index
        private int tipCount = 6; // Total number of tips available

        public override void OnChatButtonClicked(bool firstButton, ref string shop)
        {
            if (firstButton) // Shop button
            { 
                shop = "Shop"; // Open the shop
            }
            if (!firstButton) // Tips button
            {
                tipIndex++; // Increment the tip index
                Main.npcChatText = Language.GetTextValue($"Mods.MechMod.NPCs.Mechanist.Dialogue.Tip{tipIndex}"); // Show the tip corresponding to the current tip index
                if (tipIndex >= tipCount) // If the tip index exceeds the total number of tips,
                {
                    tipIndex = 0; // Reset the tip index to 0
                }
            }
        }

        public override void AddShops()
        {
            // Conditions for shop inventory
            Condition condition1 = Condition.DownedSkeletron;
            Condition condition2 = Condition.Hardmode;
            //Condition condition3 = Condition.DownedMechBossAny;
            Condition condition4 = Condition.DownedGolem;

            // Set up the shop inventory
            var npcShop = new NPCShop(Type, "Shop")
                    // Base (Parts = 1 Gold, Weapons = 2 Gold, Passive Modules = 4 Gold, Active Modules = 8 Gold)
                    .Add<Items.MechMisc.MechSpawner>()
                    .Add<Items.MechMisc.MechBench>()
                    .Add<Items.MechHeads.BaseHead>()
                    .Add<Items.MechBodies.BaseBody>()
                    .Add<Items.MechArms.BaseArms>()
                    .Add<Items.MechLegs.BaseLegs>()
                    .Add<Items.MechWeapons.BaseGun>()
                    .Add<Items.MechWeapons.BaseSword>()
                    .Add<Items.MechModules.Passive.Brace>()
                    .Add<Items.MechModules.Passive.NuclearEject>()
                    .Add<Items.MechModules.Passive.Spikes>()
                    .Add<Items.MechModules.Active.MissileLauncher>()
                    .Add<Items.MechModules.Active.Repair>()
                    // Condition 1 (Parts = 2 Gold, Weapons = 4 Gold)
                    .Add<Items.MechHeads.FastHead>(condition1)
                    .Add<Items.MechBodies.FastBody>(condition1)
                    .Add<Items.MechArms.FastArms>(condition1)
                    .Add<Items.MechLegs.FastLegs>(condition1)
                    .Add<Items.MechWeapons.LaserGun>(condition1)
                    // Condition 2 (Parts = 4 Gold, Boosters = 10 Gold, Weapons = 8 Gold, Passive Modules = 10 Gold)
                    .Add<Items.MechMisc.PowerCell>(condition2)
                    .Add<Items.MechHeads.SlowHead>(condition2)
                    .Add<Items.MechBodies.SlowBody>(condition2)
                    .Add<Items.MechArms.SlowArms>(condition2)
                    .Add<Items.MechLegs.SlowLegs>(condition2)
                    .Add<Items.MechBoosters.BaseBooster>(condition2)
                    .Add<Items.MechBoosters.FastBooster>(condition2)
                    .Add<Items.MechBoosters.SlowBooster>(condition2)
                    .Add<Items.MechWeapons.ProjSword>(condition2)
                    .Add<Items.MechModules.Passive.Hover>(condition2)
                                        // Condition 3
                                        //.Add<Items.MechModules.Passive.Brace>(condition3)
                                        //.Add<Items.MechModules.Passive.Hover>(condition3)
                                        //.Add<Items.MechModules.Passive.NuclearEject>(condition3)
                                        //.Add<Items.MechModules.Passive.Spikes>(condition3)
                                        // Condition 4
                                        .Add<Items.MechWeapons.BaseLauncher>(condition4)
                    //.Add<Items.MechModules.Active.MissileLauncher>(condition4)
                    //.Add<Items.MechModules.Active.Repair>(condition4)
                    ;
            npcShop.Register();
        }

        public override bool CanGoToStatue(bool toKingStatue) => true;

        public override void TownNPCAttackStrength(ref int damage, ref float knockback)
        {
            damage = 3;
            knockback = 0f;
        }

        public override void TownNPCAttackCooldown(ref int cooldown, ref int randExtraCooldown)
        {
            cooldown = 1;
            randExtraCooldown = 1;
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

        public override void TownNPCAttackShoot(ref bool inBetweenShots)
        {
            inBetweenShots = true;
        }

        public override void DrawTownAttackGun(ref Texture2D item, ref Rectangle itemFrame, ref float scale, ref int horizontalHoldoutOffset)
        {
            item = TextureAssets.Item[ItemID.Minishark].Value;
            Main.GetItemDrawFrame(ItemID.Minishark, out _, out itemFrame);
            horizontalHoldoutOffset = (int)Main.DrawPlayerItemPos(1f, ItemID.Minishark).X - 20;
        }
    }

    /// <summary>
    /// Emote bubble for the Mechanist NPC.
    /// </summary>

    public class MechanistEmote : ModEmoteBubble
    {
        public override string Texture => "MechMod/Content/NPCs/MechanistEmote";
        public override void SetStaticDefaults()
        {
            AddToCategory(EmoteID.Category.Town);
        }
    }
}