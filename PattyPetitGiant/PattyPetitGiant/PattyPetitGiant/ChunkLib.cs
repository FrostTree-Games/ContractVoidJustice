using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PattyPetitGiant
{
    /// <summary>
    /// Maintains a library of game chunks for dungeon generation.
    /// </summary>
    class ChunkLib
    {
        private static string chunkDirectory = "Content/chunk";
        private static string chunkManifest = "Content/chunk/manifest.txt";

        private static Dictionary<string, ChunkManager.Chunk> dict = null;

        public ChunkLib()
        {
            initLib();
        }

        public static void initLib()
        {
            if (dict == null)
            {
                dict = new Dictionary<string, ChunkManager.Chunk>();
            }
        }

        private static void loadManifest()
        {
            //
        }
    }
}
