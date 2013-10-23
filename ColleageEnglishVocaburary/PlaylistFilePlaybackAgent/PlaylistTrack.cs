using System;
using Microsoft.Phone.BackgroundAudio;

namespace PlaylistFilePlaybackAgent
{
    public class PlaylistTrack
    {
        public string Source { set; get; }

        public string Title { set; get; }

        public string Artist { set; get; }

        public string Album { set; get; }

        public EnabledPlayerControls PlayerControls { set; get; }

        // Convert from this class to the standard WP 7.1 AudioTrack
        public AudioTrack ToAudioTrack()
        {
            return new AudioTrack(new Uri(this.Source,UriKind.Relative), this.Title, this.Artist, this.Album, null, null, this.PlayerControls);
        }
    }
}
