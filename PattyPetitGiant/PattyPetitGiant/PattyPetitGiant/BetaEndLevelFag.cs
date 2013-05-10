using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PattyPetitGiant
{
    class BetaEndLevelFag : Entity
    {
        private AnimationLib.FrameAnimationSet anims = null;

        public BetaEndLevelFag(LevelState parent, Vector2 position)
        {
            this.parentWorld = parent;
            this.position = position;
            this.dimensions = GlobalGameConstants.TileSize;

            anims = AnimationLib.getFrameAnimationSet("flagPic");
        }

        public override void update(GameTime currentTime)
        {
            //
        }

        public override void draw(SpriteBatch sb)
        {
            anims.drawAnimationFrame(0.0f, sb, Position, new Vector2(3.0f, 3.0f), 0.5f);
        }
    }
}
