using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace MechMod.Common.Players
{
    public class MechVisualPlayer : ModPlayer
    {
        public float animationTimer; // Timer for mech weapon animation logic (constantly ticks down)
        public int animationProgress; // Progress for mech weapon animation logic (needs to be manually incremented and decremented)

        public int boosterTimer = 0;

        public int stepTimer = 0;
        public bool changeposition = false;

        public Asset<Texture2D> headTexture;
        public Asset<Texture2D> bodyTexture;
        public Asset<Texture2D> armsRTexture;
        public Asset<Texture2D> armsLTexture;
        public Asset<Texture2D> legsRTexture;
        public Asset<Texture2D> legsLTexture;
        public Asset<Texture2D> weaponTexture; // Used so the equipped weapon can be drawn while in use

        public Vector2[] bodyOffsets = new Vector2[5];

        public bool animateOnce = false; // Used to control whether the mech weapon animation should only play once or loop

        // Used for controlling the current arm frame
        public int armRFrame = -1;
        public int armLFrame = -1;
        // Total number of frames that the arm texture has (to include the many arm rotations/positions for weapon animation)
        public int armRAnimationFrames = 10;
        public int armLAnimationFrames = 14;

        public int useDirection; // Stores the last weapon use direction

        public Vector2 weaponPosition = Vector2.Zero; // Used for positioning the weapon when it is drawn
        public float weaponRotation = 0f; // Used for rotating the weapon when it is drawn
        public Vector2 weaponOrigin = Vector2.Zero; // Used so a different origin can be set for rotation
        public float weaponScale = 1f; // Used so the weapon can be hidden when needed
        public SpriteEffects weaponSpriteEffects = SpriteEffects.None; // Used so the weapon's sprite can be flipped when needed

        #region Syncing Player Data (Networking)

        // Send out changes to server and other clients
        public override void SyncPlayer(int toWho, int fromWho, bool newPlayer)
        {
            // Sync up the equipped Parts and animation details between clients
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

        public void UpdateTextures(MechModPlayer modPlayer)
        {
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

            if (!modPlayer.equippedParts[MechMod.weaponIndex].IsAir)
            {
                // Fallback to the item’s texture if no custom visual exists
                weaponTexture = TextureAssets.Item[modPlayer.equippedParts[MechMod.weaponIndex].type];
            }
            else
            {
                weaponTexture = null;
            }
        }

        // Receive changes from server and other clients
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

        // Check clients against this client to watch for changes
        // If a change is detected, SendClientChanges will sync the changes
        public override void CopyClientState(ModPlayer targetCopy)
        {
            var clone = (MechVisualPlayer)targetCopy;

            //clone.headTexture = headTexture;
            //clone.bodyTexture = bodyTexture;
            //clone.armsRTexture = armsRTexture;
            //clone.armsLTexture = armsLTexture;
            //clone.legsRTexture = legsRTexture;
            //clone.legsLTexture = legsLTexture;
            //clone.weaponTexture = weaponTexture;
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

        // If CopyClientState detects a change, this method will be called to sync the changes
        public override void SendClientChanges(ModPlayer clientPlayer)
        {
            var clone = (MechVisualPlayer)clientPlayer;
            //bool needsSync = false;
            //if (headTexture != clone.headTexture ||
            //    bodyTexture != clone.bodyTexture ||
            //    armsRTexture != clone.armsRTexture ||
            //    armsLTexture != clone.armsLTexture ||
            //    legsRTexture != clone.legsRTexture ||
            //    legsLTexture != clone.legsLTexture ||
            //    weaponTexture != clone.weaponTexture ||
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
                //needsSync = true;
            }

            //if (needsSync) // Only sync clients if needed
            //    SyncPlayer(-1, Main.myPlayer, false);
        }

        #endregion

        public override void PreUpdate()
        {
            if (animationTimer > 0)
                animationTimer--; // Count down the animation timer
        }

    }
}
