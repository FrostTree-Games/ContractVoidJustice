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
    class GuardSquadLeader : Enemy
    {
        private enum SquadLeaderState
        {
            Patrol,
            Direct,
            Flee
        }

        
        private Vector2 follow_point_1 = Vector2.Zero;
        private Vector2 follow_point_2 = Vector2.Zero;
        
        private GuardSquadSoldiers[] squad_mates = new GuardSquadSoldiers[2];
        private SquadLeaderState state = SquadLeaderState.Patrol;
        private EnemyComponents component = new MoveSearch();

        public GuardSquadLeader(LevelState parentWorld, float initial_x, float initial_y)
        {
            position = new Vector2(initial_x, initial_y);
            dimensions = new Vector2(48.0f, 48.0f);
            velocity = new Vector2(1.0f, 0f);

            component = new MoveSearch();
            this.parentWorld = parentWorld;

            populateSquadMates();

            direction_facing = GlobalGameConstants.Direction.Right;
            state = SquadLeaderState.Patrol;

            follow_point_1 = new Vector2((float)(CenterPoint.X + 64 * Math.Cos(-3 * Math.PI / 4)), (float)(CenterPoint.Y + 64 * Math.Sin(-3 * Math.PI / 4)));
            if (squad_mates[0] != null)
            {
                squad_mates[0].Follow_Point = follow_point_1;
            }
            if (squad_mates[1] != null)
            {
                squad_mates[1].Follow_Point = follow_point_2;
            }

            enemy_life = 30;
            enemy_damage = 5;
            disable_movement = false;
            disable_movement_time = 0.0f;
            player_found = false;
            change_direction_time = 0.0f;
            range_distance = 300.0f;
            change_direction_time_threshold = 5000.0f;
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
                switch (state)
                {
                    case SquadLeaderState.Patrol:
                        change_direction_time += currentTime.ElapsedGameTime.Milliseconds;
                        foreach (Entity en in parentWorld.EntityList)
                        {
                            if (en == this)
                                continue;
                            else if (en is Player)
                            {
                                component.update(this, en, currentTime, parentWorld);
                            }
                        }

                        switch(direction_facing)
                        {
                            case GlobalGameConstants.Direction.Right:
                                follow_point_1 = new Vector2((float)(CenterPoint.X + 64 * Math.Cos(-3 * Math.PI / 4)), (float)(CenterPoint.Y + 64 * Math.Sin(-3 * Math.PI / 4)));
                                follow_point_2 = new Vector2((float)(CenterPoint.X + 64 * Math.Cos(3 * Math.PI / 4)), (float)(CenterPoint.Y + 64 * Math.Sin(3 * Math.PI / 4)));
                                break;
                            case GlobalGameConstants.Direction.Left:
                                follow_point_1 = new Vector2((float)(CenterPoint.X + 64 * Math.Cos(Math.PI / 4)), (float)(CenterPoint.Y + 64 * Math.Sin(Math.PI / 4)));
                                follow_point_2 = new Vector2((float)(CenterPoint.X + 64 * Math.Cos(-1 * Math.PI / 4)), (float)(CenterPoint.Y + 64 * Math.Sin(-1 * Math.PI / 4)));
                                break;
                            case GlobalGameConstants.Direction.Up:
                                follow_point_1 = new Vector2((float)(CenterPoint.X + 64 * Math.Cos(3 * Math.PI / 4)), (float)(CenterPoint.Y + 64 * Math.Sin(3 * Math.PI / 4)));
                                follow_point_2 = new Vector2((float)(CenterPoint.X + 64 * Math.Cos(Math.PI / 4)), (float)(CenterPoint.Y + 64 * Math.Sin(Math.PI / 4))); 
                                break;
                            default:
                                follow_point_1 = new Vector2((float)(CenterPoint.X + 64 * Math.Cos(-1 * Math.PI / 4)), (float)(CenterPoint.Y + 64 * Math.Sin(-1 * Math.PI / 4)));
                                follow_point_2 = new Vector2((float)(CenterPoint.X + 64 * Math.Cos(-3 * Math.PI / 4)), (float)(CenterPoint.Y + 64 * Math.Sin(-3 * Math.PI / 4)));
                                break;
                        }
                        if (squad_mates[0] != null)
                        {
                            squad_mates[0].Follow_Point = follow_point_1;
                        }
                        if (squad_mates[1] != null)
                        {
                            squad_mates[1].Follow_Point = follow_point_2;
                        }

                        if (player_found)
                        {
                            state = SquadLeaderState.Direct;

                            switch (direction_facing)
                            {
                                case GlobalGameConstants.Direction.Right:
                                    follow_point_1.X = 128+follow_point_1.X;
                                    follow_point_2.X = 128+follow_point_2.X;
                                    break;
                                case GlobalGameConstants.Direction.Left:
                                    follow_point_1.X = follow_point_1.X - 128;
                                    follow_point_2.X = follow_point_2.X - 128;
                                    break;
                                case GlobalGameConstants.Direction.Up:
                                    follow_point_1.Y = follow_point_1.Y -128;
                                    follow_point_2.Y = follow_point_2.Y -128;
                                    break;
                                default:
                                    follow_point_1.Y = follow_point_1.Y + 128;
                                    follow_point_2.Y = follow_point_2.Y + 128;
                                    break;
                            }

                            squad_mates[0].Player_Found = true;
                            squad_mates[1].Player_Found = true;

                            velocity = Vector2.Zero;
                        }
                        break;
                    case SquadLeaderState.Direct:
                        if (squad_mates[0] != null)
                        {
                            squad_mates[0].Follow_Point = follow_point_1;
                        }
                        if (squad_mates[1] != null)
                        {
                            squad_mates[1].Follow_Point = follow_point_2;
                        }
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
            sb.Draw(Game1.whitePixel, position, null, Color.Blue, 0.0f, Vector2.Zero, new Vector2(48, 48), SpriteEffects.None, 1.0f);
            sb.Draw(Game1.whitePixel, follow_point_1, null, Color.Orange, 0.0f, Vector2.Zero, new Vector2(16, 16), SpriteEffects.None, 1.0f);
            sb.Draw(Game1.whitePixel, follow_point_2, null, Color.Green, 0.0f, Vector2.Zero, new Vector2(16, 16), SpriteEffects.None, 1.0f);
        }

        public override void knockBack(Vector2 direction, float magnitude, int damage)
        {
        }

        public void populateSquadMates()
        {
            GuardSquadSoldiers en = new GuardSquadSoldiers(parentWorld, position.X, position.Y);
            parentWorld.EntityList.Add(en);
            squad_mates[0] = en;
            squad_mates[0].Leader = this;

            en = new GuardSquadSoldiers(parentWorld, position.X, position.Y);
            parentWorld.EntityList.Add(en);
            squad_mates[1] = en;
            squad_mates[1].Leader = this;
        }
    }
}
