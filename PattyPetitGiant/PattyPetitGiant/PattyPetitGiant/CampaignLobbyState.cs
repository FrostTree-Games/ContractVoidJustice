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
            public InputDevice2.PlayerPad InputDevice;
            public string Name;
            public float Hue;

            public CampaignLoadout(InputDevice2.PlayerPad InputDevice, string Name, float Hue)
            {
                this.InputDevice = InputDevice;
                this.Name = Name;
                this.Hue = Hue;
            }
        }

        private CampaignLoadout slot1;
        private CampaignLoadout slot2;

        public CampaignLobbyState()
        {
            slot1.InputDevice = InputDevice2.PlayerPad.NoPad;
            slot2.InputDevice = InputDevice2.PlayerPad.NoPad;
        }

        protected override void doUpdate(GameTime currentTime)
        {
            //
        }

        public override void render(SpriteBatch sb)
        {
            //
        }

        public override ScreenState.ScreenStateType nextLevelState()
        {
            throw new NotImplementedException();
        }

        public static string[] randomNames = {"Isiah",
                                                "Eric",
                                                "Wilson",
                                                "Daniel",
                                                "Marget",
                                                "Rueben",
                                                "Lovella",
                                                "Felisha",
                                                "Kandace",
                                                "Wopley",
                                                "Zippy",
                                                "Fish",
                                                "Belmont",
                                                "Jensen",
                                                "Louise",
                                                "Merrilee",
                                                "Faustina",
                                                "Kenda",
                                                "Jamison",
                                                "Monserrate",
                                                "Jacquelyn",
                                                "Nicky",
                                                "Luigi",
                                                "Alyosha",
                                                "Denton",
                                                "Jensen",
                                                "Bethanie",
                                                "Kecia",
                                                "Darline",
                                                "Roselle",
                                                "Hiram",
                                                "Velda",
                                                "Roselia",
                                                "Lizbeth",
                                                "Roselyn",
                                                "Mamie",
                                                "Isabella",
                                                "Willa",
                                                "Berry",
                                                "Lindsay",
                                                "Kimberley",
                                                "Galen",
                                                "Shea",
                                                "Bennett",
                                                "Adelaida",
                                                "Toya",
                                                "Devon",
                                                "Maye",
                                                "Georgette",
                                                "Zofia",
                                                "Enrique",
                                                "Germaine",
                                                "Seth",
                                                "Gabrielle",
                                                "Sari",
                                                "Joline",
                                                "Carolynn",
                                                "Tova",
                                                "Deborah",
                                                "Dacia",
                                                "Erin",
                                                "Commander",
                                                "Shepard",
                                                "Daphne",
                                                "Lona"};
    }
}
