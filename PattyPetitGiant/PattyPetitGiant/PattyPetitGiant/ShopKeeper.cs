using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PattyPetitGiant
{
    class ShopKeeper : Entity
    {
        /// <summary>
        /// State enum used to determine shopkeeper's behaviour.
        /// </summary>
        private enum ShopKeeperState
        {
            InvalidState = -1,
            Normal = 0,
            Enraged = 1,
        }

        /// <summary>
        /// Structure for the Shopkeeper's projectiles
        /// </summary>
        private struct FireBall
        {
            public bool active;
            public Vector2 position;
            public Vector2 startPosition;
            public Vector2 hitbox;
            public float direction;
            public float timeAlive;

            public const float fireBallVelocity = 7.5f;
            public const float fireBallDurationTime = 500f;

            public FireBall(Vector2 position, float direction)
            {
                this.position = position;
                this.startPosition = position;
                hitbox = GlobalGameConstants.TileSize;
                this.direction = direction;
                timeAlive = 0.0f;
                active = true;
            }

            public Vector2 center { get { return position + (hitbox / 2); } }
        }

        private int health;
        public int Health { get { return health; } }

        private ShopKeeperState state = ShopKeeperState.Normal;
        private GlobalGameConstants.itemType[] itemsForSale = new GlobalGameConstants.itemType[3];
        private int[] itemPrices = new int[3];
        private AnimationLib.FrameAnimationSet[] itemIcons = new AnimationLib.FrameAnimationSet[3];
        private InGameGUI.BoxWindow[] itemMessages = new InGameGUI.BoxWindow[3];
        private Pickup[] items = new Pickup[3];

        private AnimationLib.FrameAnimationSet shopKeeperFrameAnimation = null;
        private InGameGUI.BoxWindow thankYouMessage;
        private float thankYouTime;
        private const float thankYouDuration = 3000;

        private string buyMessage = "Buy";
        private string noWayMessage = "Not enough money!";
        private string soldOutMessage = "Sold out!";

        private bool playerOverlap = false;
        private int overlapIndex = 0;
        private string overlapMessage = null;
        private Vector2 buyLocation = Vector2.Zero;

        private bool switchItemPressed;

        private Entity attacker = null;
        private FireBall projectile;
        private float fireballDelayPassed;
        private const float fireballDelay = 400f;
        private Vector2 attackPoint;
        private const float attackPointSetDelay = 100f;
        private const float distanceToMaintainFromAttacker = 250f;
        private const int fireballDamage = 1;

        public ShopKeeper(LevelState parentWorld, Vector2 position)
        {
            this.parentWorld = parentWorld;
            this.position = position;
            this.velocity = Vector2.Zero;
            this.dimensions = GlobalGameConstants.TileSize;

            state = ShopKeeperState.Normal;

            health = 25;
            projectile = new FireBall(new Vector2(-10, -10), 0.0f);
            projectile.active = false;
            fireballDelayPassed = 0.0f;

            switchItemPressed = false;

            thankYouMessage = new InGameGUI.BoxWindow("thankYou", 0, 0, 150, "Thank you!");
            thankYouTime = -1;

            shopKeeperFrameAnimation = AnimationLib.getFrameAnimationSet("shopKeeper");

            //test shop data for now
            {
                itemPrices[0] = 170;
                itemPrices[1] = 30;
                itemPrices[2] = 200;

                itemsForSale[0] = GlobalGameConstants.itemType.Bomb;
                itemsForSale[1] = GlobalGameConstants.itemType.Compass;
                itemsForSale[2] = GlobalGameConstants.itemType.WandOfGyges;

                itemMessages[0] = new InGameGUI.BoxWindow("shopkeeperMessage", GlobalGameConstants.GameResolutionWidth / 2 - 300, GlobalGameConstants.GameResolutionHeight / 2, 300, "Bombs will destroy enemies in a radius, but watch out! They're dangerous!");
                itemMessages[1] = new InGameGUI.BoxWindow("shopkeeperMessage", GlobalGameConstants.GameResolutionWidth / 2 - 300, GlobalGameConstants.GameResolutionHeight / 2, 300, "The compass will point in the direction of the level's exit.");
                itemMessages[2] = new InGameGUI.BoxWindow("shopkeeperMessage", GlobalGameConstants.GameResolutionWidth / 2 - 300, GlobalGameConstants.GameResolutionHeight / 2, 300, "A magical wand that seems to let its weilder bend space and location.");
            }

            items[0] = new Pickup(parentWorld, -500, -500, itemsForSale[0]);
            items[1] = new Pickup(parentWorld, -500, -500, itemsForSale[1]);
            items[2] = new Pickup(parentWorld, -500, -500, itemsForSale[2]);
            parentWorld.EntityList.AddRange(items);

            getItemIcons();
        }

        private void getItemIcons()
        {
            for (int i = 0; i < 3; i++)
            {
                switch (itemsForSale[i])
                {
                    case GlobalGameConstants.itemType.Gun:
                        itemIcons[i] = AnimationLib.getFrameAnimationSet("gunPic");
                        break;
                    case GlobalGameConstants.itemType.Sword:
                        itemIcons[i] = AnimationLib.getFrameAnimationSet("swordPic");
                        break;
                    case GlobalGameConstants.itemType.Bomb:
                        itemIcons[i] = AnimationLib.getFrameAnimationSet("bombPic");
                        break;
                    case GlobalGameConstants.itemType.Compass:
                        itemIcons[i] = AnimationLib.getFrameAnimationSet("compassPic");
                        break;
                    case GlobalGameConstants.itemType.WandOfGyges:
                        itemIcons[i] = AnimationLib.getFrameAnimationSet("wandPic");
                        break;
                    case GlobalGameConstants.itemType.NoItem:
                    default:
                        itemIcons[i] = AnimationLib.getFrameAnimationSet("flagPic");
                        break;
                }
            }
        }

        private double distance(Vector2 a, Vector2 b)
        {
            return Math.Sqrt(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2));
        }

        private void purchaseTransaction(int goldValue)
        {
            GameCampaign.Player_Coin_Amount -= goldValue;
        }

        public override void update(GameTime currentTime)
        {
            if (health <= 0)
            {
                parentWorld.GUI.popBox("thankYou");
                remove_from_list = true;
                return;
            }

            if (state == ShopKeeperState.Enraged)
            {
                if (attacker != null)
                {
                    double angle = Math.Atan2(attacker.Position.Y - position.Y, attacker.Position.X - position.X);

                    if (Vector2.Distance(CenterPoint, attacker.CenterPoint) - distanceToMaintainFromAttacker > GlobalGameConstants.TileSize.Y)
                    {
                        velocity = FireBall.fireBallVelocity / 4 * new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));

                        velocity += FireBall.fireBallVelocity / 8 * new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
                    }
                    else if (Vector2.Distance(CenterPoint, attacker.CenterPoint) - distanceToMaintainFromAttacker < -1 * GlobalGameConstants.TileSize.Y)
                    {
                        velocity = -1 * FireBall.fireBallVelocity / 4 * new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
                    }
                }
                if (projectile.active)
                {
                    projectile.position += FireBall.fireBallVelocity * new Vector2((float)Math.Cos(projectile.direction), (float)Math.Sin(projectile.direction));
                    projectile.timeAlive += currentTime.ElapsedGameTime.Milliseconds;

                    if (projectile.timeAlive > FireBall.fireBallDurationTime || parentWorld.Map.hitTestWall(projectile.center))
                    {
                        projectile.active = false;
                        fireballDelayPassed = 0.0f;
                        attackPoint = new Vector2(-1, -1);
                    }

                    foreach (Entity en in parentWorld.EntityList)
                    {
                        if (en == this)
                        {
                            continue;
                        }

                        if (Vector2.Distance(projectile.center, en.CenterPoint) < GlobalGameConstants.TileSize.X)
                        {
                            en.knockBack(new Vector2((float)Math.Cos(projectile.direction), (float)Math.Sin(projectile.direction)), 4.0f, 10);
                            projectile.active = false;
                            fireballDelayPassed = -200f;
                            attackPoint = new Vector2(-1, -1);
                        }
                    }
                }
                else
                {
                    fireballDelayPassed += currentTime.ElapsedGameTime.Milliseconds;

                    if (attacker != null)
                    {
                        if (fireballDelayPassed > fireballDelay && attackPoint != new Vector2(-1, -1))
                        {
                            projectile = new FireBall(position, (float)Math.Atan2(attackPoint.Y - position.Y, attackPoint.X - position.X));
                        }
                        else if (fireballDelayPassed > attackPointSetDelay && attackPoint == new Vector2(-1, -1))
                        {
                            attackPoint = attacker.CenterPoint;
                        }
                    }
                }
            }
            else if (state == ShopKeeperState.Normal)
            {
                foreach (Entity en in parentWorld.EntityList)
                {
                    if (en is Player)
                    {
                        if (distance(en.Position, position) < GlobalGameConstants.TileSize.X * GlobalGameConstants.TilesPerRoomHigh / 2)
                        {
                            playerOverlap = false;

                            for (int i = 0; i < 3; i++)
                            {
                                Vector2 drawItemPos = position + new Vector2((-2 * GlobalGameConstants.TileSize.X) + (i * 2f * GlobalGameConstants.TileSize.X), (2.5f * GlobalGameConstants.TileSize.Y));

                                if (distance(drawItemPos + GlobalGameConstants.TileSize / 2, en.CenterPoint) < 32 && itemsForSale[i] != GlobalGameConstants.itemType.NoItem)
                                {
                                    playerOverlap = true;
                                    buyLocation = en.Position - new Vector2(0, 32);
                                    overlapIndex = i;
                                    if (!parentWorld.GUI.peekBox("shopkeeperMessage"))
                                    {
                                        parentWorld.GUI.pushBox(itemMessages[i]);
                                    }

                                    if (itemsForSale[i] == GlobalGameConstants.itemType.NoItem)
                                    {
                                        overlapMessage = soldOutMessage;
                                    }
                                    else if (itemPrices[i] > GameCampaign.Player_Coin_Amount)
                                    {
                                        overlapMessage = noWayMessage;
                                    }
                                    else
                                    {
                                        overlapMessage = buyMessage;
                                    }

                                    if (switchItemPressed && !(InputDeviceManager.isButtonDown(InputDeviceManager.PlayerButton.SwitchItem1) || InputDeviceManager.isButtonDown(InputDeviceManager.PlayerButton.SwitchItem2)) && GameCampaign.Player_Coin_Amount >= itemPrices[i])
                                    {
                                        items[i].Position = drawItemPos;

                                        purchaseTransaction(itemPrices[i]);
                                        itemsForSale[i] = GlobalGameConstants.itemType.NoItem;

                                        if (!parentWorld.GUI.peekBox("thankYou"))
                                        {
                                            thankYouMessage.x = GlobalGameConstants.GameResolutionWidth / 2 - 75;
                                            thankYouMessage.y = GlobalGameConstants.GameResolutionHeight / 4 - thankYouMessage.height / 2;
                                            thankYouTime = 0;
                                            parentWorld.GUI.pushBox(thankYouMessage);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if ((InputDeviceManager.isButtonDown(InputDeviceManager.PlayerButton.SwitchItem1) || InputDeviceManager.isButtonDown(InputDeviceManager.PlayerButton.SwitchItem2)) && !switchItemPressed)
                {
                    switchItemPressed = true;
                }
                else if (!(InputDeviceManager.isButtonDown(InputDeviceManager.PlayerButton.SwitchItem1) || InputDeviceManager.isButtonDown(InputDeviceManager.PlayerButton.SwitchItem2)) && switchItemPressed)
                {
                    switchItemPressed = false;
                }

                if (thankYouTime >= 0)
                {
                    thankYouTime += currentTime.ElapsedGameTime.Milliseconds;

                    if (thankYouTime > thankYouDuration)
                    {
                        thankYouTime = -1;
                        parentWorld.GUI.popBox("thankYou");
                    }
                }

                if (!playerOverlap && parentWorld.GUI.peekBox("shopkeeperMessage"))
                {
                    parentWorld.GUI.popBox("shopkeeperMessage");
                }
            }
            else if (state == ShopKeeperState.InvalidState)
            {
                throw new Exception("Shopkeeper was thrown into an invalid state: " + position.X + "," + position.Y);
            }

            Vector2 step = position + velocity;
            Vector2 finalPos = parentWorld.Map.reloactePosition(position, step, dimensions);

            position = finalPos;
        }

        public override void draw(SpriteBatch sb)
        {
            if (state == ShopKeeperState.Normal)
            {
                shopKeeperFrameAnimation.drawAnimationFrame(0.0f, sb, position, new Vector2(3, 3), 0.5f);

                for (int i = 0; i < 3; i++)
                {
                    if (itemsForSale[i] == GlobalGameConstants.itemType.NoItem)
                    {
                        continue;
                    }

                    Vector2 drawItemPos = position + new Vector2((-2 * GlobalGameConstants.TileSize.X) + (i * 2f * GlobalGameConstants.TileSize.X), (2.5f * GlobalGameConstants.TileSize.Y));

                    itemIcons[i].drawAnimationFrame(0.0f, sb, drawItemPos, new Vector2(1.0f, 1.0f), 0.5f);
                    sb.DrawString(Game1.font, itemPrices[i].ToString(), drawItemPos + new Vector2(0f, GlobalGameConstants.TileSize.Y), Color.Yellow, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.5f);
                }

                if (playerOverlap)
                {
                    sb.DrawString(Game1.font, overlapMessage, buyLocation, Color.Red, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.6f);
                }
            }
            else if (state == ShopKeeperState.Enraged)
            {
                shopKeeperFrameAnimation.drawAnimationFrame(0.0f, sb, position, new Vector2(3, 3), 0.5f, Color.Red);
            }

            if (projectile.active)
            {
                shopKeeperFrameAnimation.drawAnimationFrame(0.0f, sb, projectile.position, new Vector2(3, 3), 0.51f, Color.Yellow);
            }
        }

        public override void knockBack(Vector2 direction, float magnitude, int damage, Entity attacker)
        {
            if (attacker == null)
            {
                return;
            }

            health -= damage;

            if (state == ShopKeeperState.Normal)
            {
                for (int i = 0; i < items.Length; i++)
                {
                    if (itemsForSale[i] == GlobalGameConstants.itemType.NoItem)
                    {
                        continue;
                    }

                    Vector2 drawItemPos = this.position + new Vector2((-2 * GlobalGameConstants.TileSize.X) + (i * 2f * GlobalGameConstants.TileSize.X), (2.5f * GlobalGameConstants.TileSize.Y));

                    items[i].Position = drawItemPos;
                }

                foreach (Entity en in parentWorld.EntityList)
                {
                    if (!(en is Player || en is Enemy))
                    {
                        continue;
                    }

                    if (attacker == null)
                    {
                        attacker = en;
                        continue;
                    }

                    if (Vector2.Distance(CenterPoint, en.CenterPoint) < Vector2.Distance(CenterPoint, attacker.CenterPoint))
                    {
                        attacker = en;
                    }
                }

                state = ShopKeeperState.Enraged;
            }
            else if (state == ShopKeeperState.Enraged)
            {
                //
            }
        }

        /// <summary>
        /// Enrages the shopkeeper.
        /// </summary>
        public void poke()
        {
            if (state == ShopKeeperState.Enraged)
            {
                return;
            }

            for (int i = 0; i < items.Length; i++)
            {
                if (itemsForSale[i] == GlobalGameConstants.itemType.NoItem)
                {
                    continue;
                }

                Vector2 drawItemPos = this.position + new Vector2((-2 * GlobalGameConstants.TileSize.X) + (i * 2f * GlobalGameConstants.TileSize.X), (2.5f * GlobalGameConstants.TileSize.Y));

                items[i].Position = drawItemPos;
            }

            foreach (Entity en in parentWorld.EntityList)
            {
                if (!(en is Player || en is Enemy))
                {
                    continue;
                }

                if (attacker == null)
                {
                    attacker = en;
                    continue;
                }

                if (Vector2.Distance(CenterPoint, en.CenterPoint) < Vector2.Distance(CenterPoint, attacker.CenterPoint))
                {
                    attacker = en;
                }
            }

            state = ShopKeeperState.Enraged;
        }
    }
}
