using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.GamerServices;

namespace PattyPetitGiant
{
    class CampaignLobbyState : ScreenState
    {
        private struct CampaignLoadout
        {
            public string Name { get; set; }
        }

        public CampaignLobbyState()
        {
            //
        }

        protected override void doUpdate(GameTime currentTime)
        {
            //
        }

        public override void render(SpriteBatch sb)
        {
            throw new NotImplementedException();
        }

        public override ScreenState.ScreenStateType nextLevelState()
        {
            throw new NotImplementedException();
        }
    }
}
