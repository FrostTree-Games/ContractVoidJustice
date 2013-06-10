using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PattyPetitGiant
{
    class WaveMotionGun : Item
    {
        public struct WaveMotionBullet
        {
            public Vector2 position;
            private const float radius = 24f;
            public float direction;
            public float timePassed;

            private const float motionBulletSpeed = 0.5f;

            public WaveMotionBullet(Vector2 position, float direction)
            {
                this.position = position;
                this.direction = direction;
                timePassed = 0.0f;
            }

            public bool hitTestEntity(Entity en)
            {
                return (Vector2.Distance(en.CenterPoint, position) < radius);
            }

            public void update(LevelState parentWorld, GameTime currentTime)
            {
                timePassed += currentTime.ElapsedGameTime.Milliseconds;

                //calculate directional velocity by taking d/dt of parametric path
                Vector2 velocity = new Vector2((float)(Math.Cos(direction) - (15 * 2 * (Math.PI / 100) * Math.Sin(direction) * Math.Cos(2 * Math.PI / 13 * timePassed / 20))), (float)(Math.Sin(direction) + (15 * 2 * (Math.PI / 100) * Math.Cos(direction) * Math.Cos(2 * Math.PI / 13 * timePassed / 20))));

                position += motionBulletSpeed * velocity * currentTime.ElapsedGameTime.Milliseconds;
            }

            public void draw(SpriteBatch sb)
            {
                sb.Draw(Game1.whitePixel, position, null, Color.Purple, 0.785f, Vector2.Zero, radius, SpriteEffects.None, 0.7f);
            }
        }

        public void update(Player parent, GameTime currentTime, LevelState parentWorld)
        {
            //
        }

        public void daemonupdate(Player parent, GameTime currentTime, LevelState parentWorld)
        {
            //
        }

        public void draw(SpriteBatch sb)
        {
            //
        }

        public GlobalGameConstants.itemType ItemType()
        {
            return GlobalGameConstants.itemType.WaveMotionGun;
        }

        public string getEnumType()
        {
            return "WaveMotionGun";
        }
    }
}
