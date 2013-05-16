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
    class Sword : Item
    {
        private enum Sword_State
        {
            preslash,
            slash,
            endslash
        }
        private Sword_State sword_state = Sword_State.preslash;
        private Vector2 hitbox = Vector2.Zero;
        private Vector2 max_hitbox = Vector2.Zero;
        private Vector2 position = Vector2.Zero;
        private GlobalGameConstants.Direction item_direction = GlobalGameConstants.Direction.Right;
        private GlobalGameConstants.itemType item_type = GlobalGameConstants.itemType.Sword;
        private float delay = 100.0f;
        private float item_state_time = 0.0f;
        private bool sword_swing = false;
        protected int sword_damage;
        private AnimationLib.FrameAnimationSet swordAnim;

        public Sword(Vector2 initial_position)
        {
            position = initial_position;
            hitbox.X = 48.0f;
            hitbox.Y = 48.0f;
            item_state_time = 0.0f;
            sword_damage = 5;

            sword_state = Sword_State.preslash;

            swordAnim = AnimationLib.getFrameAnimationSet("swordPic");
        }

        public void update(Player parent, GameTime currentTime, LevelState parentWorld)
        {
            item_direction = parent.Direction_Facing;
            parent.Velocity = Vector2.Zero;

            item_state_time += currentTime.ElapsedGameTime.Milliseconds;

            if (sword_state == Sword_State.preslash)
            {
                //sword is on the right hand side of the player, if hitboxes are different dimensions, need to adjust the position of sword.
                parent.Animation_Time = 0.0f;
                parent.LoadAnimaton.Animation = parent.LoadAnimaton.Skeleton.Data.FindAnimation("rSlash");
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
                    sword_state = Sword_State.slash;
                    sword_swing = true;
                }
            }
            else if (sword_state == Sword_State.slash)
            {
                foreach (Entity en in parentWorld.EntityList)
                {
                    if (en is Enemy)
                    {
                        if (hitTest(en))
                        {
                            parent.knockBack(en, parent.Position, parent.Dimensions, sword_damage);
                        }
                    }
                }
                
                sword_state = Sword_State.endslash;
            }
            //time delay for the player to be in this state
            else if (sword_state == Sword_State.endslash)
            {
                parent.State = Player.playerState.Moving;
                item_state_time = 0.0f;
                parent.Disable_Movement = true;
                sword_swing = false;
                sword_state = Sword_State.preslash;
            }
        }

        public void daemonupdate(Player parent, GameTime currentTime, LevelState parentWorld)
        {
            return;
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
            /*if (sword_swing)
            {
                //sb.Draw(Game1.whitePixel, position, null, Color.Pink, 0.0f, Vector2.Zero, hitbox, SpriteEffects.None, 0.5f);
                swordAnim.drawAnimationFrame(0.0f, sb, position, new Vector2(3, 3), 0.5f);
            }*/
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
