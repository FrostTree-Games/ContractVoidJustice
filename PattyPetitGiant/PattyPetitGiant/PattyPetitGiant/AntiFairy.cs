using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PattyPetitGiant
{
    class AntiFairy : Enemy
    {
        private enum AntiFairyState
        {
            NorthWest = 0,
            NorthEast = 1,
            SouthEast = 2,
            SouthWest = 3,
        }

        private const float antiFairyVelocity = 3.0f;
        private const int antiFairyDamage = 10;

        private AntiFairyState fairyState;

        private AnimationLib.FrameAnimationSet anim = null;
        private float animationTime;

        public AntiFairy(LevelState parentWorld, Vector2 position)
        {
            this.position = position;
            this.parentWorld = parentWorld;
            this.dimensions = GlobalGameConstants.TileSize;

            anim = AnimationLib.getFrameAnimationSet("antiFairy");
            animationTime = 0.0f;

            fairyState = (AntiFairyState)(Game1.rand.Next() % 4);
        }

        public override void update(GameTime currentTime)
        {
            animationTime += currentTime.ElapsedGameTime.Milliseconds;

            switch (fairyState)
            {
                case AntiFairyState.NorthEast:
                    velocity = new Vector2(antiFairyVelocity, -antiFairyVelocity);
                    break;
                case AntiFairyState.NorthWest:
                    velocity = new Vector2(-antiFairyVelocity, -antiFairyVelocity);
                    break;
                case AntiFairyState.SouthWest:
                    velocity = new Vector2(-antiFairyVelocity, antiFairyVelocity);
                    break;
                case AntiFairyState.SouthEast:
                    velocity = new Vector2(antiFairyVelocity, antiFairyVelocity);
                    break;
            }

            foreach (Entity en in parentWorld.EntityList)
            {
                if (en is Player)
                {
                    if (hitTest(en))
                    {
                        en.knockBack(en.CenterPoint - CenterPoint, 4.0f, antiFairyDamage);
                    }
                }
                else if (en is Enemy && !(en == this))
                {
                    if (hitTest(en))
                    {
                        en.knockBack(en.Position - position, 2.0f, 0);
                    }
                }
            }

            Vector2 nextStep = position + (velocity);
            Vector2 finalPos = parentWorld.Map.reloactePosition(position, nextStep, dimensions);

            if (nextStep != finalPos)
            {
                fairyState = (AntiFairyState)((((int)fairyState) + 1) % 4); //bracket hell
            }
            else
            {
                position = finalPos;
            }
        }

        public override void draw(SpriteBatch sb)
        {
            anim.drawAnimationFrame(animationTime, sb, position, new Vector2(3.0f, 3.0f), 0.6f);
        }

        public override void knockBack(Vector2 direction, float magnitude, int damage)
        {
            if (damage == 0)
            {
                velocity *= -1;
            }
        }
    }
}
