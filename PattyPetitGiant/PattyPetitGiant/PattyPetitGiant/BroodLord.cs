﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PattyPetitGiant
{
    class BroodLord : Enemy
    {
        private enum BroodLordState
        {
            Idle,
            Aggro,
            Dying,
            Dead,
        }

        private const int minionCount = 3;
        private const float eggDistance = 96f;
        private BroodLing[] minions = null;

        private BroodLordState broodState;

        private float knockBackTimer = 999f;
        private const float knockBackDuration = 500f;

        private AnimationLib.SpineAnimationSet anim = null;

        private float deadCushySoundTimer;

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

            broodState = BroodLordState.Idle;

            enemy_life = 25;
            enemy_type = EnemyType.Alien;

            prob_item_drop = 0.45;
            number_drop_items = 4;

            anim = AnimationLib.loadNewAnimationSet("broodlord");
            anim.Animation = anim.Skeleton.Data.FindAnimation("idle");
            animation_time = 0;

            deadCushySoundTimer = 0;
        }

        public override void update(GameTime currentTime)
        {
            animation_time += currentTime.ElapsedGameTime.Milliseconds / 1000f;
            knockBackTimer += currentTime.ElapsedGameTime.Milliseconds;
            deadCushySoundTimer += currentTime.ElapsedGameTime.Milliseconds;

            if (broodState != BroodLordState.Dying && broodState != BroodLordState.Dead)
            {
                for (int i = 0; i < minionCount; i++)
                {
                    if (minions[i].MinionState == BroodLing.BroodLingState.Dead)
                    {
                        float dir = (float)((Math.PI * 2 / minionCount) * i);
                        minions[i].spawn(CenterPoint + (new Vector2((float)(Math.Cos(dir)), (float)(Math.Sin(dir))) * eggDistance));
                    }
                }
            }

            if (broodState == BroodLordState.Idle)
            {
                for (int i = 0; i < minionCount; i++)
                {
                    float dir = (float)((Math.PI * 2 / minionCount) * i);
                    minions[i].setTarget(CenterPoint + (new Vector2((float)(Math.Cos(dir)), (float)(Math.Sin(dir))) * eggDistance));
                }

                foreach (Entity en in parentWorld.EntityList)
                {
                    if (en is Player && Vector2.Distance(en.CenterPoint, CenterPoint) < 500)
                    {
                        broodState = BroodLordState.Aggro;
                    }
                }
            }
            else if (broodState == BroodLordState.Aggro)
            {
                foreach (Entity en in parentWorld.EntityList)
                {
                    if (en.Enemy_Type == EnemyType.Player || en.Enemy_Type == EnemyType.Prisoner || en.Enemy_Type == EnemyType.Guard)
                    {
                        if (Vector2.Distance(en.CenterPoint, CenterPoint) < 500)
                        {
                            if (hitTest(en))
                            {
                                en.knockBack(en.CenterPoint - CenterPoint, 3.0f, 4);
                            }
                        }
                    }

                    if (en is Player && Vector2.Distance(en.CenterPoint, CenterPoint) < 500)
                    {
                        for (int i = 0; i < minionCount; i++)
                        {
                            minions[i].setTarget(en.CenterPoint - (i * en.Dimensions / 2));
                        }

                        break;
                    }
                    else if (en is Player && Vector2.Distance(en.CenterPoint, CenterPoint) > 500)
                    {
                        broodState = BroodLordState.Idle;
                    }
                }
            }
            else if (broodState == BroodLordState.Dying)
            {
                broodState = BroodLordState.Dead;
            }
            else if (broodState == BroodLordState.Dead)
            {
                position = new Vector2(float.MinValue, float.MaxValue);
                dimensions = Vector2.Zero;
                bool removeCheck = true;

                for (int i = 0; i < minionCount; i++)
                {
                    removeCheck = (minions[i].MinionState == BroodLing.BroodLingState.Dead) && removeCheck;
                }

                remove_from_list = removeCheck;
                if (removeCheck)
                {
                    for (int i = 0; i < minionCount; i++)
                    {
                        minions[i].delete();
                    }
                }
            }
            else
            {
                throw new Exception("Invalid Broodlord State");
            }

            anim.Animation.Apply(anim.Skeleton, animation_time, true);
        }

        public override void draw(Spine.SkeletonRenderer sb)
        {
            //
        }

        public override void knockBack(Vector2 direction, float magnitude, int damage, Entity attacker)
        {
            if (knockBackTimer > knockBackDuration)
            {
                parentWorld.Particles.pushBloodParticle(CenterPoint);
                parentWorld.Particles.pushBloodParticle(CenterPoint);
                parentWorld.Particles.pushBloodParticle(CenterPoint);
                parentWorld.Particles.pushBloodParticle(CenterPoint);
                parentWorld.Particles.pushBloodParticle(CenterPoint);
                parentWorld.Particles.pushBloodParticle(CenterPoint);
                parentWorld.Particles.pushBloodParticle(CenterPoint);
                parentWorld.Particles.pushBloodParticle(CenterPoint);
                parentWorld.Particles.pushBloodParticle(CenterPoint);

                enemy_life -= damage;

                if (deadCushySoundTimer > 500f)
                {
                    AudioLib.playSoundEffect("fleshyKnockBack");

                    deadCushySoundTimer = 0;
                }

                if (enemy_life < 1)
                {
                    broodState = BroodLordState.Dying;
                    AudioLib.playSoundEffect("alienChaserDie");
                    for (int i = 0; i < 20; i++)
                    {
                        parentWorld.Particles.pushExplosiveGib(CenterPoint);
                    }

                    for (int i = 0; i < 10; i++)
                    {
                        parentWorld.Particles.pushGib(CenterPoint);
                    }
                    parentWorld.pushCoin(this);
                }

                knockBackTimer = 0;
            }
        }

        public override void spinerender(Spine.SkeletonRenderer renderer)
        {
            anim.Skeleton.RootBone.X = position.X + (dimensions.X / 2);
            anim.Skeleton.RootBone.Y = position.Y + (dimensions.Y);

            anim.Skeleton.UpdateWorldTransform();
            renderer.Draw(anim.Skeleton);
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
        private const float minionSpeed = 0.1f;

        private float eggTimer = 0.0f;
        private const float eggDuration = 4000f;

        private float knockBackTimer;
        private const float knockBackDuration = 700f;

        private float dyingTimer;
        private const float dyingDuration = 1000f;

        private float biteTimer;
        private const float biteDuration = 500f;

        private Vector2 target;

        AnimationLib.SpineAnimationSet anim = null;
        AnimationLib.FrameAnimationSet eggAnim = null;

        public BroodLing(LevelState parentWorld, Vector2 position, BroodLord lord)
        {
            minionState = BroodLingState.Dead;
            this.parentWorld = parentWorld;
            this.dimensions = GlobalGameConstants.TileSize * 0.6f;
            this.lord = lord;

            enemy_life = 0;
            enemy_type = EnemyType.Alien;

            animation_time = 0;

            anim = AnimationLib.loadNewAnimationSet("broodling");
            eggAnim = AnimationLib.getFrameAnimationSet("broodEgg");
        }

        public override void update(GameTime currentTime)
        {
            animation_time += currentTime.ElapsedGameTime.Milliseconds / ((minionState == BroodLingState.Egg) ? 1 : 1000f);

            if (direction < 0)
            {
                direction += (float)(Math.PI * 2);
            }

            if (minionState == BroodLingState.Idle)
            {
                direction += 0.05f;

                velocity = new Vector2((float)(Math.Cos(direction) * minionSpeed), (float)(Math.Sin(direction) * minionSpeed));
            }
            else if (minionState == BroodLingState.Chase)
            {
                double dir = Math.Atan2(target.Y - CenterPoint.Y, target.X - CenterPoint.X);

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
                        for (int i = 0; i < 11; i++)
                        {
                            parentWorld.Particles.pushExplosiveGib(CenterPoint);
                            AudioLib.playSoundEffect("fleshyKnockBack");
                        }

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

            anim.Animation.Apply(anim.Skeleton, animation_time, true);
        }

        public override void draw(Spine.SkeletonRenderer sb)
        {
            if (minionState == BroodLingState.Dead)
            {
                return;
            }

            if (minionState == BroodLingState.Egg)
            {
                eggAnim.drawAnimationFrame(animation_time, sb, position, new Vector2(1), 0.5f, 0.0f, Vector2.Zero, Color.White);
                //eggAnim.drawAnimationFrame(animation_time, sb, position, new Vector2(1), 0.5f);
            }
        }

        public override void knockBack(Vector2 direction, float magnitude, int damage, Entity attacker)
        {
            if (minionState == BroodLingState.KnockBack || minionState == BroodLingState.Dying || minionState == BroodLingState.Dead)
            {
                return;
            }

            parentWorld.Particles.pushBloodParticle(CenterPoint);
            parentWorld.Particles.pushBloodParticle(CenterPoint);
            parentWorld.Particles.pushBloodParticle(CenterPoint);

            direction.Normalize();

            magnitude /= 2.5f;
            velocity = magnitude * direction;

            velocity = Vector2.Clamp(velocity, Vector2.Zero, new Vector2(0.3f));

            enemy_life -= damage;

            minionState = BroodLingState.KnockBack;
            knockBackTimer = 0;
        }

        public override void spinerender(Spine.SkeletonRenderer renderer)
        {
            if (minionState == BroodLingState.Chase || minionState == BroodLingState.Biting || minionState == BroodLingState.Idle || minionState == BroodLingState.KnockBack)
            {
                anim.Skeleton.RootBone.X = position.X + (dimensions.X / 2);
                anim.Skeleton.RootBone.Y = position.Y + (dimensions.Y / 2);

                anim.Skeleton.RootBone.Rotation = direction * (float)(180 / -Math.PI) + 90;

                anim.Skeleton.UpdateWorldTransform();
                renderer.Draw(anim.Skeleton);
            }
        }

        public void spawn(Vector2 position)
        {
            minionState = BroodLingState.Egg;
            eggTimer = 0;

            this.position = position - (dimensions / 2);
            enemy_life = 8;

            direction = (float)(Game1.rand.NextDouble() * Math.PI);
        }

        public void setTarget(Vector2 en)
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

        /// <summary>
        /// NOTE: Only used with BroodLord. Do not use.
        /// </summary>
        public void delete()
        {
            remove_from_list = true;
        }
    }
}
