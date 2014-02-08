using ColleageEnglishVocaburary.ViewModels;
using Microsoft.Phone.BackgroundAudio;
using Microsoft.Phone.Controls;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace ColleageEnglishVocaburary
{
    public partial class WordCard : PhoneApplicationPage
    {
        bool _isForeground = true;

        private int index;

        private CourseViewModel viewModel = null;

        private WordViewModel _learningWord;
        public WordViewModel LearningWord
        {
            get
            {
                // Delay creation of the view model until necessary
                if (_learningWord == null)
                    _learningWord = new WordViewModel();

                return _learningWord;
            }
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

            DataContext = LearningWord;

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

            var learningWord = ViewModel.Words[index];

            SetWordCardProperties(learningWord.Word,
                learningWord.WordVoice,
                learningWord.WordPhrase,
                learningWord.Sentence,
                learningWord.SentenceVoice);
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
            var learningWord = ViewModel.Words[index];

            SetWordCardProperties(learningWord.Word,
                learningWord.WordVoice,
                learningWord.WordPhrase,
                learningWord.Sentence,
                learningWord.SentenceVoice);
        }


        private void SetWordCardProperties(String word, String wordVoice, String wordPhrase, String sentence, String sentenceVoice)
        {
            LearningWord.Word = word;
            LearningWord.WordVoice = wordVoice;
            LearningWord.WordPhrase = wordPhrase;
            LearningWord.Sentence = sentence;
            LearningWord.SentenceVoice = sentenceVoice;
        }
        
    }
}