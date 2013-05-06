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
    class Player : Entity
    {
        public enum playerState
        {
            Moving,
            Item1,
            Item2
        }

        private Item player_item_1 = null;
        private Item player_item_2 = null;
        
        private playerState state = playerState.Moving;

        public playerState State
        {
            set { state = value; }
            get { return state;  }

        }
        
        public Player(LevelState parentWorld, float initial_x, float initial_y)
        {
            position.X = initial_x;
            position.Y = initial_y;

            dimensions.X = 47.9f;
            dimensions.Y = 47.9f;

            player_item_1 = new Sword(position);

            state = playerState.Moving;

            disable_movement = false;

            direction_facing = GlobalGameConstants.Direction.Right;

            this.parentWorld = parentWorld;
        }

        public override void update(GameTime currentTime)
        {
            double delta = currentTime.ElapsedGameTime.Milliseconds;
            KeyboardState ks = Keyboard.GetState();

            if (state == playerState.Item1)
            {
                //itemType = player_item_1.itemCheck;
                if (player_item_1 == null)
                {
                    state = playerState.Moving;
                }
                else
                {
                    player_item_1.update(this, currentTime, parentWorld);
                }
                
            }
            else if (state == playerState.Item2 )
            {
                if (player_item_2 == null)
                {
                    state = playerState.Moving;
                    disable_movement = false;
                }
            }
            else if (state == playerState.Moving)
            {
                if (ks.IsKeyDown(Keys.A))
                {
                    state = playerState.Item1;
                    velocity = Vector2.Zero;
                    disable_movement = true;
                }
                if (ks.IsKeyDown(Keys.S))
                {
                    state = playerState.Item2;
                    disable_movement = true;
                }

                if (disable_movement == false)
                {
                    if (ks.IsKeyDown(Keys.Right))
                    {
                        velocity.X = 1.5f;
                        direction_facing = GlobalGameConstants.Direction.Right;
                    }
                    else if (ks.IsKeyDown(Keys.Left))
                    {
                        velocity.X = -1.5f;
                        direction_facing = GlobalGameConstants.Direction.Left;
                    }
                    else
                    {
                        velocity.X = 0.0f;
                    }

                    if (ks.IsKeyDown(Keys.Up))
                    {
                        velocity.Y = -1.5f;
                        direction_facing = GlobalGameConstants.Direction.Up;
                    }
                    else if (ks.IsKeyDown(Keys.Down))
                    {
                        velocity.Y = 1.5f;
                        direction_facing = GlobalGameConstants.Direction.Down;
                    }
                    else
                    {
                        velocity.Y = 0.0f;
                    }
                }

                if (disable_movement == true)
                {
                    disable_movement_time += currentTime.ElapsedGameTime.Milliseconds;
                    if (disable_movement_time > 300)
                    {
                        velocity = Vector2.Zero;
                        disable_movement = false;
                        disable_movement_time = 0;
                    }
                }

            }

            Vector2 pos = new Vector2(position.X, position.Y);
            Vector2 nextStep = new Vector2(position.X + velocity.X, position.Y + velocity.Y);

            Vector2 finalPos = parentWorld.Map.reloactePosition(pos, nextStep, dimensions);
            position.X = finalPos.X;
            position.Y = finalPos.Y;
        }

        public override void draw(SpriteBatch sb)
        {
            sb.Draw(Game1.whitePixel, position, null, Color.White, 0.0f, Vector2.Zero, new Vector2(48, 48), SpriteEffects.None, 0.5f);
        }
    }
}
