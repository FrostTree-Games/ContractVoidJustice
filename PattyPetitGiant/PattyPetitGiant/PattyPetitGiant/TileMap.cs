using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace PattyPetitGiant
{
    class TileMap
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

        private TileDimensions size;
        public TileDimensions SizeInTiles { get { return size; } }
        public Vector2 SizeInPixels { get { return new Vector2(size.x * tileSize.X, size.y * tileSize.Y); } }

        private Vector2 tileSize;
        public Vector2 TileSize { get { return tileSize; } }

        private TileType[,] map = null;

        public TileMap(TileDimensions size, Vector2 tileSize)
        {
            this.size = size;
            this.tileSize = tileSize;

            map = new TileType[size.x, size.y];
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
                    switch (map[i, j])
                    {
                        case TileType.NoWall:
                            break;
                        case TileType.TestWall:
                            spriteBatch.Draw(Game1.whitePixel, new Vector2(i * tileSize.X, j * tileSize.Y), null, Color.Blue, 0.0f, Vector2.Zero, new Vector2(tileSize.X, tileSize.Y), SpriteEffects.None, depth);
                            break;
                        default:
                            spriteBatch.Draw(Game1.whitePixel, new Vector2(i * tileSize.X, j * tileSize.Y), null, Color.Red, 0.0f, Vector2.Zero, new Vector2(tileSize.X, tileSize.Y), SpriteEffects.None, depth);
                            break;
                    }
                }
            }
        }

        /// <summary>
        ///  Hit-checks a point against the map. Will return false if point is outside the bounds of the map.
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
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
    }
}
