using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace PattyPetitGiant
{
    public class DungeonGenerator
    {
        private const float probability_connectOldRooms = 0.1f;

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

            public DungeonRoomClass(DungeonRoomClass parent, int X, int Y)
            {
                this.parent = parent;
                this.children = new DungeonRoomClass[4];

                x = X;
                y = Y;

                intensity = -1;

                this.attributes = new List<string>();
            }

            public DungeonRoom outputStruct()
            {
                DungeonRoom output = new DungeonRoom();

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

                if (output.north) { this.attributes.Add("north"); }
                if (output.south) { this.attributes.Add("south"); }
                if (output.east) { this.attributes.Add("east"); }
                if (output.west) { this.attributes.Add("west"); }

                return output;
            }
        }

        public struct DungeonRoom
        {
            public bool north;
            public bool south;
            public bool west;
            public bool east;

            public float intensity;

            public List<string> attributes;
        }

        private static int recurseDungeonIntensity(DungeonRoomClass room, int intensity)
        {
            if (room == null || room.Intensity != -1)
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

            int maxIntensity = recurseDungeonIntensity(dungeonModel[startingRoomX, startingRoomY], 0);

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

        public static DungeonRoom[,] generateRoomData(int desiredWidth, int desiredHeight)
        {
            DungeonRoom[,] output = new DungeonRoom[desiredWidth, desiredHeight];
            DungeonRoomClass[,] model = new DungeonRoomClass[desiredWidth, desiredHeight];

            Random rand = new Random();
            List<DungeonRoomClass> addedRooms = new List<DungeonRoomClass>();

            //place initial room
            int randX = (rand.Next() % (desiredWidth - 2)) + 1;
            int randY = (rand.Next() % (desiredHeight - 2)) + 1;
            DungeonRoomClass startingRoom = new DungeonRoomClass(null, randX, randY);
            startingRoom.Attributes.Add("start");
            addedRooms.Add(startingRoom);
            model[randX, randY] = startingRoom;

            //iterate and expand the dungeon according to the constraints
            int iterate = 0;
            const int maxIterations = 1000;
            int globalIterations = 0;
            while (iterate < (int)(model.GetLength(0) * model.GetLength(1) * 0.75f) && globalIterations < maxIterations)
            {
                globalIterations++;

                DungeonRoomClass room = addedRooms[rand.Next() % addedRooms.Count];

                int newDir = rand.Next() % 4;
                
                //don't create a new room outside of the boundaries
                if ((newDir == 0 && room.Y == 0) || (newDir == 1 && room.X == desiredWidth - 1) || ((newDir == 2 && room.Y == desiredHeight - 1)) || (newDir == 3 && room.X == 0))
                {
                    continue;
                }

                //don't create a new room on top of an old one
                if ((newDir == 0 && model[room.X, room.Y - 1] != null) || (newDir == 1 && model[room.X + 1, room.Y] != null) || ((newDir == 2 && model[room.X, room.Y + 1] != null)) || (newDir == 3 && model[room.X - 1, room.Y] != null))
                {
                    //but, there's a chance we'd like some loops so the dungeon looks less boring
                    double newRoomChance = rand.NextDouble();
                    if ((float)(newRoomChance) <= probability_connectOldRooms)
                    {
                        switch ((DungeonRoomClass.ChildDirection)newDir)
                        {
                            case DungeonRoomClass.ChildDirection.North:
                                room.Children[newDir] = model[room.X, room.Y - 1];
                                model[room.X, room.Y - 1].Children[(int)DungeonRoomClass.ChildDirection.South] = room;
                                break;
                            case DungeonRoomClass.ChildDirection.South:
                                room.Children[newDir] = model[room.X, room.Y + 1];
                                model[room.X, room.Y + 1].Children[(int)DungeonRoomClass.ChildDirection.North] = room;
                                break;
                            case DungeonRoomClass.ChildDirection.East:
                                room.Children[newDir] = model[room.X + 1, room.Y];
                                model[room.X + 1, room.Y].Children[(int)DungeonRoomClass.ChildDirection.West] = room;
                                break;
                            case DungeonRoomClass.ChildDirection.West:
                                room.Children[newDir] = model[room.X - 1, room.Y];
                                model[room.X - 1, room.Y].Children[(int)DungeonRoomClass.ChildDirection.East] = room;
                                break;
                        }
                    }

                    continue;
                }

                //don't replace an already-existing connection
                if (room.Children[newDir] != null)
                {
                    continue;
                }

                switch ((DungeonRoomClass.ChildDirection)newDir)
                {
                    case DungeonRoomClass.ChildDirection.North:
                        room.Children[newDir] = new DungeonRoomClass(room, room.X, room.Y - 1);
                        room.Children[newDir].Children[(int)(DungeonRoomClass.ChildDirection.South)] = room;
                        model[room.X, room.Y - 1] = room.Children[newDir];
                        addedRooms.Add(room.Children[newDir]);
                        break;
                    case DungeonRoomClass.ChildDirection.South:
                        room.Children[newDir] = new DungeonRoomClass(room, room.X, room.Y + 1);
                        room.Children[newDir].Children[(int)(DungeonRoomClass.ChildDirection.North)] = room;
                        model[room.X, room.Y + 1] = room.Children[newDir];
                        addedRooms.Add(room.Children[newDir]);
                        break;
                    case DungeonRoomClass.ChildDirection.East:
                        room.Children[newDir] = new DungeonRoomClass(room, room.X + 1, room.Y);
                        room.Children[newDir].Children[(int)(DungeonRoomClass.ChildDirection.West)] = room;
                        model[room.X + 1, room.Y] = room.Children[newDir];
                        addedRooms.Add(room.Children[newDir]);
                        break;
                    case DungeonRoomClass.ChildDirection.West:
                        room.Children[newDir] = new DungeonRoomClass(room, room.X - 1, room.Y);
                        room.Children[newDir].Children[(int)(DungeonRoomClass.ChildDirection.East)] = room;
                        model[room.X - 1, room.Y] = room.Children[newDir];
                        addedRooms.Add(room.Children[newDir]);
                        break;
                }

                iterate++;
            }

            //compute intensity values
            computeDungeonIntensity(model, startingRoom.X, startingRoom.Y);

            //convert the class data to a room strucutre model
            for (int i = 0; i < output.GetLength(0); i++)
            {
                for (int j = 0; j < output.GetLength(1); j++)
                {
                    if (model[i, j] != null)
                    {
                        output[i, j] = model[i, j].outputStruct();
                    }
                }
            }

            return output;
        }
    } //class DungeonGenerator
}
