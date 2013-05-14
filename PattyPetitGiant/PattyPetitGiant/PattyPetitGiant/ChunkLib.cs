using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace PattyPetitGiant
{
    /// <summary>
    /// Maintains a library of game chunks for dungeon generation.
    /// </summary>
    class ChunkLib
    {
        private static string chunkDirectory = "Content/chunk/";
        private static string chunkManifest = "Content/chunk/manifest.txt";

        private static bool manifestLoaded = false;

        private static Dictionary<string, ChunkManager.Chunk> dict = null;

        public ChunkLib()
        {
            initLib();
            loadFromManifest();
        }

        public static void initLib()
        {
            if (dict == null)
            {
                dict = new Dictionary<string, ChunkManager.Chunk>();
            }
        }

        private static void loadFromManifest()
        {
            if (manifestLoaded || dict == null)
            {
                return;
            }

#if WINDOWS
            string[] chunks = File.ReadAllLines(chunkManifest);

            foreach (string line in chunks)
            {
                ChunkManager.Chunk c = ChunkManager.ReadChunk(chunkDirectory + line);

                dict.Add(line, c);
            }

#elif XBOX
            String xboxLine;
            int counter = 0;

            StreamReader file = new StreamReader(chunkManifest);

            while ((xboxLine = file.ReadLine()) != null)
            {
                ChunkManager.Chunk c = ChunkManager.ReadChunk(chunkDirectory + xboxLine);

                dict.Add(xboxLine, c);

                counter++;
            }

            file.Close();

#endif

            manifestLoaded = true;
        }

        private static byte chunkToByteValue(ChunkManager.Chunk c)
        {
            byte output = 0;

            foreach (ChunkManager.Chunk.ChunkAttribute attr in c.Attributes)
            {
                if (attr.AttributeName == "north")
                {
                    output |= 1;
                }
                if (attr.AttributeName == "south")
                {
                    output |= 2;
                }
                if (attr.AttributeName == "east")
                {
                    output |= 4;
                }
                if (attr.AttributeName == "west")
                {
                    output |= 8;
                }
            }

            return output;
        }

        private static byte stringsToByteValue(string[] attributes)
        {
            byte output = 0;

            foreach (String s in attributes)
            {
                if (s == "north")
                {
                    output |= 1;
                }
                if (s == "south")
                {
                    output |= 2;
                }
                if (s == "east")
                {
                    output |= 4;
                }
                if (s == "west")
                {
                    output |= 8;
                }
            }

            return output;
        }

        public static ChunkManager.Chunk getRandomChunkByValues(string[] requiredAttributes, Random rand)
        {
            List<ChunkManager.Chunk> potentials = new List<ChunkManager.Chunk>();

            foreach (KeyValuePair<string, ChunkManager.Chunk> val in dict)
            {
                byte b1 = chunkToByteValue(val.Value);
                byte b2 = stringsToByteValue(requiredAttributes);

                if (b1 == b2)
                {
                    potentials.Add(val.Value);
                }
            }

            if (potentials.Count < 1)
            {
                return null;
            }
            else
            {
                return potentials[rand.Next() % potentials.Count];
            }
        }
    }
}
