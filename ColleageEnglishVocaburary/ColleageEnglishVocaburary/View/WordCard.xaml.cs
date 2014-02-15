using System;
using System.Linq;
using System.Windows;
using System.Windows.Navigation;
using ColleageEnglishVocaburary.ViewModel;
using Microsoft.Phone.BackgroundAudio;
using Microsoft.Phone.Controls;

namespace ColleageEnglishVocaburary.View
{
    public partial class WordCard : PhoneApplicationPage
    {

        public WordCard()
        {
            InitializeComponent();

            this.myStoryboardX1.Completed += new EventHandler(Completed_StoryBoard1);
            this.myStoryboardX3.Completed += new EventHandler(Completed_StoryBoard3);

            this.Loaded += WordCard_Loaded;
        }

        void WordCard_Loaded(object sender, RoutedEventArgs e)
        {
            var learningWord = ViewModel.Words.FirstOrDefault();

            if (learningWord != null)
            { 
                SetWordCardProperties(learningWord.Word,
                    learningWord.WordVoice,
                    learningWord.WordPhrase,
                    learningWord.Sentence,
                    learningWord.SentenceVoice);
            }
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            string courseId = NavigationContext.QueryString["courseId"];
            ViewModel.CourseId = courseId.Replace("/", "_");

            await ViewModel.LoadData();

            var appSettings = new AppSettingsViewModel();

            readingWord = appSettings.AutoReadingSetting;
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

        

       

        

        

        //private void UITransform_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
        //{
        //    if (_isForeground)
        //    {
        //        this.myStoryboardX1.Begin();
        //    }
        //    else
        //    {
        //        this.myStoryboardX3.Begin();
        //    }

        //    this._isForeground = !this._isForeground;

        //    if (readingWord)
        //    {
        //        ReadWord();
        //    }
        //}

        private void ApplicationBarIconButton_OnClick(object sender, EventArgs e)
        {
            string courseId = NavigationContext.QueryString["courseId"];
            NavigationService.Navigate(new Uri("/Setting.xaml?courseId=" + courseId, UriKind.Relative));
        }
    }
}