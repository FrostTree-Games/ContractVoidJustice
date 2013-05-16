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

        private Vector2 drawPos = Vector2.Zero;
        private float theta = 0.0f;

        private AnimationLib.FrameAnimationSet img = null;

        public Compass()
        {
            if (img == null)
            {
                img = AnimationLib.getFrameAnimationSet("compassPic");
            }
        }

        public void update(Player parent, GameTime currentTime, LevelState parentWorld)
        {
            daemonupdate(parent, currentTime, parentWorld);

            parent.State = Player.playerState.Moving;
            parent.Disable_Movement = true;
        }

        public void daemonupdate(Player parent, GameTime currentTime, LevelState parentWorld)
        {
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
                theta = (float)(Math.Atan2(parent.CenterPoint.Y - exit.CenterPoint.Y, parent.CenterPoint.X - exit.CenterPoint.X) - Math.PI/2);

                drawPos = parent.CenterPoint + new Vector2((float)(GlobalGameConstants.TileSize.X * Math.Cos(theta)), (float)(GlobalGameConstants.TileSize.Y * Math.Sin(theta)));
            }
        }

        public GlobalGameConstants.itemType ItemType()
        {
            return GlobalGameConstants.itemType.Compass;
        }

        public string getEnumType()
        {
            return GlobalGameConstants.itemType.Compass.ToString();
        }

        public void draw(SpriteBatch sb)
        {
            img.drawAnimationFrame(0.0f, sb, drawPos, new Vector2(3.0f, 3.0f), 0.5f, theta, GlobalGameConstants.TileSize / 2);
        }
    }
}
