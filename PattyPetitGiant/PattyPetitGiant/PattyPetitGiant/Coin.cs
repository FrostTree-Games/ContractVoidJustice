﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace PattyPetitGiant
{
    public class Coin : Entity
    {
        public enum DropItemType
        {
            CoinDrop = 0,
            MedDrop = 1,
            AmmoDrop = 2,
        }

        public enum AmmoValue
        {
            smallAmmo = 15,
            mediumAmmo = 30,
            largeAmmo = 50,
            fullAmmo = 100,
        }

        private AmmoValue ammo_value = AmmoValue.smallAmmo;
        public AmmoValue ammoValue
        {
            get { return ammo_value; }
        }

        public enum MedValue
        {
            smallPack = 15,
            mediumPack = 30,
            largePack = 50,
            fullPack = 100,
        }

        private MedValue med_value = MedValue.smallPack;
        public MedValue medValue
        {
            get { return med_value; }
        }

        public enum DropState
        {
            Active,
            Inactive,
        }

        public enum CoinValue
        {
            Loonie = 1,
            Twoonie = 2,
            Laurier = 5,
            MacDonald = 10,
            Elizabeth = 20,
            Mackenzie = 50,
            Borden = 100,
        }

        private DropItemType dropItem;
        public DropItemType DropItem { get { return dropItem; } }

        private DropState state;
        public DropState State { get { return state; } }

        private CoinValue value = CoinValue.Loonie;
        public CoinValue Value { get { return value; } }

        private AnimationLib.FrameAnimationSet coinAnim = null;
        private AnimationLib.FrameAnimationSet medAnim = null;
        private AnimationLib.FrameAnimationSet ammoAnim = null;
        private float animationTime;

        private float image_size = 0.0f;
        private bool isKnockedBack = false;
        private const float knockBackSpeed = 0.4f;
        private float knockedBackTime = 0.0f;
        private const float knockBackDuration = 250f;

        public Coin(LevelState parentWorld, Vector2 position)
        {
            this.position = position;
            this.parentWorld = parentWorld;

            dimensions = new Vector2(24, 24);

            dropItem = DropItemType.CoinDrop;

            state = DropState.Inactive;

            coinAnim = AnimationLib.getFrameAnimationSet("testCoin");
            medAnim = AnimationLib.getFrameAnimationSet("itemHealth");
            ammoAnim = AnimationLib.getFrameAnimationSet("itemBattery");
            animationTime = 0.0f;

            isKnockedBack = false;
        }

        public override void update(GameTime currentTime)
        {
            switch(dropItem)
            {
                case DropItemType.CoinDrop:
                    if (state == DropState.Active)
                    {
                        animationTime += currentTime.ElapsedGameTime.Milliseconds;

                        for (int i = 0; i < parentWorld.EntityList.Count; i++)
                        {
                            if (parentWorld.EntityList[i] is Player)
                            {
                                if (hitTest(parentWorld.EntityList[i]))
                                {
                                    GameCampaign.Player_Coin_Amount = GameCampaign.Player_Coin_Amount + (int)value;
                                    LevelState.ElapsedCoinAmount += (int)value;

                                    AudioLib.playSoundEffect("testCoin");

                                    state = DropState.Inactive;
                                }
                            }
                        }

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
                    else
                    {
                        position = new Vector2(-100, -100);
                    }
                break;
                case DropItemType.AmmoDrop:
                if (state == DropState.Active)
                {
                    for (int i = 0; i < parentWorld.EntityList.Count; i++)
                    {
                        if (parentWorld.EntityList[i] is Player)
                        {
                            InputDevice2.PPG_Player playerIndex = ((Player)parentWorld.EntityList[i]).Index;

                            if (hitTest(parentWorld.EntityList[i]))
                            {
                                AudioLib.playSoundEffect("ammoPickup");
                                if (ammo_value == AmmoValue.fullAmmo)
                                {
                                    if (playerIndex == InputDevice2.PPG_Player.Player_1)
                                    {
                                        GameCampaign.Player_Ammunition = 100;
                                    }
                                    else
                                    {
                                        GameCampaign.Player2_Ammunition = 100;
                                    }
                                }
                                else
                                {
                                    if (playerIndex == InputDevice2.PPG_Player.Player_1)
                                    {
                                        float ammo = GameCampaign.Player_Ammunition + (int)ammo_value;
                                        if (ammo > 100)
                                        {
                                            GameCampaign.Player_Ammunition = 100;
                                        }
                                        else
                                        {
                                            GameCampaign.Player_Ammunition += (int)ammo_value;
                                        }
                                    }
                                    else
                                    {
                                        float ammo = GameCampaign.Player_Ammunition + (int)ammo_value;
                                        if (ammo > 100)
                                        {
                                            GameCampaign.Player2_Ammunition = 100;
                                        }
                                        else
                                        {
                                            GameCampaign.Player2_Ammunition += (int)ammo_value;
                                        }
                                    }
                                }
                                state = DropState.Inactive;
                            }
                        }
                    }

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
                else
                {
                    position = new Vector2(-100, -100);
                }
                break;
                case DropItemType.MedDrop:
                if (state == DropState.Active)
                {
                    for (int i = 0; i < parentWorld.EntityList.Count; i++)
                    {
                        if (parentWorld.EntityList[i] is Player)
                        {
                            if (hitTest(parentWorld.EntityList[i]))
                            {
                                InputDevice2.PPG_Player playerIndex = ((Player)parentWorld.EntityList[i]).Index;
                                AudioLib.playSoundEffect("healthSound");
                                if (med_value == MedValue.fullPack)
                                {
                                    if (((Player)parentWorld.EntityList[i]).Index == InputDevice2.PPG_Player.Player_1)
                                    {
                                        GameCampaign.Player_Health = 100;
                                    }
                                    else
                                    {
                                        GameCampaign.Player2_Health = 100;
                                    }
                                }
                                else
                                {
                                    if (((Player)parentWorld.EntityList[i]).Index == InputDevice2.PPG_Player.Player_1)
                                    {
                                        float health = GameCampaign.Player_Health + (int)med_value;
                                        if (health > 100)
                                        {
                                            GameCampaign.Player_Health = 100;
                                        }
                                        else
                                        {
                                            GameCampaign.Player_Health += (int)med_value;
                                        }
                                    }
                                    else
                                    {
                                        float health = GameCampaign.Player2_Health + (int)med_value;
                                        if (health > 100)
                                        {
                                            GameCampaign.Player2_Health = 100;
                                        }
                                        else
                                        {
                                            GameCampaign.Player2_Health += (int)med_value;
                                        }
                                    }
                                    
                                }
                                state = DropState.Inactive;
                            }
                        }
                    }

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
                else
                {
                    position = new Vector2(-100, -100);
                }
                break;
                default:
                throw new System.InvalidOperationException("invalid DropItem state");
                }
        }

        public override void draw(Spine.SkeletonRenderer sb)
        {
            if (state == DropState.Active)
            {
                if (dropItem == DropItemType.CoinDrop)
                {
                    coinAnim.drawAnimationFrame(animationTime, sb, this.position, new Vector2(image_size), 0.5f, 0.0f, Vector2.Zero, Color.White);
                }
                else if (dropItem == DropItemType.MedDrop)
                {
                    medAnim.drawAnimationFrame(animationTime, sb, this.position, new Vector2(image_size), 0.5f, 0.0f, Vector2.Zero, Color.White);
                }
                else if (dropItem == DropItemType.AmmoDrop)
                {
                    ammoAnim.drawAnimationFrame(animationTime, sb, this.position, new Vector2(image_size), 0.5f, 0.0f, Vector2.Zero, Color.White);
                }
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
        
        public void activate(Vector2 position, DropItemType drop_type, int drop_value)
        {
            if (state == DropState.Active)
            {
                return;
            }

            state = DropState.Active;

            this.position = position;

            if (drop_type == DropItemType.CoinDrop)
            {
                dropItem = DropItemType.CoinDrop;

                if (Enum.IsDefined(typeof(CoinValue), drop_value))
                {
                    this.value = (CoinValue)drop_value;
                }
                else
                {
                    throw new System.InvalidOperationException("value is not specified in Coin Value");
                }

                isKnockedBack = false;
                float randDir = (float)(Game1.rand.NextDouble() * 3.14 * 2);
                knockBack(new Vector2((float)Math.Cos(randDir), (float)Math.Sin(randDir)), 0.0f, 0);

                switch (value)
                {
                    case CoinValue.Loonie:
                        image_size = 0.75f;
                        break;
                    case CoinValue.Twoonie:
                        image_size = 0.75f;
                        break;
                    case CoinValue.Laurier:
                        image_size = 0.75f;
                        break;
                    case CoinValue.MacDonald:
                        image_size = 0.90f;
                        break;
                    case CoinValue.Elizabeth:
                        image_size = 0.90f;
                        break;
                    case CoinValue.Mackenzie:
                        image_size = 0.90f;
                        break;
                    case CoinValue.Borden:
                        image_size = 1.0f;
                        break;
                    default:
                        image_size = 0.75f;
                        value = CoinValue.Loonie;
                        break;
                }
            }
            else if (drop_type == DropItemType.MedDrop)
            {
                dropItem = DropItemType.MedDrop;

                if (Enum.IsDefined(typeof(MedValue), drop_value))
                {
                    this.med_value = (MedValue)drop_value;
                }
                else
                {
                    throw new System.InvalidOperationException("value is not specified in Coin Value");
                }

                isKnockedBack = false;
                float randDir = (float)(Game1.rand.NextDouble() * 3.14 * 2);
                knockBack(new Vector2((float)Math.Cos(randDir), (float)Math.Sin(randDir)), 0.0f, 0);

                switch (med_value)
                {
                    case MedValue.smallPack:
                        image_size = 0.65f;
                        break;
                    case MedValue.mediumPack:
                        image_size = 0.80f;
                        break;
                    case MedValue.largePack:
                        image_size = 0.90f;
                        break;
                    case MedValue.fullPack:
                        image_size = 1.0f;
                        break;
                    default:
                        image_size = 0.65f;
                        med_value = MedValue.smallPack;
                        break;
                }

            }
            else if (drop_type == DropItemType.AmmoDrop)
            {
                dropItem = DropItemType.AmmoDrop;

                if (Enum.IsDefined(typeof(AmmoValue), drop_value))
                {
                    this.ammo_value = (AmmoValue)drop_value;
                }
                else
                {
                    throw new System.InvalidOperationException("value is not specified in Coin Value");
                }

                isKnockedBack = false;
                float randDir = (float)(Game1.rand.NextDouble() * 3.14 * 2);
                knockBack(new Vector2((float)Math.Cos(randDir), (float)Math.Sin(randDir)), 0.0f, 0);
                switch (ammo_value)
                {
                    case AmmoValue.smallAmmo:
                        image_size = 0.65f;
                        break;
                    case AmmoValue.mediumAmmo:
                        image_size = 0.80f;
                        break;
                    case AmmoValue.largeAmmo:
                        image_size = 1.0f;
                        break;
                    case AmmoValue.fullAmmo:
                        image_size = 1.0f;
                        break;
                    default:
                        image_size = 0.65f;
                        med_value = MedValue.smallPack;
                        break;
                }
            }
            else
            {
                throw new System.InvalidOperationException("not a valid dropItem");
            }
        }
    }
}
