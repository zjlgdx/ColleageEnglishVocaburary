using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Xml.Serialization;
using System.IO;

namespace PlaylistFilePlaybackAgent
{
    public class Playlist
    {
        public Playlist()
        {
            Tracks = new List<PlaylistTrack>();
        }

        [XmlElement(ElementName = "Track")]
        public List<PlaylistTrack> Tracks { set; get; }

        public static Playlist Load(string filename)
        {
            Playlist playlist = null;

            using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (IsolatedStorageFileStream stream = storage.OpenFile(filename, FileMode.Open))
                {
                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(Playlist));
                    playlist = xmlSerializer.Deserialize(stream) as Playlist;
                }
            }
            return playlist;
        }

        public void Save(string filename)
        {
            using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (storage.FileExists(filename))
                {
                    storage.DeleteFile(filename);
                }

                using (IsolatedStorageFileStream stream = storage.CreateFile(filename))
                {
                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(Playlist));
                    xmlSerializer.Serialize(stream, this);
                }
            }
        }
    }
}
