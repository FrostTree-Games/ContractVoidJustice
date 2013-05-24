﻿using System;
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
            Item2,
            Item_Pickup
        }

        public struct PlayerItems
        {
            public GlobalGameConstants.itemType item1;
            public GlobalGameConstants.itemType item2;
        }

        private Item player_item_1 = null;
        private Item player_item_2 = null;
        public PlayerItems CurrentItemTypes
        {
            get
            {
                PlayerItems p = new PlayerItems();
                p.item1 = player_item_1.ItemType();
                p.item2 = player_item_2.ItemType();
                return p;
            }
        }

        private AnimationLib.SpineAnimationSet walk_down = null;
        private AnimationLib.SpineAnimationSet walk_right = null;
        private AnimationLib.SpineAnimationSet walk_up = null;
        private AnimationLib.SpineAnimationSet current_skeleton = null;
        public AnimationLib.SpineAnimationSet LoadAnimation { set { current_skeleton = value; } get { return current_skeleton; } }

        private float animation_time = 0.0f;
        public float Animation_Time { set { animation_time = value; } get { return animation_time; } }
        private float switch_weapon_interval = 0.0f;

        private playerState state = playerState.Moving;
        public playerState State
        {
            set { state = value; }
            get { return state;  }

        }
        
        public Player(LevelState parentWorld, float initial_x, float initial_y)
        {
            position = new Vector2(initial_x, initial_y);

            dimensions = new Vector2(32.0f, 58.5f);
            
            velocity = Vector2.Zero;

            player_item_1 = new Bomb(position);
            player_item_2 = new DungeonMap();
            GlobalGameConstants.Player_Item_1 = player_item_1.getEnumType();
            GlobalGameConstants.Player_Item_2 = player_item_2.getEnumType();
            switch_weapon_interval = 0.0f;

            state = playerState.Moving;

            disable_movement = false;

            direction_facing = GlobalGameConstants.Direction.Right;

            this.parentWorld = parentWorld;

            remove_from_list = false;

            walk_down = AnimationLib.getSkeleton("jensenDown");
            walk_right = AnimationLib.getSkeleton("jensenRight");
            walk_up = AnimationLib.getSkeleton("jensenUp");
            current_skeleton = walk_right;
            current_skeleton.Animation = current_skeleton.Skeleton.Data.FindAnimation("run");
        }

        public override void update(GameTime currentTime)
        {
            double delta = currentTime.ElapsedGameTime.Milliseconds;

            //update the world map if you've visited a new room
            int currentNodeX = (int)((CenterPoint.X / GlobalGameConstants.TileSize.X) / GlobalGameConstants.TilesPerRoomWide);
            int currentNodeY = (int)((CenterPoint.Y / GlobalGameConstants.TileSize.Y) / GlobalGameConstants.TilesPerRoomHigh);
            if (currentNodeX >= 0 && currentNodeX < parentWorld.NodeMap.GetLength(0) && currentNodeY >= 0 && currentNodeY < parentWorld.NodeMap.GetLength(1))
            {
                parentWorld.NodeMap[currentNodeX, currentNodeY].visited = true;
            }

            if (state == playerState.Item1)
            {
                if (player_item_1 == null)
                {
                    state = playerState.Moving;
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
                }
                else
                {
                    player_item_2.update(this, currentTime, parentWorld);
                }
            }
            else if (state == playerState.Moving)
            {
                switch_weapon_interval += currentTime.ElapsedGameTime.Milliseconds;
                //knocked back
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
                else
                {
                    if (InputDeviceManager.isButtonDown(InputDeviceManager.PlayerButton.UseItem1))
                    {
                        state = playerState.Item1;

                    }
                    if (InputDeviceManager.isButtonDown(InputDeviceManager.PlayerButton.UseItem2))
                    {
                        state = playerState.Item2;
                    }

                    if (disable_movement == false)
                    {
                        if (InputDeviceManager.isButtonDown(InputDeviceManager.PlayerButton.RightDirection))
                        {
                            velocity.X = 1.5f;
                            direction_facing = GlobalGameConstants.Direction.Right;
                            current_skeleton = walk_right ;
                            current_skeleton.Skeleton.FlipX = false;
                        }
                        else if (InputDeviceManager.isButtonDown(InputDeviceManager.PlayerButton.LeftDirection))
                        {
                            velocity.X = -1.5f;
                            direction_facing = GlobalGameConstants.Direction.Left;
                            current_skeleton = walk_right;
                            current_skeleton.Skeleton.FlipX = true;
                        }
                        else
                        {
                            velocity.X = 0.0f;
                        }

                        if (InputDeviceManager.isButtonDown(InputDeviceManager.PlayerButton.UpDirection))
                        {
                            velocity.Y = -1.5f;
                            direction_facing = GlobalGameConstants.Direction.Up;
                            current_skeleton = walk_up;
                            current_skeleton.Skeleton.FlipX = false;
                        }
                        else if (InputDeviceManager.isButtonDown(InputDeviceManager.PlayerButton.DownDirection))
                        {
                            velocity.Y = 1.5f;
                            direction_facing = GlobalGameConstants.Direction.Down;
                            current_skeleton = walk_down;
                            current_skeleton.Skeleton.FlipX = false;
                        }
                        else
                        {
                            velocity.Y = 0.0f;
                        }

                        //if player stands still then animation returns to idle
                        if (velocity.X == 0.0f && velocity.Y == 0.0f)
                        {
                            current_skeleton.Animation = current_skeleton.Skeleton.Data.FindAnimation("idle");
                        }
                        else
                        {
                            current_skeleton.Animation = current_skeleton.Skeleton.Data.FindAnimation("run");
                        }
                    }

                    
                }
                if (player_item_1 != null)
                {
                    player_item_1.daemonupdate(this, currentTime, parentWorld);
                }
                if (player_item_2 != null)
                {
                    player_item_2.daemonupdate(this, currentTime, parentWorld);
                }


                //Check to see if player has encountered a pickup item
                foreach (Entity en in parentWorld.EntityList)
                {
                    if (en == this)
                        continue;

                    if (en is Pickup)
                    {
                        if (hitTest(en))
                        {
                            if (switch_weapon_interval > 100)
                            {
                                if (InputDeviceManager.isButtonDown(InputDeviceManager.PlayerButton.SwitchItem1))
                                {
                                    player_item_1 = ((Pickup)en).assignItem(player_item_1, currentTime);
                                    GlobalGameConstants.Player_Item_1 = player_item_1.getEnumType();
                                }
                                else if (InputDeviceManager.isButtonDown(InputDeviceManager.PlayerButton.SwitchItem2))
                                {
                                    player_item_2 = ((Pickup)en).assignItem(player_item_2, currentTime);
                                    GlobalGameConstants.Player_Item_2 = player_item_2.getEnumType();
                                }
                                switch_weapon_interval = 0.0f;
                            }
                        }
                    }
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

        public override void knockBack(Entity other, int damage)
        {
            
            if (disable_movement_time == 0.0)
            {
                disable_movement = true;
                float direction_x = CenterPoint.X - other.CenterPoint.X;
                float direction_y = CenterPoint.Y - other.CenterPoint.Y;

                if (Math.Abs(direction_x) > (Math.Abs(direction_y)))
                {
                    if (direction_x < 0)
                    {
                        velocity = new Vector2(-5.51f, direction_y / 100);
                    }
                    else
                    {
                        velocity = new Vector2(5.51f, direction_y / 100);
                    }
                }
                else
                {
                    if (direction_y < 0)
                    {
                        velocity = new Vector2(direction_x / 100f, -5.51f);
                    }
                    else
                    {
                        velocity = new Vector2(direction_x / 100f, 5.51f);
                    }
                }
                GlobalGameConstants.Player_Health = GlobalGameConstants.Player_Health - damage;
            }
        }

        public void spinerender(SkeletonRenderer renderer)
        {
            current_skeleton.Skeleton.RootBone.X = CenterPoint.X * (current_skeleton.Skeleton.FlipX ? -1 : 1);
            current_skeleton.Skeleton.RootBone.Y = CenterPoint.Y+(dimensions.Y/2f);

            current_skeleton.Skeleton.RootBone.ScaleX = 0.1f;
            current_skeleton.Skeleton.RootBone.ScaleY = 0.1f;

            current_skeleton.Skeleton.UpdateWorldTransform();
            renderer.Draw(current_skeleton.Skeleton);
        }
    }
}
