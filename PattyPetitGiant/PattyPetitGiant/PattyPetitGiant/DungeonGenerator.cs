using System;
using System.Collections.Generic;
using System.Text;

namespace PattyPetitGiant
{
    public class DungeonGenerator
    {
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
            public DungeonRoomClass[] Children { get { return Children; } }

            private List<string> attributes = null;
            public List<String> Attributes { get { return attributes; } }

            public DungeonRoomClass(DungeonRoomClass parent)
            {
                this.parent = parent;
                this.children = new DungeonRoomClass[4];

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

                return output;
            }
        }

        public struct DungeonRoom
        {
            public bool north;
            public bool south;
            public bool west;
            public bool east;

            public List<string> attributes;
        }

        public static DungeonRoom[,] generateRoomData(int desiredWidth, int desiredHeight)
        {
            DungeonRoom[,] output = new DungeonRoom[desiredWidth, desiredHeight];

            for (int i = 0; i < output.GetLength(0); i++)
            {
                for (int j = 0; j < output.GetLength(1); j++)
                {
                    output[i, j].north = true;
                    output[i, j].south = true;
                    output[i, j].east = true;
                    output[i, j].west = true;
                }
            }

            return output;
        }
    } //class DungeonGenerator
}
