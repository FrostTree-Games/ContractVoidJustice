﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PattyPetitGiant
{
    class KeyDoor : Entity
    {
        /// <summary>
        /// Determines which sort of corridor the enum should be placed in.
        /// </summary>
        public enum DoorDirection
        {
            /// <summary>
            /// Cooridors that have the player move along the Y axis.
            /// </summary>
            NorthSouth,

            /// <summary>
            /// Cooridors that have the player move along the X axis.
            /// </summary>
            EastWest
        }

        private LevelKeyModule.KeyColor color;
        private DoorDirection directions;

        private AnimationLib.FrameAnimationSet keyGraphic = null;

        public KeyDoor(LevelState parent, Vector2 position, LevelKeyModule.KeyColor color, DoorDirection directions)
        {
            this.parentWorld = parent;
            this.position = position;
            this.velocity = Vector2.Zero;
            dimensions = GlobalGameConstants.TileSize;

            this.color = color;
            this.directions = directions;

            keyGraphic = AnimationLib.getFrameAnimationSet("keyPic");

            // reposition and resize the door to fill the current wall
            {
                this.position.X -= this.position.X % GlobalGameConstants.TileSize.X;
                this.position.Y -= this.position.Y % GlobalGameConstants.TileSize.Y;

                if (directions == DoorDirection.EastWest)
                {
                    while (parentWorld.Map.hitTestWall(this.position - new Vector2(1.0f, 0.0f)) != true)
                    {
                        this.position.X -= GlobalGameConstants.TileSize.X;
                    }

                    while (parentWorld.Map.hitTestWall(this.position + new Vector2(this.dimensions.X, 0.0f)) != true)
                    {
                        this.dimensions.X += GlobalGameConstants.TileSize.X;
                    }
                }
                else if (directions == DoorDirection.NorthSouth)
                {
                    while (parentWorld.Map.hitTestWall(this.position - new Vector2(0.0f, 1.0f)) != true)
                    {
                        this.position.Y -= GlobalGameConstants.TileSize.Y;
                    }

                    while (parentWorld.Map.hitTestWall(this.position + new Vector2(0.0f, this.dimensions.Y)) != true)
                    {
                        this.dimensions.Y += GlobalGameConstants.TileSize.Y;
                    }
                }
            }
        }

        public override void knockBack(Vector2 direction, float magnitude, int damage, Entity attacker)
        {
            return;
        }

        public override void update(GameTime currentTime)
        {
            if (directions == DoorDirection.NorthSouth)
            {
                int rootTileX = (int)(position.X / GlobalGameConstants.TileSize.X);
                int rootTileY = (int)(position.Y / GlobalGameConstants.TileSize.Y);

                for (int i = 0; i < (int)(dimensions.Y / GlobalGameConstants.TileSize.Y); i++)
                {
                    parentWorld.Map.Map[rootTileX, rootTileY + i] = TileMap.TileType.WallUnidentified;
                }
            }
            else
            {
                int rootTileX = (int)(position.X / GlobalGameConstants.TileSize.X);
                int rootTileY = (int)(position.Y / GlobalGameConstants.TileSize.Y);

                for (int i = 0; i < (int)(dimensions.X / GlobalGameConstants.TileSize.X); i++)
                {
                    parentWorld.Map.Map[rootTileX + i, rootTileY] = TileMap.TileType.WallUnidentified;
                }
            }

            //this is a bit complicated
            foreach (Entity en in parentWorld.EntityList)
            {
                //checking the type twice seems a bit redundant, but that way we only bother to check stuff that matters
                if (en is Player || en is Entity || en is ShopKeeper || en is Key)
                {
                    if (hitTest(en))
                    {
                        if (en is Key)
                        {
                            en.Velocity = Vector2.Zero;
                        }
                        else if (en is Player && parentWorld.KeyModule.isKeyFound(color))
                        {
                            remove_from_list = true;

                            if (directions == DoorDirection.NorthSouth)
                            {
                                int rootTileX = (int)(position.X / GlobalGameConstants.TileSize.X);
                                int rootTileY = (int)(position.Y / GlobalGameConstants.TileSize.Y);

                                for (int i = 0; i < (int)(dimensions.Y / GlobalGameConstants.TileSize.Y); i++)
                                {
                                    parentWorld.Map.Map[rootTileX, rootTileY + i] = TileMap.TileType.NoWall;
                                }
                            }
                            else
                            {
                                int rootTileX = (int)(position.X / GlobalGameConstants.TileSize.X);
                                int rootTileY = (int)(position.Y / GlobalGameConstants.TileSize.Y);

                                for (int i = 0; i < (int)(dimensions.X / GlobalGameConstants.TileSize.X); i++)
                                {
                                    parentWorld.Map.Map[rootTileX + i, rootTileY] = TileMap.TileType.NoWall;
                                }
                            }
                        }
                        else
                        {
                            Vector2 repelDirection = Vector2.Zero;

                            if (directions == DoorDirection.NorthSouth)
                            {
                                if (en.Position.X > position.X)
                                {
                                    repelDirection = new Vector2(1, 0);
                                }
                                else
                                {
                                    repelDirection = new Vector2(-1, 0);
                                }
                            }
                            else if (directions == DoorDirection.EastWest)
                            {
                                if (en.Position.Y > position.Y)
                                {
                                    repelDirection = new Vector2(0, 1);
                                }
                                else
                                {
                                    repelDirection = new Vector2(0, -1);
                                }
                            }

                            //en.knockBack(repelDirection, 3.0f, 0);
                        }
                    }
                }
            }
        }

        public override void draw(Spine.SkeletonRenderer sb)
        {
            sb.DrawSpriteToSpineVertexArray(Game1.whitePixel, new Rectangle(0, 0, 1, 1), position, Color.Lerp(parentWorld.KeyModule.KeyColorSet[(int)color], new Color(0.0f, 0.0f, 1.0f, 0.25f), 0.5f), 0.0f, dimensions);
            //sb.Draw(Game1.whitePixel, position, null, Color.Lerp(parentWorld.KeyModule.KeyColorSet[(int)color], new Color(0.0f, 0.0f, 1.0f, 0.25f), 0.5f), 0.0f, Vector2.Zero, dimensions, SpriteEffects.None, 0.45f);
        }
    }

    class Key : Entity
    {
        private bool isKnockedBack;
        private float knockedBackTime;
        private const float knockedBackTimeDuration = 300f;
        private const float knockBackVelocity = 5.0f;

        private LevelKeyModule.KeyColor color;

        private AnimationLib.FrameAnimationSet keyGraphic = null;

        public Key(LevelState parent, Vector2 position, LevelKeyModule.KeyColor color)
        {
            this.parentWorld = parent;
            this.position = position;
            this.velocity = Vector2.Zero;
            this.dimensions = new Vector2(48f, 48f);

            this.color = color;

            isKnockedBack = false;
            knockedBackTime = 0.0f;

            keyGraphic = AnimationLib.getFrameAnimationSet("keyPic");
        }

        public override void knockBack(Vector2 direction, float magnitude, int damage, Entity attacker)
        {
            if (attacker == null)
            {
                return;
            }

            if (isKnockedBack)
            {
                return;
            }
            else
            {
                direction.Normalize();

                isKnockedBack = true;
                knockedBackTime = 0.0f;
                velocity = direction * knockBackVelocity;
            }
        }

        public override void update(GameTime currentTime)
        {
            if (isKnockedBack)
            {
                knockedBackTime += currentTime.ElapsedGameTime.Milliseconds;

                if (knockedBackTime > knockedBackTimeDuration)
                {
                    isKnockedBack = false;
                    velocity = Vector2.Zero;
                }
            }

            foreach (Entity en in parentWorld.EntityList)
            {
                if (en is Player)
                {
                    if (hitTest(en))
                    {
                        remove_from_list = true;

                        parentWorld.KeyModule.setKey(color, true);
                    }
                }
            }

            Vector2 nextPos = position + velocity;
            Vector2 finalPos = parentWorld.Map.reloactePosition(position, nextPos, dimensions);

            position = finalPos;
        }

        public override void draw(Spine.SkeletonRenderer sb)
        {
            keyGraphic.drawAnimationFrame(0.0f, sb, position, new Vector2(3), 0.5f, 0.0f, Vector2.Zero, parentWorld.KeyModule.KeyColorSet[(int)color]);
        }
    }
}
