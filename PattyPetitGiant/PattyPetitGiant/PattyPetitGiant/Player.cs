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

        protected Vector2 velocity = Vector2.Zero;
        protected Vector2 dimensions = Vector2.Zero;

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

            if (ks.IsKeyDown(Keys.Right) == true)
            {
                velocity.X = 0.2f;
            }
            else if (ks.IsKeyDown(Keys.Left) == true)
            {
                velocity.X = -0.2f;
            }
            else
            {
                velocity.X = 0.0f;
            }

            if (ks.IsKeyDown(Keys.Up) == true)
            {
                velocity.Y = -0.2f;
            }
            else if (ks.IsKeyDown(Keys.Down) == true)
            {
                velocity.Y = 0.2f;
            }
            else
            {
                velocity.Y = 0.0f;
            }
            horizontal_pos += velocity.X;
            vertical_pos += velocity.Y;
        }

        public override void draw(SpriteBatch sb)
        {
            sb.Begin();
            sb.Draw(Game1.whitePixel, new Vector2(horizontal_pos, vertical_pos), null, Color.White, 0.0f, Vector2.Zero, new Vector2(48, 48), SpriteEffects.None, 1.0f);
            //sb.Draw(Game1.whitePixel, new Vector2(horizontal_pos, vertical_pos), Color.White);
            sb.End();
        }

    }
}
