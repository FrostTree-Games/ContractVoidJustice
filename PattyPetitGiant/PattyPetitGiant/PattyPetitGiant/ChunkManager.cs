using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace PattyPetitGiant
{
    /// <summary>
    /// Used for loading, organizing, and generating level chunks. It's cool stuff.
    /// </summary>
    public class ChunkManager
    {
        /// <summary>
        /// Serializable class for reading/writing chunk data to disk.
        /// </summary>
        public class Chunk
        {
            public class ChunkAttribute
            {
                /// <summary>
                /// Attribute identifier.
                /// </summary>
                public string AttributeName;
            }

            /// <summary>
            /// Identifier.
            /// </summary>
            public string Name;

            /// <summary>
            /// 1D array of tile data.
            /// </summary>
            public int[] tilemap;

            /// <summary>
            /// Array of attributes per chunk.
            /// </summary>
            public ChunkAttribute[] Attributes;
        }

        /// <summary>
        /// Reads a Chunk from disk.
        /// </summary>
        /// <param name="path">URL path for reading Chunk.</param>
        /// <returns></returns>
        public static Chunk ReadChunk(string path)
        {
            System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(Chunk));

            StreamReader stream = new StreamReader(path);

            Chunk c = (Chunk)serializer.Deserialize(stream);

            return c;
        }

        /// <summary>
        /// Reads a Chunk from disk.
        /// </summary>
        /// <param name="path">Stream for reading Chunk.</param>
        /// <returns></returns>
        public static Chunk ReadChunk(Stream stream)
        {
            System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(Chunk));

            Chunk c = (Chunk)serializer.Deserialize(stream);

            return c;
        }

        /// <summary>
        /// Writes a Chunk to disk using a specified Path.
        /// </summary>
        /// <param name="path">URL path for writing Chunk.</param>
        /// <param name="chunk">Chunk to write to disk.</param>
        public static void WriteChunk(string path, Chunk chunk)
        {
            System.Xml.Serialization.XmlSerializer writer = new System.Xml.Serialization.XmlSerializer(chunk.GetType());
            System.IO.StreamWriter file = new System.IO.StreamWriter(path);

            writer.Serialize(file, chunk);
            file.Close();
        }

        /// <summary>
        /// Writes a Chunk to disk using a specified Stream.
        /// </summary>
        /// <param name="stream">Stream for writing Chunk.</param>
        /// <param name="chunk">Chunk to write to disk.</param>
        public static void WriteChunk(Stream stream, Chunk chunk)
        {
            System.Xml.Serialization.XmlSerializer writer = new System.Xml.Serialization.XmlSerializer(chunk.GetType());
            System.IO.StreamWriter file = new System.IO.StreamWriter(stream);

            writer.Serialize(file, chunk);
            file.Close();
        }
    }
}
