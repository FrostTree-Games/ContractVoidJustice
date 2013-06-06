using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PattyPetitGiant
{
    class PatrolGuard : Enemy
    {
        private enum PatrolGuardState
        {
            InvalidState = -1,
            MoveWait = 0,
            Chase = 1,
            WindUp = 2,
            Shooting = 3,
            RetreatToCenter = 4,
        }

        private struct GunBullet
        {
            public bool active;
            public Vector2 position;
            public Vector2 hitbox;
            public Vector2 velocity;
            private float reloadTime;
            private const float bulletReloadDuration = 300f;

            private const float bulletSpeed = 1.0f;

            public Vector2 center { get { return position + (hitbox / 2.0f); } }

            public GunBullet(Vector2 position, GlobalGameConstants.Direction direction)
            {
                active = true;
                this.position = position;
                hitbox = GlobalGameConstants.TileSize / new Vector2(2, 2);
                reloadTime = 0.0f;

                switch (direction)
                {
                    case GlobalGameConstants.Direction.Up:
                        velocity = new Vector2(0, -bulletSpeed);
                        break;
                    case GlobalGameConstants.Direction.Down:
                        velocity = new Vector2(0, bulletSpeed);
                        break;
                    case GlobalGameConstants.Direction.Left:
                        velocity = new Vector2(-bulletSpeed, 0);
                        break;
                    case GlobalGameConstants.Direction.Right:
                        velocity = new Vector2(bulletSpeed, 0);
                        break;
                    default:
                        velocity = Vector2.Zero;
                        break;
                }
            }

            public bool hitTestBullet(Entity other)
            {
                if ((position.X - (hitbox.X / 2)) > other.Position.X + other.Dimensions.X || (position.X + (hitbox.X / 2)) < other.Position.X || (position.Y - (hitbox.Y / 2)) > other.Position.Y + other.Dimensions.Y || (position.Y + (hitbox.Y / 2)) < other.Position.Y)
                {
                    return false;
                }
                return true;
            }

            public void update(LevelState parentWorld, GameTime currentTime)
            {
                if (active)
                {
                    reloadTime += currentTime.ElapsedGameTime.Milliseconds;

                    if (parentWorld.Map.hitTestWall(center) || reloadTime > 5000f)
                    {
                        active = false;
                        reloadTime = 0.0f;
                        return;
                    }

                    position += velocity * currentTime.ElapsedGameTime.Milliseconds;
                }
                else
                {
                    reloadTime += currentTime.ElapsedGameTime.Milliseconds;
                }
            }
        }

        private GunBullet[] bullets = null;
        private const int bulletSupply = 5;

        private PatrolGuardState guardState;

        private bool moveWaitStepping;
        private float moveWaitTimer;
        private const float moveStepTime = 1000f;
        private const float moveStepWaitTime = 500f;

        private const float patrolMoveSpeed = 0.125f;

        private AnimationLib.SpineAnimationSet[] directionAnims = null;

        private Entity target = null;
        private Vector2 chunkCenter;

        public PatrolGuard(LevelState parentWorld, Vector2 position)
        {
            this.parentWorld = parentWorld;
            this.position = position;
            this.dimensions = GlobalGameConstants.TileSize;

            bullets = new GunBullet[bulletSupply];
            for (int i = 0; i < bulletSupply; i++ ) { bullets[i].active = false; }

            direction_facing = GlobalGameConstants.Direction.Down;
            guardState = PatrolGuardState.MoveWait;
            enemy_type = EnemyType.Guard;

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

            //calculate the center of the chunk you're placed in
            chunkCenter.X = (((int)((position.X / (GlobalGameConstants.TileSize.X)) / GlobalGameConstants.TilesPerRoomWide)) * GlobalGameConstants.TilesPerRoomWide * GlobalGameConstants.TileSize.X) + ((GlobalGameConstants.TilesPerRoomWide / 2) * GlobalGameConstants.TileSize.X);
            chunkCenter.Y = (((int)((position.Y / (GlobalGameConstants.TileSize.Y)) / GlobalGameConstants.TilesPerRoomHigh)) * GlobalGameConstants.TilesPerRoomHigh * GlobalGameConstants.TileSize.Y) + ((GlobalGameConstants.TilesPerRoomHigh / 2) * GlobalGameConstants.TileSize.Y);

        }

        public override void update(GameTime currentTime)
        {
            animation_time += currentTime.ElapsedGameTime.Milliseconds;

            if (guardState == PatrolGuardState.InvalidState)
            {
                throw new Exception("Patrol Guard went into invalid state");
            }
            else if (guardState == PatrolGuardState.MoveWait)
            {
                moveWaitTimer += currentTime.ElapsedGameTime.Milliseconds;

                if (moveWaitStepping)
                {
                    switch (direction_facing)
                    {
                        case GlobalGameConstants.Direction.Up:
                            velocity = new Vector2(0.0f, -patrolMoveSpeed);
                            break;
                        case GlobalGameConstants.Direction.Down:
                            velocity = new Vector2(0.0f, patrolMoveSpeed);
                            break;
                        case GlobalGameConstants.Direction.Left:
                            velocity = new Vector2(-patrolMoveSpeed, 0.0f);
                            break;
                        case GlobalGameConstants.Direction.Right:
                            velocity = new Vector2(patrolMoveSpeed, 0.0f);
                            break;
                    }

                    directionAnims[(int)direction_facing].Animation = directionAnims[(int)direction_facing].Skeleton.Data.FindAnimation("run");
                }
                else
                {
                    velocity = Vector2.Zero;

                    directionAnims[(int)direction_facing].Animation = directionAnims[(int)direction_facing].Skeleton.Data.FindAnimation("idle");
                }

                if (moveWaitTimer > (moveWaitStepping ? moveStepTime : moveStepWaitTime))
                {
                    moveWaitStepping = !moveWaitStepping;
                    moveWaitTimer = 0.0f;

                    if (moveWaitStepping)
                    {
                        direction_facing = (GlobalGameConstants.Direction)(((int)direction_facing + ((Game1.rand.Next() % 3) - 1) + 4) % 4);
                    }
                }

                if (Vector2.Distance(CenterPoint, chunkCenter) > 500)
                {
                    guardState = PatrolGuardState.RetreatToCenter;
                }
            }
            else if (guardState == PatrolGuardState.RetreatToCenter)
            {
                if (Vector2.Distance(CenterPoint, chunkCenter) < GlobalGameConstants.TileSize.X * 3)
                {
                    guardState = PatrolGuardState.MoveWait;
                    return;
                }

                double angle = Math.Atan2(CenterPoint.Y - chunkCenter.Y, CenterPoint.X - chunkCenter.X);

                if (angle < Math.PI / 4 || angle > (Math.PI * 2) - (Math.PI / 4))
                {
                    direction_facing = GlobalGameConstants.Direction.Down;
                }
                else if (angle > Math.PI / 4 && angle < Math.PI - (Math.PI / 4))
                {
                    direction_facing = GlobalGameConstants.Direction.Left;
                }
                else if (angle > Math.PI - (Math.PI / 4) / 4 && angle < Math.PI + (Math.PI / 4))
                {
                    direction_facing = GlobalGameConstants.Direction.Right;
                }
                else if (angle > Math.PI + (Math.PI / 4) / 4 && angle < (Math.PI * 2) - (Math.PI / 4))
                {
                    direction_facing = GlobalGameConstants.Direction.Up;
                }

                velocity = -patrolMoveSpeed * new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
            }

            Vector2 newPos = position + (currentTime.ElapsedGameTime.Milliseconds * velocity);
            Vector2 finalPos = parentWorld.Map.reloactePosition(position, newPos, dimensions);

            position = finalPos;

            directionAnims[(int)direction_facing].Animation = directionAnims[(int)direction_facing].Skeleton.Data.FindAnimation("run");

            directionAnims[(int)direction_facing].Animation.Apply(directionAnims[(int)direction_facing].Skeleton, animation_time / 1000f, true);
        }

        public override void draw(SpriteBatch sb)
        {
 	         //base.draw(sb);
        }

        public override void knockBack(Vector2 direction, float magnitude, int damage)
        {
            //base.knockBack(direction, magnitude, damage);
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
    }
}
