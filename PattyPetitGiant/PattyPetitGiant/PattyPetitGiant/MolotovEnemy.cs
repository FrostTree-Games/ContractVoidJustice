using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PattyPetitGiant
{
    class MolotovEnemy : Enemy
    {
        private MolotovState molotovState;
        private MolotovFlame flame;

        private AnimationLib.FrameAnimationSet templateAnim = null;
        private AnimationLib.FrameAnimationSet flameAnim = null;

        private AnimationLib.SpineAnimationSet[] directionAnims = null;

        private bool moveWaitStepping;
        private float moveWaitTimer;
        private const float moveStepTime = 500f;

        private float windUpTimer;
        private const float windUpDuration = 500f;

        private float throwingTimer;
        private const float throwDuration = 500f;
        private Vector2 throwPosition = Vector2.Zero;
        private const float throwVelocity = 0.5f;

        private const float molotovMovementSpeed = 0.1f;

        private float knockBackTime;
        private const float knockBackDuration = 250f;

        private int health;

        public MolotovEnemy(LevelState parentWorld, Vector2 position)
        {
            this.parentWorld = parentWorld;
            this.position = position;
            this.dimensions = GlobalGameConstants.TileSize;

            molotovState = MolotovState.MoveWait;
            moveWaitStepping = false;
            moveWaitTimer = 0.0f;

            flame = new MolotovFlame(position);
            flame.active = false;

            direction_facing = (GlobalGameConstants.Direction)(Game1.rand.Next() % 4);

            health = 15;

            templateAnim = AnimationLib.getFrameAnimationSet("molotov");
            flameAnim = AnimationLib.getFrameAnimationSet("molotovFlame");
            animation_time = 0.0f;

            directionAnims = new AnimationLib.SpineAnimationSet[4];
            directionAnims[(int)GlobalGameConstants.Direction.Up] = AnimationLib.loadNewAnimationSet("molotovUp");
            directionAnims[(int)GlobalGameConstants.Direction.Down] = AnimationLib.loadNewAnimationSet("molotovDown");
            directionAnims[(int)GlobalGameConstants.Direction.Left] = AnimationLib.loadNewAnimationSet("molotovRight");
            directionAnims[(int)GlobalGameConstants.Direction.Left].Skeleton.FlipX = true;
            directionAnims[(int)GlobalGameConstants.Direction.Right] = AnimationLib.loadNewAnimationSet("molotovRight");

            for (int i = 0; i < 4; i++)
            {
                directionAnims[i].Animation = directionAnims[i].Skeleton.Data.FindAnimation("run");
            }

            enemy_type = EnemyType.Prisoner;
        }

        public override void update(GameTime currentTime)
        {
            if (health < 1)
            {
                remove_from_list = true;
                return;
            }

            animation_time += currentTime.ElapsedGameTime.Milliseconds;

            if (molotovState == MolotovState.InvalidState)
            {
                throw new Exception("Invalid State handling");
            }
            else if (molotovState == MolotovState.MoveWait)
            {
                moveWaitTimer += currentTime.ElapsedGameTime.Milliseconds;

                // check if player is in front of enemy within range. if so throw a molotov
                if (flame.active == false)
                {
                    MolotovFlame f = new MolotovFlame();
                    if (direction_facing == GlobalGameConstants.Direction.Up || direction_facing == GlobalGameConstants.Direction.Down)
                    {
                        f.dimensions = new Vector2(GlobalGameConstants.TileSize.X * 3, GlobalGameConstants.TileSize.Y * 4);
                    }
                    else
                    {
                        f.dimensions = new Vector2(GlobalGameConstants.TileSize.X * 4, GlobalGameConstants.TileSize.Y * 3);
                    }

                    switch (direction_facing)
                    {
                        case GlobalGameConstants.Direction.Up:
                            f.position = position + new Vector2(-GlobalGameConstants.TileSize.X, -GlobalGameConstants.TileSize.Y * 4);
                            break;
                        case GlobalGameConstants.Direction.Down:
                            f.position = position + new Vector2(-GlobalGameConstants.TileSize.X, dimensions.Y);
                            break;
                        case GlobalGameConstants.Direction.Left:
                            f.position = position + new Vector2(-(GlobalGameConstants.TileSize.X * 4), -GlobalGameConstants.TileSize.Y);
                            break;
                        case GlobalGameConstants.Direction.Right:
                            f.position = position + new Vector2(dimensions.X, -GlobalGameConstants.TileSize.Y);
                            break; 
                    }

                    foreach (Entity en in parentWorld.EntityList)
                    {
                        if (en == this)
                        {
                            continue;
                        }

                        if (en is Player || en is Enemy)
                        {
                            if (en is Enemy)
                            {
                                if (((Enemy)en).EnemyFaction == Enemy.EnemyType.Prisoner)
                                {
                                    continue;
                                }
                            }

                            if (f.hitTestWithEntity(en))
                            {
                                animation_time = 0.0f;

                                molotovState = MolotovState.WindUp;
                                windUpTimer = 0.0f;
                                break;
                            }
                        }
                    }
                }

                if (moveWaitStepping)
                {
                    switch (direction_facing)
                    {
                        case GlobalGameConstants.Direction.Up:
                            velocity = new Vector2(0.0f, -molotovMovementSpeed);
                            break;
                        case GlobalGameConstants.Direction.Down:
                            velocity = new Vector2(0.0f, molotovMovementSpeed);
                            break;
                        case GlobalGameConstants.Direction.Left:
                            velocity = new Vector2(-molotovMovementSpeed, 0.0f);
                            break;
                        case GlobalGameConstants.Direction.Right:
                            velocity = new Vector2(molotovMovementSpeed, 0.0f);
                            break;
                    }

                    directionAnims[(int)direction_facing].Animation = directionAnims[(int)direction_facing].Skeleton.Data.FindAnimation("run");
                }
                else
                {
                    velocity = Vector2.Zero;

                    directionAnims[(int)direction_facing].Animation = directionAnims[(int)direction_facing].Skeleton.Data.FindAnimation("idle");
                }

                if (moveWaitTimer > moveStepTime)
                {
                    moveWaitStepping = !moveWaitStepping;
                    moveWaitTimer = 0.0f;

                    if (moveWaitStepping)
                    {
                        direction_facing = (GlobalGameConstants.Direction)(Game1.rand.Next() % 4);
                    }
                }
            }
            else if (molotovState == MolotovState.WindUp)
            {
                directionAnims[(int)direction_facing].Animation = directionAnims[(int)direction_facing].Skeleton.Data.FindAnimation("windUp");

                windUpTimer += currentTime.ElapsedGameTime.Milliseconds;

                if (windUpTimer > windUpDuration)
                {
                    animation_time = 0.0f;

                    molotovState = MolotovState.Throwing;
                    throwingTimer = 0.0f;
                    throwPosition = position;
                }

                velocity = Vector2.Zero;
            }
            else if (molotovState == MolotovState.Throwing)
            {
                directionAnims[(int)direction_facing].Animation = directionAnims[(int)direction_facing].Skeleton.Data.FindAnimation("attack");

                throwingTimer += currentTime.ElapsedGameTime.Milliseconds;

                Vector2 throwDirection = Vector2.Zero;
                switch (direction_facing)
                {
                    case GlobalGameConstants.Direction.Up:
                        throwDirection = new Vector2(0, -1);
                        break;
                    case GlobalGameConstants.Direction.Down:
                        throwDirection = new Vector2(0, 1);
                        break;
                    case GlobalGameConstants.Direction.Left:
                        throwDirection = new Vector2(-1, 0);
                        break;
                    case GlobalGameConstants.Direction.Right:
                        throwDirection = new Vector2(1, 0);
                        break;
                }

                throwPosition = position + (throwVelocity * throwingTimer * throwDirection);
                flame.position = throwPosition;

                if (parentWorld.Map.hitTestWall(flame.position) || parentWorld.Map.hitTestWall(flame.position + flame.dimensions))
                {
                    throwingTimer = 999f;
                }
                else
                {
                    foreach (Entity en in parentWorld.EntityList)
                    {
                        if (en is Player)
                        {
                            if (flame.hitTestWithEntity(en))
                            {
                                throwingTimer = 999f;
                                break;
                            }
                        }
                    }
                }

                if (throwingTimer > throwDuration)
                {
                    animation_time = 0.0f;

                    flame = new MolotovFlame(throwPosition);

                    molotovState = MolotovState.MoveWait;
                    moveWaitTimer = 0.0f;
                }
            }
            else if (molotovState == MolotovState.KnockedBack)
            {
                knockBackTime += currentTime.ElapsedGameTime.Milliseconds;

                if (knockBackTime > knockBackDuration)
                {
                    molotovState = MolotovState.MoveWait;
                }
            }
            else
            {
                throw new NotImplementedException("Need to complete other states first");
            }

            if (flame.active)
            {
                flame.update(parentWorld, currentTime);
            }

            Vector2 newPos = position + (currentTime.ElapsedGameTime.Milliseconds * velocity);
            Vector2 finalPos = parentWorld.Map.reloactePosition(position, newPos, dimensions);

            position = finalPos;

            directionAnims[(int)direction_facing].Animation.Apply(directionAnims[(int)direction_facing].Skeleton, animation_time / 1000f, molotovState == MolotovState.MoveWait ? true : false);
        }

        public override void draw(SpriteBatch sb)
        {
            if (molotovState == MolotovState.Throwing)
            {
                templateAnim.drawAnimationFrame(animation_time, sb, throwPosition, new Vector2(1, 1), 0.51f, animation_time / 100f, new Vector2(16, 16));
            }

            if (flame.active)
            {
                flameAnim.drawAnimationFrame(animation_time + 100f, sb, flame.position, new Vector2(1, 1), 0.4f, Color.White);
                sb.Draw(Game1.whitePixel, flame.position, null, Color.Red, 0.0f, Vector2.Zero, 48, SpriteEffects.None, 0.2f);
            }
        }

        public override void knockBack(Vector2 direction, float magnitude, int damage)
        {
            if (molotovState == MolotovState.KnockedBack)
            {
                return;
            }

            direction.Normalize();
            velocity = direction * (magnitude / 2);

            health -= damage;

            knockBackTime = 0.0f;

            molotovState = MolotovState.KnockedBack;
        }

        public override void spinerender(Spine.SkeletonRenderer renderer)
        {
            AnimationLib.SpineAnimationSet currentSkeleton = directionAnims[(int)direction_facing];

            currentSkeleton.Skeleton.RootBone.X = CenterPoint.X * (currentSkeleton.Skeleton.FlipX ? -1 : 1);
            currentSkeleton.Skeleton.RootBone.Y = CenterPoint.Y + (dimensions.Y / 2f);

            currentSkeleton.Skeleton.RootBone.ScaleX = 1.0f;
            currentSkeleton.Skeleton.RootBone.ScaleY = 1.0f;

            currentSkeleton.Skeleton.UpdateWorldTransform();
            renderer.Draw(currentSkeleton.Skeleton);
        }

        /// <summary>
        /// Represents the current state of the MolotovEnemy
        /// </summary>
        private enum MolotovState
        {
            /// <summary>
            /// Should not happen
            /// </summary>
            InvalidState = -1,
            /// <summary>
            /// Waiting around for a target. May wander around a small spot.
            /// </summary>
            MoveWait = 0,
            /// <summary>
            /// Target spotted and is not waiting for previous flame to burn out. This state exists for animation purposes.
            /// </summary>
            WindUp = 1,
            /// <summary>
            /// Animation purposes.
            /// </summary>
            Throwing = 2,
            /// <summary>
            /// For logic when knocked backwards.
            /// </summary>
            KnockedBack = 3,
        }

        /// <summary>
        /// Represents the splash damage area caused by the molotov cocktail thrown.
        /// </summary>
        private struct MolotovFlame
        {
            public bool active;

            public Vector2 position;

            public Vector2 dimensions;

            public Vector2 CenterPoint
            {
                get
                {
                    return position + (dimensions / 2);
                }
            }

            public float timeAlive;
            private const float flameDuration = 4000f;

            public MolotovFlame(Vector2 position)
            {
                this.position = position;
                this.active = true;
                this.dimensions = GlobalGameConstants.TileSize;

                timeAlive = 0.0f;
            }

            public bool hitTestWithEntity(Entity en)
            {
                if (position.X > en.Position.X + en.Dimensions.X || position.X + dimensions.X < en.Position.X || position.Y > en.Position.Y + en.Dimensions.Y || position.Y + dimensions.Y < en.Position.Y)
                {
                    return false;
                }

                return true;
            }

            public void update(LevelState parentWorld, GameTime currentTime)
            {
                timeAlive += currentTime.ElapsedGameTime.Milliseconds;

                foreach (Entity en in parentWorld.EntityList)
                {
                    if (en is MolotovEnemy)
                    {
                        continue;
                    }

                    if (hitTestWithEntity(en))
                    {
                        en.knockBack(en.CenterPoint - CenterPoint, 5.0f, 10);
                    }
                }

                if (timeAlive > flameDuration)
                {
                    active = false;
                }
            }
        }
    }
}
