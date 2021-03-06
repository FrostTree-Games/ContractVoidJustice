﻿using System;
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
            KnockedBack = 4,
            MomentaryStall = 5,
        }

        private const float antiFairyVelocity = 3.0f;
        private const int antiFairyDamage = 10;

        private AntiFairyState fairyState;

        private AnimationLib.FrameAnimationSet anim = null;

        private AntiFairy other = null;
        private bool doubled;

        private float knockBackTime;
        private const float knockBackDuration = 600f;

        private float waitTime;
        private const float waitDuration = 300f;

        public AntiFairy(LevelState parentWorld, Vector2 position)
        {
            this.position = position;
            this.parentWorld = parentWorld;
            this.dimensions = GlobalGameConstants.TileSize;

            anim = AnimationLib.getFrameAnimationSet("antiFairy");
            animation_time = 0.0f;

            enemy_type = EnemyType.Guard;
            enemy_life = 70;

            prob_item_drop = 0.5;
            number_drop_items = 7;

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

            animation_time += currentTime.ElapsedGameTime.Milliseconds;

            if (fairyState == AntiFairyState.MomentaryStall)
            {
                velocity = Vector2.Zero;

                waitTime += currentTime.ElapsedGameTime.Milliseconds;

                if (waitTime > waitDuration)
                {
                    fairyState = (AntiFairyState)(Game1.rand.Next() % 4);
                }
            }
            else if (fairyState == AntiFairyState.KnockedBack)
            {
                knockBackTime += currentTime.ElapsedGameTime.Milliseconds;

                if (knockBackTime > knockBackDuration)
                {
                    waitTime = 0.0f;
                    fairyState = AntiFairyState.MomentaryStall;
                }
            }
            else
            {
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
                            en.knockBack(en.CenterPoint - CenterPoint, 4.0f, antiFairyDamage, this);
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
            }

            Vector2 nextStep = position + (velocity);
            Vector2 finalPos = parentWorld.Map.reloactePosition(position, nextStep, dimensions);

            if (fairyState != AntiFairyState.KnockedBack && fairyState != AntiFairyState.MomentaryStall)
            {
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
            else
            {
                position = finalPos;
            }
        }

        public override void draw(Spine.SkeletonRenderer sb)
        {
            if (doubled && other != null)
            {
                other.draw(sb);
            }

            anim.drawAnimationFrame(animation_time, sb, position, new Vector2(1), 0.5f, 0.0f, Vector2.Zero, Color.White);
        }

        public override void knockBack(Vector2 direction, float magnitude, int damage, Entity attacker)
        {
            if (fairyState == AntiFairyState.KnockedBack)
            {
                return;
            }

            fairyState = AntiFairyState.KnockedBack;
            knockBackTime = 0;

            direction.Normalize();
            velocity = 6 * direction;

            enemy_life -= damage;
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
