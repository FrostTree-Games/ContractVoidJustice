using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PattyPetitGiant
{
    class BroodLord : Enemy
    {
        private const int minionCount = 3;
        private const float eggDistance = 96f;
        private BroodLing[] minions = null;

        public BroodLord(LevelState parentWorld, Vector2 position)
        {
            this.parentWorld = parentWorld;
            this.position = position;
            this.dimensions = GlobalGameConstants.TileSize * 1.5f;
            this.velocity = Vector2.Zero;

            minions = new BroodLing[minionCount];
            for (int i = 0; i < minionCount; i++)
            {
                minions[i] = new BroodLing(parentWorld, Vector2.Zero, this);
                parentWorld.EntityList.Add(minions[i]);
            }

            enemy_life = 25;
            enemy_type = EnemyType.Alien;
        }

        public override void update(GameTime currentTime)
        {
            for (int i = 0; i < minionCount; i++)
            {
                minions[i].update(currentTime);
            }

            for (int i = 0; i < minionCount; i++)
            {
                if (minions[i].MinionState == BroodLing.BroodLingState.Dead)
                {
                    float dir = (float)((Math.PI * 2 / minionCount) * i);
                    minions[i].spawn(CenterPoint + (new Vector2((float)(Math.Cos(dir)), (float)(Math.Sin(dir))) * eggDistance));
                }
            }

            foreach (Entity en in parentWorld.EntityList)
            {
                if (en is Player)
                {
                    for (int i = 0; i < minionCount; i++)
                    {
                        minions[i].setTarget(en);
                    }

                    break;
                }
            }
        }

        public override void draw(SpriteBatch sb)
        {
            sb.Draw(Game1.whitePixel, position, null, Color.LimeGreen, 0.0f, Vector2.Zero, dimensions, SpriteEffects.None, 0.5f);

            for (int i = 0; i < minionCount; i++)
            {
                minions[i].draw(sb);
            }
        }

        public override void knockBack(Vector2 direction, float magnitude, int damage, Entity attacker)
        {
            //throw new NotImplementedException();
        }

        public override void spinerender(Spine.SkeletonRenderer renderer)
        {
           // throw new NotImplementedException();
        }
    }

    // the capitalized L is for "Lulz"
    class BroodLing : Enemy
    {
        public enum BroodLingState
        {
            Idle = 0,
            Chase = 1,
            Egg = 2,
            Dying = 3,
            Dead = 4,
            KnockBack = 5,
            Biting = 6,
        }

        private BroodLingState minionState;
        public BroodLingState MinionState { get { return minionState; } }

        private BroodLord lord = null;

        private float direction;
        private const float minionSpeed = 0.05f;

        private float eggTimer = 0.0f;
        private const float eggDuration = 1750f;

        private float knockBackTimer;
        private const float knockBackDuration = 700f;

        private float dyingTimer;
        private const float dyingDuration = 1000f;

        private float biteTimer;
        private const float biteDuration = 500f;

        private Entity target;

        public BroodLing(LevelState parentWorld, Vector2 position, BroodLord lord)
        {
            minionState = BroodLingState.Dead;
            this.parentWorld = parentWorld;
            this.dimensions = GlobalGameConstants.TileSize;
            this.lord = lord;

            enemy_life = 0;
            enemy_type = EnemyType.Alien;

            animation_time = 0;
        }

        public override void update(GameTime currentTime)
        {
            animation_time += currentTime.ElapsedGameTime.Milliseconds;

            if (minionState == BroodLingState.Idle)
            {
                target = null;

                direction += 0.05f;

                velocity = new Vector2((float)(Math.Cos(direction) * minionSpeed), (float)(Math.Sin(direction) * minionSpeed));
            }
            else if (minionState == BroodLingState.Chase)
            {
                double dir = Math.Atan2(target.CenterPoint.Y - CenterPoint.Y, target.CenterPoint.X - CenterPoint.X);

                if (Math.Abs(dir - direction) > Math.PI)
                {
                    if (dir > direction)
                    {
                        dir -= Math.PI * 2;
                    }
                    else if (dir < direction)
                    {
                        dir += Math.PI * 2;
                    }
                }

                if (dir > direction)
                {
                    direction += 0.02f;
                }
                else if (dir < direction)
                {
                    direction -= 0.02f;
                }

                velocity = new Vector2((float)(Math.Cos(direction) * minionSpeed), (float)(Math.Sin(direction) * minionSpeed));

                foreach (Entity en in parentWorld.EntityList)
                {
                    if (!(en.Enemy_Type == EnemyType.Player || en.Enemy_Type == EnemyType.Guard || en.Enemy_Type == EnemyType.Prisoner || en.Enemy_Type == EnemyType.NoType))
                    {
                        continue;
                    }

                    if (Vector2.Distance(en.CenterPoint, CenterPoint) > 500)
                    {
                        continue;
                    }
                    

                    if (hitTest(en))
                    {
                        en.knockBack(Vector2.Normalize(en.CenterPoint - CenterPoint), 8, 5, lord);

                        velocity = Vector2.Normalize(en.CenterPoint - CenterPoint) * -0.2f;
                        biteTimer = 0;
                        minionState = BroodLingState.Biting;
                    }
                }
            }
            else if (minionState == BroodLingState.Egg)
            {
                velocity = Vector2.Zero;

                eggTimer += currentTime.ElapsedGameTime.Milliseconds;

                if (eggTimer > eggDuration)
                {
                    if (target == null)
                    {
                        minionState = BroodLingState.Idle;
                    }
                    else
                    {
                        minionState = BroodLingState.Chase;
                    }
                }
            }
            else if (minionState == BroodLingState.Dying)
            {
                velocity = Vector2.Zero;

                dyingTimer += currentTime.ElapsedGameTime.Milliseconds;
                if (dyingTimer > dyingDuration)
                {
                    minionState = BroodLingState.Dead;
                }
            }
            else if (minionState == BroodLingState.Dead)
            {
                position = new Vector2(float.MinValue, float.MaxValue);
                velocity = Vector2.Zero;

                return;
            }
            else if (minionState == BroodLingState.KnockBack)
            {
                knockBackTimer += currentTime.ElapsedGameTime.Milliseconds;

                direction += 0.1f;

                if (knockBackTimer > knockBackDuration)
                {
                    minionState = BroodLingState.Chase;

                    if (enemy_life < 1)
                    {
                        minionState = BroodLingState.Dying;
                        dyingTimer = 0;
                    }
                }
            }
            else if (minionState == BroodLingState.Biting)
            {
                biteTimer += currentTime.ElapsedGameTime.Milliseconds;

                if (biteTimer > biteDuration)
                {
                    minionState = BroodLingState.Chase;
                }
            }
            else
            {
                throw new NotImplementedException("Invalid broodling minion state");
            }

            direction = direction % (float)(Math.PI * 2);

            Vector2 newPos = position + (this.velocity * currentTime.ElapsedGameTime.Milliseconds);
            position = parentWorld.Map.reloactePosition(position, newPos, dimensions);
        }

        public override void draw(SpriteBatch sb)
        {
            if (minionState == BroodLingState.Dead)
            {
                return;
            }

            Color clr = Color.LightBlue;

            switch (minionState)
            {
                case BroodLingState.Egg:
                    clr = Color.DarkBlue;
                    break;
            }

            sb.Draw(Game1.testArrow, position, null, clr, direction, new Vector2(24, 24), new Vector2(1), SpriteEffects.None, 0.49f);
        }

        public override void knockBack(Vector2 direction, float magnitude, int damage, Entity attacker)
        {
            if (minionState == BroodLingState.KnockBack || minionState == BroodLingState.Dying || minionState == BroodLingState.Dead)
            {
                return;
            }

            direction.Normalize();

            magnitude /= 2.5f;
            velocity = magnitude * direction;

            enemy_life -= damage;

            minionState = BroodLingState.KnockBack;
            knockBackTimer = 0;
        }

        public override void spinerender(Spine.SkeletonRenderer renderer)
        {
            // throw new NotImplementedException();
        }

        public void spawn(Vector2 position)
        {
            minionState = BroodLingState.Egg;
            eggTimer = 0;

            this.position = position - (dimensions / 2);
            enemy_life = 8;

            direction = (float)(Game1.rand.NextDouble() * Math.PI);
        }

        public void setTarget(Entity en)
        {
            target = en;

            if (minionState == BroodLingState.Idle && en != null)
            {
                minionState = BroodLingState.Chase;
            }
            else if (en == null && minionState == BroodLingState.Chase)
            {
                minionState = BroodLingState.Idle;
            }
        }
    }
}
