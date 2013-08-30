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
    public class MutantAcidSpitter : Enemy
    {
        private enum SpitterState
        {
            KnockBack,
            Alert,
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
        private float alert_timer = 0.0f;
        private float spitter_timer = 0.0f;
        private bool spit_fired = false;

        private float angle = 0.0f;

        private float distance = 0.0f;

        private const int size_of_spit_array = 5;
        private int spitter_count = 0;
        private SpitProjectile[] projectile = new SpitProjectile[size_of_spit_array];

        private Entity entity_found = null;

        private AnimationLib.SpineAnimationSet[] directionAnims = null;
        private string[] deathAnims = { "die", "die2", "die3" };

        private AnimationLib.FrameAnimationSet acid_pool;

        public MutantAcidSpitter(LevelState parentWorld, float initial_x, float initial_y)
        {
            position = new Vector2(initial_x, initial_y);
            velocity_speed = 2.0f;
            velocity = new Vector2(velocity_speed, 0);
            dimensions = new Vector2(48.0f, 48.0f);

            enemy_damage = 4;
            enemy_life = 5;
            windup_timer = 0.0f;
            spitter_count = 0;
            change_direction_time = 0.0f;
            change_direction_time_threshold = 1000.0f;
            angle = 0.0f;
            range_distance = 500.0f;

            prob_item_drop = 0.3;
            number_drop_items = 4;

            state = SpitterState.Search;
            this.parentWorld = parentWorld;
            direction_facing = GlobalGameConstants.Direction.Right;
            enemy_type = EnemyType.Alien;
            entity_found = null;
            acid_pool = AnimationLib.getFrameAnimationSet("acidPool");

            for (int i = 0; i < size_of_spit_array; i++)
            {
                projectile[i] = new SpitProjectile(new Vector2(0,0), 0);
                projectile[i].active = false;
            }

            death = false;

            directionAnims = new AnimationLib.SpineAnimationSet[4];
            directionAnims[(int)GlobalGameConstants.Direction.Up] = AnimationLib.loadNewAnimationSet("acidSpitterUp");
            directionAnims[(int)GlobalGameConstants.Direction.Down] = AnimationLib.loadNewAnimationSet("acidSpitterDown");
            directionAnims[(int)GlobalGameConstants.Direction.Left] = AnimationLib.loadNewAnimationSet("acidSpitterRight");
            directionAnims[(int)GlobalGameConstants.Direction.Left].Skeleton.FlipX = true;
            directionAnims[(int)GlobalGameConstants.Direction.Right] = AnimationLib.loadNewAnimationSet("acidSpitterRight");
            
            for (int i = 0; i < 4; i++)
            {
                directionAnims[i].Animation = directionAnims[i].Skeleton.Data.FindAnimation("run");
            }
        }

        public override void update(GameTime currentTime)
        {
            change_direction_time += currentTime.ElapsedGameTime.Milliseconds;
            animation_time += currentTime.ElapsedGameTime.Milliseconds;

            if (state == SpitterState.Search && sound_alert && entity_found == null && !death)
            {
                alert_timer = 0.0f;
                animation_time = 0.0f;
                state = SpitterState.Alert;
            }

            if (!death && enemy_life <= 0)
            {
                state = SpitterState.Death;
                death = true;
                directionAnims[(int)direction_facing].Animation = directionAnims[(int)direction_facing].Skeleton.Data.FindAnimation(deathAnims[Game1.rand.Next() % 3]);
                animation_time = 0.0f;
                parentWorld.pushCoin(this);
            }

            switch (state)
            {
                case SpitterState.Search:
                    directionAnims[(int)direction_facing].Animation = directionAnims[(int)direction_facing].Skeleton.Data.FindAnimation("run");

                    if (death == false)
                    {
                        if (enemy_found == false)
                        {

                            foreach (Entity en in parentWorld.EntityList)
                            {
                                if (en == this)
                                    continue;
                                else if (en.Enemy_Type != enemy_type && en.Enemy_Type != EnemyType.NoType && en.Death == false && en.Death == false)
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
                            animation_time = 0.0f;
                        }
                    }
                    break;
                case SpitterState.Alert:
                    directionAnims[(int)direction_facing].Animation = directionAnims[(int)direction_facing].Skeleton.Data.FindAnimation("idle");

                    if (sound_alert && entity_found == null)
                    {
                        //if false then sound didn't hit a wall
                        if (!parentWorld.Map.soundInSight(this, sound_position))
                        {
                            directionAnims[(int)direction_facing].Animation = directionAnims[(int)direction_facing].Skeleton.Data.FindAnimation("run");
                            alert_timer += currentTime.ElapsedGameTime.Milliseconds;
                            for (int i = 0; i < parentWorld.EntityList.Count; i++)
                            {
                                if (parentWorld.EntityList[i].Enemy_Type != enemy_type && parentWorld.EntityList[i].Enemy_Type != EnemyType.NoType && parentWorld.EntityList[i].Death == false)
                                {
                                    distance = Vector2.Distance(CenterPoint, parentWorld.EntityList[i].CenterPoint);
                                    if (distance <= range_distance)
                                    {
                                        enemy_found = true;
                                        entity_found = parentWorld.EntityList[i];
                                        state = SpitterState.WindUp;
                                        animation_time = 0.0f;
                                        sound_alert = false;
                                        alert_timer = 0.0f;
                                        windup_timer = 0.0f;
                                        animation_time = 0.0f;
                                        velocity = Vector2.Zero;
                                        break;
                                    }
                                }
                            }

                            if (alert_timer > 3000 || ((int)CenterPoint.X == (int)sound_position.X && (int)CenterPoint.Y == (int)sound_position.Y))
                            {
                                entity_found = null;
                                enemy_found = false;
                                sound_alert = false;
                                state = SpitterState.Search;
                                velocity = Vector2.Zero;
                                animation_time = 0.0f;
                                windup_timer = 0.0f;
                                animation_time = 0.0f;
                            }
                        }
                        else
                        {
                            entity_found = null;
                            enemy_found = false;
                            sound_alert = false;
                            state = SpitterState.Search;
                            velocity = Vector2.Zero;
                            animation_time = 0.0f;
                            windup_timer = 0.0f;
                            animation_time = 0.0f;
                        }
                    }
                    else if (entity_found != null)
                    {
                        sound_alert = false;
                        distance = Vector2.Distance(CenterPoint, entity_found.CenterPoint);
                        if (parentWorld.Map.enemyWithinRange(entity_found, this, range_distance) && distance < range_distance && entity_found.Death == false)
                        {
                            state = SpitterState.WindUp;
                            animation_time = 0.0f;
                            windup_timer = 0.0f;
                        }
                        else
                        {
                            entity_found = null;
                            enemy_found = false;
                            state = SpitterState.Search;
                            velocity = Vector2.Zero;
                            animation_time = 0.0f;
                            windup_timer = 0.0f;
                            animation_time = 0.0f;
                        }
                    }
                    break;
                case SpitterState.WindUp:
                    windup_timer += currentTime.ElapsedGameTime.Milliseconds;
                    directionAnims[(int)direction_facing].Animation = directionAnims[(int)direction_facing].Skeleton.Data.FindAnimation("windUp");
                    if (windup_timer > max_windup_timer)
                    {
                        state = SpitterState.Fire;
                        spitter_timer = 0.0f;
                        windup_timer = 0.0f;
                        animation_time = 0.0f;
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
                    directionAnims[(int)direction_facing].Animation = directionAnims[(int)direction_facing].Skeleton.Data.FindAnimation("attack");
                    spitter_timer += currentTime.ElapsedGameTime.Milliseconds;

                    if(!spit_fired)
                    {
                        for (int i = 0; i < size_of_spit_array; i++)
                        {
                            if (!projectile[i].active)
                            {
                                AudioLib.playSoundEffect("acidSpit");
                                projectile[i] = new SpitProjectile(new Vector2(directionAnims[(int)direction_facing].Skeleton.FindBone("head").WorldX, directionAnims[(int)direction_facing].Skeleton.FindBone("head").WorldY), angle);
                                spit_fired = true;
                                break;
                            }
                        }
                    }

                    if (spitter_timer > 500)
                    {
                        if (entity_found.Death)
                        {
                            entity_found = null;
                            enemy_found = false;
                        }
                        spit_fired = false;
                        state = SpitterState.Alert;
                        spitter_timer = 0.0f;
                    }
                    break;
                case SpitterState.KnockBack:
                    disable_movement_time += currentTime.ElapsedGameTime.Milliseconds;
                    directionAnims[(int)direction_facing].Animation = directionAnims[(int)direction_facing].Skeleton.Data.FindAnimation("hurt");
                    
                    if (disable_movement_time > 300)
                    {
                        state = SpitterState.Search;
                        disable_movement_time = 0.0f;
                    }
                    break;
                case SpitterState.Death:
                    velocity = Vector2.Zero;
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

            directionAnims[(int)direction_facing].Animation.Apply(directionAnims[(int)direction_facing].Skeleton, animation_time / 1000f, state == SpitterState.Search ? true : false);
        }

        public override void draw(Spine.SkeletonRenderer sb)
        {
            //sb.DrawSpriteToSpineVertexArray(Game1.whitePixel, new Rectangle(0, 0, 1, 1), position, Color.White, 0.0f, dimensions);

            for (int i = 0; i < size_of_spit_array; i++)
            {
                if (projectile[i].active)
                {
                    if(projectile[i].Projectile_State == SpitProjectile.ProjectileState.Travel)
                    {
                        //sb.DrawSpriteToSpineVertexArray(Game1.whitePixel, new Rectangle(0, 0, 1, 1), projectile[i].position, Color.Pink, 0.0f, projectile[i].dimensions);
                        acid_pool.drawAnimationFrame(0.0f, sb, projectile[i].position - new Vector2(64), new Vector2(0.23f), 0.5f, projectile[i].alive_timer, Vector2.Zero, Color.White);
                    }
                }
            }

            //drawAcidSplotches(sb);
        }

        public void drawAcidSplotches(Spine.SkeletonRenderer sb)
        {
            for (int i = 0; i < size_of_spit_array; i++)
            {
                if (projectile[i].active)
                {
                    if (projectile[i].Projectile_State != SpitProjectile.ProjectileState.Travel)
                    {
                        acid_pool.drawAnimationFrame(projectile[i].alive_timer, sb, projectile[i].CenterPoint - acid_pool.FrameDimensions / 2, new Vector2(projectile[i].scale), 0.5f, 0.0f, projectile[i].CenterPoint, Color.White);
                    }
                }
            }
        }

        public override void knockBack(Vector2 direction, float magnitude, int damage, Entity attacker)
        {
            if (death == false)
            {
                if (disable_movement_time == 0.0)
                {
                    AudioLib.playSoundEffect("fleshyKnockBack");
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
            parentWorld.Particles.pushBloodParticle(CenterPoint);
            parentWorld.Particles.pushBloodParticle(CenterPoint);
            parentWorld.Particles.pushBloodParticle(CenterPoint);
        }

        public override void spinerender(SkeletonRenderer renderer)
        {
            AnimationLib.SpineAnimationSet currentSkeleton = directionAnims[(int)direction_facing];

            currentSkeleton.Skeleton.RootBone.X = CenterPoint.X * (currentSkeleton.Skeleton.FlipX ? -1 : 1);
            currentSkeleton.Skeleton.RootBone.Y = CenterPoint.Y + (dimensions.Y / 2f);

            currentSkeleton.Skeleton.RootBone.ScaleX = 1.0f;
            currentSkeleton.Skeleton.RootBone.ScaleY = 1.0f;

            currentSkeleton.Skeleton.UpdateWorldTransform();
            renderer.Draw(currentSkeleton.Skeleton);
        }

        public struct SpitProjectile
        {
            public enum ProjectileState
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
            private bool on_wall;
            private Vector2 nextStep_temp;

            public float scale;
            private const float scale_factor = 128.0f;

            public float alive_timer;
            private const float max_alive_timer = 500.0f;
            private const float max_pool_alive_timer = 1000.0f;
            private ProjectileState projectile_state;
            public ProjectileState Projectile_State
            {
                get { return projectile_state; }
            }

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
                scale = 0.0f;

                projectile_state = ProjectileState.Travel;
                alive_timer = 0.0f;
                damage_timer = 0.0f;

                on_wall = false;
                nextStep_temp = Vector2.Zero;
                active = true;
            }

            public void update(LevelState parentWorld, GameTime currentTime, Entity parent)
            {
                if (active)
                {
                    parentWorld.Particles.pushDotParticle(position + new Vector2((float)Game1.rand.NextDouble() * dimensions.X, (float)Game1.rand.NextDouble() * dimensions.Y), (float)(1.5 * Math.PI), Color.DarkGreen);
                }

                switch(projectile_state) 
                {
                    case ProjectileState.Travel:
                        alive_timer += currentTime.ElapsedGameTime.Milliseconds;
                        position += velocity;

                        if (alive_timer > max_alive_timer)
                        {
                            alive_timer = 0.0f;
                            projectile_state = ProjectileState.GrowPool;
                            original_position = CenterPoint;
                        }
                        else
                        {
                            nextStep_temp = new Vector2(position.X - (dimensions.X / 2) + velocity.X, (position.Y + velocity.X));

                            on_wall = parentWorld.Map.hitTestWall(nextStep_temp);
                            int check_corners = 0;
                            while (check_corners != 4)
                            {
                                if (on_wall == false)
                                {
                                    if (check_corners == 0)
                                    {
                                        nextStep_temp = new Vector2(position.X + (dimensions.X / 2) + velocity.X, position.Y + velocity.Y);
                                    }
                                    else if (check_corners == 1)
                                    {
                                        nextStep_temp = new Vector2(position.X + velocity.X, position.Y - (dimensions.Y / 2) + velocity.Y);
                                    }
                                    else if (check_corners == 2)
                                    {
                                        nextStep_temp = new Vector2(position.X + velocity.X, position.Y + dimensions.Y + velocity.Y);
                                    }
                                    else
                                    {
                                        position += velocity;
                                    }
                                    on_wall = parentWorld.Map.hitTestWall(nextStep_temp);
                                }
                                else
                                {
                                    projectile_state = ProjectileState.GrowPool;
                                    alive_timer = 0.0f;
                                    original_position = CenterPoint;
                                    break;
                                }
                                check_corners++;
                            }

                            if (on_wall == false)
                            {
                                foreach (Entity en in parentWorld.EntityList)
                                {
                                    if (en == parent || (en.Enemy_Type == EnemyType.Alien))
                                        continue;
                                    else if (spitHitTest(en))
                                    {
                                        projectile_state = ProjectileState.GrowPool;
                                        alive_timer = 0.0f;
                                        original_position = CenterPoint;
                                    }
                                }
                            }
                        }
                        break;
                    case ProjectileState.GrowPool:
                        damage_timer += currentTime.ElapsedGameTime.Milliseconds;
                        if (dimensions.X < max_dimensions && dimensions.Y < max_dimensions)
                        {
                            scale = (dimensions.X + 1) / scale_factor;
                            dimensions += new Vector2(1, 1);
                            position = original_position - (dimensions/2);
                            alive_timer = 0.0f;
                            if (damage_timer > damage_timer_threshold)
                            {
                                foreach (Entity en in parentWorld.EntityList)
                                {
                                    if (spitHitTest(en))
                                    {
                                        if (en is Enemy && !(en is MutantAcidSpitter))
                                        {
                                            ((Enemy)en).Enemy_Life -= acid_damage;
                                        }
                                        else if (en is Player)
                                        {
                                            if (((Player)en).Index == InputDevice2.PPG_Player.Player_1)
                                            {
                                                GameCampaign.Player_Health -= acid_damage;
                                            }
                                            else if (((Player)en).Index == InputDevice2.PPG_Player.Player_2)
                                            {
                                                GameCampaign.Player2_Health -= acid_damage;
                                            }
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
                                    if (en is Enemy && !(en is MutantAcidSpitter))
                                    {
                                        ((Enemy)en).Enemy_Life -= acid_damage;
                                    }
                                    else if (en is Player)
                                    {
                                        if (((Player)en).Index == InputDevice2.PPG_Player.Player_1)
                                        {
                                            GameCampaign.Player_Health -= acid_damage;
                                        }
                                        else if (((Player)en).Index == InputDevice2.PPG_Player.Player_2)
                                        {
                                            GameCampaign.Player2_Health -= acid_damage;
                                        }
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
                            if (dimensions.X - 1 != 0)
                            {
                                scale = (dimensions.X - 1) / scale_factor;
                            }
                            else
                            {
                                scale = 0.0f;
                            }
                            dimensions -= new Vector2(1, 1);
                            position = original_position - (dimensions / 2);
                            if (damage_timer > damage_timer_threshold)
                            {
                                foreach (Entity en in parentWorld.EntityList)
                                {
                                    if (spitHitTest(en))
                                    {
                                        if (en is Enemy && !(en is MutantAcidSpitter))
                                        {
                                            ((Enemy)en).Enemy_Life -= acid_damage;
                                        }
                                        else if (en is Player)
                                        {
                                            if (((Player)en).Index == InputDevice2.PPG_Player.Player_1)
                                            {
                                                GameCampaign.Player_Health -= acid_damage;
                                            }
                                            else if (((Player)en).Index == InputDevice2.PPG_Player.Player_2)
                                            {
                                                GameCampaign.Player2_Health -= acid_damage;
                                            }
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
