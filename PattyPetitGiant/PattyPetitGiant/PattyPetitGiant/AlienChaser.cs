using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PattyPetitGiant
{
    class AlienChaser : Enemy
    {
        private enum SlowChaserState
        {
            InvalidState = -1,
            Idle = 0,
            WindUp = 1,
            Sprint = 2,
            Cooldown = 3,
            KnockedBack = 4,
            Dying = 5,
        }

        private SlowChaserState chaserState;
        private int chaseIteration;
        private Entity targetEntity;
        private Vector2 targetPosition;

        private const float attackRadius = 500f;

        //if aggressionTime maxes out, then the entity goes from idle to chase
        private float aggressionTime;
        private const float maxAggressionTime = 1000f;

        private const float walkSpeed = 0.1f;
        private const int numberOfChaseIterations = 3;
        private const float chaseSpeed = 0.500f;

        private float timer;
        private const float idleTime = 200f;
        private const float windUpTime = 500f;
        private const float chaseTime = 350f;
        private const float coolDownTime = 2000f;

        private float knockBackTime;
        private const float knockBackDuration = 250f;

        private AnimationLib.SpineAnimationSet[] directionAnims = null;

        private AnimationLib.FrameAnimationSet konamiAlert = null;

        public AlienChaser(LevelState parentWorld, Vector2 position)
        {
            this.parentWorld = parentWorld;
            this.position = position;
            this.dimensions = GlobalGameConstants.TileSize;

            enemy_type = EnemyType.Alien;
            enemy_life = 10;
            chaserState = SlowChaserState.Idle;

            direction_facing = GlobalGameConstants.Direction.Down;

            animation_time = 0.0f;

            konamiAlert = AnimationLib.getFrameAnimationSet("konamiPic");

            directionAnims = new AnimationLib.SpineAnimationSet[4];
            directionAnims[(int)GlobalGameConstants.Direction.Up] = AnimationLib.loadNewAnimationSet("patrolUp");
            directionAnims[(int)GlobalGameConstants.Direction.Down] = AnimationLib.loadNewAnimationSet("patrolDown");
            directionAnims[(int)GlobalGameConstants.Direction.Left] = AnimationLib.loadNewAnimationSet("patrolRight");
            directionAnims[(int)GlobalGameConstants.Direction.Left].Skeleton.FlipX = true;
            directionAnims[(int)GlobalGameConstants.Direction.Right] = AnimationLib.loadNewAnimationSet("patrolRight");
            for (int i = 0; i < 4; i++)
            {
                directionAnims[i].Animation = directionAnims[i].Skeleton.Data.FindAnimation("run");
            }

            aggressionTime = 0.0f;
        }

        public override void update(GameTime currentTime)
        {
            animation_time += currentTime.ElapsedGameTime.Milliseconds;

            if (chaserState == SlowChaserState.Idle)
            {
                state = EnemyState.Moving;

                timer += currentTime.ElapsedGameTime.Milliseconds;

                directionAnims[(int)direction_facing].Animation = directionAnims[(int)direction_facing].Skeleton.Data.FindAnimation("idle");

                velocity = Vector2.Zero;

                if (timer > idleTime)
                {
                    targetEntity = null;

                    foreach (Entity en in parentWorld.EntityList)
                    {
                        if (en.Enemy_Type == EnemyType.Player || en.Enemy_Type == EnemyType.Guard || en.Enemy_Type == EnemyType.Prisoner)
                        {
                            if (Vector2.Distance(en.Position, position) < attackRadius && (targetEntity == null || Vector2.Distance(en.Position, position) < Vector2.Distance(targetEntity.Position, position)))
                            {
                                targetEntity = en;
                            }
                        }
                    }

                    if (targetEntity != null && aggressionTime > maxAggressionTime)
                    {
                        targetPosition = targetEntity.CenterPoint;
                        timer = 0;
                        chaseIteration = 0;
                        chaserState = SlowChaserState.WindUp;
                    }
                    else if (targetEntity != null && aggressionTime <= maxAggressionTime)
                    {
                        aggressionTime += currentTime.ElapsedGameTime.Milliseconds;
                    }
                    else if (targetEntity == null)
                    {
                        aggressionTime -= currentTime.ElapsedGameTime.Milliseconds;

                        if (aggressionTime < 0)
                        {
                            aggressionTime = 0;
                        }
                    }
                }
            }
            else if (chaserState == SlowChaserState.WindUp)
            {
                state = EnemyState.Chase;

                velocity = Vector2.Zero;

                directionAnims[(int)direction_facing].Animation = directionAnims[(int)direction_facing].Skeleton.Data.FindAnimation("chaseWindUp");

                timer += currentTime.ElapsedGameTime.Milliseconds;

                if (timer > windUpTime)
                {
                    timer = 0;
                    chaserState = SlowChaserState.Sprint;

                    double angle = Math.Atan2(targetPosition.Y - CenterPoint.Y, targetPosition.X - CenterPoint.X);
                    velocity = new Vector2((float)(Math.Cos(angle)), (float)(Math.Sin(angle))) * chaseSpeed;
                }
            }
            else if (chaserState == SlowChaserState.Sprint)
            {
                state = EnemyState.Chase;

                timer += currentTime.ElapsedGameTime.Milliseconds;

                directionAnims[(int)direction_facing].Animation = directionAnims[(int)direction_facing].Skeleton.Data.FindAnimation("chase");

                if (timer > chaseTime)
                {
                    chaseIteration++;

                    if (chaseIteration >= numberOfChaseIterations)
                    {
                        timer = 0;
                        targetEntity = null;
                        chaserState = SlowChaserState.Cooldown;

                        velocity = Vector2.Zero;
                    }
                    else
                    {
                        timer = 0;
                        targetPosition = targetEntity.CenterPoint;
                        chaserState = SlowChaserState.WindUp;
                    }
                }
            }
            else if (chaserState == SlowChaserState.Cooldown)
            {
                state = EnemyState.Idle;

                timer += currentTime.ElapsedGameTime.Milliseconds;

                directionAnims[(int)direction_facing].Animation = directionAnims[(int)direction_facing].Skeleton.Data.FindAnimation("idle");

                if (timer > coolDownTime)
                {
                    timer = 0;
                    chaserState = SlowChaserState.Idle;
                    aggressionTime = maxAggressionTime * 0.8f;
                }
                else if (timer > coolDownTime / 4)
                {
                    velocity = Vector2.Zero;
                }
            }
            else if (chaserState == SlowChaserState.KnockedBack)
            {
                state = EnemyState.Moving;

                if (enemy_life < 1)
                {
                    chaserState = SlowChaserState.Dying;
                    return;
                }

                knockBackTime += currentTime.ElapsedGameTime.Milliseconds;

                if (knockBackTime > knockBackDuration)
                {
                    targetEntity = null;
                    chaserState = SlowChaserState.Idle;
                    aggressionTime = 0.95f * maxAggressionTime;
                }
            }
            else if (chaserState == SlowChaserState.Dying)
            {
                remove_from_list = true;
                return;
            }
            else
            {
                throw new Exception("Invalid SlowChaser state");
            }

            foreach (Entity en in parentWorld.EntityList)
            {
                if (en.Enemy_Type != EnemyType.Alien)
                {
                    if (Vector2.Distance(en.Position, position) < 300)
                    {
                        if (hitTest(en))
                        {
                            en.knockBack(en.CenterPoint - CenterPoint, 6, 7);

                            if (chaserState == SlowChaserState.Sprint && Vector2.Distance(position, targetEntity.Position) < GlobalGameConstants.TileSize.X)
                            {
                                timer = 0;
                                targetEntity = null;
                                chaserState = SlowChaserState.Cooldown;
                                velocity *= -0.1f;
                            }
                        }
                    }
                }
            }

            if (Math.Abs(velocity.X) > Math.Abs(velocity.Y))
            {
                if (velocity.X > 0)
                {
                    direction_facing = GlobalGameConstants.Direction.Right;
                }
                else if (velocity.X < 0)
                {
                    direction_facing = GlobalGameConstants.Direction.Left;
                }
            }
            else if (Math.Abs(velocity.X) < Math.Abs(velocity.Y))
            {
                if (velocity.Y > 0)
                {
                    direction_facing = GlobalGameConstants.Direction.Down;
                }
                else if (velocity.Y < 0)
                {
                    direction_facing = GlobalGameConstants.Direction.Up;
                }
            }

            Vector2 newPos = position + (this.velocity * currentTime.ElapsedGameTime.Milliseconds);
            position = parentWorld.Map.reloactePosition(position, newPos, dimensions);

            directionAnims[(int)direction_facing].Animation.Apply(directionAnims[(int)direction_facing].Skeleton, animation_time / 1000f, true);
        }

        public override void draw(SpriteBatch sb)
        {
            if (chaserState == SlowChaserState.WindUp && chaseIteration == 0)
            {
                konamiAlert.drawAnimationFrame(0, sb, position - new Vector2(0, konamiAlert.FrameHeight * 3), new Vector2(3), 0.7f);
            }

            sb.Draw(Game1.whitePixel, position, null, Color.Lerp(Color.Blue, Color.Red, aggressionTime / maxAggressionTime), 0.0f, Vector2.Zero, dimensions, SpriteEffects.None, 0.5f);
        }

        public override void knockBack(Vector2 direction, float magnitude, int damage, Entity attacker)
        {
            if (chaserState == SlowChaserState.KnockedBack)
            {
                return;
            }

            enemy_life -= damage;

            direction.Normalize();
            velocity = direction * (magnitude / 2);

            knockBackTime = 0.0f;

            chaserState = SlowChaserState.KnockedBack;
        }

        public override void spinerender(Spine.SkeletonRenderer renderer)
        {
            AnimationLib.SpineAnimationSet currentSkeleton = directionAnims[(int)direction_facing];

            currentSkeleton.Skeleton.RootBone.X = CenterPoint.X * (currentSkeleton.Skeleton.FlipX ? -1 : 1);
            currentSkeleton.Skeleton.RootBone.Y = CenterPoint.Y + (dimensions.Y / 2f);

            currentSkeleton.Skeleton.RootBone.ScaleX = 1.0f;
            currentSkeleton.Skeleton.RootBone.ScaleY = 1.0f;

            currentSkeleton.Skeleton.UpdateWorldTransform();
            renderer.Draw(currentSkeleton.Skeleton);
        }
    }
}
