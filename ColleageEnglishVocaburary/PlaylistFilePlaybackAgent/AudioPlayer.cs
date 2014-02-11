using System.Collections.Generic;
using Microsoft.Phone.BackgroundAudio;
using System;
using System.Windows;

namespace PlaylistFilePlaybackAgent
{
    public class AudioPlayer : AudioPlayerAgent
    {
        private const string _colleageenglishvocaburaryplaylistXml = "ColleageEnglishVocaburaryPlaylist.xml";
        static Playlist playlist;
        static int currentTrack = 0;

        private static volatile bool classInitialized;

        /// <remarks>
        /// AudioPlayer instances can share the same process. 
        /// Static fields can be used to share state between AudioPlayer instances
        /// or to communicate with the Audio Streaming agent.
        /// </remarks>
        public AudioPlayer()
        {
            if (!classInitialized)
            {
                classInitialized = true;

                // Subscribe to the managed exception handler
                Deployment.Current.Dispatcher.BeginInvoke(delegate
                {
                    Application.Current.UnhandledException += AudioPlayer_UnhandledException;
                });
            }
        }

        /// Code to execute on Unhandled Exceptions
        private void AudioPlayer_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // An unhandled exception has occurred; break into the debugger
                System.Diagnostics.Debugger.Break();
            }
        }

        /// <summary>
        /// Called when the playstate changes, except for the Error state (see OnError)
        /// </summary>
        /// <param name="player">The BackgroundAudioPlayer</param>
        /// <param name="track">The track playing at the time the playstate changed</param>
        /// <param name="playState">The new playstate of the player</param>
        /// <remarks>
        /// Play State changes cannot be cancelled. They are raised even if the application
        /// caused the state change itself, assuming the application has opted-in to the callback
        /// </remarks>
        protected override void OnPlayStateChanged(BackgroundAudioPlayer player, AudioTrack track, PlayState playState)
        {
            System.Diagnostics.Debug.WriteLine("OnPlayStateChanged");
            switch (playState)
            {
                case PlayState.TrackReady:
                    player.Play();
                    break;

                case PlayState.TrackEnded:

                    if (player.Track == null)
                    {
                        if (playlist == null || playlist.Tracks.Count == 0)
                        {
                            // Load playlist from isolated storage
                            playlist = Playlist.Load(_colleageenglishvocaburaryplaylistXml);

                            currentTrack = 0;

                            if (playlist.Tracks.Count == 0)
                            {
                                player.Track = null;
                                break;
                            }
                            else
                            {
                                player.Track = playlist.Tracks[currentTrack].ToAudioTrack();
                                break;
                            }
                        }
                    }

                    if (playlist != null && player.Track != null && player.Track.Tag == "S")
                    {
                        System.Diagnostics.Debug.WriteLine("play single");
                        playlist.Tracks= new List<PlaylistTrack>();
                        playlist.Save(_colleageenglishvocaburaryplaylistXml);
                        player.Track = null;
                        break;
                    }

                    if (null != playlist && playlist.Tracks.Count ==0)
                    {
                        player.Track = null;
                        break;
                    }

                    if (null != playlist && ++currentTrack >= playlist.Tracks.Count)
                    {
                        currentTrack = 0;
                    }

                    if (null != playlist)
                    {
                        player.Track = playlist == null ? null : playlist.Tracks[currentTrack].ToAudioTrack();
                    }
                   
                    System.Diagnostics.Debug.WriteLine("TrackEnded:" + (player.Track == null));
                    System.Diagnostics.Debug.WriteLine("currentTrack:" + currentTrack);
                    break;
                    
            }
            System.Diagnostics.Debug.WriteLine("playState:" + playState.ToString());
            NotifyComplete();
        }

        /// <summary>
        /// Called when the user requests an action using system-provided UI and the application has requesed
        /// notifications of the action
        /// </summary>
        /// <param name="player">The BackgroundAudioPlayer</param>
        /// <param name="track">The track playing at the time of the user action</param>
        /// <param name="action">The action the user has requested</param>
        /// <param name="param">The data associated with the requested action.
        /// In the current version this parameter is only for use with the Seek action,
        /// to indicate the requested position of an audio track</param>
        /// <remarks>
        /// User actions do not automatically make any changes in system state; the agent is responsible
        /// for carrying out the user actions if they are supported
        /// </remarks>
        protected override void OnUserAction(BackgroundAudioPlayer player, AudioTrack track, UserAction action, object param)
        {
            System.Diagnostics.Debug.WriteLine("OnUserAction");
            System.Diagnostics.Debug.WriteLine("UserAction:" + action.ToString());
            switch (action)
            {
                case UserAction.Play:
                    if (player.PlayerState != PlayState.Playing)
                    {
                        player.Play();
                    }
                  
                    System.Diagnostics.Debug.WriteLine("player.Play()" );
                   
                    break;
                case UserAction.Stop:
                    player.Stop();
                    break;
                case UserAction.Pause:
                    player.Pause();
                    break;
                case UserAction.FastForward:
                    player.FastForward();
                    break;
                case UserAction.Rewind:
                    player.Rewind();
                    break;
                case UserAction.SkipNext:
                    if (null != playlist && ++currentTrack >= playlist.Tracks.Count)
                    {
                        currentTrack = 0;
                        player.Track = playlist.Tracks[currentTrack].ToAudioTrack();
                    }

                    break;

                case UserAction.SkipPrevious:
                    if (null != playlist && --currentTrack < 0)
                    {
                        currentTrack = playlist.Tracks.Count - 1;
                        player.Track = playlist.Tracks[currentTrack].ToAudioTrack();
                    }
                    break;

                case UserAction.Seek:
                    player.Position = (TimeSpan)param;
                    break;
            }
            NotifyComplete();
        }

        /// <summary>
        /// Called whenever there is an error with playback, such as an AudioTrack not downloading correctly
        /// </summary>
        /// <param name="player">The BackgroundAudioPlayer</param>
        /// <param name="track">The track that had the error</param>
        /// <param name="error">The error that occured</param>
        /// <param name="isFatal">If true, playback cannot continue and playback of the track will stop</param>
        /// <remarks>
        /// This method is not guaranteed to be called in all cases. For example, if the background agent 
        /// itself has an unhandled exception, it won't get called back to handle its own errors.
        /// </remarks>
        protected override void OnError(BackgroundAudioPlayer player, AudioTrack track, Exception error, bool isFatal)
        {
            base.OnError(player, track, error, isFatal);

            if (isFatal)
            {
                Abort();
            }
            else
            {
                // force the track to stop
                player.Track = null;
                NotifyComplete();
            }
        }

        /// <summary>
        /// Called when the agent request is getting cancelled
        /// </summary>
        protected override void OnCancel()
        {
            base.OnCancel();
            NotifyComplete();
        }
    }
}
