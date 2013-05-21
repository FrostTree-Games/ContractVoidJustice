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

        public ShopKeeper(LevelState parentWorld, Vector2 position)
        {
            this.parentWorld = parentWorld;
            this.position = position;
            this.dimensions = GlobalGameConstants.TileSize;

            state = ShopKeeperState.Normal;

            switchItemPressed = false;

            thankYouMessage = new InGameGUI.BoxWindow("thankYou", 0, 0, 150, "Thank you!");
            thankYouTime = -1;

            shopKeeperFrameAnimation = AnimationLib.getFrameAnimationSet("shopKeeper");

            //test shop data for now
            {
                itemPrices[0] = 30;
                itemPrices[1] = 100;
                itemPrices[2] = 210;

                itemsForSale[0] = GlobalGameConstants.itemType.Bomb;
                itemsForSale[1] = GlobalGameConstants.itemType.Compass;
                itemsForSale[2] = GlobalGameConstants.itemType.Gun;

                itemMessages[0] = new InGameGUI.BoxWindow("shopkeeperMessage", GlobalGameConstants.GameResolutionWidth / 2 - 300, GlobalGameConstants.GameResolutionHeight / 2, 300, "Bombs will destroy enemies in a radius, but watch out! They're dangerous!");
                itemMessages[1] = new InGameGUI.BoxWindow("shopkeeperMessage", GlobalGameConstants.GameResolutionWidth / 2 - 300, GlobalGameConstants.GameResolutionHeight / 2, 300, "The compass will point in the direction of the level's exit.");
                itemMessages[2] = new InGameGUI.BoxWindow("shopkeeperMessage", GlobalGameConstants.GameResolutionWidth / 2 - 300, GlobalGameConstants.GameResolutionHeight / 2, 300, "Fires a projectile that will damage enemies. Kind of slow and short-ranged.");
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
            GlobalGameConstants.Player_Coin_Amount -= goldValue;
        }

        public override void update(GameTime currentTime)
        {
            foreach (Entity en in parentWorld.EntityList)
            {
                if (en is Player)
                {
                    if (distance(en.Position, position) < GlobalGameConstants.TileSize.X * GlobalGameConstants.TilesPerRoomHigh/2)
                    {
                        playerOverlap = false;

                        for (int i = 0; i < 3; i++)
                        {
                            Vector2 drawItemPos = position + new Vector2((-2 * GlobalGameConstants.TileSize.X) + (i * 2f * GlobalGameConstants.TileSize.X), (2.5f * GlobalGameConstants.TileSize.Y));

                            if (distance(drawItemPos + GlobalGameConstants.TileSize/2, en.CenterPoint) < 32 && itemsForSale[i] != GlobalGameConstants.itemType.NoItem)
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
                                else if (itemPrices[i] > GlobalGameConstants.Player_Coin_Amount)
                                {
                                    overlapMessage = noWayMessage;
                                }
                                else
                                {
                                    overlapMessage = buyMessage;
                                }

                                if (switchItemPressed && !(InputDeviceManager.isButtonDown(InputDeviceManager.PlayerButton.SwitchItem1) || InputDeviceManager.isButtonDown(InputDeviceManager.PlayerButton.SwitchItem2)) && GlobalGameConstants.Player_Coin_Amount >= itemPrices[i])
                                {
                                    items[i].Position = drawItemPos;

                                    purchaseTransaction(itemPrices[i]);
                                    itemsForSale[i] = GlobalGameConstants.itemType.NoItem;

                                    if (!parentWorld.GUI.peekBox("thankYou"))
                                    {
                                        thankYouMessage.x = GlobalGameConstants.GameResolutionWidth / 2 - 75;
                                        thankYouMessage.y = GlobalGameConstants.GameResolutionHeight / 4 - thankYouMessage.height/2;
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

        public override void draw(SpriteBatch sb)
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

        public override void knockBack(Entity other, Vector2 position, Vector2 dimensions, int damage)
        {
            return;
        }
    }
}
