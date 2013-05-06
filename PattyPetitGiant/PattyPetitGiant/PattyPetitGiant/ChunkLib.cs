﻿using System;
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

            string[] chunks = File.ReadAllLines(chunkManifest);

            foreach (string line in chunks)
            {
                ChunkManager.Chunk c = ChunkManager.ReadChunk(chunkDirectory + line);

                dict.Add(line, c);
            }

            manifestLoaded = true;
        }
    }
}
