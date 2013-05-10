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
        public enum EnemyState
        {
            Moving,
            Pause,
            Chase
        }

        protected EnemyState state = EnemyState.Moving;

        private bool item_hit;
        public bool Item_Hit
        { 
            set { this.item_hit = value; }
            get { return item_hit; }
        }

        protected float change_direction_time = 0.0f;
        public float Change_Direction_Time 
        { 
            set { change_direction_time = value; } 
            get { return change_direction_time; } 
        }

        protected int enemy_life = 100;
        public int Enemy_Life
        {
            set { enemy_life = value; }
            get { return enemy_life; }
        }
        protected int enemy_damage = 0;
        public int Enemy_Damage
        {
            get { return enemy_damage; }
        }

        protected float damage_player_time = 0.0f;

        public Enemy()
        {
            velocity = Vector2.Zero;
            disable_movement = false;
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

            state = EnemyState.Moving;

            this.parentWorld = parentWorld;

            remove_from_list = false;
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
                        this.knockBack(en, this.position, this.dimensions, enemy_damage);
                        en.Disable_Movement = true;
                        ((Player)en).State = Player.playerState.Moving;
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
