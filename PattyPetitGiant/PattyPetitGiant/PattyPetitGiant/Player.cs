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
    class Player : Entity, SpineEntity
    {
        public enum playerState
        {
            Moving,
            Item1,
            Item2
        }

        private Item player_item_1 = null;
        private Item player_item_2 = null;

        private AnimationLib.SpineAnimationSet walk_down = null;
        private AnimationLib.SpineAnimationSet walk_right = null;
        private AnimationLib.SpineAnimationSet walk_up = null;
        private AnimationLib.SpineAnimationSet walk_left = null;
        private AnimationLib.SpineAnimationSet current_skeleton = null;
        private float animation_time = 0.0f;

        private playerState state = playerState.Moving;
        public playerState State
        {
            set { state = value; }
            get { return state;  }

        }
        
        public Player(LevelState parentWorld, float initial_x, float initial_y)
        {
            position.X = initial_x;
            position.Y = initial_y;

            dimensions.X = 32.0f;
            dimensions.Y = 78.0f;

            player_item_1 = new Sword(position);
            player_item_2 = new Gun(position);

            state = playerState.Moving;

            disable_movement = false;

            direction_facing = GlobalGameConstants.Direction.Right;

            this.parentWorld = parentWorld;

            remove_from_list = false;

            walk_down = AnimationLib.getSkeleton("jensenDown");
            walk_right = AnimationLib.getSkeleton("jensenRight");
            walk_up = AnimationLib.getSkeleton("jensenUp");
            current_skeleton = walk_down;
        }

        public override void update(GameTime currentTime)
        {
            double delta = currentTime.ElapsedGameTime.Milliseconds;
            KeyboardState ks = Keyboard.GetState();

            if (state == playerState.Item1)
            {
                //itemType = player_item_1.itemCheck;
                if (player_item_1 == null)
                {
                    state = playerState.Moving;
                    disable_movement = false;
                }
                else
                {
                    player_item_1.update(this, currentTime, parentWorld);
                }
                
            }
            else if (state == playerState.Item2 )
            {
                if (player_item_2 == null)
                {
                    state = playerState.Moving;
                    disable_movement = false;
                }
                else
                {
                    player_item_2.update(this, currentTime, parentWorld);
                }
            }
            else if (state == playerState.Moving)
            {
                if (ks.IsKeyDown(Keys.A))
                {
                    state = playerState.Item1;
                    velocity = Vector2.Zero;
                    disable_movement = true;
                }
                if (ks.IsKeyDown(Keys.S))
                {
                    state = playerState.Item2;
                    disable_movement = true;
                }

                if (disable_movement == false)
                {
                    if (ks.IsKeyDown(Keys.Right))
                    {
                        velocity.X = 1.5f;
                        direction_facing = GlobalGameConstants.Direction.Right;
                        current_skeleton = AnimationLib.getSkeleton("jensenRight");
                        current_skeleton.Skeleton.FlipX = false;
                    }
                    else if (ks.IsKeyDown(Keys.Left))
                    {
                        velocity.X = -1.5f;
                        direction_facing = GlobalGameConstants.Direction.Left;
                        current_skeleton = AnimationLib.getSkeleton("jensenRight");
                        current_skeleton.Skeleton.FlipX = true;
                    }
                    else
                    {
                        velocity.X = 0.0f;
                    }

                    if (ks.IsKeyDown(Keys.Up))
                    {
                        velocity.Y = -1.5f;
                        direction_facing = GlobalGameConstants.Direction.Up;
                        current_skeleton = AnimationLib.getSkeleton("jensenUp");
                        current_skeleton.Skeleton.FlipX = false;
                    }
                    else if (ks.IsKeyDown(Keys.Down))
                    {
                        velocity.Y = 1.5f;
                        direction_facing = GlobalGameConstants.Direction.Down;
                        current_skeleton = AnimationLib.getSkeleton("jensenDown");
                        current_skeleton.Skeleton.FlipX = false;
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

                if (player_item_1 != null)
                {
                    player_item_1.daemonupdate(currentTime, parentWorld);
                }
                if (player_item_2 != null)
                {
                    player_item_2.daemonupdate(currentTime, parentWorld);
                }


                //Check to see if player has encountered a pickup item
                foreach (Entity en in parentWorld.EntityList)
                {

                }

            }

            Vector2 pos = new Vector2(position.X, position.Y);
            Vector2 nextStep = new Vector2(position.X + velocity.X, position.Y + velocity.Y);

            Vector2 finalPos = parentWorld.Map.reloactePosition(pos, nextStep, dimensions);
            position.X = finalPos.X;
            position.Y = finalPos.Y;

            animation_time += currentTime.ElapsedGameTime.Milliseconds / 1000f;
            current_skeleton.Animation.Apply(current_skeleton.Skeleton, animation_time, true);
        }

        public override void draw(SpriteBatch sb)
        {
            //sb.Draw(Game1.whitePixel, position, null, Color.White, 0.0f, Vector2.Zero, dimensions, SpriteEffects.None, 0.5f);
            if (player_item_1 != null)
            {
                player_item_1.draw(sb);
            }
            if (player_item_2 != null)
            {
                player_item_2.draw(sb);
            }
        }

        public void spinerender(SkeletonRenderer renderer)
        {
            current_skeleton.Skeleton.RootBone.X = CenterPoint.X * (current_skeleton.Skeleton.FlipX ? -1 : 1);
            current_skeleton.Skeleton.RootBone.Y = CenterPoint.Y + (dimensions.Y / 2);

            current_skeleton.Skeleton.RootBone.ScaleX = 0.1f;
            current_skeleton.Skeleton.RootBone.ScaleY = 0.1f;

            current_skeleton.Skeleton.UpdateWorldTransform();
            renderer.Draw(current_skeleton.Skeleton);
        }
    }
}
