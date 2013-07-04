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
    class MutantAcidSpitter : Enemy
    {
        private enum SpitterState
        {
            KnockBack,
            Search,
            WindUp,
            Fire,
            Death,
            Reset
        }

        private SpitterState state;
        private EnemyComponents component = new MoveSearch();

        private float windup_timer = 0.0f;
        private const float max_windup_timer = 500f;

        private float angle = 0.0f;

        private const int size_of_spit_array = 3;
        private int spitter_count = 0;
        private SpitProjectile[] projectile = new SpitProjectile[size_of_spit_array];

        private Entity entity_found = null;

        public MutantAcidSpitter(LevelState parentWorld, float initial_x, float initial_y)
        {
            position = new Vector2(initial_x, initial_y);
            velocity_speed = 2.0f;
            velocity = new Vector2(velocity_speed, 0);
            dimensions = new Vector2(48.0f, 48.0f);

            enemy_damage = 4;
            enemy_life = 20;
            windup_timer = 0.0f;
            spitter_count = 0;
            change_direction_time = 0.0f;
            change_direction_time_threshold = 1000.0f;
            angle = 0.0f;

            state = SpitterState.Search;
            this.parentWorld = parentWorld;
            direction_facing = GlobalGameConstants.Direction.Right;
            enemy_type = EnemyType.Alien;
            entity_found = null;

            for (int i = 0; i < size_of_spit_array; i++)
            {
                projectile[i] = new SpitProjectile(new Vector2(0,0), 0);
                projectile[i].active = false;
            }

            death = false;
        }

        public override void update(GameTime currentTime)
        {
            change_direction_time += currentTime.ElapsedGameTime.Milliseconds;

            switch (state)
            {
                case SpitterState.Search:
                    if (death == false)
                    {
                        if (enemy_found == false)
                        {

                            foreach (Entity en in parentWorld.EntityList)
                            {
                                if (en == this)
                                    continue;
                                else if (en.Enemy_Type != enemy_type && en.Enemy_Type != EnemyType.NoType && en.Death == false)
                                {
                                    component.update(this, en, currentTime, parentWorld);
                                    if (enemy_found)
                                    {
                                        entity_found = en;
                                        break;
                                    }
                                }
                            }
                        }
                        if (enemy_found)
                        {
                            state = SpitterState.WindUp;
                            velocity = Vector2.Zero;
                        }
                    }
                    break;
                case SpitterState.WindUp:
                    windup_timer += currentTime.ElapsedGameTime.Milliseconds;

                    if (windup_timer > max_windup_timer)
                    {
                        state = SpitterState.Fire;
                        windup_timer = 0.0f;
                    }

                    switch (direction_facing)
                    {
                        case GlobalGameConstants.Direction.Right:
                            if (angle < -1 * Math.PI / 3.27)
                            {
                                direction_facing = GlobalGameConstants.Direction.Up;
                                current_skeleton = walk_up;
                            }
                            else if (angle > Math.PI / 3.27)
                            {
                                direction_facing = GlobalGameConstants.Direction.Down;
                                current_skeleton = walk_down;
                            }
                            break;
                        case GlobalGameConstants.Direction.Left:
                            if (angle < Math.PI / 1.44 && angle > Math.PI / 1.5)
                            {
                                direction_facing = GlobalGameConstants.Direction.Down;
                                current_skeleton = walk_down;
                            }
                            else if (angle > -1 * Math.PI / 1.44 && angle < -1 * Math.PI / 1.5)
                            {
                                direction_facing = GlobalGameConstants.Direction.Up;
                                current_skeleton = walk_up;
                            }
                            break;
                        case GlobalGameConstants.Direction.Up:
                            if (angle < -1 * Math.PI / 1.24)
                            {
                                direction_facing = GlobalGameConstants.Direction.Left;
                                current_skeleton = walk_right;
                            }
                            else if (angle > -1 * Math.PI / 5.14)
                            {
                                direction_facing = GlobalGameConstants.Direction.Right;
                                current_skeleton = walk_right;
                            }
                            break;
                        default:
                            if (angle < Math.PI / 5.14)
                            {
                                direction_facing = GlobalGameConstants.Direction.Right;
                                current_skeleton = walk_right;
                            }
                            else if (angle > Math.PI / 1.24)
                            {
                                direction_facing = GlobalGameConstants.Direction.Left;
                                current_skeleton = walk_right;
                            }
                            break;
                    }
                        
                    break;
                case SpitterState.Fire:
                    angle = (float)(Math.Atan2(entity_found.CenterPoint.Y - CenterPoint.Y, entity_found.CenterPoint.X - CenterPoint.X));

                    for (int i = 0; i < size_of_spit_array; i++)
                    {
                        if (!projectile[i].active)
                        {
                            projectile[i] = new SpitProjectile(CenterPoint, angle);
                            state = SpitterState.WindUp;
                            break;
                        }
                    }

                    float distance = Vector2.Distance(CenterPoint, entity_found.CenterPoint);
                    if (Math.Abs(distance) > 300)
                    {
                        state = SpitterState.Search;
                        enemy_found = false;
                        entity_found = null;
                    }
                    break;
                case SpitterState.KnockBack:
                    disable_movement_time += currentTime.ElapsedGameTime.Milliseconds;
                    if (disable_movement_time > 300)
                    {
                        state = SpitterState.Search;
                        disable_movement_time = 0.0f;
                    }
                    break;
                case SpitterState.Death:
                    break;
                default:
                    break;
            }

            for(int i = 0; i < size_of_spit_array; i++)
            {
                if (projectile[i].active)
                {
                    projectile[i].update(parentWorld, currentTime, this);
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

            for (int i = 0; i < size_of_spit_array; i++)
            {
                if (projectile[i].active)
                {
                    sb.Draw(Game1.whitePixel, projectile[i].position, null, Color.Pink, 0.0f, Vector2.Zero, projectile[i].dimensions, SpriteEffects.None, 0.5f);
                }
            }
        }

        public override void knockBack(Vector2 direction, float magnitude, int damage, Entity attacker)
        {
            //base.knockBack(direction, magnitude, damage, attacker);
            if (death == false)
            {
                if (disable_movement_time == 0.0)
                {
                    state = SpitterState.KnockBack;
                    if (Math.Abs(direction.X) > (Math.Abs(direction.Y)))
                    {
                        if (direction.X < 0)
                        {
                            velocity = new Vector2(-2.0f * magnitude, direction.Y / 100 * magnitude);
                        }
                        else
                        {
                            velocity = new Vector2(2.0f * magnitude, direction.Y / 100 * magnitude);
                        }
                    }
                    else
                    {
                        if (direction.Y < 0)
                        {
                            velocity = new Vector2(direction.X / 100f * magnitude, -2.0f * magnitude);
                        }
                        else
                        {
                            velocity = new Vector2((direction.X / 100f) * magnitude, 2.0f * magnitude);
                        }
                    }
                    enemy_life = enemy_life - damage;
                }

                if (attacker == null)
                {
                    return;
                }
                else if (attacker.Enemy_Type != enemy_type && attacker.Enemy_Type != EnemyType.NoType)
                {
                    enemy_found = true;
                    entity_found = attacker;

                    switch (attacker.Direction_Facing)
                    {
                        case GlobalGameConstants.Direction.Right:
                            direction_facing = GlobalGameConstants.Direction.Left;
                            break;
                        case GlobalGameConstants.Direction.Left:
                            direction_facing = GlobalGameConstants.Direction.Right;
                            break;
                        case GlobalGameConstants.Direction.Up:
                            direction_facing = GlobalGameConstants.Direction.Down;
                            break;
                        default:
                            direction_facing = GlobalGameConstants.Direction.Up;
                            break;
                    }
                }
            }
        }

        public override void spinerender(SkeletonRenderer renderer)
        {
            /*if (direction_facing == GlobalGameConstants.Direction.Right || direction_facing == GlobalGameConstants.Direction.Up || direction_facing == GlobalGameConstants.Direction.Down)
            {
                current_skeleton.Skeleton.FlipX = false;
            }
            if (direction_facing == GlobalGameConstants.Direction.Left)
            {
                current_skeleton.Skeleton.FlipX = true;
            }

            current_skeleton.Skeleton.RootBone.X = CenterPoint.X * (current_skeleton.Skeleton.FlipX ? -1 : 1);
            current_skeleton.Skeleton.RootBone.Y = CenterPoint.Y + (dimensions.Y / 2f);

            current_skeleton.Skeleton.RootBone.ScaleX = 1.0f;
            current_skeleton.Skeleton.RootBone.ScaleY = 1.0f;

            current_skeleton.Skeleton.UpdateWorldTransform();
            renderer.Draw(current_skeleton.Skeleton);*/
        }

        public struct SpitProjectile
        {
            private enum ProjectileState
            {
                Travel,
                GrowPool,
                IdlePool,
                DecreasePool,
                Reset
            }
            public Vector2 position;
            private Vector2 original_position;
            private Vector2 velocity;
            public Vector2 dimensions;
            private const float max_dimensions = 100.0f;
            public bool active;

            private float alive_timer;
            private const float max_alive_timer = 800.0f;
            private const float max_pool_alive_timer = 1000.0f;
            private ProjectileState projectile_state;
            public Vector2 CenterPoint { get { return new Vector2(position.X + dimensions.X / 2, position.Y + dimensions.Y / 2); } }
     
            private float damage_timer;
            private const float damage_timer_threshold = 500.0f;
            private const int acid_damage = 2;

            public SpitProjectile(Vector2 launch_position, float angle)
            {
                this.position = launch_position;
                original_position = Vector2.Zero;
                velocity = new Vector2((float)(8*Math.Cos(angle)), (float)(8*Math.Sin(angle)));
                dimensions = new Vector2(10, 10);

                projectile_state = ProjectileState.Travel;
                alive_timer = 0.0f;
                damage_timer = 0.0f;

                active = true;
            }

            public void update(LevelState parentWorld, GameTime currentTime, Entity parent)
            {
                switch(projectile_state) 
                {
                    case ProjectileState.Travel:
                        alive_timer += currentTime.ElapsedGameTime.Milliseconds;
                        position += velocity;

                        if (alive_timer > max_alive_timer)
                        {
                            alive_timer = 0.0f;
                            projectile_state = ProjectileState.GrowPool;
                            original_position = position;
                        }
                        else
                        {
                            foreach(Entity en in parentWorld.EntityList)
                            {
                                if (en == parent)
                                    continue;
                                else if (spitHitTest(en))
                                {
                                    projectile_state = ProjectileState.GrowPool;
                                    alive_timer = 0.0f;
                                    original_position = position;
                                }
                            }
                        }
                        break;
                    case ProjectileState.GrowPool:
                        damage_timer += currentTime.ElapsedGameTime.Milliseconds;
                        if (dimensions.X < max_dimensions && dimensions.Y < max_dimensions)
                        {
                            dimensions += new Vector2(1, 1);
                            position = original_position - (dimensions/2);
                            alive_timer = 0.0f;
                            if (damage_timer > damage_timer_threshold)
                            {
                                foreach (Entity en in parentWorld.EntityList)
                                {
                                    if (spitHitTest(en))
                                    {
                                        if (en is Enemy)
                                        {
                                            ((Enemy)en).Enemy_Life -= acid_damage;
                                        }
                                        else if (en is Player)
                                        {
                                            GameCampaign.Player_Health -= acid_damage;
                                        }
                                    }
                                }
                                damage_timer = 0.0f;
                            }
                        }
                        else
                        {
                            projectile_state = ProjectileState.IdlePool;
                            /*alive_timer += currentTime.ElapsedGameTime.Milliseconds;
                            if (alive_timer > max_pool_alive_timer)
                            {
                                alive_timer = 0.0f;
                                projectile_state = ProjectileState.Reset;
                            }*/
                        }
                        break;
                    case ProjectileState.IdlePool:
                        alive_timer += currentTime.ElapsedGameTime.Milliseconds;
                        damage_timer += currentTime.ElapsedGameTime.Milliseconds;
                        if (damage_timer > damage_timer_threshold)
                        {
                            foreach (Entity en in parentWorld.EntityList)
                            {
                                if (spitHitTest(en))
                                {
                                    if (en is Enemy)
                                    {
                                        ((Enemy)en).Enemy_Life -= acid_damage;
                                    }
                                    else if (en is Player)
                                    {
                                        GameCampaign.Player_Health -= acid_damage;
                                    }
                                }
                            }
                            damage_timer = 0.0f;
                        }
                        if (alive_timer > max_pool_alive_timer)
                        {
                            alive_timer = 0.0f;
                            projectile_state = ProjectileState.DecreasePool;
                        }
                        break;
                    case ProjectileState.DecreasePool:
                        damage_timer += currentTime.ElapsedGameTime.Milliseconds;
                        if (dimensions.X > 0 && dimensions.Y > 0)
                        {
                            dimensions -= new Vector2(1, 1);
                            position = original_position - (dimensions / 2);
                            if (damage_timer > damage_timer_threshold)
                            {
                                foreach (Entity en in parentWorld.EntityList)
                                {
                                    if (spitHitTest(en))
                                    {
                                        if (en is Enemy)
                                        {
                                            ((Enemy)en).Enemy_Life -= acid_damage;
                                        }
                                        else if (en is Player)
                                        {
                                            GameCampaign.Player_Health -= acid_damage;
                                        }
                                    }
                                }
                                damage_timer = 0.0f;
                            }
                        }
                        else
                        {
                            active = false;
                        }
                        break;
                    default:
                        break;
                }
            }

            public bool spitHitTest(Entity other)
            {
                if (position.X > other.Position.X + other.Dimensions.X || position.X + dimensions.X < other.Position.X || position.Y > other.Position.Y + other.Dimensions.Y || position.Y + dimensions.Y < other.Position.Y)
                {
                    return false;
                }

                return true;
            }
        }
    }
}
