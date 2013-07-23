using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PattyPetitGiant
{
    class Compass : Item
    {
        private BetaEndLevelFag exit = null;

        private bool drawPointer;
        private Vector2 drawPos = Vector2.Zero;
        private Vector2 drawPos2 = Vector2.Zero;
        private float theta = 0.0f;

        private float offset = 0.0f;

        private AnimationLib.FrameAnimationSet img = null;
        private AnimationLib.FrameAnimationSet img2 = null;

        public Compass()
        {
            if (img == null)
            {
                img = AnimationLib.getFrameAnimationSet("compassArrow");
                img2 = AnimationLib.getFrameAnimationSet("compassOverlay");
            }

            drawPointer = false;
        }

        public void update(Player parent, GameTime currentTime, LevelState parentWorld)
        {
            offset = (float)Math.Sin(currentTime.TotalGameTime.TotalMilliseconds / 1000f) * 0.2f;

            Player.PlayerItems items = parent.CurrentItemTypes;

            if (exit == null)
            {
                foreach (Entity en in parentWorld.EntityList)
                {
                    if (en is BetaEndLevelFag)
                    {
                        exit = (BetaEndLevelFag)en;
                    }
                }
            }
            else
            {
                theta = (float)(Math.Atan2(parent.CenterPoint.Y - exit.CenterPoint.Y, parent.CenterPoint.X - exit.CenterPoint.X) - Math.PI) + offset;

                drawPos = parent.CenterPoint + new Vector2((float)(2 * GlobalGameConstants.TileSize.X * Math.Cos(theta)), (float)(2 * GlobalGameConstants.TileSize.Y * Math.Sin(theta)));
            }

            if (items.item1 == GlobalGameConstants.itemType.Compass && InputDevice2.IsPlayerButtonDown(parent.Index, InputDevice2.PlayerButton.UseItem1))
            {
                parent.Velocity = Vector2.Zero;
                parent.LoadAnimation.Animation = parent.LoadAnimation.Skeleton.Data.FindAnimation("idle");
                drawPointer = true;
            }
            else if (items.item2 == GlobalGameConstants.itemType.Compass && InputDevice2.IsPlayerButtonDown(parent.Index, InputDevice2.PlayerButton.UseItem2))
            {
                parent.Velocity = Vector2.Zero;
                parent.LoadAnimation.Animation = parent.LoadAnimation.Skeleton.Data.FindAnimation("idle");
                drawPointer = true;
            }
            else
            {
                drawPointer = false;
                parentWorld.RenderNodeMap = false;
                parent.State = Player.playerState.Moving;
                parent.Disable_Movement = false;
            }

            drawPos2 = parent.CenterPoint - img2.FrameDimensions / 2;
        }

        public void daemonupdate(Player parent, GameTime currentTime, LevelState parentWorld)
        {
            offset = (float)Math.Sin(currentTime.TotalGameTime.TotalMilliseconds / 1000f) * 0.2f;
        }

        public GlobalGameConstants.itemType ItemType()
        {
            return GlobalGameConstants.itemType.Compass;
        }

        public string getEnumType()
        {
            return GlobalGameConstants.itemType.Compass.ToString();
        }

        public void draw(Spine.SkeletonRenderer sb)
        {
            if (drawPointer)
            {

                img.drawAnimationFrame(0.0f, sb, drawPos, new Vector2(1), 0.5f, theta + (float)(Math.PI / 2), Vector2.Zero, Color.White);
                img2.drawAnimationFrame(0.0f, sb, drawPos2, new Vector2(1), 0.5f, 0.0f, Vector2.Zero, Color.White);
            }
        }
    }
}
