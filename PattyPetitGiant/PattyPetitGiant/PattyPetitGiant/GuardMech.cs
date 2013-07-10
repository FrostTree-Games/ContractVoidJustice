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
    class GuardMech : Enemy, SpineEntity
    {
        public enum MechState
        {
            Moving,
            FiringWindUp,
            Firing,
            MeleeWindUp,
            Melee,
            Reset,
            Death,
        }

        private const float enemy_range_damage = 10.0f;
        private const float enemy_melee_damage = 20.0f;
        private const float knockback_magnitude = 2.0f;

        private float windup_timer = 0.0f;
        private float turret_angle = 0.0f;
        private float tank_hull_angle = 0.0f;
        private const float tank_hull_turn_magnitude = 0.02f;
        private bool melee_active = false;
        private bool death = false;

        private MechState mech_state = MechState.Moving;
        private EnemyComponents component = null;
        private Entity entity_found = null;
        private Grenades grenade;
        private Vector2 melee_hitbox = new Vector2(48,48);
        private Vector2 melee_position;
        private float angle;

        private AnimationLib.FrameAnimationSet tankAnim;

        public GuardMech(LevelState parentWorld, float initial_x, float initial_y)
        {
            position = new Vector2(initial_x, initial_y);
            melee_position = Vector2.Zero;
            dimensions = new Vector2(96, 120);
            velocity = new Vector2(0.8f, 0.0f);

            windup_timer = 0.0f;
            angle = 0.0f;
            turret_angle = angle;

            enemy_life = 50;
            disable_movement = false;
            disable_movement_time = 0.0f;
            enemy_found = false;
            change_direction_time = 0.0f;
            this.parentWorld = parentWorld;
            enemy_type = EnemyType.Guard;
            change_direction_time_threshold = 3000.0f;
            direction_facing = GlobalGameConstants.Direction.Right;

            component = new MoveSearch();
            mech_state = MechState.Moving;
            enemy_type = EnemyType.Guard;
            velocity_speed = 3.0f;
            entity_found = null;
            death = false;

            grenade = new Grenades(Vector2.Zero, 0.0f);

            walk_down = AnimationLib.loadNewAnimationSet("tankTurret");
            current_skeleton = walk_down;
            current_skeleton.Animation = current_skeleton.Skeleton.Data.FindAnimation("idle");
            current_skeleton.Skeleton.FlipX = false;
            animation_time = 0.0f;

            tankAnim = AnimationLib.getFrameAnimationSet("tank");
        }

        public override void update(GameTime currentTime)
        {
            float distance = float.MaxValue;

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
                        current_skeleton.Animation = current_skeleton.Skeleton.Data.FindAnimation("idle");
                        change_direction_time += currentTime.ElapsedGameTime.Milliseconds;

                        if (enemy_found == false)
                        {
                            foreach (Entity en in parentWorld.EntityList)
                            {
                                if (en == this)
                                    continue;
                                else if (en.Enemy_Type != enemy_type && en.Enemy_Type != EnemyType.NoType && en.Death==false)
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

                        switch (direction_facing)
                        {
                            case GlobalGameConstants.Direction.Right:
                                velocity = new Vector2(0.5f, 0f);
                                dimensions = new Vector2(120, 96);
                                angle = 0.0f;
                                break;
                            case GlobalGameConstants.Direction.Left:
                                velocity = new Vector2(-0.5f, 0f);
                                dimensions = new Vector2(120, 96);
                                angle = (float)(Math.PI);
                                break;
                            case GlobalGameConstants.Direction.Up:
                                velocity = new Vector2(0f, -0.5f);
                                dimensions = new Vector2(96, 120);
                                angle = (float)(-1*Math.PI/2);
                                break;
                            default:
                                velocity = new Vector2(0.0f, 0.5f);
                                dimensions = new Vector2(96, 120);
                                angle = (float)(Math.PI/2);
                                break;
                        }

                        if (enemy_found)
                        {
                            distance = Vector2.Distance(position, entity_found.Position);

                            if (Math.Abs(distance) < 300)
                            {
                                if (Math.Abs(distance) > 192 && Math.Abs(distance) < 600)
                                {
                                    mech_state = MechState.Firing;
                                }
                                else
                                {
                                    mech_state = MechState.MeleeWindUp;
                                }
                            }
                        }
                        break;
                    case MechState.Firing:
                        windup_timer += currentTime.ElapsedGameTime.Milliseconds;
                        angle = (float)Math.Atan2(entity_found.CenterPoint.Y - current_skeleton.Skeleton.FindBone("muzzle").WorldY, entity_found.CenterPoint.X - current_skeleton.Skeleton.FindBone("muzzle").WorldX);

                        distance = Vector2.Distance(position, entity_found.Position);

                        velocity = Vector2.Zero;

                        if (Math.Abs(distance) > 600 || entity_found.Remove_From_List == true)
                        {
                            mech_state = MechState.Moving;
                            windup_timer = 0.0f;
                            enemy_found = false;
                            return;
                        }
                        else if (Math.Abs(distance) < 192)
                        {
                            windup_timer = 0.0f;
                            mech_state = MechState.MeleeWindUp;
                        }

                        if (windup_timer > 2000)
                        {
                            if (grenade.active == false)
                            {
                                current_skeleton.Animation = current_skeleton.Skeleton.Data.FindAnimation("attack");
                                grenade = new Grenades(new Vector2(current_skeleton.Skeleton.FindBone("muzzle").WorldX, current_skeleton.Skeleton.FindBone("muzzle").WorldY), angle);
                                grenade.active = true;
                            }
                        }

                        if (windup_timer > 2100)
                        {
                            windup_timer = 0.0f;
                            current_skeleton.Animation = current_skeleton.Skeleton.Data.FindAnimation("idle");
                        }

                        switch(direction_facing)
                        {
                            case GlobalGameConstants.Direction.Right:
                                if(angle < -1 * Math.PI/3.27)
                                {
                                    direction_facing = GlobalGameConstants.Direction.Up;
                                    dimensions = new Vector2(96, 120);
                                }
                                else if (angle > Math.PI / 3.27)
                                {
                                    direction_facing = GlobalGameConstants.Direction.Down;
                                    dimensions = new Vector2(96, 120);
                                }
                                break;
                            case GlobalGameConstants.Direction.Left:
                                if (angle < Math.PI / 1.44 && angle > Math.PI/1.5)
                                {
                                    direction_facing = GlobalGameConstants.Direction.Down;
                                    dimensions = new Vector2(96, 120);
                                }
                                else if (angle > -1*Math.PI / 1.44 && angle< -1*Math.PI/1.5)
                                {
                                    direction_facing = GlobalGameConstants.Direction.Up;
                                    dimensions = new Vector2(96, 120);
                                }
                                break;
                            case GlobalGameConstants.Direction.Up:
                                if (angle < -1 * Math.PI / 1.24)
                                {
                                    direction_facing = GlobalGameConstants.Direction.Left;
                                    dimensions = new Vector2(120, 96);
                                }
                                else if (angle > -1 * Math.PI / 5.14)
                                {
                                    direction_facing = GlobalGameConstants.Direction.Right;
                                    dimensions = new Vector2(120, 96);
                                }
                                break;
                            default:
                                if (angle < Math.PI / 5.14)
                                {
                                    direction_facing = GlobalGameConstants.Direction.Right;
                                    dimensions = new Vector2(120, 96);
                                    //current_skeleton = walk_right;
                                }
                                else if (angle > Math.PI / 1.24)
                                {
                                    direction_facing = GlobalGameConstants.Direction.Left;
                                    dimensions = new Vector2(120, 96);
                                    //current_skeleton = walk_right;
                                }
                                break;
                        }

                        break;
                    case MechState.MeleeWindUp:
                        windup_timer += currentTime.ElapsedGameTime.Milliseconds;
                        angle = (float)Math.Atan2(entity_found.CenterPoint.Y - CenterPoint.Y, entity_found.CenterPoint.X - CenterPoint.X);

                        if (windup_timer > 1000)
                        {
                            mech_state = MechState.Melee;
                            windup_timer = 0.0f;
                            melee_active = true;
                        }

                        switch (direction_facing)
                        {
                            case GlobalGameConstants.Direction.Right:
                                if (angle < -1 * Math.PI / 3.27)
                                {
                                    direction_facing = GlobalGameConstants.Direction.Up;
                                    dimensions = new Vector2(96, 120);
                                    //current_skeleton = walk_up;
                                }
                                else if (angle > Math.PI / 3.27)
                                {
                                    direction_facing = GlobalGameConstants.Direction.Down;
                                    dimensions = new Vector2(96, 120);
                                    //current_skeleton = walk_down;
                                }
                                break;
                            case GlobalGameConstants.Direction.Left:
                                if (angle < Math.PI / 1.44 && angle > Math.PI / 1.5)
                                {
                                    direction_facing = GlobalGameConstants.Direction.Down;
                                    dimensions = new Vector2(96, 120);
                                    //current_skeleton = walk_down;
                                }
                                else if (angle > -1 * Math.PI / 1.44 && angle < -1 * Math.PI / 1.5)
                                {
                                    direction_facing = GlobalGameConstants.Direction.Up;
                                    dimensions = new Vector2(96, 120);
                                    //current_skeleton = walk_up;
                                }
                                break;
                            case GlobalGameConstants.Direction.Up:
                                if (angle < -1 * Math.PI / 1.24)
                                {
                                    direction_facing = GlobalGameConstants.Direction.Left;
                                    dimensions = new Vector2(120, 96);
                                    //current_skeleton = walk_right;
                                }
                                else if (angle > -1 * Math.PI / 5.14)
                                {
                                    direction_facing = GlobalGameConstants.Direction.Right;
                                    dimensions = new Vector2(120, 96);
                                    //current_skeleton = walk_right;
                                }
                                break;
                            default:
                                if (angle < Math.PI / 5.14)
                                {
                                    direction_facing = GlobalGameConstants.Direction.Right;
                                    dimensions = new Vector2(120, 96);
                                    //current_skeleton = walk_right;
                                }
                                else if (angle > Math.PI / 1.24)
                                {
                                    direction_facing = GlobalGameConstants.Direction.Left;
                                    dimensions = new Vector2(120, 96);
                                    //current_skeleton = walk_right;
                                }
                                break;
                        }
                        break;
                    case MechState.Melee:
                        //current_skeleton.Animation = current_skeleton.Skeleton.Data.FindAnimation("idle");
                        windup_timer += currentTime.ElapsedGameTime.Milliseconds;
                        
                        switch(direction_facing)
                        {
                            case GlobalGameConstants.Direction.Right:
                                melee_position = position + new Vector2(dimensions.X, melee_hitbox.Y/4);
                                break;
                            case GlobalGameConstants.Direction.Left:
                                melee_position = position + new Vector2(-1*melee_hitbox.X, dimensions.Y/4);
                                break;
                            case GlobalGameConstants.Direction.Up:
                                melee_position = position + new Vector2(dimensions.X / 4, -1 * melee_hitbox.Y);
                                break;
                            default:
                                melee_position = position + new Vector2(dimensions.X / 4, dimensions.Y);
                                break;
                        }

                        foreach (Entity en in parentWorld.EntityList)
                        {
                            if (en == this)
                                continue;
                            else if(meleeHitTest(en))
                            {
                                Vector2 direction = en.CenterPoint - CenterPoint;
                                en.knockBack(direction, 3.5f, 10);
                            }
                        }

                        velocity = Vector2.Zero;
                        if (windup_timer > 500)
                        {
                            windup_timer = 0.0f;
                            melee_active = false;
                            if (Math.Abs(distance) > 300 || entity_found.Remove_From_List)
                            {
                                enemy_found = false;
                                mech_state = MechState.Moving;
                            }
                            else if (Math.Abs(distance) > 192 && Math.Abs(distance) < 300)
                            {
                                mech_state = MechState.Firing;
                            }
                            else
                            {
                                mech_state = MechState.MeleeWindUp;
                            }
                        }

                        
                        break;
                    case MechState.Reset:
                        windup_timer = 0.0f;
                        enemy_found = false;
                        mech_state = MechState.Moving;
                        break;
                    case MechState.Death:
                        break;
                    default:
                        break;
                }
            }

            if (grenade.active)
            {
                grenade.update(parentWorld, currentTime, this);
            }

            if (enemy_life <= 0)
            {

            }

            Vector2 pos = new Vector2(position.X, position.Y);
            Vector2 nextStep = new Vector2(position.X + velocity.X, position.Y + velocity.Y);
            Vector2 finalPos = parentWorld.Map.reloactePosition(pos, nextStep, dimensions);
            position.X = finalPos.X;
            position.Y = finalPos.Y;

            animation_time += currentTime.ElapsedGameTime.Milliseconds / 1000f;
            current_skeleton.Animation.Apply(current_skeleton.Skeleton, animation_time, true);
        }

        public override void draw(Spine.SkeletonRenderer sb)
        {
            //sb.Draw(Game1.whitePixel, position, null, Color.White, 0.0f, Vector2.Zero, dimensions, SpriteEffects.None, 0.5f);

            Vector2 offset = new Vector2(19, 0);

            switch (direction_facing)
            {
                case GlobalGameConstants.Direction.Right:
                    if ((float)(tank_hull_angle) < (float)((Math.PI / 2) -0.02f))
                    {
                        tank_hull_angle += tank_hull_turn_magnitude;
                    }
                    else if ((float)(tank_hull_angle) > ((float)((Math.PI / 2)+0.02f)))
                    {
                        tank_hull_angle -= tank_hull_turn_magnitude;
                    }
                    offset = new Vector2(-19, 0);
                    break;
                case GlobalGameConstants.Direction.Left:
                    if ((float)(tank_hull_angle) < (float)(3 * Math.PI / 2) - 0.02f)
                    {
                        tank_hull_angle += tank_hull_turn_magnitude;
                    }
                    else if ((float)(tank_hull_angle) > ((float)(3 * Math.PI / 2) + 0.02f))
                    {
                        tank_hull_angle -= tank_hull_turn_magnitude;
                    }
                    offset = new Vector2(-19, 0);
                    break;
                case GlobalGameConstants.Direction.Up:
                    if ((float)(tank_hull_angle) < (float)(0.0) - 0.02f)
                    {
                        tank_hull_angle += tank_hull_turn_magnitude;
                    }
                    else if ((float)(tank_hull_angle) > ((float)(0.0)+0.02))
                    {
                        tank_hull_angle -= tank_hull_turn_magnitude;
                    }
                    offset = new Vector2(0, 19);
                    break;
                default:
                    if ((float)(tank_hull_angle) < (float)(Math.PI) -0.02)
                    {
                        tank_hull_angle += tank_hull_turn_magnitude;
                    }
                    else if ((float)(tank_hull_angle) > ((float)(Math.PI)+0.02))
                    {
                        tank_hull_angle -= tank_hull_turn_magnitude;
                    }
                    offset = new Vector2(0, 19);
                    break;
            }

            tankAnim.drawAnimationFrame(0.0f, sb, position - offset/2, new Vector2(1), 0.5f, tank_hull_angle, new Vector2(48f, 69.5f), Color.White);

            //sb.DrawSpriteToSpineVertexArray(Game1.whitePixel, new Rectangle(0,0,1,1),position, Color.Green, 0.0f, dimensions);

            //tankAnim.drawAnimationFrame(0.0f, sb, CenterPoint, new Vector2(1, 1), 0.5f, tank_hull_angle, new Vector2(48f,69.5f));

            if(grenade.active)
            {
                sb.DrawSpriteToSpineVertexArray(Game1.whitePixel, new Rectangle(0, 0, 1, 1), grenade.Position, Color.Blue, 0.0f, grenade.Dimensions);
                //sb.Draw(Game1.whitePixel, grenade.Position, null, Color.Blue, 0.0f, Vector2.Zero, grenade.Dimensions, SpriteEffects.None, 0.5f);
            }
            if (melee_active)
            {
                sb.DrawSpriteToSpineVertexArray(Game1.whitePixel, new Rectangle(0, 0, 1, 1), melee_position, Color.Blue, 0.0f, melee_hitbox);
                //sb.Draw(Game1.whitePixel, melee_position, null, Color.Blue, 0.0f, Vector2.Zero, melee_hitbox, SpriteEffects.None, 0.5f);
            }
        }

        public override void knockBack(Vector2 direction, float magnitude, int damage, Entity attacker)
        {
            if (death == false)
            {
                if (disable_movement_time == 0.0)
                {
                    disable_movement = true;
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
            current_skeleton.Skeleton.RootBone.X = CenterPoint.X;
            current_skeleton.Skeleton.RootBone.Y = CenterPoint.Y;

            if ((int)turret_angle < (int)(angle * 180 / Math.PI))
            {
                turret_angle++;
            }
            else if ((int)turret_angle > (int)(angle * 180 / Math.PI))
            {
                turret_angle--;
            }

            current_skeleton.Skeleton.RootBone.Rotation = (float)(-1* turret_angle) - 90.0f;

            current_skeleton.Skeleton.RootBone.ScaleX = 1.0f;
            current_skeleton.Skeleton.RootBone.ScaleY = 1.0f;

            current_skeleton.Skeleton.UpdateWorldTransform();
            renderer.Draw(current_skeleton.Skeleton);
        }

        public bool meleeHitTest(Entity other)
        {
            if (melee_position.X > other.Position.X + other.Dimensions.X || melee_position.X + melee_hitbox.X < other.Position.X || melee_position.Y > other.Position.Y + other.Dimensions.Y || melee_position.Y + melee_hitbox.Y < other.Position.Y)
            {
                return false;
            }
            return true;
        }

        public struct Grenades
        {
            private Vector2 position;
            public Vector2 Position { get { return position; } }

            private Vector2 dimensions;
            public Vector2 Dimensions { get { return dimensions; } }

            private Vector2 velocity;
            public bool active;

            private enum GrenadeState
            {
                Travel,
                Explosion,
                Reset
            }
            private GrenadeState state;

            private float active_timer;
            private const float max_active_time = 2000.0f;
            private float explosion_timer;
            private const float max_explosion_timer = 1000.0f;
            public Vector2 CenterPoint { get { return new Vector2(position.X + dimensions.X / 2, position.Y + dimensions.Y / 2); } }

            public Grenades(Vector2 parent_position, float angle)
            {
                this.position = parent_position;
                dimensions = new Vector2(16, 16);
                velocity = new Vector2((float)(4*Math.Cos(angle)), (float)(4*Math.Sin(angle)));
                state = GrenadeState.Travel;

                active = false;
                active_timer = 0.0f;
                explosion_timer = 0.0f;
            }

            public void update(LevelState parentWorld, GameTime currentTime, Entity parent)
            {
                active_timer += currentTime.ElapsedGameTime.Milliseconds;
                if (active)
                {
                    switch(state)
                    {
                        case GrenadeState.Travel:
                        if (active_timer > max_active_time)
                        {
                            state = GrenadeState.Explosion;
                            active_timer = 0.0f;
                            dimensions = new Vector2(96, 96);
                            position = CenterPoint - (dimensions / 2);
                        }
                        else
                        {
                            foreach (Entity en in parentWorld.EntityList)
                            {
                                if (en == parent)
                                    continue;
                                else if (grenadeHitTest(en))
                                {
                                    state = GrenadeState.Explosion;
                                    active_timer = 0.0f;
                                    dimensions = new Vector2(96, 96);
                                    position = position - (dimensions / 2);
                                    break;
                                }
                            }
                            position += velocity;
                        }
                        break;
                        case GrenadeState.Explosion:
                        explosion_timer += currentTime.ElapsedGameTime.Milliseconds;

                        foreach (Entity en in parentWorld.EntityList)
                        {
                            if (en == parent)
                                continue;
                            else if (grenadeHitTest(en))
                            {
                                Vector2 direction = en.CenterPoint - CenterPoint;
                                en.knockBack(direction, 3.0f, 10);
                            }
                        }

                        if (explosion_timer > max_explosion_timer)
                        {
                            active_timer = 0.0f;
                            state = GrenadeState.Reset;
                        }
                        break;
                        case GrenadeState.Reset:
                        dimensions = new Vector2(16, 16);
                        position = new Vector2(0, 0);
                        active = false;
                        break;
                    }
                }
            }

            public bool grenadeHitTest(Entity other)
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
