using System.Collections;
using CaptainsLog;
using ColleageEnglishVocaburary.Model;
using ColleageEnglishVocaburary.Resources;
using ColleageEnglishVocaburary.ViewModels;
using Microsoft.Phone.BackgroundAudio;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using PlaylistFilePlaybackAgent;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;

namespace ColleageEnglishVocaburary
{
    public partial class WordList : PhoneApplicationPage
    {
        private const string _colleageenglishvocaburaryplaylistXml = "ColleageEnglishVocaburaryPlaylist.xml";

        private CourseViewModel viewModel = null;
        
        Playlist _playlist;

        /// <summary>
        /// A static ViewModel used by the views to bind against.
        /// </summary>
        /// <returns>The CourseViewModel object.</returns>
        public CourseViewModel ViewModel
        {
            get
            {
                // Delay creation of the view model until necessary
                if (viewModel == null)
                    viewModel = new CourseViewModel();

                return viewModel;
            }
        }

        public WordList()
        {
            InitializeComponent();

            ApplicationBar = (ApplicationBar)Resources["DefaultAppBar"];

            DataContext = ViewModel;

            // Set PlayStateChanged handler
            BackgroundAudioPlayer.Instance.PlayStateChanged += OnBackgroundAudioPlayerPlayStateChanged;
        }

       
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            string courseId = NavigationContext.QueryString["courseId"];
            ViewModel.CourseId = courseId.Replace("/", "_");

            await ViewModel.LoadData();
            base.OnNavigatedTo(e);
        }

        private void menuItem1_Click(object sender, EventArgs e)
        {
            SavePlaylist(ViewModel.Words);
            BackgroundAudioPlayer.Instance.Play();
        }

        private void Word_OnTap(object sender, GestureEventArgs gestureEventArgs)
        {
            var uiElement = sender as TextBlock;
            var voice = (string)uiElement.Tag;
            var word = uiElement.Text;
            var audioTrack =
                new AudioTrack(new Uri(voice, UriKind.Relative),
                                word,
                                word,
                                word,
                                null,
                                null,
                                EnabledPlayerControls.Pause);
            audioTrack.BeginEdit();
            audioTrack.Tag = "S";
            audioTrack.EndEdit();
            BackgroundAudioPlayer.Instance.Stop();
            BackgroundAudioPlayer.Instance.Track = audioTrack;
        }

        private void Sentence_OnTap(object sender, GestureEventArgs e)
        {
            var uiElement = sender as TextBlock;
            var voice = (string)uiElement.Tag;
            var sentence = uiElement.Text;
            var audioTrack =
                new AudioTrack(new Uri(voice, UriKind.Relative),
                                sentence,
                                sentence,
                                sentence,
                                null,
                                null,
                                EnabledPlayerControls.Pause);
            audioTrack.BeginEdit();
            audioTrack.Tag = "S";
            audioTrack.EndEdit();
            BackgroundAudioPlayer.Instance.Stop();
            BackgroundAudioPlayer.Instance.Track = audioTrack;
        }

        private void WordsList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (WordsList.SelectedItems.Count > 0)
            {
                ApplicationBar = (ApplicationBar)Resources["PlayAppBar"];

                // Set fields of appbar buttons; otherwise they're null
                prevAppBarButton = this.ApplicationBar.Buttons[0] as ApplicationBarIconButton;
                playAppBarButton = this.ApplicationBar.Buttons[1] as ApplicationBarIconButton;
                pauseAppBarButton = this.ApplicationBar.Buttons[2] as ApplicationBarIconButton;
                nextAppBarButton = this.ApplicationBar.Buttons[3] as ApplicationBarIconButton;
            }
            else
            {
                ApplicationBar = (ApplicationBar)Resources["DefaultAppBar"];
                WordsList.EnforceIsSelectionEnabled = false;
            }
        }

        void UpdateScreen()
        {
            if (BackgroundAudioPlayer.Instance.Track != null)
            {
                if (prevAppBarButton != null)
                {
                    prevAppBarButton.IsEnabled = 0 != (BackgroundAudioPlayer.Instance.Track.PlayerControls & EnabledPlayerControls.SkipPrevious);
                    nextAppBarButton.IsEnabled = 0 != (BackgroundAudioPlayer.Instance.Track.PlayerControls & EnabledPlayerControls.SkipNext);
                    playAppBarButton.IsEnabled = BackgroundAudioPlayer.Instance.PlayerState != PlayState.Playing;
                    pauseAppBarButton.IsEnabled = BackgroundAudioPlayer.Instance.PlayerState != PlayState.Paused;
                }
            }
            else
            {
                if (prevAppBarButton != null)
                {
                    prevAppBarButton.IsEnabled = false;
                    playAppBarButton.IsEnabled = true;
                    pauseAppBarButton.IsEnabled = false;
                    nextAppBarButton.IsEnabled = false;
                }
            }
        }

        private void OnBackgroundAudioPlayerPlayStateChanged(object sender, EventArgs e)
        {
            UpdateScreen();
        }

        private void mnuSelect_Click(object sender, EventArgs e)
        {
            WordsList.EnforceIsSelectionEnabled = !WordsList.EnforceIsSelectionEnabled;
        }

        private void OnPrevAppBarButtonClick(object sender, EventArgs e)
        {
            BackgroundAudioPlayer.Instance.SkipPrevious();
            prevAppBarButton.IsEnabled = false;
        }

        private void OnPlayAppBarButtonClick(object sender, EventArgs e)
        {
            if (WordsList.SelectedItems.Count > 0)
            {
                SavePlaylist(WordsList.SelectedItems);
            }
            BackgroundAudioPlayer.Instance.Play();
            playAppBarButton.IsEnabled = false;
        }

        private void SavePlaylist(IList words)
        {
            // Create playlist
            _playlist = new Playlist();

            // Build the playlist
            int count = 0;
            foreach (WordViewModel word in words)
            {
                EnabledPlayerControls playerControls =
                    EnabledPlayerControls.Pause |
                    EnabledPlayerControls.SkipPrevious |
                    EnabledPlayerControls.SkipNext;

                var track = new PlaylistTrack
                {
                    Source = word.WordVoice,
                    Title = word.Word,
                    Artist = "College English",
                    Album = "College English Book",
                    PlayerControls = playerControls
                };

                _playlist.Tracks.Add(track);

                count++;
                if (!string.IsNullOrEmpty(word.SentenceVoice))
                {
                    var playerControls2 =
                        EnabledPlayerControls.Pause |
                        EnabledPlayerControls.SkipPrevious |
                        EnabledPlayerControls.SkipNext;

                    PlaylistTrack track2 = new PlaylistTrack
                    {
                        Source = word.SentenceVoice,
                        Title = word.Sentence,
                        Artist = "College English",
                        Album = "College English Book",
                        PlayerControls = playerControls2
                    };
                    _playlist.Tracks.Add(track2);

                    count++;
                }
            }

            // Save it to isolated storage
            _playlist.Save(_colleageenglishvocaburaryplaylistXml);
        }

        private void OnPauseAppBarButtonClick(object sender, EventArgs e)
        {
            BackgroundAudioPlayer.Instance.Pause();
            pauseAppBarButton.IsEnabled = false;
        }

        private void OnNextAppBarButtonClick(object sender, EventArgs e)
        {
            BackgroundAudioPlayer.Instance.SkipNext();
            nextAppBarButton.IsEnabled = false;
        }


        private void ApplicationBarMenuItemSetting_OnClick(object sender, EventArgs e)
        {
            string courseId = NavigationContext.QueryString["courseId"];
            NavigationService.Navigate(new Uri("/Setting.xaml?courseId=" + courseId, UriKind.Relative));
        }
    }
}