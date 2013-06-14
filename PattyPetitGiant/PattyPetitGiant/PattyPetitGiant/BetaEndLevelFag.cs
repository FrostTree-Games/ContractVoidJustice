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
            foreach (Entity en in parentWorld.EntityList)
            {
                if (en is Player)
                {
                    if (hitTest(en))
                    {
                        parentWorld.EndFlagReached = true;
                    }
                }
            }
        }

        public override void draw(SpriteBatch sb)
        {
            anims.drawAnimationFrame(0.0f, sb, Position, new Vector2(3.0f, 3.0f), 0.5f);
        }
        public override void knockBack(Vector2 direction, float magnitude, int damage, Entity attacker)
        {
            return;
        }
    }
}
