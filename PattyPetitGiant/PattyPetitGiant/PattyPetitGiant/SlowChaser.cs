using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PattyPetitGiant
{
    class SlowChaser : Enemy
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

        private const float walkSpeed = 0.1f;
        private const int numberOfChaseIterations = 3;
        private const float chaseSpeed = 0.500f;

        private float timer;
        private const float idleTime = 750f;
        private const float windUpTime = 500f;
        private const float chaseTime = 350f;
        private const float coolDownTime = 2000f;

        public SlowChaser(LevelState parentWorld, Vector2 position)
        {
            this.parentWorld = parentWorld;
            this.position = position;
            this.dimensions = GlobalGameConstants.TileSize;

            enemy_type = EnemyType.Alien;
            chaserState = SlowChaserState.Idle;

            direction_facing = GlobalGameConstants.Direction.Down;

            animation_time = 0.0f;
        }

        public override void update(GameTime currentTime)
        {
            animation_time += currentTime.ElapsedGameTime.Milliseconds;

            if (chaserState == SlowChaserState.Idle)
            {
                state = EnemyState.Moving;

                timer += currentTime.ElapsedGameTime.Milliseconds;

                velocity = Vector2.Zero;

                if (timer > idleTime)
                {
                    targetEntity = null;

                    foreach (Entity en in parentWorld.EntityList)
                    {
                        if (en.Enemy_Type == EnemyType.Player || en.Enemy_Type == EnemyType.Guard || en.Enemy_Type == EnemyType.Prisoner)
                        {
                            if (Vector2.Distance(en.Position, position) < 300f && (targetEntity == null || Vector2.Distance(en.Position, position) < Vector2.Distance(targetEntity.Position, position)))
                            {
                                targetEntity = en;
                            }
                        }
                    }

                    if (targetEntity != null)
                    {
                        targetPosition = targetEntity.CenterPoint;
                        timer = 0;
                        chaseIteration = 0;
                        chaserState = SlowChaserState.WindUp;
                    }
                }
            }
            else if (chaserState == SlowChaserState.WindUp)
            {
                state = EnemyState.Chase;

                velocity = Vector2.Zero;

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

                if (timer > coolDownTime)
                {
                    timer = 0;
                    chaserState = SlowChaserState.Idle;
                }
                else if (timer > coolDownTime / 4)
                {
                    velocity = Vector2.Zero;
                }
            }
            else if (chaserState == SlowChaserState.KnockedBack)
            {
                state = EnemyState.Moving;
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

            Vector2 newPos = position + (this.velocity * currentTime.ElapsedGameTime.Milliseconds);
            position = parentWorld.Map.reloactePosition(position, newPos, dimensions);
        }

        public override void draw(SpriteBatch sb)
        {
            sb.Draw(Game1.whitePixel, position, null, Color.Red, 0.0f, Vector2.Zero, dimensions, SpriteEffects.None, 0.5f);
        }

        public override void knockBack(Vector2 direction, float magnitude, int damage, Entity attacker)
        {
            if (chaserState == SlowChaserState.KnockedBack)
            {
                return;
            }

            //
        }

        public override void spinerender(Spine.SkeletonRenderer renderer)
        {
            //base.spinerender(renderer);
        }
    }
}
