using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Spine;

namespace PattyPetitGiant
{
    public class AnimationLib
    {
        private static string spineAnimationDirectory = "Content/animation/spine/";
        private static string manifestFile = "Content/animation/spine/manifest.txt";

        private static XnaTextureLoader textureLoader = null;
        private static SkeletonRenderer skeletonRenderer = null;

        private static SpriteBatch spriteBatch = null;

        private static Dictionary<string, SpineAnimationSet> dict = null;

        /// <summary>
        /// Stores a Spine animation
        /// </summary>
        public class SpineAnimationSet
        {
            private Atlas atlas;
            public Atlas Atlas { get { return atlas; } }

            private Skeleton skeleton;
            public Skeleton Skeleton { get { return skeleton; } }

            private Animation animation;
            public Animation Animation { get { return animation; } }

            public SpineAnimationSet(string folderName)
            {
                atlas = new Atlas(spineAnimationDirectory + folderName + "/parts.atlas", textureLoader);

                SkeletonJson json = new SkeletonJson(atlas);
                SkeletonData skeletonData = json.ReadSkeletonData(spineAnimationDirectory + folderName + "/anims.json");
                skeleton = new Skeleton(skeletonData);

                animation = skeleton.Data.FindAnimation("run");
            }
        }

        public AnimationLib(GraphicsDevice gd, SpriteBatch spriteBatch)
        {
            AnimationLib.spriteBatch = spriteBatch;

            AnimationLib.textureLoader = new XnaTextureLoader(gd);
            AnimationLib.skeletonRenderer = new SkeletonRenderer(gd);

            if (dict == null)
            {
                dict = new Dictionary<string, SpineAnimationSet>();
            }
        }

        public static bool loadAnimation(string animationName)
        {
            SpineAnimationSet s = new SpineAnimationSet(animationName);

            dict.Add(animationName, s);

            return true;
        }

        public static SpineAnimationSet getSkeleton(string folderName)
        {
            SpineAnimationSet output = null;

            try
            {
                output = dict[folderName];

                return output;
            }
            catch (KeyNotFoundException)
            {
                return null;
            }
        }

        public static void renderSpineEntities(Matrix camera, List<Entity> entList)
        {
            skeletonRenderer.setCameraMatrix(camera);
            skeletonRenderer.Begin();

            foreach (Entity en in entList)
            {
                /*if (en is Player)
                {
                    Skeleton sk = getSkeleton("jensenDown").Skeleton;
                    Animation an = getSkeleton("jensenDown").Animation;

                    sk.RootBone.X = en.CenterPoint.X;
                    sk.RootBone.Y = en.CenterPoint.Y + (en.Dimensions.Y/2);
                    sk.RootBone.ScaleX = 0.1f;
                    sk.RootBone.ScaleY = 0.1f;

                    sk.UpdateWorldTransform();

                    an.Apply(sk, 0.0f, true);
                    skeletonRenderer.Draw(sk);
                } */

                if (en is SpineEntity)
                {
                    ((SpineEntity)en).spinerender(skeletonRenderer);
                }
            }

            skeletonRenderer.End();
        }
    }
}
