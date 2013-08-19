using System;
using System.Collections.Generic;
using System.Threading;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.GamerServices;

namespace PattyPetitGiant
{
    class SaveGameModule
    {
        private static StorageDevice device = null;
        private static IAsyncResult result = null;

        private static bool deviceFound = false;

        private const string filename = "pattypetitgiant.sav";

        private static bool saving = false;
        public static bool TouchingStorageDevice
        {
            get
            {
                return saving;
            }
        }

        public static void selectStorageDevice()
        {
            new Thread(do_selectStorageDevice).Start();
        }

        private static void do_selectStorageDevice()
        {
            // Set the request flag
            if ((!Guide.IsVisible))
            {
                deviceFound = false;

                result = StorageDevice.BeginShowSelector(InputDevice2.GetPlayerGamePadIndex(InputDevice2.PPG_Player.Player_1), null, null);

                result.AsyncWaitHandle.WaitOne();

                device = StorageDevice.EndShowSelector(result);

                deviceFound = true;
            }

        }

        public static void saveGame()
        {
            if (!saving)
            {
                new Thread(do_SaveGame).Start();
            }
        }

        private static void do_SaveGame()
        {
            saving = true;

            while (!deviceFound) ;

            if (result.IsCompleted)
            {
                if (device != null && device.IsConnected)
                {
                    try
                    {
                        IAsyncResult result2 = device.BeginOpenContainer("Contract: Void Justice", null, null);

                        result2.AsyncWaitHandle.WaitOne();

                        StorageContainer container = device.EndOpenContainer(result2);

                        result2.AsyncWaitHandle.Close();

                        // Check to see whether the save exists.
                        if (container.FileExists(filename))
                        {
                            container.DeleteFile(filename);
                        }

                        Stream stream = container.CreateFile(filename);

                        XmlSerializer serializer = new XmlSerializer(typeof(List<HighScoresState.HighScoreValue>));

                        serializer.Serialize(stream, HighScoresState.highScores);

                        stream.Close();

                        container.Dispose();
                    }
                    catch (Exception)
                    {
                        saving = false;
                        return;
                    }
                }
                else
                {
                    //Console.WriteLine("No save device detected");
                }
            }

            saving = false;
        }

        public static void loadGame()
        {
            if (!saving)
            {
                new Thread(do_LoadGame).Start();
            }
        }

        private static void do_LoadGame()
        {
            saving = true;

            while (!deviceFound) ;

            try
            {
                if (result.IsCompleted)
                {
                    if (device != null && device.IsConnected)
                    {
                        try
                        {
                            IAsyncResult result2 = device.BeginOpenContainer("Contract: Void Justice", null, null);

                            result2.AsyncWaitHandle.WaitOne();

                            StorageContainer container = device.EndOpenContainer(result2);

                            result2.AsyncWaitHandle.Close();

                            // Check to see whether the save exists.
                            if (!container.FileExists(filename))
                            {
                                saving = false;

                                container.Dispose();
                                return;
                            }

                            Stream stream = container.OpenFile(filename, FileMode.Open);

                            XmlSerializer serializer = new XmlSerializer(typeof(List<HighScoresState.HighScoreValue>));

                            HighScoresState.highScores = (List<HighScoresState.HighScoreValue>)serializer.Deserialize(stream);

                            stream.Close();

                            container.Dispose();
                        }
                        catch (Exception)
                        {
                            saving = false;
                            return;
                        }
                    }
                }
            }
            catch (Exception)
            {
                saving = false;
            }

            saving = false;
        }
    }
}
