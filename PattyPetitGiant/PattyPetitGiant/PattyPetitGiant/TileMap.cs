//#define PROTOTYPE_ROOMS

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace PattyPetitGiant
{
    public class TileMap
    {
        /// <summary>
        ///  A structure for storing the width and height of a map in integer format.
        /// </summary>
        public struct TileDimensions
        {
            public int x;
            public int y;

            public TileDimensions (int x, int y)
            {
                this.x = x;
                this.y = y;
            }

            public int area
            {
                get { return x * y; }
            }
        }

        /// <summary>
        ///  Used to determine which type of wall is represented.
        /// </summary>
        public enum TileType
        {
            NoWall = 0,
            TestWall = 1,
        }

        private enum FloorType
        {
            A = 0,
            B = 1,
            C = 2,
            D = 3,
            E = 4,
            F = 5,
            G = 6,
            H = 7,
        }

        private TileDimensions size;
        public TileDimensions SizeInTiles { get { return size; } }
        public Vector2 SizeInPixels { get { return new Vector2(size.x * tileSize.X, size.y * tileSize.Y); } }

        private Vector2 tileSize;
        public Vector2 TileSize { get { return tileSize; } }

        private TileType[,] map = null;
        private FloorType[,] floorMap = null;

        private Vector2 startPosition;
        public Vector2 StartPosition { get { return startPosition; } }

        private Texture2D tileSkin = null;
        public Texture2D TileSkin { get { return tileSkin; } set { tileSkin = value; } }

        /*
        public TileMap(TileDimensions size, Vector2 tileSize)
        {
            this.size = size;
            this.tileSize = tileSize;

            map = new TileType[size.x, size.y];
            blobTestWalls();

            startPosition = new Vector2(0, 0);
        }

        public TileMap(TileDimensions size, Vector2 tileSize, Texture2D tileMap)
        {
            this.size = size;
            this.tileSize = tileSize;

            map = new TileType[size.x, size.y];
            blobTestWalls();

            startPosition = new Vector2(0, 0);

            this.tileSkin = tileMap;
        } */

        public TileMap(DungeonGenerator.DungeonRoom[,] room, Vector2 tileSize)
        {
            this.size = new TileDimensions(room.GetLength(0) * GlobalGameConstants.TilesPerRoomWide, room.GetLength(1)  * GlobalGameConstants.TilesPerRoomHigh);
            this.tileSize = tileSize;

            map = new TileType[size.x, size.y];
            floorMap = new FloorType[size.x, size.y];

            Random rand = new Random();

            for (int i = 0; i < floorMap.GetLength(0); i++)
            {
                for (int j = 0; j < floorMap.GetLength(1); j++)
                {
                    floorMap[i, j] = ((FloorType)(rand.Next() % Enum.GetNames(typeof(FloorType)).Length));
                }
            }

            for (int i = 0; i < room.GetLength(0); i++)
            {
                for (int j = 0; j < room.GetLength(1); j++)
                {
                    if (room[i, j].attributes != null)
                    {
                        ChunkManager.Chunk c = ChunkLib.getRandomChunkByValues(room[i, j].attributes.ToArray());

                        if (c != null)
                        {
                            for (int p = 0; p < GlobalGameConstants.TilesPerRoomWide; p++)
                            {
                                for (int q = 0; q < GlobalGameConstants.TilesPerRoomHigh; q++)
                                {
                                    map[(i * GlobalGameConstants.TilesPerRoomWide) + p, (j * GlobalGameConstants.TilesPerRoomHigh) + q] = (TileType)(c.tilemap[(q * GlobalGameConstants.TilesPerRoomHigh) + p]);
                                }
                            }

                            if (room[i, j].attributes.Contains("start"))
                            {
                                int startX = 0;
                                int startY = 0;

                                while (c.tilemap[startY * GlobalGameConstants.TilesPerRoomHigh + startX] == 1)
                                {
                                    startX = rand.Next() % GlobalGameConstants.TilesPerRoomWide;
                                    startY = rand.Next() % GlobalGameConstants.TilesPerRoomHigh;
                                }

                                startPosition.X = (i * GlobalGameConstants.TilesPerRoomWide * GlobalGameConstants.TileSize.X) + (GlobalGameConstants.TileSize.X * startX);
                                startPosition.Y = (j * GlobalGameConstants.TilesPerRoomHigh * GlobalGameConstants.TileSize.Y) + (GlobalGameConstants.TileSize.Y * startY);
                            }
                        }
                        else
                        {
                            for (int p = 0; p < GlobalGameConstants.TilesPerRoomWide; p++)
                            {
                                for (int q = 0; q < GlobalGameConstants.TilesPerRoomHigh; q++)
                                {
                                    map[(i * GlobalGameConstants.TilesPerRoomWide) + p, (j * GlobalGameConstants.TilesPerRoomHigh) + q] = TileType.TestWall;
                                }
                            }
                        }
                    }
                    else
                    {
                        for (int p = 0; p < GlobalGameConstants.TilesPerRoomWide; p++)
                        {
                            for (int q = 0; q < GlobalGameConstants.TilesPerRoomHigh; q++)
                            {
                                map[(i * GlobalGameConstants.TilesPerRoomWide) + p, (j * GlobalGameConstants.TilesPerRoomHigh) + q] = TileType.TestWall;
                            }
                        }
                    }

#if PROTOTYPE_ROOMS
                    for (int p = 0; p < GlobalGameConstants.TilesPerRoomWide; p++)
                    {
                        for (int q = 0; q < GlobalGameConstants.TilesPerRoomHigh; q++)
                        {
                            if (!room[i, j].north && !room[i, j].south && !room[i, j].west && !room[i, j].east)
                            {
                                map[(i * GlobalGameConstants.TilesPerRoomWide) + p, (j * GlobalGameConstants.TilesPerRoomHigh) + q] = TileType.TestWall;
                                continue;
                            }

                            if ((q == 0 || q == 1) && ((p == GlobalGameConstants.TilesPerRoomWide / 2) || (p == GlobalGameConstants.TilesPerRoomWide / 2 - 1)) && room[i, j].north)
                            {
                                continue;
                            }
                            if ((q == GlobalGameConstants.TilesPerRoomHigh - 1 || q == GlobalGameConstants.TilesPerRoomHigh - 2) && ((p == GlobalGameConstants.TilesPerRoomWide / 2) || (p == GlobalGameConstants.TilesPerRoomWide / 2 - 1)) && room[i, j].south)
                            {
                                continue;
                            }

                            if ((p == 0 || p == 1) && ((q == GlobalGameConstants.TilesPerRoomHigh / 2) || (q == GlobalGameConstants.TilesPerRoomHigh / 2 - 1)) && room[i, j].west)
                            {
                                continue;
                            }
                            if ((p == GlobalGameConstants.TilesPerRoomWide - 1 || p == GlobalGameConstants.TilesPerRoomWide - 2) && ((q == GlobalGameConstants.TilesPerRoomWide / 2) || (q == GlobalGameConstants.TilesPerRoomWide / 2 - 1)) && room[i, j].east)
                            {
                                continue;
                            }

                            if (p == 0 || p == 1 || p == GlobalGameConstants.TilesPerRoomWide - 1 || p == GlobalGameConstants.TilesPerRoomWide - 2)
                            {
                                map[(i * GlobalGameConstants.TilesPerRoomWide) + p, (j * GlobalGameConstants.TilesPerRoomHigh) + q] = TileType.TestWall;
                            }
                            if (q == 0 || q == 1 || q == GlobalGameConstants.TilesPerRoomHigh - 1 || q == GlobalGameConstants.TilesPerRoomHigh - 2)
                            {
                                map[(i * GlobalGameConstants.TilesPerRoomWide) + p, (j * GlobalGameConstants.TilesPerRoomHigh) + q] = TileType.TestWall;
                            }
                        }
                    }
#endif
                }
            }
        }

        public TileMap(TileDimensions size, Vector2 tileSize, TileType initType)
        {
            this.size = size;
            this.tileSize = tileSize;

            map = new TileType[size.x, size.y];
            for (int i = 0; i < size.x; i++)
            {
                for (int j = 0; j < size.y; j++)
                {
                    map[i, j] = initType;
                }
            }
        }

        /// <summary>
        ///  Draws the tilemap to the specified device.
        ///  <para name="spriteBatch">The XNA SpriteBatch to render the TileMap to.</para>
        ///  <para name="depth">Specify Z-layering. Typically this tends to be below the sprites.</para>
        /// </summary>
        public void render(SpriteBatch spriteBatch, float depth)
        {
            for (int j = 0; j < size.y; j++)
            {
                for (int i = 0; i < size.x; i++)
                {
                    if (map[i, j] != TileType.NoWall)
                    {
                        continue;
                    }

                    switch (floorMap[i,j])
                    {
                        case FloorType.A:
                            spriteBatch.Draw(tileSkin, new Rectangle((int)(i * tileSize.X), (int)(j * tileSize.Y), (int)(GlobalGameConstants.TileSize.X), (int)(GlobalGameConstants.TileSize.Y)), new Rectangle((int)(3 * GlobalGameConstants.TileSize.X + 3), (int)(2 * GlobalGameConstants.TileSize.Y + 2), (int)(GlobalGameConstants.TileSize.X), (int)(GlobalGameConstants.TileSize.Y)), Color.White);
                            break;
                        case FloorType.B:
                            spriteBatch.Draw(tileSkin, new Rectangle((int)(i * tileSize.X), (int)(j * tileSize.Y), (int)(GlobalGameConstants.TileSize.X), (int)(GlobalGameConstants.TileSize.Y)), new Rectangle((int)(4 * GlobalGameConstants.TileSize.X + 4), (int)(2 * GlobalGameConstants.TileSize.Y + 2), (int)(GlobalGameConstants.TileSize.X), (int)(GlobalGameConstants.TileSize.Y)), Color.White);
                            break;
                        case FloorType.C:
                            spriteBatch.Draw(tileSkin, new Rectangle((int)(i * tileSize.X), (int)(j * tileSize.Y), (int)(GlobalGameConstants.TileSize.X), (int)(GlobalGameConstants.TileSize.Y)), new Rectangle((int)(8 * GlobalGameConstants.TileSize.X + 8), (int)(2 * GlobalGameConstants.TileSize.Y + 2), (int)(GlobalGameConstants.TileSize.X), (int)(GlobalGameConstants.TileSize.Y)), Color.White);
                            break;
                        case FloorType.D:
                            spriteBatch.Draw(tileSkin, new Rectangle((int)(i * tileSize.X), (int)(j * tileSize.Y), (int)(GlobalGameConstants.TileSize.X), (int)(GlobalGameConstants.TileSize.Y)), new Rectangle((int)(9 * GlobalGameConstants.TileSize.X + 9), (int)(2 * GlobalGameConstants.TileSize.Y + 2), (int)(GlobalGameConstants.TileSize.X), (int)(GlobalGameConstants.TileSize.Y)), Color.White);
                            break;
                        case FloorType.E:
                            spriteBatch.Draw(tileSkin, new Rectangle((int)(i * tileSize.X), (int)(j * tileSize.Y), (int)(GlobalGameConstants.TileSize.X), (int)(GlobalGameConstants.TileSize.Y)), new Rectangle((int)(3 * GlobalGameConstants.TileSize.X + 3), (int)(5 * GlobalGameConstants.TileSize.Y + 5), (int)(GlobalGameConstants.TileSize.X), (int)(GlobalGameConstants.TileSize.Y)), Color.White);
                            break;
                        case FloorType.F:
                            spriteBatch.Draw(tileSkin, new Rectangle((int)(i * tileSize.X), (int)(j * tileSize.Y), (int)(GlobalGameConstants.TileSize.X), (int)(GlobalGameConstants.TileSize.Y)), new Rectangle((int)(4 * GlobalGameConstants.TileSize.X + 4), (int)(5 * GlobalGameConstants.TileSize.Y + 5), (int)(GlobalGameConstants.TileSize.X), (int)(GlobalGameConstants.TileSize.Y)), Color.White);
                            break;
                        case FloorType.G:
                            spriteBatch.Draw(tileSkin, new Rectangle((int)(i * tileSize.X), (int)(j * tileSize.Y), (int)(GlobalGameConstants.TileSize.X), (int)(GlobalGameConstants.TileSize.Y)), new Rectangle((int)(8 * GlobalGameConstants.TileSize.X + 8), (int)(5 * GlobalGameConstants.TileSize.Y + 5), (int)(GlobalGameConstants.TileSize.X), (int)(GlobalGameConstants.TileSize.Y)), Color.White);
                            break;
                        case FloorType.H:
                            spriteBatch.Draw(tileSkin, new Rectangle((int)(i * tileSize.X), (int)(j * tileSize.Y), (int)(GlobalGameConstants.TileSize.X), (int)(GlobalGameConstants.TileSize.Y)), new Rectangle((int)(9 * GlobalGameConstants.TileSize.X + 9), (int)(5 * GlobalGameConstants.TileSize.Y + 5), (int)(GlobalGameConstants.TileSize.X), (int)(GlobalGameConstants.TileSize.Y)), Color.White);
                            break;
                    }
                }
            }

            for (int j = 0; j < size.y; j++)
            {
                for (int i = 0; i < size.x; i++)
                {
                    switch (map[i, j])
                    {
                        case TileType.NoWall:
                            break;
                        case TileType.TestWall:
                            if (tileSkin == null)
                            {
                                spriteBatch.Draw(Game1.whitePixel, new Vector2(i * tileSize.X, j * tileSize.Y), null, Color.Blue, 0.0f, Vector2.Zero, new Vector2(tileSize.X, tileSize.Y), SpriteEffects.None, depth);
                            }
                            else
                            {
                                spriteBatch.Draw(tileSkin, new Rectangle((int)(i * tileSize.X), (int)(j * tileSize.Y), (int)(GlobalGameConstants.TileSize.X), (int)(GlobalGameConstants.TileSize.Y)), new Rectangle((int)(1 * GlobalGameConstants.TileSize.X + 1), (int)(1 * GlobalGameConstants.TileSize.Y + 1), (int)(GlobalGameConstants.TileSize.X), (int)(GlobalGameConstants.TileSize.Y)), Color.White);
                            }
                            break;
                        default:
                            spriteBatch.Draw(Game1.whitePixel, new Vector2(i * tileSize.X, j * tileSize.Y), null, Color.Red, 0.0f, Vector2.Zero, new Vector2(tileSize.X, tileSize.Y), SpriteEffects.None, depth);
                            break;
                    }
                }
            }
        }

        /// <summary>
        ///  Hit-checks a point against the map. Useful for ray-casting or mouse clicks.
        /// </summary>
        /// <param name="position"></param>
        /// <returns>True if position overlaps a solid wall tile, false if otherwis.</returns>
        public bool hitTestWall(Vector2 position)
        {
            if (position.X < 0 || position.Y < 0 || position.X >= size.x * tileSize.X || position.Y >= size.y * tileSize.Y)
            {
                return false;
            }
            else
            {
                if (map[(int)(position.X / tileSize.X), (int)(position.Y / tileSize.Y)] != TileType.NoWall)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        ///  (UNTESTED) Determines if a moved hitbox intersects with a tilemap. Gives a new, valid solution if the new position is not valid.
        /// </summary>
        /// <param name="currentPosition">current position of the hitbox</param>
        /// <param name="newPosition">new position of the hitbox</param>
        /// <param name="hitBox">dimensions of the hitbox</param>
        /// <returns>A Vector2 determining the hitbox's potentially new location.</returns>
        public Vector2 reloactePosition(Vector2 currentPosition, Vector2 newPosition, Vector2 hitBox)
        {
            if (currentPosition.X < 0 || currentPosition.Y < 0 || currentPosition.X >= size.x * tileSize.X || currentPosition.Y >= size.y * tileSize.Y)
            {
                return newPosition;
            }

            // get tile coordinates for entity+hitbox
            TileDimensions tilesPosition = new TileDimensions((int)(currentPosition.X / tileSize.X), (int)(currentPosition.Y / tileSize.Y));
            TileDimensions tilesPositionPlusHitBox = new TileDimensions((int)((currentPosition.X + hitBox.X) / tileSize.X), (int)((currentPosition.Y + hitBox.Y) / tileSize.Y));

            Vector2 delta = newPosition - currentPosition;
            Vector2 resultLocation = newPosition;

            if (delta.X > 0)
            {
                bool breakLoop = false;
                TileDimensions pos = new TileDimensions(-1, -1);

                for (int i = tilesPositionPlusHitBox.x; i < size.x && !breakLoop; i++)
                {
                    for (int j = tilesPosition.y; j <= tilesPositionPlusHitBox.y && j < size.y && !breakLoop; j++)
                    {
                        if (map[i, j] != TileType.NoWall)
                        {
                            breakLoop = true;
                            pos = new TileDimensions(i, j);
                        }
                    }
                }

                if (breakLoop)
                {
                    resultLocation.X = Math.Min(newPosition.X, (pos.x * tileSize.X) - (hitBox.X + 1));

                    tilesPosition = new TileDimensions((int)(resultLocation.X / tileSize.X), (int)(currentPosition.Y / tileSize.Y));
                    tilesPositionPlusHitBox = new TileDimensions((int)((resultLocation.X + hitBox.X) / tileSize.X), (int)((currentPosition.Y + hitBox.Y) / tileSize.Y));
                }
            }
            else if (delta.X < 0)
            {
                bool breakLoop = false;
                TileDimensions pos = new TileDimensions(-1, -1);

                for (int i = tilesPosition.x; i >= 0 && !breakLoop; i--)
                {
                    for (int j = tilesPosition.y; j <= tilesPositionPlusHitBox.y && j < size.y && !breakLoop; j++)
                    {
                        if (map[i, j] != TileType.NoWall)
                        {
                            breakLoop = true;
                            pos = new TileDimensions(i, j);
                        }
                    }
                }

                if (breakLoop)
                {
                    resultLocation.X = Math.Max(newPosition.X, ((pos.x + 1) * tileSize.X));

                    tilesPosition = new TileDimensions((int)(resultLocation.X / tileSize.X), (int)(currentPosition.Y / tileSize.Y));
                    tilesPositionPlusHitBox = new TileDimensions((int)((resultLocation.X + hitBox.X) / tileSize.X), (int)((currentPosition.Y + hitBox.Y) / tileSize.Y));
                }
            }

            if (delta.Y > 0)
            {
                bool breakLoop = false;
                TileDimensions pos = new TileDimensions(-1, -1);

                for (int i = tilesPositionPlusHitBox.y; i < size.y && !breakLoop; i++)
                {
                    for (int j = tilesPosition.x; j <= tilesPositionPlusHitBox.x && j < size.x && !breakLoop; j++)
                    {
                        if (map[j, i] != TileType.NoWall)
                        {
                            breakLoop = true;
                            pos = new TileDimensions(j, i);
                        }
                    }
                }

                if (breakLoop)
                {
                    resultLocation.Y = Math.Min(newPosition.Y, (pos.y * tileSize.Y) - (hitBox.Y + 1));
                }
            }
            else if (delta.Y < 0)
            {
                bool breakLoop = false;
                TileDimensions pos = new TileDimensions(-1, -1);

                for (int i = tilesPositionPlusHitBox.y; i > 0 && !breakLoop; i--)
                {
                    for (int j = tilesPosition.x; j <= tilesPositionPlusHitBox.x && j < size.x && !breakLoop; j++)
                    {
                        if (map[j, i] != TileType.NoWall)
                        {
                            breakLoop = true;
                            pos = new TileDimensions(j, i);
                        }
                    }
                }

                if (breakLoop)
                {
                    resultLocation.Y = Math.Max(newPosition.Y, (pos.y + 1) * tileSize.Y);
                }
            }
            
            return resultLocation;
        }

        /// <summary>
        ///  Scrambles the map with a 0.5 probability that each tile will be a TestWall.
        /// </summary>
        public void randomizeTestWalls()
        {
            Random rand = new Random();

            for (int j = 0; j < size.y; j++)
            {
                for (int i = 0; i < size.x; i++)
                {
                    map[i, j] = (TileType)(rand.Next() % 2);
                }
            }
        }

        public void blobTestWalls()
        {
            for (int j = 0; j < size.y; j++)
            {
                for (int i = 0; i < size.x; i++)
                {
                    if (!(i % 4 == 0 || i % 4 == 3 || j % 4 == 2 || j % 4 == 1))
                    {
                        map[i, j] = TileType.TestWall;
                    }
                    else 
                    {
                        map[i, j] = TileType.NoWall;
                    }

                    map[i, size.y - 1] = TileType.TestWall;
                    map[i, size.y - 2] = TileType.TestWall;
                    map[i, 0] = TileType.TestWall;
                    map[i, 1] = TileType.TestWall;
                    map[i, 2] = TileType.TestWall;
                }

                map[size.x - 1, j] = TileType.TestWall;
                map[size.x - 2, j] = TileType.TestWall;
                map[1, j] = TileType.TestWall;
                map[0, j] = TileType.TestWall;
            }
        }
    }
}
