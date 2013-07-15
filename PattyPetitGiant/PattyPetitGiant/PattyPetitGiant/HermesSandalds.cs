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

        private const float hermesRunSpeed = 9f;

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

                if (GameCampaign.Player_Item_1 == ItemType() && !InputDeviceManager.isButtonDown(InputDeviceManager.PlayerButton.UseItem1))
                {
                    state = HermesSandalsState.Idle;

                    parent.Disable_Movement = false;
                    parent.State = Player.playerState.Moving;
                }
                else if (GameCampaign.Player_Item_2 == ItemType() && !InputDeviceManager.isButtonDown(InputDeviceManager.PlayerButton.UseItem2))
                {
                    state = HermesSandalsState.Idle;

                    parent.Disable_Movement = false;
                    parent.State = Player.playerState.Moving;
                }

                parent.Animation_Time += (currentTime.ElapsedGameTime.Milliseconds / 400f);

                GameCampaign.Player_Ammunition -= (currentTime.ElapsedGameTime.Milliseconds / 1000f) * ammoCostPerSecond;
            }
            else if (state == HermesSandalsState.WindUp)
            {
                windUpTime += currentTime.ElapsedGameTime.Milliseconds;

                parent.Animation_Time += (currentTime.ElapsedGameTime.Milliseconds / 400f);

                if (windUpTime > windUpDuration)
                {
                    if (GameCampaign.Player_Ammunition > 0.01f)
                    {
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
        }

        public void draw(SpriteBatch sb)
        {
            if (state == HermesSandalsState.WindUp)
            {
                //building strings every frame like this is going to allocate a lot of garbage. don't do it on the Xbox 360
                //later implementations will use some particle or sprite
#if WINDOWS
                sb.DrawString(Game1.font, "charging", windUpDrawPosition, Color.HotPink, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.65f);
#endif
            }
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
