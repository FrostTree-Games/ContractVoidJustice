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
            exploded,
            none
        }

        private Vector2 hitbox = Vector2.Zero;
        private Vector2 hitbox_placed = new Vector2(48.0f, 48.0f);
        private Vector2 hitbox_exploded = new Vector2(48.0f * 3.0f, 48.0f * 3.0f);
        private Vector2 center_placed_bomb = Vector2.Zero;
        private int bomb_damage = 15;
        private Vector2 max_hitbox = Vector2.Zero;
        private Vector2 position = Vector2.Zero;
        private float time_explosion = 0.0f;
        private GlobalGameConstants.itemType item_type = GlobalGameConstants.itemType.Bomb;
        Bomb_State bomb_state = Bomb_State.reset;
        private AnimationLib.FrameAnimationSet bombAnim;
        private AnimationLib.FrameAnimationSet explosionAnim;
        private float animation_time;
        private float knockback_magnitude;
        private const float ammo_consumption = 10;
        private float bomb_timer = 0.0f;

        public Bomb()
        {
            hitbox = hitbox_placed;
            time_explosion = 0.0f;
            bomb_state = Bomb_State.reset;
            bomb_damage = 5;
            bombAnim = AnimationLib.getFrameAnimationSet("bombArmed");
            explosionAnim = AnimationLib.getFrameAnimationSet("rocketExplode");
            knockback_magnitude = 5.0f;
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
            bomb_timer += currentTime.ElapsedGameTime.Milliseconds;
            
            switch (bomb_state)
            {
                case Bomb_State.placed:
                    animation_time += currentTime.ElapsedGameTime.Milliseconds;
                    if ((parent.Index == InputDevice2.PPG_Player.Player_1 ? GameCampaign.Player_Ammunition : GameCampaign.Player2_Ammunition) >= 10)
                    {
                        if (bomb_timer > 1500f / time_explosion * 50.0f)
                        {
                            bomb_timer = 0.0f;
                            AudioLib.playSoundEffect("bombBeep");
                        }
                        
                        if (time_explosion > 1500)
                        {
                            if (parent.Index == InputDevice2.PPG_Player.Player_1)
                            {
                                GameCampaign.Player_Ammunition -= ammo_consumption;
                            }
                            else
                            {
                                GameCampaign.Player2_Ammunition -= ammo_consumption;
                            }

                            hitbox = hitbox_exploded;
                            position = new Vector2(center_placed_bomb.X - hitbox.X / 2, center_placed_bomb.Y - hitbox.Y / 2);
                            bomb_state = Bomb_State.exploded;
                            time_explosion = 0.0f;
                            AudioLib.playSoundEffect("testExplosion");
                            animation_time = 0.0f;
                        }
                    }
                    else
                    {
                        parent.State = Player.playerState.Moving;
                        bomb_state = Bomb_State.none;
                        return;
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
                                    Vector2 direction = en.CenterPoint - center_placed_bomb;

                                    float bomb_knockback_power = Vector2.Distance(center_placed_bomb, en.CenterPoint) / hitbox.X;

                                    if (bomb_knockback_power < 1)
                                    {
                                        bomb_knockback_power = 1.0f;
                                    }

                                    float temp_knockback_magnitude = knockback_magnitude / bomb_knockback_power;

                                    en.knockBack(direction, temp_knockback_magnitude, bomb_damage, parent);
                                }
                            }
                            else if (en is Enemy || en is ShopKeeper)
                            {
                                if (hitTest(en))
                                {
                                    Vector2 direction = en.CenterPoint - center_placed_bomb;
                                    float bomb_knockback_power = Vector2.Distance(center_placed_bomb, en.CenterPoint) / hitbox.X;

                                    if (bomb_knockback_power < 1)
                                    {
                                        bomb_knockback_power = 1.0f;
                                    }

                                    float temp_knockback_magnitude = knockback_magnitude / bomb_knockback_power;
                                                                        
                                    en.knockBack(direction, temp_knockback_magnitude, bomb_damage);
                                }
                            }
                        }
                    }
                    animation_time += currentTime.ElapsedGameTime.Milliseconds;
                    break;
                case Bomb_State.none:
                    break;
                default:
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

        public void draw(Spine.SkeletonRenderer sb)
        {
            switch(bomb_state)
            {
                case Bomb_State.placed:
                    //sb.DrawSpriteToSpineVertexArray(Game1.whitePixel, new Rectangle(0, 0, 1, 1), position, Color.Red, 0.0f, hitbox);
                    
                    bombAnim.drawAnimationFrame(animation_time, sb, position, new Vector2(1), 0.5f, 0.0f, Vector2.Zero, Color.White);
                    break;
                case Bomb_State.exploded:
                    //sb.DrawSpriteToSpineVertexArray(Game1.whitePixel, new Rectangle(0, 0, 1, 1), position, Color.Red, 0.0f, hitbox);
                    explosionAnim.drawAnimationFrame(animation_time, sb, position, new Vector2(2.25f), 0.5f, 0.0f, Vector2.Zero, Color.White);
                    break;
                default:
                    break;
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
