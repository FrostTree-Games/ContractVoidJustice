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

        /// <summary>
        /// Initializes the texture storage.
        /// </summary>
        public static void initTextureLib()
        {
            if (dict == null)
            {
                dict = new Dictionary<string, Texture2D>();
            }
        }

        /// <summary>
        /// Sets the graphics device to load the textures to.
        /// </summary>
        /// <param name="device"></param>
        public static void setDevice(GraphicsDevice device)
        {
            TextureLib.device = device;
        }

        /// <summary>
        /// Loads a texture into memory.
        /// </summary>
        /// <param name="filename">The filename of the texture.</param>
        /// <returns></returns>
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
                fs = new FileStream(graphicsDirectory + filename, FileMode.Open, FileAccess.Read);
                texture = Texture2D.FromStream(device, fs);
            }
            catch (FileNotFoundException)
            {
                return false;
            }

            dict.Add(filename, texture);

            return true;
        }

        /// <summary>
        /// Get a texture loaded into memory.
        /// </summary>
        /// <param name="filename">The filename of the texture.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Removes a texture from TextureLib. The texture will be unloaded from memory once collected by C#'s garbage collector.
        /// </summary>
        /// <param name="filename">The filename of the texture.</param>
        public static void removeTexture(string filename)
        {
            dict.Remove(filename);
        }

        /// <summary>
        /// Removes all textures from TextureLib. The textures will be unloaded from memory once collected by C#'s garbage collector.
        /// </summary>
        public static void removeAllTextures()
        {
            dict.Clear();
        }

        public class TextureLibNotInitializedException : Exception
        {
            //
        }
    }
}
