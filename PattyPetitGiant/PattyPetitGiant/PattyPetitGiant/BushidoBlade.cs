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
    class BushidoBlade : Item
    {
        private enum BushidoState
        {
            preslash,
            slash,
            endslash,
            bushido
        }
        private Vector2 hitbox = Vector2.Zero;
        private Vector2 position = Vector2.Zero;
        private Vector2 enemy_explode_position = Vector2.Zero;
        private GlobalGameConstants.itemType item_type = GlobalGameConstants.itemType.BushidoBlade;
        private GlobalGameConstants.Direction item_direction = GlobalGameConstants.Direction.Right;
        private BushidoState bushido_state = BushidoState.preslash;
        private AnimationLib.FrameAnimationSet bushidoAnim;
        
        private bool sword_swing = false;
        private bool enemy_explode = false;

        private float item_state_time;
        private float player_health;
        private float delay = 100.0f;
        private float animation_time;

        protected int sword_damage;
        protected float knockback_magnitude;

        public BushidoBlade(Vector2 initial_position)
        {
            position = initial_position;
            hitbox = new Vector2(48.0f, 48.0f);
            sword_damage = 999999999;
            item_state_time = 0.0f;
            player_health = GameCampaign.Player_Health;
            animation_time = 0.0f;
            bushidoAnim = AnimationLib.getFrameAnimationSet("bombExplosion");
            knockback_magnitude = 1.0f;
        }

        public void update(Player parent, GameTime currentTime, LevelState parentWorld)
        {
            item_direction = parent.Direction_Facing;
            parent.Velocity = Vector2.Zero;

            item_state_time += currentTime.ElapsedGameTime.Milliseconds;

            switch(bushido_state)
            {
                case BushidoState.preslash:
                    parent.Animation_Time = 0.0f;
                    parent.LoadAnimation.Animation = parent.LoadAnimation.Skeleton.Data.FindAnimation("rSlash");
                    switch (item_direction)
                {
                    case GlobalGameConstants.Direction.Right:
                        position.X = parent.Position.X + parent.Dimensions.X;
                        position.Y = parent.Position.Y;
                        break;
                    case GlobalGameConstants.Direction.Left:
                        position.X = parent.Position.X - hitbox.X;
                        position.Y = parent.Position.Y;
                        break;
                    case GlobalGameConstants.Direction.Up:
                        position.Y = parent.Position.Y - hitbox.Y;
                        position.X = parent.CenterPoint.X - hitbox.X / 2;
                        break;
                    default:
                        position.Y = parent.CenterPoint.Y + parent.Dimensions.Y / 2;
                        position.X = parent.CenterPoint.X - hitbox.X / 2;
                        break;
                }
                if (item_state_time > delay)
                {
                    bushido_state = BushidoState.slash;
                    sword_swing = true;
                }
                    break;
                case BushidoState.slash:
                    foreach (Entity en in parentWorld.EntityList)
                    {
                        if (en is Enemy || en is ShopKeeper)
                        {
                            if (hitTest(en))
                            {
                                Vector2 direction = en.CenterPoint - parent.CenterPoint;
                                en.knockBack(direction, knockback_magnitude, sword_damage);
                                enemy_explode = true;
                                enemy_explode_position = en.CenterPoint - new Vector2(24.0f * 3.0f, 24.0f * 3.0f);
                            }
                        }
                    }
                    bushido_state = BushidoState.endslash;
                    break;
                default:
                    parent.State = Player.playerState.Moving;
                    item_state_time = 0.0f;
                    parent.Disable_Movement = true;
                    sword_swing = false;
                    bushido_state = BushidoState.preslash;
                    break;
            }
        }

        public void daemonupdate(Player parent, GameTime currentTime, LevelState parentWorld)
        {
            if (GameCampaign.Player_Health != player_health && GameCampaign.Player_Health > 0)
            {
                bushido_state = BushidoState.bushido;
            }

            if (bushido_state == BushidoState.bushido)
            {
                GameCampaign.Player_Health = 0;
                parent.Velocity = Vector2.Zero;
                position = parent.CenterPoint - new Vector2(24.0f * 3.0f, 24.0f * 3.0f);
                //animate bushido death
                animation_time += currentTime.ElapsedGameTime.Milliseconds;

                if (animation_time > 700)
                {
                    bushido_state = BushidoState.preslash;
                    animation_time = 0.0f;
                }
                Console.WriteLine(animation_time);
            }
            if (enemy_explode == true)
            {
                animation_time += currentTime.ElapsedGameTime.Milliseconds;
                if (animation_time > 700)
                {
                    animation_time = 0.0f;
                    enemy_explode = false;
                }
            }
        }

        public GlobalGameConstants.itemType ItemType()
        {
            return item_type;
        }

        public string getEnumType()
        {
            return item_type.ToString();
        }

        public void draw(SpriteBatch sb)
        {
            if (bushido_state == BushidoState.bushido)
            {
                bushidoAnim.drawAnimationFrame(animation_time, sb, position, new Vector2(2.25f, 2.25f), 0.5f);
            }
            else if (enemy_explode == true)
            {
                bushidoAnim.drawAnimationFrame(animation_time, sb, enemy_explode_position, new Vector2(2.25f, 2.25f), 0.5f);
            }
        }

        public bool hitTest(Entity other)
        {
            if (position.X > other.Position.X + other.Dimensions.X || position.X + hitbox.X < other.Position.X || position.Y > other.Position.Y + other.Dimensions.Y || position.Y + hitbox.Y < other.Position.Y)
            {
                return false;
            }

            return true;
        }
    }
}
