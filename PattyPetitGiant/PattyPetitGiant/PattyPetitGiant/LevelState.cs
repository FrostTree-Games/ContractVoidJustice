//comment this out if you don't want to generate any entities
#define TEST_ENTITIES

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PattyPetitGiant
{
    public class LevelState : ScreenState
    {
        private enum LoadingState
        {
            UninitializedAndWaiting,
            Loading,
            Running,
            Closing,
            ClosedAndWaiting,
        }

        private LoadingState state = LoadingState.UninitializedAndWaiting;

        private TileMap map = null;
        public TileMap Map { get { return map; } }

        private List<Entity> entityList = null;
        public List<Entity> EntityList { get { return entityList; } }

        private Entity cameraFocus = null;
        public Entity CameraFocus { get { return cameraFocus; } set { value = cameraFocus; } }
        Matrix camera;

        public LevelState()
        {
            map = new TileMap(GlobalGameConstants.StandardMapSize, GlobalGameConstants.TileSize);

            entityList = new List<Entity>();

#if TEST_ENTITIES
            map.blobTestWalls();

            entityList.Add(new Player(this, 150, 150));
            entityList.Add(new Enemy(this, 300, 272));

            foreach (Entity en in entityList)
            {
                if (en is Player)
                {
                    cameraFocus = en;
                }
            }
#endif

            state = LoadingState.Running;
        }

        protected override void doUpdate(Microsoft.Xna.Framework.GameTime currentTime)
        {
            foreach (Entity en in entityList)
            {
                en.update(currentTime);
            }

            if (cameraFocus != null)
            {
                camera = Matrix.Identity * Matrix.CreateTranslation(new Vector3((cameraFocus.CenterPoint.X * -1) + (GlobalGameConstants.GameResolutionWidth / 2), (cameraFocus.CenterPoint.Y * -1) + (GlobalGameConstants.GameResolutionHeight / 2), 0.0f));
            }
        }

        public override void render(SpriteBatch sb)
        {
            sb.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, camera);

            map.render(sb, 0.0f);

            foreach (Entity en in entityList)
            {
                en.draw(sb);
            }

            sb.End();
        }
    }
}
