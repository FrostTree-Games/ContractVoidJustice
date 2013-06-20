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
    class GuardMech : Enemy
    {
        public enum MechState
        {
            Moving,
            Firing,
            Melee,
            Reset,
            Death,
        }

        private const float enemy_range_damage = 10.0f;
        private const float enemy_melee_damage = 20.0f;
        private const float knockback_magnitude = 2.0f;

        private float windup_timer = 0.0f;
        private float firing_timer = 0.0f;
        private MechState mech_state = MechState.Moving;
        private EnemyComponents component = null;

        public struct Grenades
        {
        }

        public GuardMech(LevelState parentWorld, float initial_x, float initial_y)
        {
            position = new Vector2(initial_x, initial_y);
            dimensions = new Vector2(48, 48);
            velocity = new Vector2(0.8f, 0.0f);

            windup_timer = 0.0f;
            firing_timer = 0.0f;

            enemy_life = 50;
            disable_movement = false;
            disable_movement_time = 0.0f;
            enemy_found = false;
            change_direction_time = 0.0f;
            this.parentWorld = parentWorld;
            enemy_type = EnemyType.Guard;
            change_direction_time_threshold = 300.0f;
            direction_facing = GlobalGameConstants.Direction.Right;

            component = new MoveSearch();
            mech_state = MechState.Moving;
            velocity_speed = 0.8f;
        }

        public override void update(GameTime currentTime)
        {
            if (disable_movement == true)
            {
                disable_movement_time += currentTime.ElapsedGameTime.Milliseconds;
                if (disable_movement_time > 300)
                {
                    disable_movement = false;
                    disable_movement_time = 0.0f;
                    velocity = Vector2.Zero;
                }
            }
            else
            {
                switch(mech_state)
                {
                    case MechState.Moving:
                        change_direction_time += currentTime.ElapsedGameTime.Milliseconds;
                        Console.WriteLine("First " + direction_facing);
                        foreach(Entity en in parentWorld.EntityList)
                        {
                            component.update(this, en, currentTime, parentWorld);
                        }
                        enemy_found = false;
                        Console.WriteLine(direction_facing);
                        break;
                    case MechState.Firing:
                        break;
                    case MechState.Melee:
                        break;
                    case MechState.Reset:
                        break;
                    case MechState.Death:
                        break;
                    default:
                        break;
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
            sb.Draw(Game1.whitePixel, position, null, Color.White, 0.0f, Vector2.Zero, dimensions, SpriteEffects.None, 0.5f);
        }

        public override void knockBack(Vector2 direction, float magnitude, int damage, Entity attacker)
        {
        }
    }
}
