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
            Item2,
            Item_Pickup
        }
        
        public struct PlayerItems
        {
            public GlobalGameConstants.itemType item1;
            public GlobalGameConstants.itemType item2;
        }

        private Item Player_Right_Item = null;
        private Item Player_Left_Item = null;
        public PlayerItems CurrentItemTypes
        {
            get
            {
                PlayerItems p = new PlayerItems();
                p.item1 = Player_Right_Item.ItemType();
                p.item2 = Player_Left_Item.ItemType();
                return p;
            }
        }

        private AnimationLib.SpineAnimationSet walk_down = null;
        private AnimationLib.SpineAnimationSet walk_right = null;
        private AnimationLib.SpineAnimationSet walk_left = null;
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

        private InputDevice2.PPG_Player index;
        public InputDevice2.PPG_Player Index { get { return index; } set { index = value; } }

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
        
        public Player(LevelState parentWorld, float initial_x, float initial_y, InputDevice2.PPG_Player index)
        {
            position = new Vector2(initial_x, initial_y);

            dimensions = new Vector2(32.0f, 58.5f);
            
            velocity = Vector2.Zero;

            if (index == InputDevice2.PPG_Player.Player_1)
            {
                Player_Right_Item = getItemWhenLoading(GameCampaign.Player_Right_Item);
                Player_Left_Item = getItemWhenLoading(GameCampaign.Player_Left_Item);
            }
            else if (index == InputDevice2.PPG_Player.Player_2)
            {
                Player_Right_Item = getItemWhenLoading(GameCampaign.Player2_Item_1);
                Player_Left_Item = getItemWhenLoading(GameCampaign.Player2_Item_2);
            }

            state = playerState.Moving;

            disable_movement = false;

            direction_facing = GlobalGameConstants.Direction.Down;

            this.parentWorld = parentWorld;

            if (index == InputDevice2.PPG_Player.Player_1)
            {
                if (GameCampaign.PlayerColor == 0)
                {
                    walk_down = AnimationLib.loadNewAnimationSet("jensenDown");
                    walk_right = AnimationLib.loadNewAnimationSet("jensenRight");
                    walk_left = AnimationLib.loadNewAnimationSet("jensenRight");
                    walk_up = AnimationLib.loadNewAnimationSet("jensenUp");
                }
                else if (GameCampaign.PlayerColor == 1)
                {
                    walk_down = AnimationLib.loadNewAnimationSet("jensenDown_RED");
                    walk_right = AnimationLib.loadNewAnimationSet("jensenRight_RED");
                    walk_left = AnimationLib.loadNewAnimationSet("jensenRight_RED");
                    walk_up = AnimationLib.loadNewAnimationSet("jensenUp_RED");
                }
                else if (GameCampaign.PlayerColor == 2)
                {
                    walk_down = AnimationLib.loadNewAnimationSet("jensenDown_PURPLE");
                    walk_right = AnimationLib.loadNewAnimationSet("jensenRight_PURPLE");
                    walk_left = AnimationLib.loadNewAnimationSet("jensenRight_PURPLE");
                    walk_up = AnimationLib.loadNewAnimationSet("jensenUp_PURPLE");
                }
                else if (GameCampaign.PlayerColor == 3)
                {
                    walk_down = AnimationLib.loadNewAnimationSet("jensenDown_BLUE");
                    walk_right = AnimationLib.loadNewAnimationSet("jensenRight_BLUE");
                    walk_left = AnimationLib.loadNewAnimationSet("jensenRight_BLUE");
                    walk_up = AnimationLib.loadNewAnimationSet("jensenUp_BLUE");
                }
                else if (GameCampaign.PlayerColor == 4)
                {
                    walk_down = AnimationLib.loadNewAnimationSet("jensenDown_CYAN");
                    walk_right = AnimationLib.loadNewAnimationSet("jensenRight_CYAN");
                    walk_left = AnimationLib.loadNewAnimationSet("jensenRight_CYAN");
                    walk_up = AnimationLib.loadNewAnimationSet("jensenUp_CYAN");
                }
                else if (GameCampaign.PlayerColor == 5)
                {
                    walk_down = AnimationLib.loadNewAnimationSet("jensenDown_BROWN");
                    walk_right = AnimationLib.loadNewAnimationSet("jensenRight_BROWN");
                    walk_left = AnimationLib.loadNewAnimationSet("jensenRight_BROWN");
                    walk_up = AnimationLib.loadNewAnimationSet("jensenUp_BROWN");
                }
            }
            else if (index == InputDevice2.PPG_Player.Player_2)
            {
                walk_down = AnimationLib.loadNewAnimationSet("femaleJensenDown");
                walk_right = AnimationLib.loadNewAnimationSet("femaleJensenRight");
                walk_left = AnimationLib.loadNewAnimationSet("femaleJensenRight");
                walk_up = AnimationLib.loadNewAnimationSet("femaleJensenUp");

                /*
                if (GameCampaign.Player2Color == 0)
                {
                    walk_down = AnimationLib.loadNewAnimationSet("jensenDown");
                    walk_right = AnimationLib.loadNewAnimationSet("jensenRight");
                    walk_left = AnimationLib.loadNewAnimationSet("jensenRight");
                    walk_up = AnimationLib.loadNewAnimationSet("jensenUp");
                }
                else if (GameCampaign.Player2Color == 1)
                {
                    walk_down = AnimationLib.loadNewAnimationSet("jensenDown_RED");
                    walk_right = AnimationLib.loadNewAnimationSet("jensenRight_RED");
                    walk_left = AnimationLib.loadNewAnimationSet("jensenRight_RED");
                    walk_up = AnimationLib.loadNewAnimationSet("jensenUp_RED");
                }
                else if (GameCampaign.Player2Color == 2)
                {
                    walk_down = AnimationLib.loadNewAnimationSet("jensenDown_PURPLE");
                    walk_right = AnimationLib.loadNewAnimationSet("jensenRight_PURPLE");
                    walk_left = AnimationLib.loadNewAnimationSet("jensenRight_PURPLE");
                    walk_up = AnimationLib.loadNewAnimationSet("jensenUp_PURPLE");
                }
                else if (GameCampaign.Player2Color == 3)
                {
                    walk_down = AnimationLib.loadNewAnimationSet("jensenDown_BLUE");
                    walk_right = AnimationLib.loadNewAnimationSet("jensenRight_BLUE");
                    walk_left = AnimationLib.loadNewAnimationSet("jensenRight_BLUE");
                    walk_up = AnimationLib.loadNewAnimationSet("jensenUp_BLUE");
                }
                else if (GameCampaign.Player2Color == 4)
                {
                    walk_down = AnimationLib.loadNewAnimationSet("jensenDown_CYAN");
                    walk_right = AnimationLib.loadNewAnimationSet("jensenRight_CYAN");
                    walk_left = AnimationLib.loadNewAnimationSet("jensenRight_CYAN");
                    walk_up = AnimationLib.loadNewAnimationSet("jensenUp_CYAN");
                }
                else if (GameCampaign.Player2Color == 5)
                {
                    walk_down = AnimationLib.loadNewAnimationSet("jensenDown_BROWN");
                    walk_right = AnimationLib.loadNewAnimationSet("jensenRight_BROWN");
                    walk_left = AnimationLib.loadNewAnimationSet("jensenRight_BROWN");
                    walk_up = AnimationLib.loadNewAnimationSet("jensenUp_BROWN");
                }*/
            }

            remove_from_list = false;
            current_skeleton = walk_right;
            current_skeleton.Animation = current_skeleton.Skeleton.Data.FindAnimation("run");

            enemy_type = EnemyType.Player;

            this.index = index;

            setAnimationWeapons(walk_down, GlobalGameConstants.Direction.Right);
            setAnimationWeapons(walk_right, GlobalGameConstants.Direction.Right);
            setAnimationWeapons(walk_left, GlobalGameConstants.Direction.Left);
            setAnimationWeapons(walk_up, GlobalGameConstants.Direction.Right);
        }

        public override void update(GameTime currentTime)
        {
            double delta = currentTime.ElapsedGameTime.Milliseconds;

            /*
            if (Keyboard.GetState().IsKeyDown(Keys.Space))
            {
                parentWorld.Particles.pushGib(CenterPoint);
            }
             */
               

            if (index == InputDevice2.PPG_Player.Player_1 ? GameCampaign.Player_Health <= 0.0f : GameCampaign.Player2_Health <= 0.0f)
            {
                if (!parentWorld.Player1Dead)
                {
                    BackGroundAudio.stopAllSongs();
                    AudioLib.playSoundEffect("missionFailed");

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

                if (Player_Right_Item != null)
                {
                    Player_Right_Item.daemonupdate(this, currentTime, parentWorld);
                }
                if (Player_Left_Item != null)
                {
                    Player_Left_Item.daemonupdate(this, currentTime, parentWorld);
                }
            }
            else
            {
                if (state == playerState.Item1)
                {
                    if (Player_Right_Item == null)
                    {
                        state = playerState.Moving;
                    }
                    else
                    {
                        Player_Right_Item.update(this, currentTime, parentWorld);
                    }


                    if (Player_Left_Item != null)
                    {
                        Player_Left_Item.daemonupdate(this, currentTime, parentWorld);
                    }

                }
                else if (state == playerState.Item2)
                {
                    if (Player_Left_Item == null)
                    {
                        state = playerState.Moving;
                    }
                    else
                    {
                        Player_Left_Item.update(this, currentTime, parentWorld);
                    }

                    if (Player_Right_Item != null)
                    {
                        Player_Right_Item.daemonupdate(this, currentTime, parentWorld);
                    }
                }
                else if (state == playerState.Moving)
                {
                    loopAnimation = true;

                    if (InputDevice2.IsPlayerButtonDown(index, InputDevice2.PlayerButton.UseItem1))
                    {
                        state = playerState.Item1;
                    }
                    if (InputDevice2.IsPlayerButtonDown(index, InputDevice2.PlayerButton.UseItem2))
                    {
                        state = playerState.Item2;
                    }

                    if (disable_movement == false)
                    {
                        if (InputDevice2.IsPlayerButtonDown(index, InputDevice2.PlayerButton.RightDirection))
                        {
                            velocity.X = playerMoveSpeed;
                            direction_facing = GlobalGameConstants.Direction.Right;
                        }
                        else if (InputDevice2.IsPlayerButtonDown(index, InputDevice2.PlayerButton.LeftDirection))
                        {
                            velocity.X = -playerMoveSpeed;
                            direction_facing = GlobalGameConstants.Direction.Left;
                        }
                        else
                        {
                            velocity.X = 0.0f;
                        }

                        if (InputDevice2.IsPlayerButtonDown(index, InputDevice2.PlayerButton.UpDirection))
                        {
                            velocity.Y = -playerMoveSpeed;
                            direction_facing = GlobalGameConstants.Direction.Up;
                        }
                        else if (InputDevice2.IsPlayerButtonDown(index, InputDevice2.PlayerButton.DownDirection))
                        {
                            velocity.Y = playerMoveSpeed;
                            direction_facing = GlobalGameConstants.Direction.Down;
                        }
                        else
                        {
                            velocity.Y = 0.0f;
                        }

                        GlobalGameConstants.Direction analogDirection = InputDevice2.PlayerAnalogStickDirection(index);
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
                                current_skeleton = walk_left;
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

                                if (InputDevice2.IsPlayerButtonDown(index, InputDevice2.PlayerButton.SwitchItem1) && !item1_switch_button_down)
                                {
                                    item1_switch_button_down = true;
                                }
                                else if (!InputDevice2.IsPlayerButtonDown(index, InputDevice2.PlayerButton.SwitchItem1) && item1_switch_button_down)
                                {
                                    item1_switch_button_down = false;

                                    Player_Right_Item = ((Pickup)parentWorld.EntityList[i]).assignItem(Player_Right_Item, currentTime);

                                    if (index == InputDevice2.PPG_Player.Player_1)
                                    {
                                        GameCampaign.Player_Right_Item = Player_Right_Item.ItemType();
                                    }
                                    else if (index == InputDevice2.PPG_Player.Player_2)
                                    {
                                        GameCampaign.Player2_Item_1 = Player_Right_Item.ItemType();
                                    }

                                    setAnimationWeapons(walk_down, GlobalGameConstants.Direction.Right);
                                    setAnimationWeapons(walk_right, GlobalGameConstants.Direction.Right);
                                    setAnimationWeapons(walk_left, GlobalGameConstants.Direction.Left);
                                    setAnimationWeapons(walk_up, GlobalGameConstants.Direction.Right);
                                }

                                if (InputDevice2.IsPlayerButtonDown(index, InputDevice2.PlayerButton.SwitchItem2) && !item2_switch_button_down)
                                {
                                    item2_switch_button_down = true;
                                }
                                else if (!InputDevice2.IsPlayerButtonDown(index, InputDevice2.PlayerButton.SwitchItem2) && item2_switch_button_down)
                                {
                                    item2_switch_button_down = false;

                                    Player_Left_Item = ((Pickup)parentWorld.EntityList[i]).assignItem(Player_Left_Item, currentTime);

                                    if (index == InputDevice2.PPG_Player.Player_1)
                                    {
                                        GameCampaign.Player_Left_Item = Player_Left_Item.ItemType();
                                    }
                                    else if (index == InputDevice2.PPG_Player.Player_2)
                                    {
                                        GameCampaign.Player2_Item_2 = Player_Left_Item.ItemType();
                                    }

                                    setAnimationWeapons(walk_down, GlobalGameConstants.Direction.Right);
                                    setAnimationWeapons(walk_right, GlobalGameConstants.Direction.Right);
                                    setAnimationWeapons(walk_left, GlobalGameConstants.Direction.Left);
                                    setAnimationWeapons(walk_up, GlobalGameConstants.Direction.Right);
                                }
                            }
                        }
                    }


                    if (!itemTouched && (item1_switch_button_down || item2_switch_button_down))
                    {
                        item1_switch_button_down = false;
                        item2_switch_button_down = false;
                    }

                    if (Player_Right_Item != null)
                    {
                        Player_Right_Item.daemonupdate(this, currentTime, parentWorld);
                    }
                    if (Player_Left_Item != null)
                    {
                        Player_Left_Item.daemonupdate(this, currentTime, parentWorld);
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
            if (Player_Right_Item != null)
            {
                Player_Right_Item.draw(sb);
            }
            if (Player_Left_Item != null)
            {
                Player_Left_Item.draw(sb);
            }
        }

        public override void knockBack(Vector2 direction, float magnitude, int damage, Entity attacker)
        {
            if (state == playerState.Item1 || state == playerState.Item2)
            {
                state = playerState.Moving;
            }

            if (disable_movement == false)
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

                if (index == InputDevice2.PPG_Player.Player_1)
                {
                    GameCampaign.Player_Health = GameCampaign.Player_Health - damage;
                }
                else if (index == InputDevice2.PPG_Player.Player_2)
                {
                    GameCampaign.Player2_Health = GameCampaign.Player2_Health - damage;
                }
            }
        }

        public void setAnimationWeapons(AnimationLib.SpineAnimationSet current_skeleton, GlobalGameConstants.Direction direction_facing)
        {
            current_skeleton.Skeleton.B = 1.0f;
            current_skeleton.Skeleton.G = 1.0f;
            current_skeleton.Skeleton.R = 1.0f;

            switch (direction_facing == GlobalGameConstants.Direction.Left ? Player_Right_Item.ItemType() : Player_Left_Item.ItemType())
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

            switch (direction_facing == GlobalGameConstants.Direction.Left ? Player_Left_Item.ItemType() : Player_Right_Item.ItemType())
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
            current_skeleton.Skeleton.RootBone.X = CenterPoint.X * (current_skeleton.Skeleton.FlipX ? -1 : 1);
            current_skeleton.Skeleton.RootBone.Y = CenterPoint.Y+(dimensions.Y/2f);

            current_skeleton.Skeleton.UpdateWorldTransform();
            renderer.Draw(current_skeleton.Skeleton);
        }
    }
}
