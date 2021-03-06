﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PattyPetitGiant
{
    class SnakeWorm : Enemy
    {
        private enum SnakeWormState
        {
            InvalidState = -1,
            Moving = 0,
            KnockedBack = 2,
            Dying = 3, // may not use
        }

        private SnakeWormState snakeState;

        private float direction;
        private const float wormSpeed = 0.20f;
        private const float turnAmount = 0.05f;

        private float switchTimer;
        private float switchDuration;
        private const float averageSwitchDuration = 1000f;
        private float dirModifier;

        private float knockBackTime;
        private const float knockBackDuration = 750f;
        private const float knockBackMagnitude = 0.4f;

        private const int positionsCount = 2;
        private const int tailPiecesCount = 100;
        private TailPosition[,] tailData = null; //using a 2d array over jagged; garbage collection should be simpler with the autoboxed value types
        int tailMostRecent;

        private const int secondaryHitBoxCount = 8;

        private struct TailPosition
        {
            public Vector2 position;
            public float rotation;

            public TailPosition(Vector2 position, float rotation)
            {
                this.position = position;
                this.rotation = rotation;
            }
        }

        private AnimationLib.FrameAnimationSet testAnim = null;
        private AnimationLib.FrameAnimationSet tailAnimA = null;
        private AnimationLib.FrameAnimationSet tailAnimB = null;

        public SnakeWorm(LevelState parentWorld, Vector2 position)
        {
            this.parentWorld = parentWorld;
            this.position = position;
            this.dimensions = new Vector2(24);

            snakeState = SnakeWormState.Moving;

            velocity = Vector2.Zero;

            direction = (float)(Game1.rand.NextDouble() * Math.PI * 2);
            dirModifier = 1.0f;
            switchTimer = 0.0f;
            switchDuration = averageSwitchDuration;

            enemy_life = 16;
            enemy_type = EnemyType.Alien;

            prob_item_drop = 0.3;
            number_drop_items = 4;

            tailData = new TailPosition[tailPiecesCount, positionsCount];
            tailMostRecent = 0;
            for (int j = 0; j < tailPiecesCount; j++)
            {
                for (int i = 0; i < positionsCount; i++)
                {
                    tailData[j, i] = new TailPosition(position, direction);
                }
            }

            secondaryHitBoxes = new SecondaryHitBox[secondaryHitBoxCount];
            for (int i = 0; i < secondaryHitBoxCount; i++)
            {
                secondaryHitBoxes[i] = new SecondaryHitBox(position, new Vector2(16));
            }

            testAnim = AnimationLib.getFrameAnimationSet("snakeA");
            tailAnimA = AnimationLib.getFrameAnimationSet("snakeB");
            tailAnimB = AnimationLib.getFrameAnimationSet("snakeC");
            animation_time = 0;
        }

        public override void update(GameTime currentTime)
        {
            animation_time += currentTime.ElapsedGameTime.Milliseconds;

            if (snakeState == SnakeWormState.Moving)
            {
                switchTimer += currentTime.ElapsedGameTime.Milliseconds;
                if (switchTimer > switchDuration)
                {
                    dirModifier *= -1;

                    switchTimer = 0;
                    switchDuration = averageSwitchDuration + (float)(1000f * Game1.rand.NextDouble());
                }

                direction += turnAmount * dirModifier;

                velocity = new Vector2((float)(Math.Cos(direction) * wormSpeed), (float)(Math.Sin(direction) * wormSpeed));

                if (enemy_life < 1)
                {
                    snakeState = SnakeWormState.Dying;
                    parentWorld.pushCoin(this);
                }

                foreach (Entity en in parentWorld.EntityList)
                {
                    if (en.Enemy_Type == EnemyType.Alien)
                    {
                        continue;
                    }

                    if (Vector2.Distance(en.CenterPoint, CenterPoint) < 500f)
                    {
                        if (hitTest(en))
                        {
                            en.knockBack(en.Position - position, 2.0f, 5);
                        }
                    }
                }
            }
            else if (snakeState == SnakeWormState.KnockedBack)
            {
                knockBackTime += currentTime.ElapsedGameTime.Milliseconds;

                //direction += turnAmount * 3;

                if (knockBackTime > knockBackDuration)
                {
                    snakeState = SnakeWormState.Moving;
                }
            }
            else if (snakeState == SnakeWormState.Dying)
            {
                remove_from_list = true;
            }


            for (int i = 0; i < tailPiecesCount; i++)
            {
                if (i == tailPiecesCount - 1)
                {
                    tailData[i, tailMostRecent] = new TailPosition(position, direction + (float)(Math.PI));
                }
                else
                {
                    tailData[i, tailMostRecent] = tailData[(i - 1 + tailPiecesCount) % tailPiecesCount, (tailMostRecent + 1) % positionsCount];
                }
            }

            for (int i = 0; i < tailPiecesCount - 1; i++)
            {
                Vector2 posDiff = tailData[i + 1, tailMostRecent].position - tailData[i, tailMostRecent].position;

                tailData[i, tailMostRecent].rotation = (float)(Math.Atan2(posDiff.Y, posDiff.X));
            }

            tailMostRecent = (tailMostRecent + 1) % positionsCount;

            for (int i = 0; i < secondaryHitBoxCount; i++)
            {
                secondaryHitBoxes[i].Position = tailData[(int)(i * ((tailPiecesCount - 1.0f) / secondaryHitBoxCount)), tailMostRecent].position;
            }

            Vector2 newPos = position + (this.velocity * currentTime.ElapsedGameTime.Milliseconds);
            position = parentWorld.Map.reloactePosition(position, newPos, dimensions);
        }

        public override void draw(Spine.SkeletonRenderer sb)
        {
            for (int i = 0; i < tailPiecesCount; i++)
            {
                if (i == 8)
                {
                    //tailAnimB.drawAnimationFrame(0.0f, sb, tailData[i, tailMostRecent].position + tailAnimB.FrameDimensions / 2, new Vector2(1), 0.5f, tailData[i, tailMostRecent].rotation, tailAnimB.FrameDimensions / 2);
                }
                else
                {
                    tailAnimA.drawAnimationFrame(animation_time, sb, tailData[i, tailMostRecent].position + tailAnimA.FrameDimensions / 2, new Vector2(1), 0.5f, tailData[i, tailMostRecent].rotation, Vector2.Zero, Color.White);
                    //tailAnimA.drawAnimationFrame(animation_time, sb, tailData[i, tailMostRecent].position + tailAnimA.FrameDimensions / 2, new Vector2(1), 0.5f, tailData[i, tailMostRecent].rotation, tailAnimA.FrameDimensions / 2);
                }
            }

            testAnim.drawAnimationFrame(animation_time, sb, position + dimensions / 2, new Vector2(0), 0.5f, 0.0f, Vector2.Zero, Color.White);
            //testAnim.drawAnimationFrame(animation_time, sb, position + dimensions / 2, new Vector2(1), 0.6f, direction, testAnim.FrameDimensions / 2);

            /*
            for (int i = 0; i < secondaryHitBoxCount; i++)
            {
                sb.Draw(Game1.whitePixel, secondaryHitBoxes[i].Position, null, Color.Magenta, 0.0f, Vector2.Zero, secondaryHitBoxes[i].Dimensions, SpriteEffects.None, 0.7f);
            }*/
        }

        public override void knockBack(Vector2 direction, float magnitude, int damage, Entity attacker)
        {
            if (snakeState == SnakeWormState.KnockedBack)
            {
                return;
            }

            snakeState = SnakeWormState.KnockedBack;

            enemy_life -= damage;

            knockBackTime = 0;
            direction.Normalize();
            this.direction = (float)(Math.Atan2(direction.Y, direction.X));
            velocity = direction * knockBackMagnitude;
        }

        public override void spinerender(Spine.SkeletonRenderer renderer)
        {
            //spine animations may not be used here
        }
    }
}
