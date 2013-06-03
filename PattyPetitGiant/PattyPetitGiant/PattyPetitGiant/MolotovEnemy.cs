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

        /// <summary>
        /// will switch to a spine animation later
        /// </summary>
        AnimationLib.FrameAnimationSet templateAnim = null;

        private bool moveWaitStepping;
        private float moveWaitTimer;
        private const float moveStepTime = 500f;

        private const float molotovMovementSpeed = 0.1f;

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

            templateAnim = AnimationLib.getFrameAnimationSet("antiFairy");
            animation_time = 0.0f;
        }

        public override void update(GameTime currentTime)
        {
            animation_time += currentTime.ElapsedGameTime.Milliseconds;

            if (molotovState == MolotovState.InvalidState)
            {
                throw new Exception("Invalid State handling");
            }
            else if (molotovState == MolotovState.MoveWait)
            {
                moveWaitTimer += currentTime.ElapsedGameTime.Milliseconds;

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
                }
                else
                {
                    velocity = Vector2.Zero;
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
            else
            {
                throw new NotImplementedException("Need to complete other states first");
            }

            Vector2 newPos = position + (currentTime.ElapsedGameTime.Milliseconds * velocity);
            Vector2 finalPos = parentWorld.Map.reloactePosition(position, newPos, dimensions);

            position = finalPos;
        }

        public override void draw(SpriteBatch sb)
        {
            templateAnim.drawAnimationFrame(animation_time, sb, position, new Vector2(3, 3), 0.5f, Color.LimeGreen);
        }

        public override void knockBack(Vector2 direction, float magnitude, int damage)
        {
            return;
        }

        public override void spinerender(Spine.SkeletonRenderer renderer)
        {
            //base.spinerender(renderer);
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

            public MolotovFlame(Vector2 position)
            {
                this.position = position;
                this.active = true;
                this.dimensions = GlobalGameConstants.TileSize;
            }

            public bool hitTestWithEntity(Entity en)
            {
                if (position.X > en.Position.X + en.Dimensions.X || position.X + dimensions.X < en.Position.X || position.Y > en.Position.Y + en.Dimensions.Y || position.Y + dimensions.Y < en.Position.Y)
                {
                    return false;
                }

                return true;
            }

            public void update(GameTime currentTime)
            {
                //
            }
        }
    }
}
