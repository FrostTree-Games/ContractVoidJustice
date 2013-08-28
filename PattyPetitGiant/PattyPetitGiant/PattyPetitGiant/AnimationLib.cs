using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Spine;

namespace PattyPetitGiant
{
    public class AnimationLib
    {
        private static string spineAnimationDirectory = "Content/animation/spine/";
        private static string spineManifestFile = "Content/animation/spine/manifest.txt";

        private static string frameAnimationDirectory = "Content/animation/frame/";
        private static string frameManifestFile = "Content/animation/frame/manifest.txt";

        private static GraphicsDevice gd = null;
        public static GraphicsDevice GraphicsDevice { get { return gd; } }

        private static XnaTextureLoader textureLoader = null;
        private static SkeletonRenderer skeletonRenderer = null;

        private static SpriteBatch spriteBatch = null;

        private static bool spineManifestLoaded = false;
        private static bool frameManifestLoaded = false;

        private static Dictionary<string, SkeletonData> skeletonDataLib = null;
        private static Dictionary<string, Atlas> atlasDataLib = null;
        private static Dictionary<string, SpineAnimationSet> spineDict = null;
        private static Dictionary<string, FrameAnimationSet> frameDict = null;

        /// <summary>
        /// Stores a Spine animation
        /// </summary>
        public class SpineAnimationSet
        {
            private Atlas atlas;
            public Atlas Atlas { get { return atlas; } }

            private Skeleton skeleton;
            public Skeleton Skeleton { get { return skeleton; } set { skeleton = value; } }

            private Animation animation;
            public Animation Animation { get { return animation; } set { animation = value; } }

            public SpineAnimationSet(string folderName)
            {
                try
                {
                    atlas = atlasDataLib[spineAnimationDirectory + folderName + "/parts.atlas"];
                }
                catch (Exception)
                {
                    atlas = new Atlas(spineAnimationDirectory + folderName + "/parts.atlas", textureLoader);
                    atlasDataLib.Add(spineAnimationDirectory + folderName + "/parts.atlas", atlas);
                }

                SkeletonData skeletonData;
                try
                {
                    skeletonData = skeletonDataLib[spineAnimationDirectory + folderName + "/anims.json"];
                }
                catch (Exception)
                {
                    SkeletonJson json = new SkeletonJson(atlas);
                    skeletonData = json.ReadSkeletonData(spineAnimationDirectory + folderName + "/anims.json");
                    skeletonDataLib.Add(spineAnimationDirectory + folderName + "/anims.json", skeletonData);
                }
                
                skeleton = new Skeleton(skeletonData);

                animation = skeleton.Data.FindAnimation("run");
            }
        }

        public class FrameAnimationSet
        {
            private Texture2D sheet = null;
            public Texture2D Sheet { get { return sheet; } }

            private int x, y;
            public int InitialX { get { return x; } }
            public int InitialY { get { return y; } }

            private int offsetX, offsetY;
            public int OffsetX { get { return offsetX; } }
            public int OffsetY { get { return offsetY; } }

            private int frameWidth, frameHeight;
            public int FrameWidth { get { return frameWidth; } }
            public int FrameHeight { get { return frameHeight; } }
            public Vector2 FrameDimensions { get { return new Vector2(frameWidth, frameHeight); } }

            private int frameCount = 0;
            public int FrameCount { get { return frameCount; } }

            private float frameDuration;
            public float FrameDuration { get { return frameDuration; } }

            private bool loop = false;
            public bool Loop { get { return loop; } }

            public FrameAnimationSet(SerializableAnimationData data)
            {
                x = data.initalX;
                y = data.initalY;
                offsetX = data.offsetX;
                offsetY = data.offsetY;
                frameWidth = data.frameWidth;
                frameHeight = data.frameHeight;
                frameCount = data.frameCount;
                frameDuration = data.frameSpeed;
                loop = data.loop;

                sheet = TextureLib.getLoadedTexture(data.sheetName);
            }

            public void drawAnimationFrame(float time, SpriteBatch sb, Vector2 position)
            {
                int frame = (int)(time / frameDuration);

                if (loop)
                {
                    frame = frame % frameCount;
                }
                else
                {
                    frame = frameCount - 1;
                }

                sb.Draw(sheet, position + new Vector2((int)offsetX, (int)offsetY), new Rectangle(x + (frame * frameWidth), y, frameWidth, FrameHeight), Color.White);
            }

            public void drawAnimationFrame(float time, SpriteBatch sb, Vector2 position, Vector2 scale, float depth)
            {
                int frame = (int)(time / frameDuration);

                if (loop)
                {
                    frame = frame % frameCount;
                }
                else
                {
                    frame = frameCount - 1;
                }

                sb.Draw(sheet, position + new Vector2((int)offsetX, (int)offsetY), new Rectangle(x + (frame * frameWidth), y, frameWidth, frameHeight), Color.White, 0.0f, Vector2.Zero, scale, SpriteEffects.None, depth);
            }

            public void drawAnimationFrame(float time, SpriteBatch sb, Vector2 position, Vector2 scale, float depth, Color color)
            {
                int frame = (int)(time / frameDuration);

                if (loop)
                {
                    frame = frame % frameCount;
                }
                else
                {
                    frame = frameCount - 1;
                }

                sb.Draw(sheet, position + new Vector2((int)offsetX, (int)offsetY), new Rectangle(x + (frame * frameWidth), y, frameWidth, frameHeight), color, 0.0f, Vector2.Zero, scale, SpriteEffects.None, depth);
            }

            public void drawAnimationFrame(float time, SpriteBatch sb, Vector2 position, Vector2 scale, float depth, float rotation, Vector2 centerPoint)
            {
                int frame = (int)(time / frameDuration);

                if (loop)
                {
                    frame = frame % frameCount;
                }
                else
                {
                    frame = frameCount - 1;
                }

                sb.Draw(sheet, position + new Vector2((int)offsetX, (int)offsetY), new Rectangle(x + (frame * frameWidth), y, frameWidth, frameHeight), Color.White, rotation, centerPoint, scale, SpriteEffects.None, depth);
            }

            public void drawAnimationFrame(float time, SpriteBatch sb, Vector2 position, Vector2 scale, float depth, float rotation, Vector2 centerPoint, Color color)
            {
                int frame = (int)(time / frameDuration);

                if (loop)
                {
                    frame = frame % frameCount;
                }
                else if (frame >= frameCount)
                {
                    frame = frameCount - 1;
                }

                sb.Draw(sheet, position + new Vector2((int)offsetX, (int)offsetY), new Rectangle(x + (frame * frameWidth), y, frameWidth, frameHeight), color, rotation, centerPoint, scale, SpriteEffects.None, depth);
            }

            public void drawAnimationFrame(float time, Spine.SkeletonRenderer sb, Vector2 position, Vector2 scale, float depth, float rotation, Vector2 centerPoint, Color color)
            {
                int frame = (int)(time / frameDuration);

                if (loop)
                {
                    frame = frame % frameCount;
                }
                else if (frame >= frameCount)
                {
                    frame = frameCount - 1;
                }

                sb.DrawSpriteToSpineVertexArray(sheet, new Rectangle(x + (frame * frameWidth), y, frameWidth, frameHeight), position + new Vector2((int)offsetX, (int)offsetY), color, rotation, scale);
            }

        }

        public class SerializableAnimationData
        {
            public string name;

            public string sheetName;

            public int initalX = 0, initalY = 0;
            public int offsetX = 0, offsetY = 0;
            public int frameWidth, frameHeight;
            public int frameCount;
            public float frameSpeed;
            public bool loop;
        }

        public AnimationLib(GraphicsDevice gd, SpriteBatch spriteBatch)
        {
            if (AnimationLib.gd == null)
            {
                AnimationLib.gd = gd;
            }

            if (AnimationLib.spriteBatch == null)
            {
                AnimationLib.spriteBatch = spriteBatch;
            }

            if (AnimationLib.textureLoader == null || AnimationLib.skeletonRenderer == null)
            {
                AnimationLib.textureLoader = new XnaTextureLoader(gd);
                AnimationLib.skeletonRenderer = new SkeletonRenderer(gd);
            }

            if (skeletonDataLib == null)
            {
                skeletonDataLib = new Dictionary<string, SkeletonData>();
            }

            if (atlasDataLib == null)
            {
                atlasDataLib = new Dictionary<string, Atlas>();
            }

            if (spineDict == null)
            {
                spineDict = new Dictionary<string, SpineAnimationSet>();
            }

            if (frameDict == null)
            {
                frameDict = new Dictionary<string, FrameAnimationSet>();
            }
        }

        public static void cacheAtlasFiles()
        {
            if (spineManifestLoaded || spineDict == null)
            {
                return;
            }

#if WINDOWS
            string[] spineAnims = File.ReadAllLines(spineManifestFile);

            foreach (string line in spineAnims)
            {
                loadAtlas(line);
            }

#elif XBOX
            String xboxLine;
            int counter = 0;

            StreamReader file = new StreamReader(spineManifestFile);

            while ((xboxLine = file.ReadLine()) != null)
            {
                loadAtlas(xboxLine);

                counter++;
            }

            file.Close();

#endif
        }

        public static void cacheSpineJSON()
        {
            if (spineManifestLoaded || spineDict == null)
            {
                return;
            }

#if WINDOWS
            string[] spineAnims = File.ReadAllLines(spineManifestFile);

            foreach (string line in spineAnims)
            {
                loadSpineJSon(line);
            }

#elif XBOX
            String xboxLine;
            int counter = 0;

            StreamReader file = new StreamReader(spineManifestFile);

            while ((xboxLine = file.ReadLine()) != null)
            {
                loadSpineJSon(xboxLine);

                counter++;
            }

            file.Close();

#endif
        }

        public static void loadSpineFromManifest()
        {
            if (spineManifestLoaded || spineDict == null)
            {
                return;
            }

#if WINDOWS
            string[] spineAnims = File.ReadAllLines(spineManifestFile);

            foreach (string line in spineAnims)
            {
                loadSpineAnimation(line);
            }

#elif XBOX
            String xboxLine;
            int counter = 0;

            StreamReader file = new StreamReader(spineManifestFile);

            while ((xboxLine = file.ReadLine()) != null)
            {
                loadSpineAnimation(xboxLine);

                counter++;
            }

            file.Close();

#endif

            spineManifestLoaded = true;
        }

        public static void loadFrameFromManifest()
        {
            if (frameManifestLoaded || frameDict == null)
            {
                return;
            }

#if WINDOWS
            string[] frameAnims = File.ReadAllLines(frameManifestFile);

            foreach (string line in frameAnims)
            {
                loadFrameAnimation(line);
            }

#elif XBOX
            String xboxLine;
            int counter = 0;

            StreamReader file = new StreamReader(frameManifestFile);

            while ((xboxLine = file.ReadLine()) != null)
            {
                loadFrameAnimation(xboxLine);

                counter++;
            }

            file.Close();

#endif

            frameManifestLoaded = true;
        }

        public static bool loadSpineAnimation(string animationName)
        {
            SpineAnimationSet s = new SpineAnimationSet(animationName);

            spineDict.Add(animationName, s);

            return true;
        }

        private static void loadAtlas(string animationName)
        {
            if (!atlasDataLib.ContainsKey(spineAnimationDirectory + animationName + "/parts.atlas"))
            {
                atlasDataLib.Add(spineAnimationDirectory + animationName + "/parts.atlas", new Atlas(spineAnimationDirectory + animationName + "/parts.atlas", textureLoader));
            }
        }

        private static void loadSpineJSon(string animationName)
        {
            if (!skeletonDataLib.ContainsKey(spineAnimationDirectory + animationName + "/anims.json"))
            {
                SkeletonJson json = new SkeletonJson(atlasDataLib[spineAnimationDirectory + animationName + "/parts.atlas"]);
                SkeletonData skeletonData = json.ReadSkeletonData(spineAnimationDirectory + animationName + "/anims.json");
                skeletonDataLib.Add(spineAnimationDirectory + animationName + "/anims.json", skeletonData);
            }
        }

        public static bool loadFrameAnimation(string animationName)
        {
            SerializableAnimationData serialized = loadFrameAnimationFromFile(frameAnimationDirectory + animationName + ".zpy");

            FrameAnimationSet fas = new FrameAnimationSet(serialized);

            frameDict.Add(animationName, fas);

            return true;
        }

        public static FrameAnimationSet getFrameAnimationSet(string animationName)
        {
            FrameAnimationSet output = null;

            try
            {
                output = frameDict[animationName];

                return output;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static SpineAnimationSet getSkeleton(string folderName)
        {
            SpineAnimationSet output = null;

            try
            {
                output = spineDict[folderName];

                return output;
            }
            catch (KeyNotFoundException)
            {
                return null;
            }
        }

        /// <summary>
        /// Loads a spine animation from disc
        /// </summary>
        /// <param name="folderName"></param>
        /// <returns></returns>
        public static SpineAnimationSet loadNewAnimationSet(string folderName)
        {
            return new SpineAnimationSet(folderName);
        }

        public static void renderSpineEntities(Matrix camera, List<Entity> entList, Entity cameraEntity, TileMap map, ParticleSet set, List<MutantAcidSpitter> spitters)
        {
            skeletonRenderer.setCameraMatrix(camera);
            skeletonRenderer.Begin();

            skeletonRenderer.DrawSpriteToSpineVertexArray(Game1.whitePixel, new Rectangle(0, 0, 1, 1), new Vector2(cameraEntity.CenterPoint.X - GlobalGameConstants.GameResolutionWidth / 2, cameraEntity.CenterPoint.Y - GlobalGameConstants.GameResolutionHeight / 2), Color.Black, 0.0f, new Vector2(GlobalGameConstants.GameResolutionWidth, GlobalGameConstants.GameResolutionHeight));

            map.renderSPINEBATCHTEST(skeletonRenderer, 0.5f, spitters);

            for (int i = 0; i < entList.Count; i++)
            {
                if (Vector2.Distance(cameraEntity.Position, entList[i].Position) > (GlobalGameConstants.GameResolutionWidth * 0.75f))
                {
                    continue;
                }

                entList[i].draw(skeletonRenderer);

                if (entList[i] is SpineEntity)
                {
                    ((SpineEntity)entList[i]).spinerender(skeletonRenderer);
                }
            }

            set.drawSpineSet(skeletonRenderer, cameraEntity.Position, 0.5f);

            skeletonRenderer.End();
        }

        private static SerializableAnimationData loadFrameAnimationFromFile(string path)
        {
            System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(SerializableAnimationData));

            StreamReader stream = new StreamReader(path);

            SerializableAnimationData s = (SerializableAnimationData)serializer.Deserialize(stream);

            return s;
        }
    }
}
