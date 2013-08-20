using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Spine;

namespace PattyPetitGiant
{
    public class Enemy : Entity, SpineEntity
    {
        public enum EnemyState
        {
            Moving,
            Agressive,
            KnockBack,
            Chase,
            Idle,
            Firing,
            Death,
        }

        protected EnemyState state = EnemyState.Moving;

        private bool item_hit;
        public bool Item_Hit
        { 
            set { this.item_hit = value; }
            get { return item_hit; }
        }

        protected float change_direction_time_threshold = 2000.0f;
        public float Change_Direction_Time_Threshold
        {
            set { change_direction_time_threshold = value; }
            get { return change_direction_time_threshold; }
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

        protected bool sound_alert = false;
        public bool Sound_Alert
        {
            set { sound_alert = value; }
            get { return sound_alert; }
        }

        protected float range_distance = 600.0f;
        public float Range_Distance { get { return range_distance; } }

        protected float knockback_magnitude;

        protected bool enemy_found;
        public bool Enemy_Found { set { enemy_found = value; } get { return enemy_found; } }

        protected float velocity_speed = 1.0f;
        public float Velocity_Speed{ set { velocity_speed = value; } get { return velocity_speed; } }

        protected Vector2 sound_position = Vector2.Zero;
        public Vector2 Sound_Position
        {
            get { return sound_position; }
            set { sound_position = value; }
        }

        protected AnimationLib.SpineAnimationSet walk_down = null;
        protected AnimationLib.SpineAnimationSet walk_right = null;
        protected AnimationLib.SpineAnimationSet walk_up = null;
        protected AnimationLib.SpineAnimationSet current_skeleton = null;
        public AnimationLib.SpineAnimationSet LoadAnimation { set { current_skeleton = value; } get { return current_skeleton; } }
        protected float animation_time;
        
        protected float damage_player_time = 0.0f;

        protected float enemy_speed = 2.0f;

        protected float sight_angle1 = 0.523f;
        public float Sight_Angle1
        {
            get { return sight_angle1; }
        }

        protected float sight_angle2 = 2.617f;
        public float Sight_Angle2
        {
            get { return sight_angle2; }
        }

        public float Enemy_Speed
        {
            get { return enemy_speed; }
        }

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

            knockback_magnitude = 1.0f;
            //animation_time = 0.0f;

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
                        Vector2 direction = CenterPoint - en.CenterPoint;
                        en.knockBack(direction, knockback_magnitude, enemy_damage, this);
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


        public override void draw(Spine.SkeletonRenderer sb)
        {
            //sb.Draw(Game1.whitePixel, position, null, Color.Green, 0.0f, Vector2.Zero, new Vector2(48, 48), SpriteEffects.None, 1.0f);
        }

        public override void knockBack(Vector2 direction, float magnitude, int damage, Entity attacker)
        {
            return;
        }

        public virtual void spinerender(SkeletonRenderer renderer)
        {
        }
    }
}
