using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Navigation;
using ColleageEnglishVocaburary.ViewModels;
using Microsoft.Phone.BackgroundAudio;
using Microsoft.Phone.Controls;
using System;
using System.Windows;

namespace ColleageEnglishVocaburary
{
    public partial class WordCard : PhoneApplicationPage, INotifyPropertyChanged
    {
        bool _isForeground = true;

        private CourseViewModel viewModel = null;

        private int index;

        private WordViewModel _word;
        public WordViewModel Word
        {
            get
            {
                return _word;
            }
            set { this.SetProperty(ref this._word, value); }
        }

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

        public WordCard()
        {
            InitializeComponent();

            DataContext = Word;

            this.myStoryboardX1.Completed += new EventHandler(Completed_StoryBoard1);
            this.myStoryboardX3.Completed += new EventHandler(Completed_StoryBoard3);
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            string courseId = NavigationContext.QueryString["courseId"];
            ViewModel.CourseId = courseId.Replace("/", "_");

            await ViewModel.LoadData();
            base.OnNavigatedTo(e);
        }

        private void Completed_StoryBoard1(Object sender, EventArgs e)
        {
            this.WordStackPanel.Visibility = Visibility.Collapsed;
            this.WordPhraseStackPanel.Visibility = Visibility.Visible;
            this.myStoryboardX2.Begin();
        }

        private void Completed_StoryBoard3(Object sender, EventArgs e)
        {
            this.WordPhraseStackPanel.Visibility = Visibility.Collapsed;
            this.WordStackPanel.Visibility = Visibility.Visible;
            this.myStoryboardX4.Begin();
        }

        private void GestureListener_OnDoubleTap(object sender, GestureEventArgs e)
        {
            if (_isForeground)
            {
                this.myStoryboardX1.Begin();
            }
            else
            {
                this.myStoryboardX3.Begin();
            }

            this._isForeground = !this._isForeground;
        }

        private void UIWord_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
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
            BackgroundAudioPlayer.Instance.Stop();
            BackgroundAudioPlayer.Instance.Track = audioTrack;
        }

        private void UISentence_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
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
            BackgroundAudioPlayer.Instance.Stop();
            BackgroundAudioPlayer.Instance.Track = audioTrack;
        }

        private void UIPrevious_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (index == 0)
            {
                index = ViewModel.Words.Count -1;
            }
            else
            {
                index--;
            }
            Word = ViewModel.Words[index];
        }

        private void UINext_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (index == ViewModel.Words.Count -1)
            {
                index = 0;
            }
            else
            {
                index++;
            }
            Word = ViewModel.Words[index];
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] String propertyName = null)
        {
            if (object.Equals(storage, value))
            {
                return false;
            }

            storage = value;
            this.OnPropertyChanged(propertyName);
            return true;
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var eventHandler = this.PropertyChanged;
            if (eventHandler != null)
            {
                eventHandler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}