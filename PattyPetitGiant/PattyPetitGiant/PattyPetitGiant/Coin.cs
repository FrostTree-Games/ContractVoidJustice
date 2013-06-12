using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace PattyPetitGiant
{
    public class Coin : Entity
    {
        public enum CoinState
        {
            Active,
            Inactive,
        }

        public enum CoinValue
        {
            Loonie = 1,
            Twoonie = 2,
            Laurier = 5,
            MacDonald = 10,
            Elizabeth = 20,
            Mackenzie = 50,
            Borden = 100,
        }

        private CoinState state;
        public CoinState State { get { return state; } }

        private CoinValue value = CoinValue.Loonie;
        public CoinValue Value { get { return value; } }

        private Color shadeColor = Color.Yellow;

        private AnimationLib.FrameAnimationSet coinAnim = null;
        private float animationTime;

        private bool isKnockedBack = false;
        private const float knockBackSpeed = 0.4f;
        private float knockedBackTime = 0.0f;
        private const float knockBackDuration = 250f;

        public Coin(LevelState parentWorld, Vector2 position)
        {
            this.position = position;
            this.parentWorld = parentWorld;

            dimensions = new Vector2(24, 24);

            state = CoinState.Inactive;

            coinAnim = AnimationLib.getFrameAnimationSet("testCoin");
            animationTime = 0.0f;

            isKnockedBack = false;
        }

        public override void update(GameTime currentTime)
        {
            if (state == CoinState.Active)
            {
                animationTime += currentTime.ElapsedGameTime.Milliseconds;

                foreach (Entity en in parentWorld.EntityList)
                {
                    if (en is Player)
                    {
                        if (hitTest(en))
                        {
                            GlobalGameConstants.Player_Coin_Amount = GlobalGameConstants.Player_Coin_Amount + (int)value;

                            AudioLib.playSoundEffect("testCoin");

                            state = CoinState.Inactive;
                        }
                    }
                }

                if (isKnockedBack)
                {
                    knockedBackTime += currentTime.ElapsedGameTime.Milliseconds;

                    if (knockedBackTime > knockBackDuration)
                    {
                        isKnockedBack = false;
                    }
                }
                else
                {
                    velocity = Vector2.Zero;
                }

                Vector2 nextStep = position + (velocity * currentTime.ElapsedGameTime.Milliseconds);

                Vector2 finalPos = parentWorld.Map.reloactePosition(position, nextStep, dimensions);
                position = finalPos;
            }
            else
            {
                position = new Vector2(-100, -100);
            }
        }

        public override void draw(SpriteBatch sb)
        {
            if (state == CoinState.Active)
            {
                coinAnim.drawAnimationFrame(animationTime, sb, this.position, new Vector2(1.5f), 0.51f, shadeColor);
            }
        }

        public override void knockBack(Vector2 direction, float magnitude, int damage)
        {
            if (isKnockedBack)
            {
                return;
            }
            else
            {
                direction.Normalize();

                isKnockedBack = true;
                knockedBackTime = 0.0f;
                velocity = direction * knockBackSpeed;
            }
        }

        public void activate(Vector2 position, CoinValue value)
        {
            if (state == CoinState.Active)
            {
                return;
            }

            state = CoinState.Active;

            this.position = position;
            this.value = value;

            isKnockedBack = false;
            float randDir = (float)(Game1.rand.NextDouble() * 3.14 * 2);
            Console.WriteLine(randDir);
            knockBack(new Vector2((float)Math.Cos(randDir), (float)Math.Sin(randDir)), 0.0f, 0);

            switch (value)
            {
                case CoinValue.Loonie:
                    shadeColor = Color.Brown;
                    break;
                case CoinValue.Twoonie:
                    shadeColor = Color.White;
                    break;
                case CoinValue.Laurier:
                    shadeColor = Color.LightBlue;
                    break;
                case CoinValue.MacDonald:
                    shadeColor = Color.Purple;
                    break;
                case CoinValue.Elizabeth:
                    shadeColor = Color.Green;
                    break;
                case CoinValue.Mackenzie:
                    shadeColor = Color.Red;
                    break;
                case CoinValue.Borden:
                    shadeColor = Color.Goldenrod;
                    break;
                default:
                    shadeColor = Color.Brown;
                    value = CoinValue.Loonie;
                    break;
            }
        }
    }
}
