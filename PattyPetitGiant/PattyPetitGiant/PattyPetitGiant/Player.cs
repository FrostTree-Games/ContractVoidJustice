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

        private bool disable_movement = false;
        private float disable_movement_time = 0.0f;

        public Player()
        {
            creation();

            horizontal_pos = 300.0f;
            vertical_pos = 300.0f;

            dimensions.X = 47.9f;
            dimensions.Y = 47.9f;
        }

        public Player(float initial_x, float initial_y)
        {
            horizontal_pos = initial_x;
            vertical_pos = initial_y;

            dimensions.X = 47.9f;
            dimensions.Y = 47.9f;
        }

        public override void update(GameTime currentTime)
        {
            double delta = currentTime.ElapsedGameTime.Milliseconds;
            KeyboardState ks = Keyboard.GetState();

            if (disable_movement == false)
            {
                if (ks.IsKeyDown(Keys.Right) == true)
                {
                    velocity.X = 1.0f;
                }
                else if (ks.IsKeyDown(Keys.Left) == true)
                {
                    velocity.X = -1.0f;
                }
                else
                {
                    velocity.X = 0.0f;
                }

                if (ks.IsKeyDown(Keys.Up) == true)
                {
                    velocity.Y = -1.0f;
                }
                else if (ks.IsKeyDown(Keys.Down) == true)
                {
                    velocity.Y = 1.0f;
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

            //checking if player hits another entity if he does then disables player movement and knocks player back
            foreach (Entity en in level_entity_list)
            {
                if (hitTest(en))
                {
                    if (en is Enemy)
                    {
                        disable_movement = true;

                        if (velocity.X > 0)
                        {
                            velocity.X = -5.0f;
                        }
                        else if (velocity.X < 0)
                        {
                            velocity.X = 5.0f;
                        }

                        if (velocity.Y > 0)
                        {
                            velocity.Y = -5.0f;
                        }
                        else if (velocity.Y < 0)
                        {
                            velocity.Y = 5.0f;
                        }
                    }
                }
            }

            Vector2 pos = new Vector2(horizontal_pos, vertical_pos);
           /* Vector2 nextStep = new Vector2(horizontal_pos + velocity.X, vertical_pos + velocity.Y);

            Vector2 finalPos = Game1.map.reloactePosition(pos, nextStep, dimensions);
            horizontal_pos = finalPos.X;
            vertical_pos = finalPos.Y;
            Console.WriteLine(velocity.X);*/
            //updates the position of the entity
            horizontal_pos += velocity.X;
            vertical_pos += velocity.Y;
        }

        public override void draw(SpriteBatch sb)
        {
            sb.Draw(Game1.whitePixel, new Vector2(horizontal_pos, vertical_pos), null, Color.White, 0.0f, Vector2.Zero, new Vector2(48, 48), SpriteEffects.None, 1.0f);
        }

    }
}
