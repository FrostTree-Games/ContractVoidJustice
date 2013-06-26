using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace PattyPetitGiant
{
    public class DungeonGenerator
    {
        private const float probability_connectOldRooms = 0.145f;

        public class DungeonGeneratonValues
        {
            /// <summary>
            /// Probability that a shopkeeper will be placed in a dead end room.
            /// </summary>
            public static float ShopkeeperProbability { get { return shopkeeperProbability; } }
            private static float shopkeeperProbability = 0.210312f;
        }

        public struct RoomColors
        {
            public bool Red;
            public bool Green;
            public bool Blue;
            public bool Purple;
            public bool Gray;

            public RoomColors(RoomColors old)
            {
                this.Red = old.Red;
                this.Green = old.Green;
                this.Blue = old.Blue;
                this.Purple = old.Purple;
                this.Gray = old.Gray;
            }

            public RoomColors(bool Red, bool Green, bool Blue, bool Purple, bool Gray)
            {
                this.Red = Red;
                this.Green = Green;
                this.Blue = Blue;
                this.Purple = Purple;
                this.Gray = Gray;
            }

            public int KeyFlagCount
            {
                get
                {
                    int output = 0;
                    output += Convert.ToInt32(this.Red);
                    output += Convert.ToInt32(this.Blue);
                    output += Convert.ToInt32(this.Green);
                    output += Convert.ToInt32(this.Purple);
                    output += Convert.ToInt32(this.Gray);
                    return output;
                }
            }

            public static RoomColors operator +(RoomColors a, RoomColors b)
            {
                return new RoomColors(a.Red || b.Red, a.Green || b.Green, a.Blue || b.Blue, a.Purple || b.Purple, a.Gray || b.Gray);
            }

            public static bool operator ==(RoomColors a, RoomColors b)
            {
                return ((a.Red == b.Red) && (a.Green == b.Green) && (a.Blue == b.Blue) && (a.Purple == b.Purple) && (a.Gray == b.Gray));
            }

            public static bool operator !=(RoomColors a, RoomColors b)
            {
                return ((a.Red != b.Red) || (a.Green != b.Green) || (a.Blue != b.Blue) || (a.Purple != b.Purple) || (a.Gray != b.Gray));
            }

            public static bool operator >(RoomColors a, RoomColors b)
            {
                return a.KeyFlagCount > b.KeyFlagCount;
            }

            public override bool Equals(object obj)
            {
                return base.Equals(obj);
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }

            public static bool operator <(RoomColors a, RoomColors b)
            {
                return a.KeyFlagCount < b.KeyFlagCount;
            }
        }

        private class DungeonRoomClass
        {
            public enum ChildDirection
            {
                North = 0,
                East = 1,
                South = 2,
                West = 3,
            }

            private DungeonRoomClass parent = null;

            private DungeonRoomClass[] children = null;
            public DungeonRoomClass[] Children { get { return children; } }

            private List<string> attributes = null;
            public List<String> Attributes { get { return attributes; } }

            private int x, y;
            public int X { get { return x; } }
            public int Y { get { return y; } }

            private float intensity;
            public float Intensity { get { return intensity; } set { intensity = value; } }

            public RoomColors colors;

            public int ActualChildCount
            {
                get
                {
                    if (children == null)
                    {
                        return 0;
                    }

                    int output = 0;

                    for (int i = 0; i < children.Length; i++)
                    {
                        if (children[i] != null)
                        {
                            output++;
                        }
                    }

                    return output;
                }
            }

            public List<DungeonRoomClass> adjacentChildren(DungeonRoomClass room)
            {
                List<DungeonRoomClass> output = new List<DungeonRoomClass>();

                for (int i = 0; i < room.Children.Length; i++)
                {
                    if (room.Children[i] == null)
                    {
                        output.Add(room.Children[i]);
                    }
                }

                return output;
            }

            public DungeonRoomClass()
            {
            }

            public DungeonRoomClass(DungeonRoomClass parent, int X, int Y)
            {
                this.parent = parent;
                this.children = new DungeonRoomClass[4];

                x = X;
                y = Y;

                if (parent != null)
                {
                    this.colors = parent.colors;
                }

                intensity = 999;

                this.attributes = new List<string>();
            }

            public virtual DungeonRoom outputStruct()
            {
                DungeonRoom output = new DungeonRoom();

                output.visited = false;

                if (children[(int)ChildDirection.North] != null)
                {
                    output.north = true;
                }
                if (children[(int)ChildDirection.East] != null)
                {
                    output.east = true;
                }
                if (children[(int)ChildDirection.South] != null)
                {
                    output.south = true;
                }
                if (children[(int)ChildDirection.West] != null)
                {
                    output.west = true;
                }

                output.attributes = this.attributes;
                Debug.Assert(output.attributes != null);

                output.intensity = this.intensity;

                output.colors = this.colors;

                if (output.north) { output.attributes.Add("north"); }
                if (output.south) { output.attributes.Add("south"); }
                if (output.east) { output.attributes.Add("east"); }
                if (output.west) { output.attributes.Add("west"); }

                return output;
            }
        }

        private class NullRoomSpace : DungeonRoomClass
        {
            public NullRoomSpace()
            {
                //
            }
        }

        public struct DungeonRoom
        {
            public bool north;
            public bool south;
            public bool west;
            public bool east;

            public float intensity;

            public bool visited;

            public RoomColors colors;

            public List<string> attributes;
        }

        private static int recurseDungeonIntensity(DungeonRoomClass room, int intensity)
        {
            if (room == null || room.Intensity < intensity)
            {
                return 0;
            }

            room.Intensity = intensity;

            return Math.Max(intensity, Math.Max(Math.Max(recurseDungeonIntensity(room.Children[0], intensity + 1), recurseDungeonIntensity(room.Children[1], intensity + 1)), Math.Max(recurseDungeonIntensity(room.Children[2], intensity + 1), recurseDungeonIntensity(room.Children[3], intensity + 1))));
        }

        private static void computeDungeonIntensity(DungeonRoomClass[,] dungeonModel, int startingRoomX, int startingRoomY)
        {
            if (dungeonModel == null)
            {
                throw new Exception("No dungeon model passed into intensity");
            }

            recurseDungeonIntensity(dungeonModel[startingRoomX, startingRoomY], 0);

            float maxIntensity = 0;

            for (int i = 0; i < dungeonModel.GetLength(0); i++)
            {
                for (int j = 0; j < dungeonModel.GetLength(1); j++)
                {
                    if (dungeonModel[i, j] != null)
                    {
                        if (dungeonModel[i, j].Intensity > maxIntensity)
                        {
                            maxIntensity = dungeonModel[i, j].Intensity;
                        }
                    }
                }
            }

            for (int i = 0; i < dungeonModel.GetLength(0); i++)
            {
                for (int j = 0; j < dungeonModel.GetLength(1); j++)
                {
                    if (dungeonModel[i, j] == null)
                    {
                        continue;
                    }

                    dungeonModel[i, j].Intensity /= maxIntensity;
                }
            }
        }

        public static DungeonRoom[,] generateEntityZoo()
        {
            DungeonRoom[,] output = new DungeonRoom[5, 5];

            //shopkeeper 
            output[1, 0].north = false;
            output[1, 0].south = true;
            output[1, 0].east = false;
            output[1, 0].west = false;
            output[1, 0].attributes = new List<string>();
            output[1, 0].attributes.Add("south");
            output[1, 0].attributes.Add("shopkeeper");

            //player spawn below
            output[1, 1].north = true;
            output[1, 1].south = false;
            output[1, 1].east = true;
            output[1, 1].west = true;
            output[1, 1].attributes = new List<string>();
            output[1, 1].attributes.Add("start");
            output[1, 1].attributes.Add("north");
            output[1, 1].attributes.Add("east");
            output[1, 1].attributes.Add("west");

            //exit
            output[0, 1].north = false;
            output[0, 1].south = false;
            output[0, 1].east = true;
            output[0, 1].west = false;
            output[0, 1].attributes = new List<string>();
            output[0, 1].attributes.Add("end");
            output[0, 1].attributes.Add("east");

            return output;
        }

        public static DungeonRoom[,] generateRoomData(int desiredWidth, int desiredHeight)
        {
            return generateRoomData(desiredWidth, desiredHeight, Game1.rand.Next());
        }

        public static DungeonRoom[,] generateRoomData(int desiredWidth, int desiredHeight, string seed)
        {
            return generateRoomData(desiredWidth, desiredHeight, seed.GetHashCode());
        }

        /// <summary>
        /// Given a room, will check if all the prescribed theme rooms are contained
        /// </summary>
        /// <param name="startRoom">Room to start the BFS from</param>
        /// <param name="themeRooms">List of requisite rooms to find</param>
        /// <returns></returns>
        private static bool isRoomFullyConnected(DungeonRoomClass startRoom, List<DungeonRoomClass> themeRooms)
        {
            Queue<DungeonRoomClass> bfsQueue = new Queue<DungeonRoomClass>();
            List<DungeonRoomClass> foundRooms = new List<DungeonRoomClass>();

            bfsQueue.Enqueue(startRoom);

            while (bfsQueue.Count > 0)
            {
                DungeonRoomClass room = bfsQueue.Dequeue();

                for (int i = 0; i < room.Children.Length; i++)
                {
                    if (room.Children[i] == null) { continue; }

                    if (foundRooms.Contains(room.Children[i])) { continue; }

                    foundRooms.Add(room.Children[i]);
                    bfsQueue.Enqueue(room.Children[i]);
                }
            }

            List<DungeonRoomClass> foundThemeRooms = new List<DungeonRoomClass>();
            foreach (DungeonRoomClass d in foundRooms)
            {
                if (d.Attributes.Count > 0)
                {
                    foundThemeRooms.Add(d);
                }
            }

            bool foundAllThemeRooms = true;

            foreach (DungeonRoomClass d in themeRooms)
            {
                foundAllThemeRooms = foundAllThemeRooms && foundThemeRooms.Contains(d);
            }

            return foundAllThemeRooms;
        }

        private static DungeonRoomClass addRoom(DungeonRoomClass room, List<DungeonRoomClass> addedRooms, DungeonRoomClass[,] model, Random rand)
        {
            DungeonRoomClass output = null;

            //pick a random direction to place a new room
            DungeonRoomClass.ChildDirection newDir = (DungeonRoomClass.ChildDirection)(rand.Next() % 4);

            if ((newDir == DungeonRoomClass.ChildDirection.North && room.Y == 0) || (newDir == DungeonRoomClass.ChildDirection.West && room.X == 0) || (newDir == DungeonRoomClass.ChildDirection.South && room.Y == model.GetLength(1) - 1) || (newDir == DungeonRoomClass.ChildDirection.East && room.X == model.GetLength(0) - 1))
            {
                return room;
            }

            if (room.Children[(int)newDir] != null)
            {
                return room;
            }

            if ((newDir == DungeonRoomClass.ChildDirection.North && model[room.X, room.Y - 1] != null) || (newDir == DungeonRoomClass.ChildDirection.West && model[room.X - 1, room.Y] != null) || (newDir == DungeonRoomClass.ChildDirection.South && model[room.X, room.Y + 1] != null) || (newDir == DungeonRoomClass.ChildDirection.East && model[room.X + 1, room.Y] != null))
            {
                switch (newDir)
                {
                    case DungeonRoomClass.ChildDirection.North:
                        room.Children[(int)newDir] = model[room.X, room.Y - 1];
                        model[room.X, room.Y - 1].Children[(int)DungeonRoomClass.ChildDirection.South] = room;
                        break;
                    case DungeonRoomClass.ChildDirection.South:
                        room.Children[(int)newDir] = model[room.X, room.Y + 1];
                        model[room.X, room.Y + 1].Children[(int)DungeonRoomClass.ChildDirection.North] = room;
                        break;
                    case DungeonRoomClass.ChildDirection.East:
                        room.Children[(int)newDir] = model[room.X + 1, room.Y];
                        model[room.X + 1, room.Y].Children[(int)DungeonRoomClass.ChildDirection.West] = room;
                        break;
                    case DungeonRoomClass.ChildDirection.West:
                        room.Children[(int)newDir] = model[room.X - 1, room.Y];
                        model[room.X - 1, room.Y].Children[(int)DungeonRoomClass.ChildDirection.East] = room;
                        break;
                }

                return room;
            }
            else
            {
                switch (newDir)
                {
                    case DungeonRoomClass.ChildDirection.North:
                        room.Children[(int)newDir] = new DungeonRoomClass(room, room.X, room.Y - 1);
                        room.Children[(int)newDir].Children[(int)(DungeonRoomClass.ChildDirection.South)] = room;
                        model[room.X, room.Y - 1] = room.Children[(int)newDir];
                        //addedRooms.Add(room.Children[(int)newDir]);
                        break;
                    case DungeonRoomClass.ChildDirection.South:
                        room.Children[(int)newDir] = new DungeonRoomClass(room, room.X, room.Y + 1);
                        room.Children[(int)newDir].Children[(int)(DungeonRoomClass.ChildDirection.North)] = room;
                        model[room.X, room.Y + 1] = room.Children[(int)newDir];
                        //addedRooms.Add(room.Children[(int)newDir]);
                        break;
                    case DungeonRoomClass.ChildDirection.East:
                        room.Children[(int)newDir] = new DungeonRoomClass(room, room.X + 1, room.Y);
                        room.Children[(int)newDir].Children[(int)(DungeonRoomClass.ChildDirection.West)] = room;
                        model[room.X + 1, room.Y] = room.Children[(int)newDir];
                        //addedRooms.Add(room.Children[(int)newDir]);
                        break;
                    case DungeonRoomClass.ChildDirection.West:
                        room.Children[(int)newDir] = new DungeonRoomClass(room, room.X - 1, room.Y);
                        room.Children[(int)newDir].Children[(int)(DungeonRoomClass.ChildDirection.East)] = room;
                        model[room.X - 1, room.Y] = room.Children[(int)newDir];
                        //addedRooms.Add(room.Children[(int)newDir]);
                        break;
                }

                output = room.Children[(int)newDir];
            }

            return output;
        }

        public static DungeonRoom[,] generateRoomData(int desiredWidth, int desiredHeight, int seed)
        {
            DungeonRoom[,] output = new DungeonRoom[desiredWidth, desiredHeight];
            DungeonRoomClass[,] model = new DungeonRoomClass[desiredWidth, desiredHeight];

            Random rand = new Random(seed);

            List<DungeonRoomClass> addedRooms = new List<DungeonRoomClass>();
            List<DungeonRoomClass> themeRooms = new List<DungeonRoomClass>();

            RoomColors colorsRegister = new RoomColors();

            //place initial room
            int randX = GlobalGameConstants.StandardMapSize.x / 2;
            int randY = GlobalGameConstants.StandardMapSize.y - 1;
            DungeonRoomClass startingRoom = new DungeonRoomClass(null, randX, randY);
            startingRoom.Attributes.Add("start");
            addedRooms.Add(startingRoom);
            themeRooms.Add(startingRoom);
            model[randX, randY] = startingRoom;

            randY = rand.Next() % 2;
            randX = rand.Next() % GlobalGameConstants.StandardMapSize.x;
            DungeonRoomClass endingRoom = new DungeonRoomClass(null, randX, randY);
            endingRoom.Attributes.Add("end");
            addedRooms.Add(endingRoom);
            themeRooms.Add(endingRoom);
            model[randX, randY] = endingRoom;

            randY = (rand.Next() % 2) + 2;
            randX = 0;
            DungeonRoomClass shopRoom = new DungeonRoomClass(null, randX, randY);
            shopRoom.Attributes.Add("shopkeeper");
            addedRooms.Add(shopRoom);
            themeRooms.Add(shopRoom);
            model[randX, randY] = shopRoom;

            //randY = (rand.Next() % 2) + 2;
            randX = (rand.Next() % (GlobalGameConstants.StandardMapSize.x - 1)) + 1;
            DungeonRoomClass pickupRoom = new DungeonRoomClass(null, randX, randY);
            pickupRoom.Attributes.Add("pickup");
            addedRooms.Add(pickupRoom);
            themeRooms.Add(pickupRoom);
            model[randX, randY] = pickupRoom;

            while (!isRoomFullyConnected(startingRoom, themeRooms))
            {
                int randRoom = rand.Next() % addedRooms.Count;

                DungeonRoomClass room = addedRooms[randRoom];
                DungeonRoomClass room2 = addRoom(room, addedRooms, model, rand);

                addedRooms.Remove(room);
                addedRooms.Add(room2);
            }

            //compute intensity values
            computeDungeonIntensity(model, startingRoom.X, startingRoom.Y);

            //convert the class data to a room strucutre model
            for (int i = 0; i < output.GetLength(0); i++)
            {
                for (int j = 0; j < output.GetLength(1); j++)
                {
                    if (model[i, j] != null && !(model[i, j] is NullRoomSpace))
                    {
                        output[i, j] = model[i, j].outputStruct();
                    }
                }
            }

            return output;
        }
    } //class DungeonGenerator
}
