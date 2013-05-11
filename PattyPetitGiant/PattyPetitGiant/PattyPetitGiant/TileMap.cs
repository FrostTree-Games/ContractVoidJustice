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
            WallUnidentified = -1,
            NoWall = 0,
            TestWall = 1,
            WallA = 2,
            WallB = 3,
            WallC = 4,
            WallD = 5,
            WallE = 6,
            WallF = 7,
            WallG = 8,
            WallH = 9,
            WallI = 10,
            WallJ = 11,
            WallK = 12,
            WallL = 13,
            WallM = 14,
            WallN = 15,
            WallO = 16,
            WallP = 17,
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
        public TileType[,] Map { get { return map; } }
        private FloorType[,] floorMap = null;

        private Vector2 startPosition;
        public Vector2 StartPosition { get { return startPosition; } }
        private Vector2 endFlagPosition = new Vector2(-1, -1);
        public Vector2 EndFlagPosition { get { return endFlagPosition; } }

        private Texture2D tileSkin = null;
        public Texture2D TileSkin { get { return tileSkin; } set { tileSkin = value; } }

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

                                startPosition.X = (i * GlobalGameConstants.TilesPerRoomWide * GlobalGameConstants.TileSize.X) + ((GlobalGameConstants.TilesPerRoomWide / 2) * GlobalGameConstants.TileSize.X);
                                startPosition.Y = (j * GlobalGameConstants.TilesPerRoomHigh * GlobalGameConstants.TileSize.Y) + ((GlobalGameConstants.TilesPerRoomHigh / 2) * GlobalGameConstants.TileSize.Y);
                            }

                            if (room[i, j].intensity > 0.95f)
                            {
                                endFlagPosition.X = (i * GlobalGameConstants.TilesPerRoomWide * GlobalGameConstants.TileSize.X) + ((GlobalGameConstants.TilesPerRoomWide / 2) * GlobalGameConstants.TileSize.X);
                                endFlagPosition.Y = (j * GlobalGameConstants.TilesPerRoomHigh * GlobalGameConstants.TileSize.Y) + ((GlobalGameConstants.TilesPerRoomHigh / 2) * GlobalGameConstants.TileSize.Y);
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
                        //fill a room with empty tiles if it is empty
                        for (int p = 0; p < GlobalGameConstants.TilesPerRoomWide; p++)
                        {
                            for (int q = 0; q < GlobalGameConstants.TilesPerRoomHigh; q++)
                            {
                                map[(i * GlobalGameConstants.TilesPerRoomWide) + p, (j * GlobalGameConstants.TilesPerRoomHigh) + q] = TileType.TestWall;
                            }
                        }
                    }
                }
            }

            convertTestWallsToWalls();
        }

        /// <summary>
        /// Takes all of the tiles labelled "TestWall" in the map and converts them into walls depending on their adjacency
        /// </summary>
        private void convertTestWallsToWalls()
        {
            for (int i = 0; i < map.GetLength(0); i++)
            {
                for (int j = 0; j < map.GetLength(1); j++)
                {
                    if (map[i, j] == TileType.TestWall)
                    {
                        byte adjacencyValue = 0;
                        byte flag = 1;

                        for (int l = j - 1; l < j + 2; l++)
                        {
                            for (int k = i - 1; k < i + 2; k++)
                            {
                                if (k == i && l == j)
                                {
                                    continue;
                                }

                                if (k < 0 || k >= map.GetLength(0) || l < 0 || l >= map.GetLength(1))
                                {
                                    flag <<= 1;
                                    continue;
                                }

                                if (map[k, l] != TileType.NoWall)
                                {
                                    adjacencyValue |= flag;
                                }

                                flag <<= 1;
                            }
                        }

                        switch (adjacencyValue)
                        {
                            case 43:
                            case 47:
                            case 39:
                            case 11:
                            case 15:
                                map[i, j] = TileType.WallM;
                                break;
                            case 22:
                            case 150:
                            case 151:
                            case 23:
                                map[i, j] = TileType.WallK;
                                break;
                            case 208:
                            case 212:
                            case 240:
                            case 244:
                                map[i, j] = TileType.WallA;
                                break;
                            case 104:
                            case 105:
                            case 232:
                            case 233:
                                map[i, j] = TileType.WallC;
                                break;
                            case 248:
                            case 249:
                            case 252:
                            case 253:
                                map[i, j] = TileType.WallB;
                                break;
                            case 31:
                            case 63:
                            case 159:
                            case 191:
                                map[i, j] = TileType.WallL;
                                break;
                            case 214:
                            case 215:
                            case 246:
                            case 247:
                                map[i, j] = TileType.WallF;
                                break;
                            case 107:
                            case 111:
                            case 235:
                            case 239:
                                map[i, j] = TileType.WallH;
                                break;
                            case 251:
                                map[i, j] = TileType.WallI;
                                break;
                            case 127:
                                map[i, j] = TileType.WallD;
                                break;
                            case 254:
                                map[i, j] = TileType.WallJ;
                                break;
                            case 223:
                                map[i, j] = TileType.WallE; //lol pixar
                                break;
                            case 255:
                                map[i, j] = TileType.WallG;
                                break;
                            default:
                                map[i, j] = TileType.WallUnidentified;
                                break;
                        }
                    }
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
                    int tileX;
                    int tileY;

                    switch (map[i, j])
                    {
                        case TileType.NoWall:
                            break;
                        case TileType.TestWall:
                            if (tileSkin == null)
                                spriteBatch.Draw(Game1.whitePixel, new Vector2(i * tileSize.X, j * tileSize.Y), null, Color.Blue, 0.0f, Vector2.Zero, new Vector2(tileSize.X, tileSize.Y), SpriteEffects.None, depth);
                            break;
                        case TileType.WallA:
                        case TileType.WallB:
                        case TileType.WallC:
                        case TileType.WallD:
                        case TileType.WallE:
                        case TileType.WallF:
                        case TileType.WallG:
                        case TileType.WallH:
                        case TileType.WallI:
                        case TileType.WallJ:
                        case TileType.WallK:
                        case TileType.WallL:
                        case TileType.WallM:
                        case TileType.WallN:
                        case TileType.WallO:
                        case TileType.WallP:
                            tileX = ((int)(map[i, j]) - 2) % 5;
                            tileY = ((int)(map[i, j]) - 2) / 5;
                            spriteBatch.Draw(tileSkin, new Rectangle((int)(i * tileSize.X), (int)(j * tileSize.Y), (int)(GlobalGameConstants.TileSize.X), (int)(GlobalGameConstants.TileSize.Y)), new Rectangle((int)(tileX * GlobalGameConstants.TileSize.X + tileX), (int)(tileY * GlobalGameConstants.TileSize.Y + tileY), (int)(GlobalGameConstants.TileSize.X), (int)(GlobalGameConstants.TileSize.Y)), Color.White);
                            break;
                        case TileType.WallUnidentified:
                        default:
                            spriteBatch.Draw(tileSkin, new Rectangle((int)(i * tileSize.X), (int)(j * tileSize.Y), (int)(GlobalGameConstants.TileSize.X), (int)(GlobalGameConstants.TileSize.Y)), new Rectangle((int)(1 * GlobalGameConstants.TileSize.X + 1), (int)(6 * GlobalGameConstants.TileSize.Y + 6), (int)(GlobalGameConstants.TileSize.X), (int)(GlobalGameConstants.TileSize.Y)), Color.White);
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

                for (int i = tilesPositionPlusHitBox.y; i > 0 && i < size.y && !breakLoop; i--)
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
