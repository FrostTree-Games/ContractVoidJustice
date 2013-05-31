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

        private AntiFairy other = null;
        private bool doubled;

        public AntiFairy(LevelState parentWorld, Vector2 position)
        {
            this.position = position;
            this.parentWorld = parentWorld;
            this.dimensions = GlobalGameConstants.TileSize;

            anim = AnimationLib.getFrameAnimationSet("antiFairy");
            animationTime = 0.0f;

            if (position.X > 0)
            {
                other = new AntiFairy(parentWorld, new Vector2(-100, 100));
                doubled = false;
            }
            else
            {
                doubled = true;
            }

            fairyState = (AntiFairyState)(Game1.rand.Next() % 4);
        }

        public override void update(GameTime currentTime)
        {
            if (doubled && other != null)
            {
                other.update(currentTime);
            }

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
                if (nextStep.X != finalPos.X)
                {
                    switch (fairyState)
                    {
                        case AntiFairyState.NorthEast:
                            fairyState = AntiFairyState.NorthWest;
                            break;
                        case AntiFairyState.NorthWest:
                            fairyState = AntiFairyState.NorthEast;
                            break;
                        case AntiFairyState.SouthEast:
                            fairyState = AntiFairyState.SouthWest;
                            break;
                        case AntiFairyState.SouthWest:
                            fairyState = AntiFairyState.SouthEast;
                            break;
                    }
                }
                else if (nextStep.Y != finalPos.Y)
                {
                    switch (fairyState)
                    {
                        case AntiFairyState.NorthEast:
                            fairyState = AntiFairyState.SouthEast;
                            break;
                        case AntiFairyState.NorthWest:
                            fairyState = AntiFairyState.SouthWest;
                            break;
                        case AntiFairyState.SouthEast:
                            fairyState = AntiFairyState.NorthEast;
                            break;
                        case AntiFairyState.SouthWest:
                            fairyState = AntiFairyState.NorthWest;
                            break;
                    }
                }
            }
            else
            {
                position = finalPos;
            }
        }

        public override void draw(SpriteBatch sb)
        {
            if (doubled && other != null)
            {
                other.draw(sb);
            }

            anim.drawAnimationFrame(animationTime, sb, position, new Vector2(3.0f, 3.0f), 0.6f);
        }

        public override void knockBack(Vector2 direction, float magnitude, int damage)
        {
            //
        }

        public void duplicate()
        {
            if (!doubled && other != null)
            {
                doubled = true;

                other.Position = position;
            }
        }
    }
}
