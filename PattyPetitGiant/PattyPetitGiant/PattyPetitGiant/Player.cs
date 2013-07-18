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

        private bool item1_switch_button_down = false;
        private bool item2_switch_button_down = false;

        private const float playerMoveSpeed = 3.0f;

        private bool loopAnimation = false;
        public bool LoopAnimation { get { return loopAnimation; } set { loopAnimation = value; } }

        private playerState state = playerState.Moving;
        public playerState State
        {
            set { state = value; }
            get { return state;  }

        }

        private Item getItemWhenLoading(GlobalGameConstants.itemType type)
        {
            switch (type)
            {
                case GlobalGameConstants.itemType.Sword:
                    return new Sword();
                case GlobalGameConstants.itemType.Bomb:
                    return new Bomb();
                case GlobalGameConstants.itemType.BushidoBlade:
                    return new BushidoBlade(Vector2.Zero);
                case GlobalGameConstants.itemType.Gun:
                    return new Gun();
                case GlobalGameConstants.itemType.Compass:
                    return new Compass();
                case GlobalGameConstants.itemType.DungeonMap:
                    return new DungeonMap();
                case GlobalGameConstants.itemType.WandOfGyges:
                    return new WandOfGyges();
                case GlobalGameConstants.itemType.ShotGun:
                    return new ShotGun();
                case GlobalGameConstants.itemType.WaveMotionGun:
                    return new WaveMotionGun();
                case GlobalGameConstants.itemType.HermesSandals:
                    return new HermesSandals();
                case GlobalGameConstants.itemType.RocketLauncher:
                    return new RocketLauncher();
                case GlobalGameConstants.itemType.FlameThrower:
                    return new FlameThrower();
                case GlobalGameConstants.itemType.LazerGun:
                    return new LazerGun();
                case GlobalGameConstants.itemType.MachineGun:
                    return new MachineGun();
                default:
                    throw new Exception("Pickup item type ambiguous");
            }
        }
        
        public Player(LevelState parentWorld, float initial_x, float initial_y)
        {
            position = new Vector2(initial_x, initial_y);

            dimensions = new Vector2(32.0f, 58.5f);
            
            velocity = Vector2.Zero;

            player_item_1 = getItemWhenLoading(GameCampaign.Player_Item_1);
            player_item_2 = getItemWhenLoading(GameCampaign.Player_Item_2);

            state = playerState.Moving;

            disable_movement = false;

            direction_facing = GlobalGameConstants.Direction.Down;

            this.parentWorld = parentWorld;

            remove_from_list = false;
            walk_down = AnimationLib.getSkeleton("jensenDown");
            walk_right = AnimationLib.getSkeleton("jensenRight");
            walk_up = AnimationLib.getSkeleton("jensenUp");
            current_skeleton = walk_right;
            current_skeleton.Animation = current_skeleton.Skeleton.Data.FindAnimation("run");

            enemy_type = EnemyType.Player;
        }

        public override void update(GameTime currentTime)
        {
            double delta = currentTime.ElapsedGameTime.Milliseconds;

            if (GameCampaign.Player_Health <= 0.0f)
            {
                if (!parentWorld.Player1Dead)
                {
                    parentWorld.Particles.pushBloodParticle(CenterPoint);
                    parentWorld.Particles.pushBloodParticle(CenterPoint);
                    parentWorld.Particles.pushBloodParticle(CenterPoint);
                    parentWorld.Particles.pushBloodParticle(CenterPoint);
                    parentWorld.Particles.pushBloodParticle(CenterPoint);
                    parentWorld.Particles.pushBloodParticle(CenterPoint);

                    animation_time = 0;
                    switch (Game1.rand.Next() % 3)
                    {
                        case 0:
                            current_skeleton.Animation = current_skeleton.Skeleton.Data.FindAnimation("die");
                            break;
                        case 1:
                            current_skeleton.Animation = current_skeleton.Skeleton.Data.FindAnimation("die2");
                            break;
                        default:
                            current_skeleton.Animation = current_skeleton.Skeleton.Data.FindAnimation("die3");
                            break;
                    }
                }

                animation_time += currentTime.ElapsedGameTime.Milliseconds / 1000f;
                current_skeleton.Animation.Apply(current_skeleton.Skeleton, animation_time, false);

                death = true;
                parentWorld.Player1Dead = true;
                velocity = Vector2.Zero;
                return;
            }

            //update the world map if you've visited a new room
            int currentNodeX = (int)((CenterPoint.X / GlobalGameConstants.TileSize.X) / GlobalGameConstants.TilesPerRoomWide);
            int currentNodeY = (int)((CenterPoint.Y / GlobalGameConstants.TileSize.Y) / GlobalGameConstants.TilesPerRoomHigh);
            if (currentNodeX >= 0 && currentNodeX < parentWorld.NodeMap.GetLength(0) && currentNodeY >= 0 && currentNodeY < parentWorld.NodeMap.GetLength(1))
            {
                parentWorld.NodeMap[currentNodeX, currentNodeY].visited = true;
            }
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

                if (player_item_1 != null)
                {
                    player_item_1.daemonupdate(this, currentTime, parentWorld);
                }
                if (player_item_2 != null)
                {
                    player_item_2.daemonupdate(this, currentTime, parentWorld);
                }
            }
            else
            {
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


                    if (player_item_2 != null)
                    {
                        player_item_2.daemonupdate(this, currentTime, parentWorld);
                    }

                }
                else if (state == playerState.Item2)
                {
                    if (player_item_2 == null)
                    {
                        state = playerState.Moving;
                    }
                    else
                    {
                        player_item_2.update(this, currentTime, parentWorld);
                    }

                    if (player_item_1 != null)
                    {
                        player_item_1.daemonupdate(this, currentTime, parentWorld);
                    }
                }
                else if (state == playerState.Moving)
                {
                    loopAnimation = true;

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
                            velocity.X = playerMoveSpeed;
                            direction_facing = GlobalGameConstants.Direction.Right;
                        }
                        else if (InputDeviceManager.isButtonDown(InputDeviceManager.PlayerButton.LeftDirection))
                        {
                            velocity.X = -playerMoveSpeed;
                            direction_facing = GlobalGameConstants.Direction.Left;
                        }
                        else
                        {
                            velocity.X = 0.0f;
                        }

                        if (InputDeviceManager.isButtonDown(InputDeviceManager.PlayerButton.UpDirection))
                        {
                            velocity.Y = -playerMoveSpeed;
                            direction_facing = GlobalGameConstants.Direction.Up;
                        }
                        else if (InputDeviceManager.isButtonDown(InputDeviceManager.PlayerButton.DownDirection))
                        {
                            velocity.Y = playerMoveSpeed;
                            direction_facing = GlobalGameConstants.Direction.Down;
                        }
                        else
                        {
                            velocity.Y = 0.0f;
                        }

                        GlobalGameConstants.Direction analogDirection = InputDeviceManager.AnalogStickDirection();
                        direction_facing = (analogDirection != GlobalGameConstants.Direction.NoDirection) ? analogDirection : direction_facing;

                        switch (direction_facing)
                        {
                            case GlobalGameConstants.Direction.Down:
                                current_skeleton = walk_down;
                                current_skeleton.Skeleton.FlipX = false;
                                break;
                            case GlobalGameConstants.Direction.Up:
                                current_skeleton = walk_up;
                                current_skeleton.Skeleton.FlipX = false;
                                break;
                            case GlobalGameConstants.Direction.Left:
                                current_skeleton = walk_right;
                                current_skeleton.Skeleton.FlipX = true;
                                break;
                            case GlobalGameConstants.Direction.Right:
                                current_skeleton = walk_right;
                                current_skeleton.Skeleton.FlipX = false;
                                break;
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

                    bool itemTouched = false;

                    //Check to see if player has encountered a pickup item
                    for (int i = 0; i < parentWorld.EntityList.Count; i++)
                    {
                        if (parentWorld.EntityList[i] == this)
                            continue;

                        if (parentWorld.EntityList[i] is Pickup)
                        {
                            if (hitTest(parentWorld.EntityList[i]))
                            {
                                itemTouched = true;

                                if (InputDeviceManager.isButtonDown(InputDeviceManager.PlayerButton.SwitchItem1) && !item1_switch_button_down)
                                {
                                    item1_switch_button_down = true;
                                }
                                else if (!InputDeviceManager.isButtonDown(InputDeviceManager.PlayerButton.SwitchItem1) && item1_switch_button_down)
                                {
                                    item1_switch_button_down = false;

                                    player_item_1 = ((Pickup)parentWorld.EntityList[i]).assignItem(player_item_1, currentTime);
                                    GameCampaign.Player_Item_1 = player_item_1.ItemType();
                                }

                                if (InputDeviceManager.isButtonDown(InputDeviceManager.PlayerButton.SwitchItem2) && !item2_switch_button_down)
                                {
                                    item2_switch_button_down = true;
                                }
                                else if (!InputDeviceManager.isButtonDown(InputDeviceManager.PlayerButton.SwitchItem2) && item2_switch_button_down)
                                {
                                    item2_switch_button_down = false;

                                    player_item_2 = ((Pickup)parentWorld.EntityList[i]).assignItem(player_item_2, currentTime);
                                    GameCampaign.Player_Item_2 = player_item_2.ItemType();
                                }
                            }
                        }
                    }


                    if (!itemTouched && (item1_switch_button_down || item2_switch_button_down))
                    {
                        item1_switch_button_down = false;
                        item2_switch_button_down = false;
                    }

                    if (player_item_1 != null)
                    {
                        player_item_1.daemonupdate(this, currentTime, parentWorld);
                    }
                    if (player_item_2 != null)
                    {
                        player_item_2.daemonupdate(this, currentTime, parentWorld);
                    }
                }
            }

            Vector2 pos = new Vector2(position.X, position.Y);
            Vector2 nextStep = new Vector2(position.X + velocity.X, position.Y + velocity.Y);

            Vector2 finalPos = parentWorld.Map.reloactePosition(pos, nextStep, dimensions);
            position = finalPos;

            animation_time += currentTime.ElapsedGameTime.Milliseconds / 1000f;
            current_skeleton.Animation.Apply(current_skeleton.Skeleton, animation_time, loopAnimation);
        }

        public override void draw(Spine.SkeletonRenderer sb)
        {
            if (player_item_1 != null)
            {
                player_item_1.draw(sb);
            }
            if (player_item_2 != null)
            {
                player_item_2.draw(sb);
            }
        }

        public override void knockBack(Vector2 direction, float magnitude, int damage, Entity attacker)
        {
            if (state == playerState.Item1 || state == playerState.Item2)
            {
                state = playerState.Moving;
            }

            if (disable_movement_time == 0.0 && disable_movement == false)
            {
                disable_movement = true;

                parentWorld.Particles.pushBloodParticle(CenterPoint);
                parentWorld.Particles.pushBloodParticle(CenterPoint);
                parentWorld.Particles.pushBloodParticle(CenterPoint);
                
                if (Math.Abs(direction.X) > (Math.Abs(direction.Y)))
                {
                    if (direction.X < 0)
                    {
                        velocity = new Vector2(-1.51f * magnitude, direction.Y / 100 * magnitude);
                    }
                    else
                    {
                        velocity = new Vector2(1.51f * magnitude, direction.Y / 100 * magnitude);
                    }
                }
                else
                {
                    if (direction.Y < 0)
                    {
                        velocity = new Vector2(direction.Y / 100f * magnitude, -1.51f * magnitude);
                    }
                    else
                    {
                        velocity = new Vector2(direction.Y / 100f * magnitude, 1.51f * magnitude);
                    }
                }
                GameCampaign.Player_Health = GameCampaign.Player_Health - damage;
            }
        }

        private void setAnimationWeapons()
        {
            switch (direction_facing == GlobalGameConstants.Direction.Left ? player_item_1.ItemType() : player_item_2.ItemType())
            {
                case GlobalGameConstants.itemType.Sword:
                    current_skeleton.Skeleton.SetAttachment("lWeapon", "lSword");
                    break;
                case GlobalGameConstants.itemType.Bomb:
                    current_skeleton.Skeleton.SetAttachment("lWeapon", "lBomb");
                    break;
                case GlobalGameConstants.itemType.Gun:
                    current_skeleton.Skeleton.SetAttachment("lWeapon", "lPistol");
                    break;
                case GlobalGameConstants.itemType.MachineGun:
                    current_skeleton.Skeleton.SetAttachment("lWeapon", "lMGun");
                    break;
                case GlobalGameConstants.itemType.DungeonMap:
                    current_skeleton.Skeleton.SetAttachment("lWeapon", "lMap");
                    break;
                case GlobalGameConstants.itemType.WaveMotionGun:
                    current_skeleton.Skeleton.SetAttachment("lWeapon", "lRayGun");
                    break;
                case GlobalGameConstants.itemType.ShotGun:
                    current_skeleton.Skeleton.SetAttachment("lWeapon", "lShotgun");
                    break;
                case GlobalGameConstants.itemType.Compass:
                    current_skeleton.Skeleton.SetAttachment("lWeapon", "lCompass");
                    break;
                case GlobalGameConstants.itemType.BushidoBlade:
                    current_skeleton.Skeleton.SetAttachment("lWeapon", "lBushidoBlade");
                    break;
                case GlobalGameConstants.itemType.WandOfGyges:
                    current_skeleton.Skeleton.SetAttachment("lWeapon", "lWand");
                    break;
                case GlobalGameConstants.itemType.HermesSandals:
                    current_skeleton.Skeleton.SetAttachment("lWeapon", "lHermes");
                    break;
                case GlobalGameConstants.itemType.RocketLauncher:
                    current_skeleton.Skeleton.SetAttachment("lWeapon", "lRocket");
                    break;
                case GlobalGameConstants.itemType.FlameThrower:
                    current_skeleton.Skeleton.SetAttachment("lWeapon", "lFlamethrower");
                    break;
                case GlobalGameConstants.itemType.LazerGun:
                    current_skeleton.Skeleton.SetAttachment("lWeapon", "lLaser");
                    break;
                default:
                    current_skeleton.Skeleton.SetAttachment("lWeapon", "lEmpty");
                    break;
            }

            switch (direction_facing == GlobalGameConstants.Direction.Left ? player_item_2.ItemType() : player_item_1.ItemType())
            {
                case GlobalGameConstants.itemType.Sword:
                    current_skeleton.Skeleton.SetAttachment("rWeapon", "rSword");
                    break;
                case GlobalGameConstants.itemType.Bomb:
                    current_skeleton.Skeleton.SetAttachment("rWeapon", "rBomb");
                    break;
                case GlobalGameConstants.itemType.Gun:
                    current_skeleton.Skeleton.SetAttachment("rWeapon", "rPistol");
                    break;
                case GlobalGameConstants.itemType.MachineGun:
                    current_skeleton.Skeleton.SetAttachment("rWeapon", "rMGun");
                    break;
                case GlobalGameConstants.itemType.DungeonMap:
                    current_skeleton.Skeleton.SetAttachment("rWeapon", "rMap");
                    break;
                case GlobalGameConstants.itemType.WaveMotionGun:
                    current_skeleton.Skeleton.SetAttachment("rWeapon", "rRayGun");
                    break;
                case GlobalGameConstants.itemType.ShotGun:
                    current_skeleton.Skeleton.SetAttachment("rWeapon", "rShotgun");
                    break;
                case GlobalGameConstants.itemType.Compass:
                    current_skeleton.Skeleton.SetAttachment("rWeapon", "rCompass");
                    break;
                case GlobalGameConstants.itemType.BushidoBlade:
                    current_skeleton.Skeleton.SetAttachment("rWeapon", "rBushidoBlade");
                    break;
                case GlobalGameConstants.itemType.WandOfGyges:
                    current_skeleton.Skeleton.SetAttachment("rWeapon", "rWand");
                    break;
                case GlobalGameConstants.itemType.HermesSandals:
                    current_skeleton.Skeleton.SetAttachment("rWeapon", "rHermes");
                    break;
                case GlobalGameConstants.itemType.RocketLauncher:
                    current_skeleton.Skeleton.SetAttachment("rWeapon", "rRocket");
                    break;
                case GlobalGameConstants.itemType.FlameThrower:
                    current_skeleton.Skeleton.SetAttachment("rWeapon", "rFlamethrower");
                    break;
                case GlobalGameConstants.itemType.LazerGun:
                    current_skeleton.Skeleton.SetAttachment("rWeapon", "rLaser");
                    break;
                default:
                    current_skeleton.Skeleton.SetAttachment("rWeapon", "rEmpty");
                    break;
            }
        }

        public void spinerender(SkeletonRenderer renderer)
        {
            setAnimationWeapons();

            current_skeleton.Skeleton.RootBone.X = CenterPoint.X * (current_skeleton.Skeleton.FlipX ? -1 : 1);
            current_skeleton.Skeleton.RootBone.Y = CenterPoint.Y+(dimensions.Y/2f);

            current_skeleton.Skeleton.UpdateWorldTransform();
            renderer.Draw(current_skeleton.Skeleton);
        }
    }
}
