using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PattyPetitGiant
{
    class ShopKeeper : Entity, SpineEntity
    {
        /// <summary>
        /// State enum used to determine shopkeeper's behaviour.
        /// </summary>
        private enum ShopKeeperState
        {
            InvalidState = -1,
            Normal = 0,
            Enraged = 1,
            KnockBack = 2,
            Dying = 3,
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
        private Pickup[] items = new Pickup[3];

        private AnimationLib.FrameAnimationSet shopKeeperFrameAnimationTest = null;
        private AnimationLib.SpineAnimationSet[] directionAnims = null;
        private AnimationLib.FrameAnimationSet buyPic = null;
        private AnimationLib.FrameAnimationSet leftBuyButton = null;
        private AnimationLib.FrameAnimationSet rightBuyButton = null;

        private float animationTime = 0;

        private bool playerOverlap = false;
        private int overlapIndex = 0;
        private Vector2 buyLocation = Vector2.Zero;

        private bool switchItemPressed;

        private Entity attackerTarget = null;
        private FireBall projectile;
        private float fireballDelayPassed;
        private const float fireballDelay = 400f;
        private Vector2 attackPoint;
        private const float attackPointSetDelay = 100f;
        private const float distanceToMaintainFromAttacker = 250f;
        private const int fireballDamage = 1;

        private float knockBackTimer;
        private const float knockBackDuration = 500f;

        private bool windingUp = false;

        public ShopKeeper(LevelState parentWorld, Vector2 position)
        {
            this.parentWorld = parentWorld;
            this.position = position;
            this.velocity = Vector2.Zero;
            this.dimensions = GlobalGameConstants.TileSize;

            this.direction_facing = GlobalGameConstants.Direction.Down;

            state = ShopKeeperState.Normal;

            health = 40;
            projectile = new FireBall(new Vector2(-10, -10), 0.0f);
            projectile.active = false;
            fireballDelayPassed = 0.0f;

            switchItemPressed = false;

            shopKeeperFrameAnimationTest = AnimationLib.getFrameAnimationSet("shopKeeper");
            buyPic = AnimationLib.getFrameAnimationSet("buyPic");
            leftBuyButton = AnimationLib.getFrameAnimationSet("gamepadLB");
            rightBuyButton = AnimationLib.getFrameAnimationSet("gamepadRB");

            //test shop data for now
            {
                itemsForSale[0] = GlobalGameConstants.itemType.HermesSandals;
                itemsForSale[1] = GlobalGameConstants.itemType.ShotGun;
                itemsForSale[2] = GlobalGameConstants.itemType.WandOfGyges;

                for (int i = 0; i < 3; i++)
                {
                    itemPrices[i] = GlobalGameConstants.WeaponDictionary.weaponInfo[(int)itemsForSale[i]].price;
                    itemIcons[i] = GlobalGameConstants.WeaponDictionary.weaponInfo[(int)itemsForSale[i]].pickupImage;
                }
            }

            items[0] = new Pickup(parentWorld, new Vector2(-500, -500), itemsForSale[0]);
            items[1] = new Pickup(parentWorld, new Vector2(-500, -500), itemsForSale[1]);
            items[2] = new Pickup(parentWorld, new Vector2(-500, -500), itemsForSale[2]);
            parentWorld.EntityList.AddRange(items);

            directionAnims = new AnimationLib.SpineAnimationSet[4];
            directionAnims[(int)GlobalGameConstants.Direction.Up] = AnimationLib.loadNewAnimationSet("shopUp");
            directionAnims[(int)GlobalGameConstants.Direction.Down] = AnimationLib.loadNewAnimationSet("shopDown");
            directionAnims[(int)GlobalGameConstants.Direction.Left] = AnimationLib.loadNewAnimationSet("shopRight");
            directionAnims[(int)GlobalGameConstants.Direction.Left].Skeleton.FlipX = true;
            directionAnims[(int)GlobalGameConstants.Direction.Right] = AnimationLib.loadNewAnimationSet("shopRight");
            for (int i = 0; i < 4; i++)
            {
                directionAnims[i].Animation = directionAnims[i].Skeleton.Data.FindAnimation("idle");
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
            animationTime += currentTime.ElapsedGameTime.Milliseconds / 1000f;

            if (health <= 0 && state != ShopKeeperState.Dying)
            {
                state = ShopKeeperState.Dying;
                animationTime = 0;
                projectile.active = false;

                this.death = true;

                directionAnims[(int)direction_facing].Animation = directionAnims[(int)direction_facing].Skeleton.Data.FindAnimation(Game1.rand.Next() % 3 == 0 ? "die" : Game1.rand.Next() % 2 == 0 ? "die2" : "die3");
            }

            if (state == ShopKeeperState.Dying)
            {
                windingUp = false;

                velocity = Vector2.Zero;
            }
            else if (state == ShopKeeperState.Enraged)
            {
                if (!windingUp && fireballDelayPassed > fireballDelay - 150f)
                {
                    animationTime = 0;
                    directionAnims[(int)direction_facing].Animation = directionAnims[(int)direction_facing].Skeleton.Data.FindAnimation("attack");
                    windingUp = true;
                }
                else if (windingUp)
                {
                    directionAnims[(int)direction_facing].Animation = directionAnims[(int)direction_facing].Skeleton.Data.FindAnimation("attack");
                }
                else if (!windingUp)
                {
                    directionAnims[(int)direction_facing].Animation = directionAnims[(int)direction_facing].Skeleton.Data.FindAnimation("chase");
                }

                if (attackerTarget != null)
                {
                    double theta = Math.Atan2(CenterPoint.Y - attackerTarget.CenterPoint.Y, CenterPoint.X - attackerTarget.CenterPoint.X);

                    if (theta > Math.PI / 4 && theta < (Math.PI) - (Math.PI / 4))
                    {
                        direction_facing = GlobalGameConstants.Direction.Up;
                    }
                    else if (theta > (Math.PI) - (Math.PI / 4) || theta < (-Math.PI) + (Math.PI / 4))
                    {
                        direction_facing = GlobalGameConstants.Direction.Right;
                    }
                    else if (theta < Math.PI / 4 && theta > Math.PI / -4)
                    {
                        direction_facing = GlobalGameConstants.Direction.Left;
                    }
                    else if (theta < Math.PI / -4 && theta > (-Math.PI) + (Math.PI / 4))
                    {
                        direction_facing = GlobalGameConstants.Direction.Down;
                    }

                    double angle = Math.Atan2(attackerTarget.Position.Y - position.Y, attackerTarget.Position.X - position.X);

                    if (Vector2.Distance(CenterPoint, attackerTarget.CenterPoint) - distanceToMaintainFromAttacker > GlobalGameConstants.TileSize.Y)
                    {
                        velocity = FireBall.fireBallVelocity / 4 * new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));

                        velocity += FireBall.fireBallVelocity / 8 * new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
                    }
                    else if (Vector2.Distance(CenterPoint, attackerTarget.CenterPoint) - distanceToMaintainFromAttacker < -1 * GlobalGameConstants.TileSize.Y)
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

                    for (int i = 0; i < parentWorld.EntityList.Count; i++)
                    {
                        if (parentWorld.EntityList[i] == this || parentWorld.EntityList[i] is Pickup)
                        {
                            continue;
                        }

                        if (Vector2.Distance(projectile.center, parentWorld.EntityList[i].CenterPoint) < GlobalGameConstants.TileSize.X)
                        {
                            parentWorld.EntityList[i].knockBack(new Vector2((float)Math.Cos(projectile.direction), (float)Math.Sin(projectile.direction)), 4.0f, 10);
                            projectile.active = false;
                            fireballDelayPassed = -200f;
                            attackPoint = new Vector2(-1, -1);
                        }
                    }
                }
                else
                {
                    fireballDelayPassed += currentTime.ElapsedGameTime.Milliseconds;

                    if (attackerTarget != null)
                    {
                        if (fireballDelayPassed > fireballDelay && attackPoint != new Vector2(-1, -1))
                        {
                            projectile = new FireBall(position, (float)Math.Atan2(attackPoint.Y - position.Y, attackPoint.X - position.X));
                            windingUp = false;
                            fireballDelayPassed = 0;
                        }
                        else if (fireballDelayPassed > attackPointSetDelay && attackPoint == new Vector2(-1, -1))
                        {
                            attackPoint = attackerTarget.CenterPoint;
                        }
                    }
                }
            }
            else if (state == ShopKeeperState.Normal)
            {
                directionAnims[(int)direction_facing].Animation = directionAnims[(int)direction_facing].Skeleton.Data.FindAnimation("idle");

                for (int it = 0; it < parentWorld.EntityList.Count; it++)
                {
                    if (parentWorld.EntityList[it] is Player)
                    {
                        if (distance(parentWorld.EntityList[it].Position, position) < GlobalGameConstants.TileSize.X * GlobalGameConstants.TilesPerRoomHigh / 2)
                        {
                            playerOverlap = false;

                            for (int i = 0; i < 3; i++)
                            {
                                Vector2 drawItemPos = position + new Vector2((-2 * GlobalGameConstants.TileSize.X) + (i * 2f * GlobalGameConstants.TileSize.X), (2.5f * GlobalGameConstants.TileSize.Y));

                                if (distance(drawItemPos + GlobalGameConstants.TileSize / 2, parentWorld.EntityList[it].CenterPoint) < 32 && itemsForSale[i] != GlobalGameConstants.itemType.NoItem)
                                {
                                    playerOverlap = true;
                                    buyLocation = parentWorld.EntityList[it].Position - new Vector2(0, 48);
                                    overlapIndex = i;

                                    if (switchItemPressed && !(InputDeviceManager.isButtonDown(InputDeviceManager.PlayerButton.SwitchItem1) || InputDeviceManager.isButtonDown(InputDeviceManager.PlayerButton.SwitchItem2)) && GameCampaign.Player_Coin_Amount >= itemPrices[i])
                                    {
                                        items[i].Position = drawItemPos;

                                        purchaseTransaction(itemPrices[i]);
                                        itemsForSale[i] = GlobalGameConstants.itemType.NoItem;
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
            }
            else if (state == ShopKeeperState.KnockBack)
            {
                directionAnims[(int)direction_facing].Animation = directionAnims[(int)direction_facing].Skeleton.Data.FindAnimation("hurt");

                knockBackTimer += currentTime.ElapsedGameTime.Milliseconds;

                if (knockBackTimer > knockBackDuration)
                {
                    state = ShopKeeperState.Enraged;
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

                    for (int i = 0; i < parentWorld.EntityList.Count; i++)
                    {
                        if (parentWorld.EntityList[i] == this)
                        {
                            continue;
                        }

                        if (Vector2.Distance(projectile.center, parentWorld.EntityList[i].CenterPoint) < GlobalGameConstants.TileSize.X)
                        {
                            parentWorld.EntityList[i].knockBack(new Vector2((float)Math.Cos(projectile.direction), (float)Math.Sin(projectile.direction)), 4.0f, 10);
                            projectile.active = false;
                            fireballDelayPassed = -200f;
                            attackPoint = new Vector2(-1, -1);
                        }
                    }
                }
            }
            else if (state == ShopKeeperState.InvalidState)
            {
                throw new Exception("Shopkeeper was thrown into an invalid state: " + position.X + "," + position.Y);
            }

            directionAnims[(int)direction_facing].Animation.Apply(directionAnims[(int)direction_facing].Skeleton, animationTime, state == ShopKeeperState.KnockBack || state == ShopKeeperState.Dying || windingUp ? false : true);

            Vector2 step = position + velocity;
            Vector2 finalPos = parentWorld.Map.reloactePosition(position, step, dimensions);

            position = finalPos;
        }

        public override void draw(Spine.SkeletonRenderer sb)
        {
            if (state == ShopKeeperState.Normal)
            {
                for (int i = 0; i < 3; i++)
                {
                    if (itemsForSale[i] == GlobalGameConstants.itemType.NoItem)
                    {
                        continue;
                    }

                    Vector2 drawItemPos = position + new Vector2((-2 * GlobalGameConstants.TileSize.X) + (i * 2f * GlobalGameConstants.TileSize.X), (2.5f * GlobalGameConstants.TileSize.Y));

                    itemIcons[i].drawAnimationFrame(0.0f, sb, drawItemPos, new Vector2(1.0f, 1.0f), 0.5f, 0.0f, Vector2.Zero, Color.White);
                }

                if (playerOverlap)
                {
                    buyPic.drawAnimationFrame(0.0f, sb, buyLocation, new Vector2(1), 0.5f, 0.0f, Vector2.Zero, Color.White);
                    leftBuyButton.drawAnimationFrame(0.0f, sb, buyLocation - new Vector2(58, 0) + (new Vector2(0, -0.1f) * (((float)Math.Sin(animationTime)) / 0.01f)), new Vector2(1), 0.5f, 0.0f, Vector2.Zero, Color.White);
                    rightBuyButton.drawAnimationFrame(0.0f, sb, buyLocation + new Vector2(70, 0) + (new Vector2(0, -0.1f) * (((float)Math.Sin(animationTime)) / 0.01f)), new Vector2(1), 0.5f, 0.0f, Vector2.Zero, Color.White);
                }
            }

            if (projectile.active)
            {
                shopKeeperFrameAnimationTest.drawAnimationFrame(0.0f, sb, projectile.position, new Vector2(3), 0.5f, 0.0f, Vector2.Zero, Color.Yellow);
            }
        }

        public override void knockBack(Vector2 direction, float magnitude, int damage, Entity attacker)
        {
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

                attackerTarget = attacker;

                knockBackTimer = 0;
                state = ShopKeeperState.KnockBack;
                velocity = 2 * Vector2.Normalize(direction);

                animationTime = 0;

                health -= damage;

                parentWorld.Particles.pushBloodParticle(CenterPoint);
                parentWorld.Particles.pushBloodParticle(CenterPoint);
                parentWorld.Particles.pushBloodParticle(CenterPoint);
            }
            else if (state == ShopKeeperState.Enraged)
            {
                attackerTarget = attacker;

                health -= damage;

                animationTime = 0;

                knockBackTimer = 0;
                state = ShopKeeperState.KnockBack;
                velocity = 2 * Vector2.Normalize(direction);

                parentWorld.Particles.pushBloodParticle(CenterPoint);
                parentWorld.Particles.pushBloodParticle(CenterPoint);
                parentWorld.Particles.pushBloodParticle(CenterPoint);
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

            for (int it = 0; it < parentWorld.EntityList.Count; it++)
            {
                if (!(parentWorld.EntityList[it] is Player || parentWorld.EntityList[it] is Enemy))
                {
                    continue;
                }

                if (attackerTarget == null)
                {
                    attackerTarget = parentWorld.EntityList[it];
                    continue;
                }

                if (Vector2.Distance(CenterPoint, parentWorld.EntityList[it].CenterPoint) < Vector2.Distance(CenterPoint, attackerTarget.CenterPoint))
                {
                    attackerTarget = parentWorld.EntityList[it];
                }
            }

            state = ShopKeeperState.Enraged;
        }

        public void spinerender(Spine.SkeletonRenderer renderer)
        {
            AnimationLib.SpineAnimationSet currentSkeleton = directionAnims[(int)direction_facing];

            currentSkeleton.Skeleton.RootBone.X = CenterPoint.X * (currentSkeleton.Skeleton.FlipX ? -1 : 1);
            currentSkeleton.Skeleton.RootBone.Y = CenterPoint.Y + (dimensions.Y / 2f);

            currentSkeleton.Skeleton.RootBone.ScaleX = 1.0f;
            currentSkeleton.Skeleton.RootBone.ScaleY = 1.0f;

            currentSkeleton.Skeleton.UpdateWorldTransform();
            renderer.Draw(currentSkeleton.Skeleton);
        }
    }
}
