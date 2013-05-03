using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PattyPetitGiant
{
    /// <summary>
    /// Used for loading, organizing, and generating level chunks. It's cool stuff.
    /// </summary>
    public class ChunkManager
    {
        /// <summary>
        /// Serializable class for storing level chunk information.
        /// </summary>
        public class Chunk
        {
            public class ChunkAttribute
            {
                public string AttributeName;
                public string[] Metadata;
            }

            public string Name;

            public int[] tilemap;

            public ChunkAttribute [] Attributes;
        }

        public static void testWriteToDisk(Chunk c)
        {
            System.Xml.Serialization.XmlSerializer writer = new System.Xml.Serialization.XmlSerializer(c.GetType());
            System.IO.StreamWriter file = new System.IO.StreamWriter("C:\\test.xml");

            writer.Serialize(file, c);
            file.Close();
        }
    }
}
