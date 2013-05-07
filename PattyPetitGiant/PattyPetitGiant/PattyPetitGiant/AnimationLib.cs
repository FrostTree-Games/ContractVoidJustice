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
        private static XnaTextureLoader textureLoader = null;
        private static SkeletonRenderer skeletonRenderer = null;

        private static SpriteBatch spriteBatch = null;

        private struct SpineAnimation
        {
            public string fileName;
            public Spine.Atlas atlas;
            public Skeleton skeleton;
            public Animation animation;
            public float animationTime;
        }

        public AnimationLib(GraphicsDevice gd, SpriteBatch spriteBatch)
        {
            AnimationLib.spriteBatch = spriteBatch;

            AnimationLib.textureLoader = new XnaTextureLoader(gd);
            AnimationLib.skeletonRenderer = new SkeletonRenderer(gd);
        }
    }
}
