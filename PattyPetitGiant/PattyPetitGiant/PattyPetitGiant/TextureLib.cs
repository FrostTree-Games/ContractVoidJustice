using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PattyPetitGiant
{
    /// <summary>
    /// Loads textures into memory from file. Storage is abstracted via the singleton pattern.
    /// </summary>
    class TextureLib
    {
        private string graphicsDirectory = "Content/gfx/";

        private static Dictionary<string, Texture2D> dict = null;

        public TextureLib()
        {
            initTextureLib();
        }

        public static void initTextureLib()
        {
            if (dict == null)
            {
                dict = new Dictionary<string, Texture2D>();
            }
        }

        public static bool loadTexture(string filename)
        {
            return false;
        }

        public static Texture2D getLoadedTexture(string filename)
        {
            return null;
        }
    }
}
