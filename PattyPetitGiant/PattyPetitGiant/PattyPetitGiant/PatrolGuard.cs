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
        }

        private PatrolGuardState guardState;

        private bool moveWaitStepping;
        private float moveWaitTimer;
        private const float moveStepTime = 800f;
        private const float moveStepWaitTime = 500f;

        private AnimationLib.SpineAnimationSet[] directionAnims = null;

        private Entity target = null;

        public PatrolGuard(LevelState parentWorld, Vector2 position)
        {
            this.parentWorld = parentWorld;
            this.position = position;
            this.dimensions = GlobalGameConstants.TileSize;

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
                //
            }
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
