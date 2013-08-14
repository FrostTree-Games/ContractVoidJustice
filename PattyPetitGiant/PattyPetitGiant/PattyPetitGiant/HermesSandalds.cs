using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PattyPetitGiant
{
    class HermesSandals : Item
    {
        private enum HermesSandalsState
        {
            InvalidState = -1,
            Idle = 0,
            WindUp = 1,
            Running = 2,
        }

        private HermesSandalsState state;

        private const float hermesRunSpeed = 15f;

        private float windUpTime;
        private const float windUpDuration = 450f;
        private Vector2 windUpDrawPosition = Vector2.Zero;

        private const float ammoCostPerSecond = 10f;

        public HermesSandals()
        {
            state = HermesSandalsState.Idle;
        }

        public void update(Player parent, GameTime currentTime, LevelState parentWorld)
        {
            if (state == HermesSandalsState.Running)
            {
                switch (parent.Direction_Facing)
                {
                    case GlobalGameConstants.Direction.Up:
                        parent.Velocity = new Vector2(0, -hermesRunSpeed);
                        break;
                    case GlobalGameConstants.Direction.Down:
                        parent.Velocity = new Vector2(0, hermesRunSpeed);
                        break;
                    case GlobalGameConstants.Direction.Left:
                        parent.Velocity = new Vector2(-hermesRunSpeed, 0);
                        break;
                    case GlobalGameConstants.Direction.Right:
                        parent.Velocity = new Vector2(hermesRunSpeed, 0);
                        break;
                }

                if ((parent.Index == InputDevice2.PPG_Player.Player_1 ? GameCampaign.Player_Right_Item : GameCampaign.Player2_Item_1) == ItemType() && !InputDevice2.IsPlayerButtonDown(parent.Index, InputDevice2.PlayerButton.UseItem1))
                {
                    state = HermesSandalsState.Idle;

                    parent.Disable_Movement = false;
                    parent.State = Player.playerState.Moving;
                }
                else if ((parent.Index == InputDevice2.PPG_Player.Player_1 ? GameCampaign.Player_Left_Item : GameCampaign.Player2_Item_2) == ItemType() && !InputDevice2.IsPlayerButtonDown(parent.Index, InputDevice2.PlayerButton.UseItem2))
                {
                    state = HermesSandalsState.Idle;

                    parent.Disable_Movement = false;
                    parent.State = Player.playerState.Moving;
                }

                parent.Animation_Time += (currentTime.ElapsedGameTime.Milliseconds / 400f);

                if (Game1.rand.Next() % 2 == 0)
                {
                    parentWorld.Particles.pushDirectedParticle(parent.CenterPoint + new Vector2(-4, parent.Dimensions.Y / 2 - 12), Color.LightCyan, (float)(((int)(parent.Direction_Facing) * (Math.PI / 2)) + (Game1.rand.NextDouble() * (Math.PI / 4)) - (Math.PI / 8)));
                }
            }
            else if (state == HermesSandalsState.WindUp)
            {
                if ((parent.Index == InputDevice2.PPG_Player.Player_1 ? GameCampaign.Player_Right_Item : GameCampaign.Player2_Item_1) == ItemType() && !InputDevice2.IsPlayerButtonDown(parent.Index, InputDevice2.PlayerButton.UseItem1))
                {
                    state = HermesSandalsState.Idle;

                    parent.Disable_Movement = false;
                    parent.State = Player.playerState.Moving;

                    return;
                }
                else if ((parent.Index == InputDevice2.PPG_Player.Player_1 ? GameCampaign.Player_Left_Item : GameCampaign.Player2_Item_2) == ItemType() && !InputDevice2.IsPlayerButtonDown(parent.Index, InputDevice2.PlayerButton.UseItem2))
                {
                    state = HermesSandalsState.Idle;

                    parent.Disable_Movement = false;
                    parent.State = Player.playerState.Moving;

                    return;
                }

                windUpTime += currentTime.ElapsedGameTime.Milliseconds;

                parent.Animation_Time += (currentTime.ElapsedGameTime.Milliseconds / 400f);

                if (windUpTime > windUpDuration)
                {
                    if ((parent.Index == InputDevice2.PPG_Player.Player_1 ? GameCampaign.Player_Ammunition : GameCampaign.Player2_Ammunition) >= 10.0f)
                    {
                        if (parent.Index == InputDevice2.PPG_Player.Player_1)
                        {
                            GameCampaign.Player_Ammunition -= 10;
                        }
                        else
                        {
                            GameCampaign.Player2_Ammunition -= 10;
                        }

                        state = HermesSandalsState.Running;
                    }
                    else
                    {
                        state = HermesSandalsState.Idle;

                        parent.LoadAnimation.Skeleton.Data.FindAnimation("idle");

                        parent.Disable_Movement = false;
                        parent.State = Player.playerState.Moving;
                    }
                }

                if (GameCampaign.Player_Ammunition >= 10.0f)
                {
                    parentWorld.Particles.pushDirectedParticle(parent.CenterPoint + new Vector2(0, parent.Dimensions.Y / 2 - 12), Color.LightCyan, (float)(Game1.rand.NextDouble() * Math.PI * 2));
                }
            }
            else if (state == HermesSandalsState.Idle)
            {
                state = HermesSandalsState.WindUp;

                windUpTime = 0.0f;
                parent.Velocity = Vector2.Zero;

                parent.LoadAnimation.Animation = parent.LoadAnimation.Skeleton.Data.FindAnimation("run");
            }
            else
            {
                throw new Exception("invalid HermesSandals state");
            }
        }

        public void daemonupdate(Player parent, GameTime currentTime, LevelState parentWorld)
        {
            windUpDrawPosition = parent.Position - new Vector2(0.0f, GlobalGameConstants.TileSize.Y);

            if (state == HermesSandalsState.Running || state == HermesSandalsState.WindUp)
            {
                state = HermesSandalsState.Idle;
            }
        }

        public void draw(Spine.SkeletonRenderer sb)
        {
            //
        }

        public GlobalGameConstants.itemType ItemType()
        {
            return GlobalGameConstants.itemType.HermesSandals;
        }

        public string getEnumType()
        {
            return "HermesSandals";
        }
    }
}
