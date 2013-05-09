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
        private int bomb_damage = 5;
        private Vector2 max_hitbox = Vector2.Zero;
        private Vector2 position = Vector2.Zero;
        private float time_explosion = 0.0f;
        private GlobalGameConstants.itemType item_type = GlobalGameConstants.itemType.Bomb;
        Bomb_State bomb_state = Bomb_State.reset;

        public Bomb(Vector2 initial_position)
        {
            position = initial_position;
            hitbox = hitbox_placed;
            time_explosion = 0.0f;
            bomb_state = Bomb_State.reset;
            bomb_damage = 5;
        }

        public void update(Player parent, GameTime currentTime, LevelState parentWorld)
        {
            position = parent.CenterPoint - hitbox/2;
            time_explosion = 0.0f;
            parent.State = Player.playerState.Moving;
            bomb_state = Bomb_State.placed;
        }
        public void daemonupdate(GameTime currentTime, LevelState parentWorld)
        {
            time_explosion += currentTime.ElapsedGameTime.Milliseconds;

            switch (bomb_state)
            {
                case Bomb_State.placed:
                    if (time_explosion > 1000)
                    {
                        hitbox = hitbox_exploded;
                        position = new Vector2(position.X - hitbox.X / 2, position.Y - hitbox.Y / 2);
                        bomb_state = Bomb_State.exploded;
                        time_explosion = 0.0f;
                    }
                    break;
                case Bomb_State.exploded:
                    //where hit test is done
                    if (time_explosion > 500)
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
                                    en.knockBack(en, position, hitbox, bomb_damage);
                                }
                            }
                            else if (en is Enemy)
                            {
                                if (hitTest(en))
                                {
                                    en.knockBack(en, position, hitbox, bomb_damage);
                                }
                            }
                        }
                    }
                    break;
                default:
                    hitbox = hitbox_placed;
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
            if (bomb_state != Bomb_State.reset)
            {
                sb.Draw(Game1.whitePixel, position, null, Color.White, 0.0f, Vector2.Zero, hitbox, SpriteEffects.None, 0.5f);
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
