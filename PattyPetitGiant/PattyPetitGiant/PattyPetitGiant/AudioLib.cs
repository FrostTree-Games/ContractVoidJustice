﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;

namespace PattyPetitGiant
{
    public class AudioLib
    {
        private static string sfxDirectory = "Content/sfx/";
        private static string sfxManifestFile = "Content/sfx/manifest.txt";

        private static string sfxFileExtension = ".wav";

        private static Dictionary<string, SoundEffect> sfxLib = null;
        private static bool sfxManifestLoaded = false;

        private static SoundEffectInstance flamethrower_sfxi = null;
        private static SoundEffectInstance tankflamethrower_sfxi = null;

        public AudioLib()
        {
            if (sfxLib == null)
            {
                sfxLib = new Dictionary<string, SoundEffect>();
            }

            if (!sfxManifestLoaded)
            {
                loadManifest();
            }

            flamethrower_sfxi = sfxLib["flameThrower"].CreateInstance();
            flamethrower_sfxi.IsLooped = true;
            tankflamethrower_sfxi = sfxLib["flameThrower"].CreateInstance();
            tankflamethrower_sfxi.IsLooped = true;
        }

        private static bool loadSFX(string url)
        {
            SoundEffect s = null;
            FileStream fs = null;

            try
            {
                fs = new FileStream(sfxDirectory + url + sfxFileExtension, FileMode.Open, FileAccess.Read);
                s = SoundEffect.FromStream(fs);
            }
            catch (FileNotFoundException)
            {
                return false;
            }

            sfxLib.Add(url, s);

            return true;
        }

        private static void loadManifest()
        {
            if (sfxManifestLoaded || sfxLib == null)
            {
                return;
            }

#if WINDOWS
            string[] frameAnims = File.ReadAllLines(sfxManifestFile);

            foreach (string line in frameAnims)
            {
                loadSFX(line);
            }

#elif XBOX
            String xboxLine;
            int counter = 0;

            StreamReader file = new StreamReader(sfxManifestFile);

            while ((xboxLine = file.ReadLine()) != null)
            {
                loadSFX(xboxLine);

                counter++;
            }

            file.Close();

#endif

            sfxManifestLoaded = true;
        }

        /// <summary>
        /// Acquire a sound effect from the library.
        /// </summary>
        /// <param name="sfxName">The name of the sound effect as specified in the manifest file.</param>
        /// <returns></returns>
        public static void playSoundEffect(string sfxName)
        {
            sfxLib[sfxName].Play();
        }

        public static void playFlameSoundEffect(bool play_sfx)
        {
            if (play_sfx && flamethrower_sfxi.State == SoundState.Stopped)
            {
                flamethrower_sfxi.Play();
            }
            else if (!play_sfx && flamethrower_sfxi.State == SoundState.Playing)
            {
                flamethrower_sfxi.Stop();
            }
        }

        public static void playTankFlameSoundEffect(bool play_sfx)
        {
            if (play_sfx && tankflamethrower_sfxi.State == SoundState.Stopped)
            {
                
                tankflamethrower_sfxi.Play();
            }
        }

        public static void stopFlameSoundEffect()
        {
            flamethrower_sfxi.Stop();
        }

        public static void stopTankFlameSoundEffect()
        {
            tankflamethrower_sfxi.Stop();
        }

        /// <summary>
        /// Acquire a sound effect from the library.
        /// </summary>
        /// <param name="sfxName">The name of the sound effect as specified in the manifest file.</param>
        /// /// <param name="pitchSlide">Adjust the pitch accordingly. (+/-)1.0f is a full octave. Passing 0.0f is the equivalent of no pitch changes.</param>
        /// <returns></returns>
        public static void playSoundEffect(string sfxName, float pitchSlide)
        {
            sfxLib[sfxName].Play(MediaPlayer.Volume, pitchSlide, 0.0f);
        }
    }
}
