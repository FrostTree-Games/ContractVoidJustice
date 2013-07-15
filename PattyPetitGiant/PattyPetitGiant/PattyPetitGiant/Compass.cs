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
        private float theta = 0.0f;

        private AnimationLib.FrameAnimationSet img = null;

        public Compass()
        {
            if (img == null)
            {
                img = AnimationLib.getFrameAnimationSet("compassPic");
            }

            drawPointer = false;
        }

        public void update(Player parent, GameTime currentTime, LevelState parentWorld)
        {
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
                theta = (float)(Math.Atan2(parent.CenterPoint.Y - exit.CenterPoint.Y, parent.CenterPoint.X - exit.CenterPoint.X) - Math.PI / 2);

                drawPos = parent.CenterPoint + new Vector2((float)(GlobalGameConstants.TileSize.X * Math.Cos(theta)), (float)(GlobalGameConstants.TileSize.Y * Math.Sin(theta)));
            }

            if (items.item1 == GlobalGameConstants.itemType.Compass && InputDeviceManager.isButtonDown(InputDeviceManager.PlayerButton.UseItem1))
            {
                parent.Velocity = Vector2.Zero;
                drawPointer = true;
            }
            else if (items.item2 == GlobalGameConstants.itemType.Compass && InputDeviceManager.isButtonDown(InputDeviceManager.PlayerButton.UseItem2))
            {
                parent.Velocity = Vector2.Zero;
                drawPointer = true;
            }
            else
            {
                drawPointer = false;
                parentWorld.RenderNodeMap = false;
                parent.State = Player.playerState.Moving;
                parent.Disable_Movement = false;
            }
        }

        public void daemonupdate(Player parent, GameTime currentTime, LevelState parentWorld)
        {
            //
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
                //img.drawAnimationFrame(0.0f, sb, drawPos, new Vector2(3.0f, 3.0f), 0.5f, theta, GlobalGameConstants.TileSize / 2);
            }
        }
    }
}
