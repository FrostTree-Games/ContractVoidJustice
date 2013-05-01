using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PattyPetitGiant
{
    /// <summary>
    /// Loads textures into memory from file. Storage is abstracted via the singleton pattern.
    /// </summary>
    class TextureLib
    {
        private static string graphicsDirectory = "Content/gfx/";

        private static GraphicsDevice device = null;
        private static Dictionary<string, Texture2D> dict = null;

        public TextureLib(GraphicsDevice device)
        {
            initTextureLib();
            setDevice(device);
        }

        public static void initTextureLib()
        {
            if (dict == null)
            {
                dict = new Dictionary<string, Texture2D>();
            }
        }

        public static void setDevice(GraphicsDevice device)
        {
            TextureLib.device = device;
        }

        public static bool loadTexture(string filename)
        {
            if (device == null || dict == null)
            {
                throw new TextureLibNotInitializedException();
            }

            Texture2D texture = null;
            FileStream fs = null;

            try
            {
                fs = new FileStream(graphicsDirectory + filename, FileMode.Open);
                texture = Texture2D.FromStream(device, fs);
            }
            catch (FileNotFoundException)
            {
                return false;
            }

            dict.Add(filename, texture);

            return true;
        }

        public static Texture2D getLoadedTexture(string filename)
        {
            if (device == null || dict == null)
            {
                throw new TextureLibNotInitializedException();
            }

            try
            {
                return dict[filename];
            }
            catch (KeyNotFoundException)
            {
                return null;
            }
            catch (ArgumentNullException)
            {
                return null;
            }
        }

        public class TextureLibNotInitializedException : Exception
        {
            //
        }
    }
}
