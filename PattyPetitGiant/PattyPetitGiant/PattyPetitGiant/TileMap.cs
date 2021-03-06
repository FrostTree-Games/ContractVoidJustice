﻿//#define PROTOTYPE_ROOMS

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

        public enum WallMod
        {
            None = 0,
            Mod1 = 1,
            Mod2 = 2,
            Mod3 = 3,
            Elevator = 4,
        }

        public enum FloorType
        {
            A = 0,
            B = 1,
            C = 2,
            D = 3,
            E = 4,
            F = 5,
            G = 6,
            H = 7,
            Elevator = 8,
        }

        private LevelState parent = null;

        private TileDimensions size;
        public TileDimensions SizeInTiles { get { return size; } }
        public Vector2 SizeInPixels { get { return new Vector2(size.x * tileSize.X, size.y * tileSize.Y); } }

        private Vector2 tileSize;
        public Vector2 TileSize { get { return tileSize; } }

        private TileType[,] map = null;
        public TileType[,] Map { get { return map; } }
        public FloorType[,] floorMap = null;
        public WallMod[,] mapMod = null;

        private Vector2 startPosition;
        public Vector2 StartPosition { get { return startPosition; } }
        private Vector2 endFlagPosition = new Vector2(-1, -1);
        public Vector2 EndFlagPosition { get { return endFlagPosition; } }

        private Texture2D[] tileSkin = null;
        public Texture2D[] TileSkin { get { return tileSkin; } set { tileSkin = value; } }

        public Texture2D ElevatorRoomSkin { get { return tileSkin[4]; } set { tileSkin[4] = value; } }

        public TileMap(LevelState parent, DungeonGenerator.DungeonRoom[,] room, Vector2 tileSize)
        {
            tileSkin = new Texture2D[5];

            this.size = new TileDimensions(room.GetLength(0) * GlobalGameConstants.TilesPerRoomWide, room.GetLength(1)  * GlobalGameConstants.TilesPerRoomHigh);
            this.tileSize = tileSize;

            this.parent = parent;

            map = new TileType[size.x, size.y];
            floorMap = new FloorType[size.x, size.y];
            mapMod = new WallMod[size.x, size.y];

            Random rand = new Random();

            for (int i = 0; i < floorMap.GetLength(0); i++)
            {
                for (int j = 0; j < floorMap.GetLength(1); j++)
                {
                    int randVal = Game1.rand.Next() % 100;

                    if (randVal > 15)
                    {
                        floorMap[i, j] = Game1.rand.Next() % 2 == 0 ? FloorType.A : FloorType.B;
                    }
                    else if (randVal > 10)
                    {
                        floorMap[i, j] = Game1.rand.Next() % 2 == 0 ? FloorType.C : FloorType.D;
                    }
                    else if (randVal > 5)
                    {
                        floorMap[i, j] = Game1.rand.Next() % 2 == 0 ? FloorType.E : FloorType.F;
                    }
                    else
                    {
                        floorMap[i, j] = Game1.rand.Next() % 2 == 0 ? FloorType.G : FloorType.H;
                    }
                }
            }

            for (int i = 0; i < room.GetLength(0); i++)
            {
                for (int j = 0; j < room.GetLength(1); j++)
                {
                    if (room[i, j].attributes != null)
                    {
                        ChunkManager.Chunk c = ChunkLib.getRandomChunkByValues(room[i, j].attributes.ToArray(), rand);

                        if (c == null)
                        {
                            throw new Exception("NO CHUNK FOR VALUES");
                        }

                        if (c != null)
                        {
                            foreach (ChunkManager.Chunk.ChunkAttribute attr in c.Attributes)
                            {
                                if (!room[i, j].attributes.Contains(attr.AttributeName))
                                {
                                    room[i, j].attributes.Add(attr.AttributeName);
                                }
                            }

                            if (c != null)
                            {
                                for (int p = 0; p < GlobalGameConstants.TilesPerRoomWide; p++)
                                {
                                    for (int q = 0; q < GlobalGameConstants.TilesPerRoomHigh; q++)
                                    {
                                        map[(i * GlobalGameConstants.TilesPerRoomWide) + p, (j * GlobalGameConstants.TilesPerRoomHigh) + q] = (TileType)(c.tilemap[(q * GlobalGameConstants.TilesPerRoomHigh) + p]);
                                    }
                                }

                                room[i, j].chunkName = c.Name;

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

                        int modRandVal = Game1.rand.Next() % 100;
                        if (modRandVal > 40)
                        {
                            mapMod[i, j] = WallMod.None;
                        }
                        else if (modRandVal > 25)
                        {
                            mapMod[i, j] = WallMod.Mod1;
                        }
                        else if (modRandVal > 10)
                        {
                            mapMod[i, j] = WallMod.Mod2;
                        }
                        else
                        {
                            mapMod[i, j] = WallMod.Mod3;
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

        public void renderSPINEBATCHTEST(Spine.SkeletonRenderer sb, float depth, List<MutantAcidSpitter> spitters)
        {
            int focusTileX = (int)(parent.CameraFocus.CenterPoint.X / GlobalGameConstants.TileSize.X);
            int focusTileY = (int)(parent.CameraFocus.CenterPoint.Y / GlobalGameConstants.TileSize.Y);

            for (int j = Math.Max(0, focusTileY - 8); j < Math.Min(size.y, focusTileY + 9); j++)
            {
                for (int i = Math.Max(0, focusTileX - 14); i < Math.Min(size.x, focusTileX + 15); i++)
                {
                    if (map[i, j] != TileType.NoWall)
                    {
                        continue;
                    }

                    if (floorMap[i, j] == FloorType.Elevator)
                    {
                        sb.DrawSpriteToSpineVertexArray(tileSkin[4], new Rectangle((int)(3 * GlobalGameConstants.TileSize.X + 3), (int)(2 * GlobalGameConstants.TileSize.Y + 3), (int)(GlobalGameConstants.TileSize.X), (int)(GlobalGameConstants.TileSize.Y)), new Vector2((int)(i * tileSize.X), (int)(j * tileSize.Y)));
                    }
                    else
                    {
                        switch (floorMap[i, j])
                        {
                            case FloorType.A:
                                sb.DrawSpriteToSpineVertexArray(tileSkin[0], new Rectangle((int)(3 * GlobalGameConstants.TileSize.X + 3), (int)(2 * GlobalGameConstants.TileSize.Y + 3), (int)(GlobalGameConstants.TileSize.X), (int)(GlobalGameConstants.TileSize.Y)), new Vector2((int)(i * tileSize.X), (int)(j * tileSize.Y)));
                                break;
                            case FloorType.B:
                                sb.DrawSpriteToSpineVertexArray(tileSkin[0], new Rectangle((int)(4 * GlobalGameConstants.TileSize.X + 4), (int)(2 * GlobalGameConstants.TileSize.Y + 3), (int)(GlobalGameConstants.TileSize.X), (int)(GlobalGameConstants.TileSize.Y)), new Vector2((int)(i * tileSize.X), (int)(j * tileSize.Y)));
                                break;
                            case FloorType.C:
                                sb.DrawSpriteToSpineVertexArray(tileSkin[1], new Rectangle((int)(3 * GlobalGameConstants.TileSize.X + 3), (int)(2 * GlobalGameConstants.TileSize.Y + 3), (int)(GlobalGameConstants.TileSize.X), (int)(GlobalGameConstants.TileSize.Y)), new Vector2((int)(i * tileSize.X), (int)(j * tileSize.Y)));
                                break;
                            case FloorType.D:
                                sb.DrawSpriteToSpineVertexArray(tileSkin[1], new Rectangle((int)(4 * GlobalGameConstants.TileSize.X + 4), (int)(2 * GlobalGameConstants.TileSize.Y + 3), (int)(GlobalGameConstants.TileSize.X), (int)(GlobalGameConstants.TileSize.Y)), new Vector2((int)(i * tileSize.X), (int)(j * tileSize.Y)));
                                break;
                            case FloorType.E:
                                sb.DrawSpriteToSpineVertexArray(tileSkin[2], new Rectangle((int)(3 * GlobalGameConstants.TileSize.X + 3), (int)(2 * GlobalGameConstants.TileSize.Y + 3), (int)(GlobalGameConstants.TileSize.X), (int)(GlobalGameConstants.TileSize.Y)), new Vector2((int)(i * tileSize.X), (int)(j * tileSize.Y)));
                                break;
                            case FloorType.F:
                                sb.DrawSpriteToSpineVertexArray(tileSkin[2], new Rectangle((int)(4 * GlobalGameConstants.TileSize.X + 4), (int)(2 * GlobalGameConstants.TileSize.Y + 3), (int)(GlobalGameConstants.TileSize.X), (int)(GlobalGameConstants.TileSize.Y)), new Vector2((int)(i * tileSize.X), (int)(j * tileSize.Y)));
                                break;
                            case FloorType.G:
                                sb.DrawSpriteToSpineVertexArray(tileSkin[3], new Rectangle((int)(3 * GlobalGameConstants.TileSize.X + 3), (int)(2 * GlobalGameConstants.TileSize.Y + 3), (int)(GlobalGameConstants.TileSize.X), (int)(GlobalGameConstants.TileSize.Y)), new Vector2((int)(i * tileSize.X), (int)(j * tileSize.Y)));
                                break;
                            case FloorType.H:
                                sb.DrawSpriteToSpineVertexArray(tileSkin[3], new Rectangle((int)(4 * GlobalGameConstants.TileSize.X + 4), (int)(2 * GlobalGameConstants.TileSize.Y + 3), (int)(GlobalGameConstants.TileSize.X), (int)(GlobalGameConstants.TileSize.Y)), new Vector2((int)(i * tileSize.X), (int)(j * tileSize.Y)));
                                break;
                        }
                    }
                }
            }

            for (int i = 0; i < spitters.Count; i++)
            {
                spitters[i].drawAcidSplotches(sb);
            }

            for (int j = Math.Max(0, focusTileY - 8); j < Math.Min(size.y, focusTileY + 9); j++)
            {
                for (int i = Math.Max(0, focusTileX - 14); i < Math.Min(size.x, focusTileX + 15); i++)
                {
                    int tileX;
                    int tileY;

                    switch (map[i, j])
                    {
                        case TileType.NoWall:
                            break;
                        case TileType.TestWall:
                            sb.DrawSpriteToSpineVertexArray(tileSkin[0], new Rectangle((int)(1 * GlobalGameConstants.TileSize.X + 1), (int)(6 * GlobalGameConstants.TileSize.Y + 6), (int)(GlobalGameConstants.TileSize.X), (int)(GlobalGameConstants.TileSize.Y)), new Vector2((int)(i * tileSize.X), (int)(j * tileSize.Y)));
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
                            sb.DrawSpriteToSpineVertexArray(tileSkin[(int)mapMod[i , j]], new Rectangle((int)(tileX * GlobalGameConstants.TileSize.X + tileX), (int)(tileY * GlobalGameConstants.TileSize.Y + tileY), (int)(GlobalGameConstants.TileSize.X), (int)(GlobalGameConstants.TileSize.Y)), new Vector2((int)(i * tileSize.X), (int)(j * tileSize.Y)));
                            break;
                        case TileType.WallUnidentified:
                        default:
                            sb.DrawSpriteToSpineVertexArray(tileSkin[0], new Rectangle((int)(1 * GlobalGameConstants.TileSize.X + 1), (int)(6 * GlobalGameConstants.TileSize.Y + 6), (int)(GlobalGameConstants.TileSize.X), (int)(GlobalGameConstants.TileSize.Y)), new Vector2((int)(i * tileSize.X), (int)(j * tileSize.Y)));    
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Checks to see if there is a wall between player and enemy
        /// </summary>
        /// <param name="angle"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public bool playerInSight(float angle, float distance, Entity enemy, Entity player)
        {
            float Xa = 0.0f;
            float Ya = 0.0f;
            Vector2 ray_position = enemy.CenterPoint;
            float ray_travelled = 0.0f;
            float ray_displacement = 0.0f;
            bool ray_hit_object = hitTestWall(ray_position);

            switch (enemy.Direction_Facing)
            {
                case GlobalGameConstants.Direction.Right:
                    Xa = GlobalGameConstants.TileSize.X / 2;
                    Ya = (float)(Xa * Math.Tan(angle));
                    break;
                case GlobalGameConstants.Direction.Left:
                    Xa = GlobalGameConstants.TileSize.X / 2;
                    Xa = -1 * Xa;
                    Ya = (float)(Xa * Math.Tan(angle));
                    break;
                case GlobalGameConstants.Direction.Up:
                    Ya = GlobalGameConstants.TileSize.Y / 2;
                    Ya = -1 * Ya;
                    Xa = (float)(Ya / Math.Tan(angle));
                    break;
                default:
                    Ya = GlobalGameConstants.TileSize.Y / 2;
                    Xa = (float)(Ya / Math.Tan(angle));
                    break;
            }
            ray_displacement = (float)(Math.Sqrt((Xa*Xa)+(Ya*Ya)));

            while (ray_hit_object == false )
            {
                if (ray_travelled >= distance)
                {
                    ray_displacement = 0.0f;
                    return ray_hit_object;
                }
                else
                {
                    ray_position += new Vector2(Xa, Ya);
                    ray_travelled += ray_displacement;
                    ray_hit_object = hitTestWall(ray_position);
                }
            }
            ray_displacement = 0.0f;
            return ray_hit_object;
        }
        /// <summary>
        /// Checks to see if there is anything between the position of the enemy and the sound that is produced.
        /// Can assume that if there is nothing between the two positions that the position is in sight
        /// </summary>
        /// <param name="current_enemy"></param>
        /// <param name="sound_position"></param>
        /// <returns></returns>
        public bool soundInSight(Entity current_enemy, Vector2 sound_position)
        {
            bool ray_hit_sound = false;
            float angle = (float)(Math.Atan2(sound_position.Y - current_enemy.CenterPoint.Y, sound_position.X - current_enemy.CenterPoint.X));

            float Xa = 0.0f;
            float Ya = 0.0f;

            //determines the step values that the ray travels
            //right
            if(angle <= Math.PI/4 && angle > -1 * Math.PI/4)
            {
                Xa = GlobalGameConstants.TileSize.X / 4;
                Ya = (float)(Xa * Math.Tan(angle));
            }
                //left
            else if( angle > 3*Math.PI/4 || angle< -3 * Math.PI/4)
            {
                Xa = GlobalGameConstants.TileSize.X / 4;
                Xa = -1 * Xa;
                Ya = (float)(Xa * Math.Tan(angle));
            }
                //up
            else if (angle > -3 * Math.PI / 4 && angle < -1 * Math.PI / 4)
            {
                Ya = GlobalGameConstants.TileSize.Y / 4;
                Ya = -1 * Ya;
                Xa = (float)(Ya / Math.Tan(angle));
            }
                // down
            else if (angle < 3 * Math.PI / 4 && angle > Math.PI/4)
            {
                Ya = GlobalGameConstants.TileSize.Y / 4;
                Xa = (float)(Ya / Math.Tan(angle));
            }
                    
            Vector2 ray_travel = current_enemy.CenterPoint;
            float distance = Vector2.Distance(current_enemy.Position, sound_position);
            float ray_displacement = (float)(Math.Sqrt((Xa * Xa) + (Ya * Ya)));
            float current_distance = 0.0f;

            while (current_distance < distance)
            {
                if (ray_hit_sound)
                {
                    return ray_hit_sound;
                }
                else
                {
                    ray_travel += new Vector2(Xa, Ya);
                    current_distance += ray_displacement;
                    ray_hit_sound = hitTestWall(ray_travel);
                }
            }

            ray_displacement = 0.0f;
            current_enemy.Velocity = new Vector2(2.0f * (float)(Math.Cos(angle)), 2.0f * (float)Math.Sin(angle));
            if (Math.Abs(current_enemy.Velocity.X) > Math.Abs(current_enemy.Velocity.Y))
            {
                if (current_enemy.Velocity.X > 0)
                {
                    current_enemy.Direction_Facing = GlobalGameConstants.Direction.Right;
                }
                else
                {
                    current_enemy.Direction_Facing = GlobalGameConstants.Direction.Left;
                }
            }
            else
            {
                if (current_enemy.Velocity.Y > 0)
                {
                    current_enemy.Direction_Facing = GlobalGameConstants.Direction.Down;
                }
                else
                {
                    current_enemy.Direction_Facing = GlobalGameConstants.Direction.Up;
                }
            }

            return ray_hit_sound;
        }

        public bool enemyWithinRange(Entity enemy_found, Entity current_entity, float current_radius)
        {
            if (enemy_found != null)
            {
                float angle = (float)(Math.Atan2(enemy_found.CenterPoint.Y - current_entity.CenterPoint.Y, enemy_found.CenterPoint.X - current_entity.CenterPoint.X));
                float distance = Vector2.Distance(enemy_found.CenterPoint, current_entity.CenterPoint);

                if ((angle > (-1*Math.PI/4) && angle < (Math.PI/4)))
                {
                    current_entity.Direction_Facing = GlobalGameConstants.Direction.Right;
                }
                else if (((angle > (3*Math.PI/4) || angle < (-3*Math.PI/4))))
                {
                    current_entity.Direction_Facing = GlobalGameConstants.Direction.Left;
                }
                else if ((angle > (-3 * Math.PI/4) && angle < (-1 * Math.PI/4)))
                {
                    current_entity.Direction_Facing = GlobalGameConstants.Direction.Up;
                }
                else if ((angle > Math.PI/4 && angle < 3*Math.PI/4))
                {
                    current_entity.Direction_Facing = GlobalGameConstants.Direction.Down;
                }

                bool enemy_in_sight = playerInSight(angle, current_radius, current_entity, enemy_found);

                if (!enemy_in_sight)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        ///  Hit-checks a point against the map. Useful for ray-casting or mouse clicks.
        /// </summary>
        /// <param name="position"></param>
        /// <returns>True if position overlaps a solid wall tile, false if otherwise.</returns>
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
        /// Determines if a moved hitbox intersects with a tilemap. Gives a new, valid solution if the new position is not valid.
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

        //may get rid of needs further discussion
        public Vector2 CalculateBezierPoint(float t, Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3)
        {
            float u = 1 - t;
            float tt = t * t;
            float uu = u * u;
            float uuu = uu * u;
            float ttt = tt * t;

            Vector2 p = uuu * p0; //first term
            p += 3 * uu * t * p1; //2nd term
            p += 3 * u * tt * p2; //third term
            p += ttt * p3;
            return p;
        }
    }
}
