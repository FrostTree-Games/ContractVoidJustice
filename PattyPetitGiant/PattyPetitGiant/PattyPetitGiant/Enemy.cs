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
    class Enemy : Entity
    {
        private bool item_hit;
        public bool Item_Hit
        { 
            set { this.item_hit = value; }
            get { return item_hit; }
        }

        public Enemy(LevelState parentWorld, float initialx, float initialy)
        {
            position.X = initialx;
            position.Y = initialy;

            dimensions.X = 47.9f;
            dimensions.Y = 47.9f;

            disable_movement = false;
            disable_movement_time = 0.0f;

            velocity = Vector2.Zero;

            this.parentWorld = parentWorld;
        }

        public override void update(GameTime currentTime)
        {
            //checking if player hits another entity if he does then disables player movement and knocks player back
            foreach (Entity en in parentWorld.EntityList)
            {
                if (en == this)
                {
                    continue;
                }

                if (hitTest(en))
                {
                    if (en is Player)
                    {
                        this.knockBack(en, this.position, this.dimensions);
                    }
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

            //updates enemies position

            Vector2 pos = new Vector2(position.X, position.Y);
            Vector2 nextStep = new Vector2(position.X + velocity.X, position.Y + velocity.Y);

            Vector2 finalPos = parentWorld.Map.reloactePosition(pos, nextStep, dimensions);
            position.X = finalPos.X;
            position.Y = finalPos.Y;
        }


        public override void draw(SpriteBatch sb)
        {
            sb.Draw(Game1.whitePixel, position, null, Color.Green, 0.0f, Vector2.Zero, new Vector2(48, 48), SpriteEffects.None, 1.0f);
        }
    }
}
