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

        private AnimationLib.FrameAnimationSet currentAnimation = null;

        private bool isKnockedBack = false;
        private const float knockBackSpeed = 0.4f;
        private float knockedBackTime = 0.0f;
        private const float knockBackDuration = 250f;

        public Pickup(LevelState parentWorld, Vector2 position, GlobalGameConstants.itemType item_choice)
        {
            this.position = position;
            dimensions = GlobalGameConstants.TileSize;

            item_type = item_choice;

            this.parentWorld = parentWorld;
        }

        public Pickup(LevelState parentWorld, Vector2 position, Random rand)
        {
            this.position = position;
            dimensions = GlobalGameConstants.TileSize;

            int rValue = rand.Next() % 14;
            if (rValue == 9) { rValue++; } // casting an int to item enum; no index for 9

            item_type = (GlobalGameConstants.itemType)rValue;

            this.parentWorld = parentWorld;
        }

        public override void update(GameTime currentTime)
        {
            if (isKnockedBack)
            {
                knockedBackTime += currentTime.ElapsedGameTime.Milliseconds;

                if (knockedBackTime > knockBackDuration)
                {
                    isKnockedBack = false;
                }
            }
            else
            {
                velocity = Vector2.Zero;
            }

            Vector2 nextStep = position + (velocity * currentTime.ElapsedGameTime.Milliseconds);

            Vector2 finalPos = parentWorld.Map.reloactePosition(position, nextStep, dimensions);
            position = finalPos;
        }

        //after the item gets picked up, it gets replaced with the item the player dropped
        public Item assignItem(Item player_item, GameTime currentTime)
        {
            GlobalGameConstants.itemType oldItem = item_type;
            item_type = player_item.ItemType();

            switch (oldItem)
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

        public override void draw(Spine.SkeletonRenderer sb)
        {
            AnimationLib.FrameAnimationSet dropAnim = null;

            dropAnim = GlobalGameConstants.WeaponDictionary.weaponInfo[(int)item_type].pickupImage;

            if (dropAnim != null)
            {
                dropAnim.drawAnimationFrame(0.0f, sb, position, new Vector2(1), 0.5f, 0.0f, Vector2.Zero, Color.White);
            }
            else
            {
                //sb.Draw(Game1.whitePixel, position, null, Color.Black, 0.0f, Vector2.Zero, dimensions, SpriteEffects.None, 0.5f);
            }
        }
        public override void knockBack(Vector2 direction, float magnitude, int damage, Entity attacker)
        {
            if (isKnockedBack)
            {
                return;
            }
            else
            {
                direction.Normalize();

                isKnockedBack = true;
                knockedBackTime = 0.0f;
                velocity = direction * knockBackSpeed;
            }
        }
    }
}
