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
    class Pickup : Entity
    {
        GlobalGameConstants.itemType item_type = GlobalGameConstants.itemType.Bomb;

        public Pickup(LevelState parentWorld, float initial_x, float initial_y, GlobalGameConstants.itemType item_choice)
        {
            position = new Vector2(initial_x, initial_y);
            dimensions = new Vector2(48.0f, 48.0f);

            item_type = item_choice;
        }

        public override void update(GameTime currentTime)
        {
            return;
        }

        //after the item gets picked up, it gets replaced with the item the player dropped
        public Item assignItem(Item player_item, GameTime currentTime)
        {
            item_type = player_item.ItemType();

            switch (item_type)
            {
                case GlobalGameConstants.itemType.Bomb:
                    return new Bomb();
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
                default:
                    return new Sword();
            }
        }

        public override void draw(SpriteBatch sb)
        {
            AnimationLib.FrameAnimationSet dropAnim = null;

            dropAnim = GlobalGameConstants.WeaponDictionary.weaponInfo[(int)item_type].pickupImage;

            if (dropAnim != null)
            {
                dropAnim.drawAnimationFrame(0.0f, sb, Position, new Vector2(1.0f, 1.0f), 0.5f);
            }
            else
            {
                sb.Draw(Game1.whitePixel, position, null, Color.Black, 0.0f, Vector2.Zero, dimensions, SpriteEffects.None, 0.5f);
            }
        }
        public override void knockBack(Vector2 direction, float magnitude, int damage, Entity attacker)
        {
            return;
        }
    }
}
