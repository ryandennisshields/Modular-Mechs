using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System.IO;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace MechMod.Common.Players
{
    /// <summary>
    /// Stores the many visual variables of the Mech, including textures, animation states and weapon drawing info.
    /// <para/>Any visual information that needs to be synced between clients is also handled here.
    /// </summary>

    public class MechVisualPlayer : ModPlayer
    {
        /// Textures
        public Asset<Texture2D> headTexture;
        public Asset<Texture2D> bodyTexture;
        public Asset<Texture2D> armsRTexture;
        public Asset<Texture2D> armsLTexture;
        public Asset<Texture2D> legsRTexture;
        public Asset<Texture2D> legsLTexture;
        public Asset<Texture2D> weaponTexture;

        public Vector2[] bodyOffsets = new Vector2[5]; // Offsets for Parts (Head, Arms, Legs) to align them properly with the Body

        /// Mech visual effects
        public int boosterTimer = 0; // Timer for booster visual effects
        public int stepTimer = 0; // Timer for mech step effects

        /// Weapon animation variables
        public float animationTimer; // Timer for mech weapon animation logic (constantly ticks down)
        public int animationProgress; // Progress for mech weapon animation logic (needs to be manually incremented and decremented)
        public bool animateOnce = false; // Used to control whether the mech weapon animation should only play once or loop
        // Used for controlling the current arm frame
        public int armRFrame = -1;
        public int armLFrame = -1;
        // Total number of frames that the arm texture has (to include the many arm rotations/positions for weapon animation)
        public int armRAnimationFrames = 10;
        public int armLAnimationFrames = 14;
        public int useDirection; // Stores the last weapon use direction

        /// Weapon drawing variables
        public Vector2 weaponPosition = Vector2.Zero; // Used for positioning the weapon when it is drawn
        public float weaponRotation = 0f; // Used for rotating the weapon when it is drawn
        public Vector2 weaponOrigin = Vector2.Zero; // Used so a different origin can be set for rotation
        public float weaponScale = 1f; // Used so the weapon can be hidden when needed
        public SpriteEffects weaponSpriteEffects = SpriteEffects.None; // Used so the weapon's sprite can be flipped when needed

        #region Syncing Player Data (Networking)

        // Function to send out changes to server and other clients as a packet that gets collected in MechMod.HandlePacket
        public override void SyncPlayer(int toWho, int fromWho, bool newPlayer)
        {
            ModPacket packet = Mod.GetPacket();
            packet.Write((byte)MechMod.MessageType.VisualSync);
            packet.Write((byte)Player.whoAmI);
            packet.Write(animationTimer);
            packet.Write(animationProgress);
            packet.Write(useDirection);
            packet.Write(armRFrame);
            packet.Write(armLFrame);
            packet.Write(weaponPosition.X);
            packet.Write(weaponPosition.Y);
            packet.Write(weaponRotation);
            packet.Write(weaponOrigin.X);
            packet.Write(weaponOrigin.Y);
            packet.Write(weaponScale);
            packet.Write((int)weaponSpriteEffects);

            packet.Send(toWho, fromWho);
        }

        // Function to update the mech textures based on the currently equipped Parts
        // Called whenever the parts are syncing in MechModPlayer so the textures are also updated
        public void UpdateTextures(MechModPlayer modPlayer)
        {
            // For each Part texture:
            // 1. Check if the Part is not air (i.e. a part is equipped)
            // 2. If a Part is equipped, check if the power cell is active to determine which texture to use (powered or unpowered)
            // 3. Grab the texture's spritesheet using the Part's name and the appropriate path
            // 4. Set the texture variable to the grabbed texture
            if (!modPlayer.equippedParts[MechMod.headIndex].IsAir)
            {
                string headPath = modPlayer.powerCellActive
                    ? $"Content/Items/MechHeads/{modPlayer.equippedParts[MechMod.headIndex].ModItem.GetType().Name}Visual"
                    : $"Content/Items/MechHeads/Pre{modPlayer.equippedParts[MechMod.headIndex].ModItem.GetType().Name}Visual";
                headTexture = Mod.Assets.Request<Texture2D>(headPath);
            }

            if (!modPlayer.equippedParts[MechMod.bodyIndex].IsAir)
            {
                string bodyPath = modPlayer.powerCellActive
                    ? $"Content/Items/MechBodies/{modPlayer.equippedParts[MechMod.bodyIndex].ModItem.GetType().Name}Visual"
                    : $"Content/Items/MechBodies/Pre{modPlayer.equippedParts[MechMod.bodyIndex].ModItem.GetType().Name}Visual";
                bodyTexture = Mod.Assets.Request<Texture2D>(bodyPath);
            }

            if (!modPlayer.equippedParts[MechMod.armsIndex].IsAir)
            {
                string armsR = modPlayer.powerCellActive
                    ? $"Content/Items/MechArms/{modPlayer.equippedParts[MechMod.armsIndex].ModItem.GetType().Name}RVisual"
                    : $"Content/Items/MechArms/Pre{modPlayer.equippedParts[MechMod.armsIndex].ModItem.GetType().Name}RVisual";
                string armsL = modPlayer.powerCellActive
                    ? $"Content/Items/MechArms/{modPlayer.equippedParts[MechMod.armsIndex].ModItem.GetType().Name}LVisual"
                    : $"Content/Items/MechArms/Pre{modPlayer.equippedParts[MechMod.armsIndex].ModItem.GetType().Name}LVisual";
                armsRTexture = Mod.Assets.Request<Texture2D>(armsR);
                armsLTexture = Mod.Assets.Request<Texture2D>(armsL);
            }

            if (!modPlayer.equippedParts[MechMod.legsIndex].IsAir)
            {
                string legsR = modPlayer.powerCellActive
                    ? $"Content/Items/MechLegs/{modPlayer.equippedParts[MechMod.legsIndex].ModItem.GetType().Name}RVisual"
                    : $"Content/Items/MechLegs/Pre{modPlayer.equippedParts[MechMod.legsIndex].ModItem.GetType().Name}RVisual";
                string legsL = modPlayer.powerCellActive
                    ? $"Content/Items/MechLegs/{modPlayer.equippedParts[MechMod.legsIndex].ModItem.GetType().Name}LVisual"
                    : $"Content/Items/MechLegs/Pre{modPlayer.equippedParts[MechMod.legsIndex].ModItem.GetType().Name}LVisual";
                legsRTexture = Mod.Assets.Request<Texture2D>(legsR);
                legsLTexture = Mod.Assets.Request<Texture2D>(legsL);
            }

            // For weapons, grab the item texture directly from the item if the weapon Part is not air
            if (!modPlayer.equippedParts[MechMod.weaponIndex].IsAir)
            {
                weaponTexture = TextureAssets.Item[modPlayer.equippedParts[MechMod.weaponIndex].type];
            }
            else
            {
                weaponTexture = null;
            }
        }

        // Function that receives changes from server and other clients from packets that get received in MechMod.HandlePacket
        public void RecievePlayerSync(BinaryReader reader)
        {
            animationTimer = reader.ReadSingle();
            animationProgress = reader.ReadInt32();
            useDirection = reader.ReadInt32();
            armRFrame = reader.ReadInt32();
            armLFrame = reader.ReadInt32();
            weaponPosition.X = reader.ReadSingle();
            weaponPosition.Y = reader.ReadSingle();
            weaponRotation = reader.ReadSingle();
            weaponOrigin.X = reader.ReadSingle();
            weaponOrigin.Y = reader.ReadSingle();
            weaponScale = reader.ReadSingle();
            weaponSpriteEffects = (SpriteEffects)reader.ReadInt32();
        }

        // These functions work together to detect changes in the player data and sync them between clients,
        // removing the need to manually call SyncPlayer whenever a change in data/variables is made

        // Function that copies other client's data to then be checked against this client in SendClientChanges
        public override void CopyClientState(ModPlayer targetCopy)
        {
            var clone = (MechVisualPlayer)targetCopy;

            clone.animationTimer = animationTimer;
            clone.animationProgress = animationProgress;
            clone.useDirection = useDirection;
            clone.armRFrame = armRFrame;
            clone.armLFrame = armLFrame;
            clone.weaponPosition = weaponPosition;
            clone.weaponRotation = weaponRotation;
            clone.weaponOrigin = weaponOrigin;
            clone.weaponScale = weaponScale;
            clone.weaponSpriteEffects = weaponSpriteEffects;
        }

        // Function that runs if CopyClientState detects a change, syncing the changes between clients
        public override void SendClientChanges(ModPlayer clientPlayer)
        {
            var clone = (MechVisualPlayer)clientPlayer;
            if  (animationTimer != clone.animationTimer ||
                animationProgress != clone.animationProgress ||
                useDirection != clone.useDirection ||
                armRFrame != clone.armRFrame ||
                armLFrame != clone.armLFrame ||
                weaponPosition != clone.weaponPosition ||
                weaponRotation != clone.weaponRotation ||
                weaponOrigin != clone.weaponOrigin ||
                weaponScale != clone.weaponScale ||
                weaponSpriteEffects != clone.weaponSpriteEffects
                )
            {
                SyncPlayer(-1, Main.myPlayer, false);
            }
        }

        #endregion

        public override void PreUpdate()
        {
            if (animationTimer > 0)
                animationTimer--; // Count down the animation timer
        }

    }
}
