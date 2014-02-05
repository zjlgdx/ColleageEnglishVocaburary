using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Threading;
using System.Xml.Serialization;
using System.IO;

namespace PlaylistFilePlaybackAgent
{
    public class Playlist
    {
        private static readonly Mutex mutex = new Mutex(false, "BackgroundCollegeEnglishPlayListMutex");

        public Playlist()
        {
            Tracks = new List<PlaylistTrack>();
        }

        [XmlElement(ElementName = "Track")]
        public List<PlaylistTrack> Tracks { set; get; }

        public static Playlist Load(string filename)
        {
            mutex.WaitOne();
            Playlist playlist = null;
            try
            {
                using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    using (IsolatedStorageFileStream stream = storage.OpenFile(filename, FileMode.Open))
                    {
                        XmlSerializer xmlSerializer = new XmlSerializer(typeof(Playlist));
                        playlist = xmlSerializer.Deserialize(stream) as Playlist;
                    }
                }
            }
            finally
            {
                // release the mutex even if we crashed
                mutex.ReleaseMutex();
            }

            return playlist;
        }

        public void Save(string filename)
        {
            // take the mutex
            mutex.WaitOne();

            try
            {
                using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    using (IsolatedStorageFileStream stream = storage.CreateFile(filename))
                    {
                        XmlSerializer xmlSerializer = new XmlSerializer(typeof(Playlist));
                        xmlSerializer.Serialize(stream, this);
                    }
                }
            }
            finally
            {
                // release the mutex even if we crashed
                mutex.ReleaseMutex();
            }
        }
    }
}
