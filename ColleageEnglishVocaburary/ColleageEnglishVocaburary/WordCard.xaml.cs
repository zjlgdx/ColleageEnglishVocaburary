using System.IO.IsolatedStorage;
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

        private bool readingWord;

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
            base.OnNavigatedTo(e);

            string courseId = NavigationContext.QueryString["courseId"];
            ViewModel.CourseId = courseId.Replace("/", "_");

            await ViewModel.LoadData();

            string readWord = "No";
            if (IsolatedStorageSettings.ApplicationSettings.Contains("ReadWord"))
            {
                readWord = IsolatedStorageSettings.ApplicationSettings["ReadWord"] as string;
            }

            readingWord = (readWord == "Yes");
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

        private void ReadWord()
        {
            var voice = _isForeground ? LearningWord.WordVoice : LearningWord.SentenceVoice;
            var text = _isForeground ? LearningWord.Word : LearningWord.Sentence;
            if (string.IsNullOrWhiteSpace(voice))
            {
                return;
            }
            var audioTrack =
                new AudioTrack(new Uri(voice, UriKind.Relative),
                                text,
                                text,
                                text,
                                null,
                                null,
                                EnabledPlayerControls.Pause);
            BackgroundAudioPlayer.Instance.Stop();
            BackgroundAudioPlayer.Instance.Track = audioTrack;
        }

        private void ReadWord_OnTap(object sender, GestureEventArgs e)
        {
            ReadWord();
        }

        private void UIPrevious_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            //_isForeground = true;
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

            if (readingWord)
            {
                ReadWord();
            }
        }

        private void UINext_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
           // _isForeground = true;
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

            if (readingWord)
            {
                ReadWord();
            }
        }

        private void SetWordCardProperties(String word, String wordVoice, String wordPhrase, String sentence, String sentenceVoice)
        {
            LearningWord.Word = word;
            LearningWord.WordVoice = wordVoice;
            LearningWord.WordPhrase = wordPhrase;
            LearningWord.Sentence = sentence;
            LearningWord.SentenceVoice = sentenceVoice;
        }

        private void UITransform_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
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

            if (readingWord)
            {
                ReadWord();
            }
        }

        private void ApplicationBarIconButton_OnClick(object sender, EventArgs e)
        {
            string courseId = NavigationContext.QueryString["courseId"];
            NavigationService.Navigate(new Uri("/Setting.xaml?courseId=" + courseId, UriKind.Relative));
        }
    }
}