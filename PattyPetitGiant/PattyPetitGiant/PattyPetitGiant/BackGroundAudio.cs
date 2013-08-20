using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;

namespace PattyPetitGiant
{
    class BackGroundAudio
    {
        public bool finished_song = false;

        private static Song song = null;
        public static SongCollection songs = null;

        public static Dictionary<string, Song> bgm_list = new Dictionary<string,Song>();

        private static ContentManager content_manager = null;

        public BackGroundAudio(ContentManager manager)
        {
            MediaPlayer.IsRepeating = true;
            content_manager = manager;
        }

        public void addSong(string new_song)
        {
            //sampleMediaLibrary.Songs;
            bgm_list.Add(new_song, content_manager.Load<Song>("bgm/"+new_song));
        }

        public static void playSong(string song_name, bool repeat)
        {
            if (!bgm_list.TryGetValue(song_name, out song))
            {
                throw new ArgumentException("No song name " + song_name + " exists");
            }

            MediaPlayer.Play(song);
            MediaPlayer.IsRepeating = repeat;
        }

        public static void changeVolume(float volume)
        {
            MediaPlayer.Volume = volume;
        }

        public static void stopAllSongs()
        {
            if (MediaPlayer.State == MediaState.Playing || MediaPlayer.State == MediaState.Paused)
            {
                MediaPlayer.Stop();
            }
        }
    }
}
