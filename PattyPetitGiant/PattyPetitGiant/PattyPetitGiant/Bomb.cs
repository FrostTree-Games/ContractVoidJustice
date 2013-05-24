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
    class Bomb : Item
    {
        private enum Bomb_State
        {
            reset,
            placed,
            exploded
        }

        private Vector2 hitbox = Vector2.Zero;
        private Vector2 hitbox_placed = new Vector2(48.0f, 48.0f);
        private Vector2 hitbox_exploded = new Vector2(48.0f * 3.0f, 48.0f * 3.0f);
        private Vector2 center_placed_bomb = Vector2.Zero;
        private int bomb_damage = 5;
        private Vector2 max_hitbox = Vector2.Zero;
        private Vector2 position = Vector2.Zero;
        private float time_explosion = 0.0f;
        private GlobalGameConstants.itemType item_type = GlobalGameConstants.itemType.Bomb;
        Bomb_State bomb_state = Bomb_State.reset;
        private AnimationLib.FrameAnimationSet bombAnim;
        private float animation_time;

        public Bomb(Vector2 initial_position)
        {
            position = initial_position;
            hitbox = hitbox_placed;
            time_explosion = 0.0f;
            bomb_state = Bomb_State.reset;
            bomb_damage = 5;
            bombAnim = AnimationLib.getFrameAnimationSet("bombPic");
        }

        public void update(Player parent, GameTime currentTime, LevelState parentWorld)
        {
            if (bomb_state == Bomb_State.reset)
            {
                position = parent.CenterPoint - hitbox / 2;
                center_placed_bomb = position + hitbox / 2;
                time_explosion = 0.0f;
                parent.State = Player.playerState.Moving;
                bomb_state = Bomb_State.placed;
            }
            else
            {
                parent.State = Player.playerState.Moving;
            }
        }
        public void daemonupdate(Player parent, GameTime currentTime, LevelState parentWorld)
        {
            time_explosion += currentTime.ElapsedGameTime.Milliseconds;

            switch (bomb_state)
            {
                case Bomb_State.placed:
                    if (time_explosion > 1500)
                    {
                        hitbox = hitbox_exploded;
                        position = new Vector2(center_placed_bomb.X - hitbox.X / 2, center_placed_bomb.Y - hitbox.Y / 2);
                        bomb_state = Bomb_State.exploded;
                        time_explosion = 0.0f;
                        bombAnim = AnimationLib.getFrameAnimationSet("bombExplosion");
                    }
                    break;
                case Bomb_State.exploded:
                    //where hit test is done
                    if (time_explosion > 700)
                    {
                        bomb_state = Bomb_State.reset;
                        time_explosion = 0.0f;
                    }
                    else
                    {
                        foreach (Entity en in parentWorld.EntityList)
                        {
                            if (en is Player)
                            {
                                if (hitTest(en))
                                {
                                    bombKnockBack(en, position, hitbox, bomb_damage);
                                }
                            }
                            else if (en is Enemy || en is ShopKeeper)
                            {
                                if (hitTest(en))
                                {
                                    bombKnockBack(en, position, hitbox, bomb_damage);
                                }
                            }
                        }
                    }
                    animation_time += currentTime.ElapsedGameTime.Milliseconds;
                    break;
                default:
                    bombAnim = AnimationLib.getFrameAnimationSet("bombPic");
                    hitbox = hitbox_placed;
                    animation_time = 0.0f;
                    break;
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
            switch(bomb_state)
            {
                case Bomb_State.placed:
                    bombAnim.drawAnimationFrame(animation_time, sb, position, new Vector2(1.0f, 1.0f), 0.5f);
                    break;
                case Bomb_State.exploded:
                    bombAnim.drawAnimationFrame(animation_time, sb, position , new Vector2(2.25f, 2.25f), 0.5f);
                    //sb.Draw(Game1.whitePixel, position, null, Color.White, 0.0f, Vector2.Zero, hitbox, SpriteEffects.None, 0.5f);
                    break;
                default:
                    break;
            }
        }

        public void bombKnockBack(Entity en, Vector2 position, Vector2 hitbox, int bomb_damage)
        {
            en.Disable_Movement= true;
            float direction_x = en.CenterPoint.X - (position.X + hitbox.X/2);
            float direction_y = en.CenterPoint.Y - (position.Y + hitbox.Y/2);

            if (Math.Abs(direction_x) > (Math.Abs(direction_y)))
            {
                if (direction_x < 0)
                {
                    en.Velocity = new Vector2(-5.51f, direction_y / 100);
                }
                else
                {
                    en.Velocity = new Vector2(5.51f, direction_y / 100);
                }
            }
            else
            {
                if (direction_y < 0)
                {
                    en.Velocity = new Vector2(direction_x / 100f, -5.51f);
                }
                else
                {
                    en.Velocity = new Vector2(direction_x / 100f, 5.51f);
                }
            }
            if (en is Player)
            {
                GlobalGameConstants.Player_Health = GlobalGameConstants.Player_Health - bomb_damage;
            }
            else if (en is Enemy)
            {
                ((Enemy)en).Enemy_Life = ((Enemy)en).Enemy_Life - bomb_damage;
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
