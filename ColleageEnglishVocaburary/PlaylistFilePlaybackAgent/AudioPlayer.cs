using System;
using System.Windows;
using AudioSharedLibrary;
using Microsoft.Phone.BackgroundAudio;
using System.Collections.Generic;

namespace PlaylistFilePlaybackAgent
{
    public class AudioPlayer : AudioPlayerAgent
    {
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
            try
            {
                Exception ex = e.ExceptionObject;
                BackgroundErrorNotifier.AddError(ex);
            }
            catch (Exception)
            {
            }

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
            switch (playState)
            {
                case PlayState.TrackReady:
                    player.Play();
                    break;

                case PlayState.TrackEnded:
                    //if (playlist != null && currentTrack < playlist.Tracks.Count - 1)
                    //{
                    //    currentTrack += 1;
                    //    player.Track = playlist.Tracks[currentTrack].ToAudioTrack();
                    //}
                    //else
                    //{
                    //    player.Track = null;
                    //}

                    PlayNextTrack(player);
                    break;
            }
            NotifyComplete();
        }

        /// <summary>
        /// Plays the track in our playlist at the currentTrackNumber position.
        /// </summary>
        /// <param name="player">The BackgroundAudioPlayer</param>
        private void PlayTrack(BackgroundAudioPlayer player)
        {
            if (PlayState.Paused == player.PlayerState)
            {
                // If we're paused, we already have 
                // the track set, so just resume playing.
                player.Play();
            }
            else
            {
                if (playlist == null)
                {
                    return;
                }
                // Set which track to play. When the TrackReady state is received 
                // in the OnPlayStateChanged handler, call player.Play().
                player.Track = playlist.Tracks[currentTrack].ToAudioTrack();
            }

        }

        /// <summary>
        /// Increments the currentTrackNumber and plays the correpsonding track.
        /// </summary>
        /// <param name="player">The BackgroundAudioPlayer</param>
        private void PlayNextTrack(BackgroundAudioPlayer player)
        {
            if (null != playlist && ++currentTrack >= playlist.Tracks.Count)
            {
                currentTrack = 0;
            }

            PlayTrack(player);
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
            switch (action)
            {
                case UserAction.Play:
                    //if (player.Track == null)
                    //{
                    //    // Load playlist from isolated storage
                    //    playlist = Playlist.Load("ColleageEnglishVocaburaryPlaylist.xml");

                    //    currentTrack = 0;
                    //    player.Track = playlist.Tracks[currentTrack].ToAudioTrack();
                    //}
                    //else
                    //{
                    //    player.Play();
                    //}

                    if (player.Track == null)
                    {
                        // Load playlist from isolated storage
                        playlist = Playlist.Load("ColleageEnglishVocaburaryPlaylist.xml");

                        currentTrack = 0;
                        //player.Track = playlist.Tracks[currentTrack].ToAudioTrack();
                    }

                    PlayTrack(player);
                    break;

                case UserAction.Pause:
                    player.Pause();
                    break;

                case UserAction.SkipNext:
                    //if (currentTrack < playlist.Tracks.Count - 1)
                    //{
                    //    currentTrack += 1;
                    //    player.Track = playlist.Tracks[currentTrack].ToAudioTrack();
                    //}
                    //else
                    //{
                    //    player.Track = null;
                    //}

                    PlayNextTrack(player);
                    break;

                case UserAction.SkipPrevious:
                    //if (currentTrack > 0)
                    //{
                    //    currentTrack -= 1;
                    //    player.Track = playlist.Tracks[currentTrack].ToAudioTrack();
                    //}
                    //else
                    //{
                    //    player.Track = null;
                    //}

                    PlayPreviousTrack(player);
                    break;

                case UserAction.Seek:
                    player.Position = (TimeSpan)param;
                    break;
            }
            NotifyComplete();
        }

        /// <summary>
        /// Decrements the currentTrackNumber and plays the correpsonding track.
        /// </summary>
        /// <param name="player">The BackgroundAudioPlayer</param>
        private void PlayPreviousTrack(BackgroundAudioPlayer player)
        {
            if (--currentTrack < 0)
            {
                currentTrack = playlist.Tracks.Count - 1;
            }

            PlayTrack(player);
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
                BackgroundErrorNotifier.AddError(error);
                Abort();
            }
            else
            {
                BackgroundErrorNotifier.AddError(error);

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
