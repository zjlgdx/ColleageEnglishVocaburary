using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Cimbalino.Phone.Toolkit.Services;
using ColleageEnglishVocaburary.Helper;
using ColleageEnglishVocaburary.Model;
using GalaSoft.MvvmLight.Command;
using Microsoft.Phone.BackgroundAudio;

namespace ColleageEnglishVocaburary.ViewModel
{
    public class CourseViewModel : INotifyPropertyChanged
    {
        private readonly IDataService _dataService;
        /// <summary>
        /// The navigation service.
        /// </summary>
        private readonly INavigationService _navigationService;

        private int index;

        bool _isForeground = true;

        private bool readingWord;

        public CourseViewModel(IDataService dataService, INavigationService navigationService)
        {
            _dataService = dataService;
            _navigationService = navigationService;
            this.Words = new ObservableCollection<WordViewModel>();
            this.LearningWord = new WordViewModel();
            CourseCommand = new RelayCommand<string>(this.ShowCourse);
            PreviousWordCommand = new RelayCommand<string>(this.ShowPreviousWord);
            NextWordCommand = new RelayCommand<string>(this.ShowNextWord);
            TransformCommand = new RelayCommand<string>(this.TransformBoard);
        }

        private void TransformBoard(string obj)
        {
            if (_isForeground)
            {
                StoryboardManager.PlayStoryboard("myStoryboardX1", (x) => StoryboardManager.PlayStoryboard("myStoryboardX2", (o) => { }, null), null);
            }
            else
            {
                StoryboardManager.PlayStoryboard("myStoryboardX3", (x) => StoryboardManager.PlayStoryboard("myStoryboardX4", (o) => { }, null), null);
            }

            this._isForeground = !this._isForeground;

            if (readingWord)
            {
                ReadWord();
            }

            
        }

        public ICommand PreviousWordCommand { get; private set; }
        public ICommand NextWordCommand { get; private set; }

        private void ShowNextWord(string obj)
        {
            if (index == this.Words.Count - 1)
            {
                index = 0;
            }
            else
            {
                index++;
            }
            var learningWord = this.Words[index];

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

        private void ReadWord()
        {
            var voice = _isForeground ? this.LearningWord.WordVoice : this.LearningWord.SentenceVoice;
            var text = _isForeground ? this.LearningWord.Word : this.LearningWord.Sentence;
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
            audioTrack.BeginEdit();
            audioTrack.Tag = "S";
            audioTrack.EndEdit();
            BackgroundAudioPlayer.Instance.Stop();
            BackgroundAudioPlayer.Instance.Track = audioTrack;
            BackgroundAudioPlayer.Instance.Play();
        }

        private void ShowPreviousWord(string obj)
        {
            if (index == 0)
            {
                index = this.Words.Count - 1;
            }
            else
            {
                index--;
            }

            var learningWord = this.Words[index];

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
            this.LearningWord.Word = word;
            this.LearningWord.WordVoice = wordVoice;
            this.LearningWord.WordPhrase = wordPhrase;
            this.LearningWord.Sentence = sentence;
            this.LearningWord.SentenceVoice = sentenceVoice;
        }

        public async Task LoadData()
        {
            if (this.Words.Count > 0)
            {
                return;
            }

            Course course = await MyDataSerializer<Course>.RestoreObjectsAsync(this.CourseId);
            this.CourseName = course.CourseName;

            foreach (var word in course.NewWords)
            {
                this.Words.Add(new WordViewModel
                    {
                        //CourseName = this.CourseName,
                        WordId = word.WordId,
                        Word = word.Word,
                        WordPhrase = word.WordPhrase,
                        Sentence = word.Sentence,
                        SentenceVoice = word.SentenceVoice,
                        WordVoice = word.WordVoice
                    });
            }

            var learingWord = course.NewWords.FirstOrDefault();
            if (learingWord != null)
            {
                //this.LearningWord.CourseName = learingWord.CourseName;
                this.LearningWord.WordId = learingWord.WordId;
                this.LearningWord.Word = learingWord.Word;
                this.LearningWord.WordPhrase = learingWord.WordPhrase;
                this.LearningWord.Sentence = learingWord.Sentence;
                this.LearningWord.SentenceVoice = learingWord.SentenceVoice;
                this.LearningWord.WordVoice = learingWord.WordVoice;
            }
        }

        private string _courseId;
        public string CourseId
        {
            get { return _courseId; }
            set { this.SetProperty(ref this._courseId, value); }
        }

        private string _courseName;
        public string CourseName
        {
            get { return _courseName; }
            set { this.SetProperty(ref this._courseName, value); }
        }

        private string _courseImage;
        public string CourseImage
        {
            get { return _courseImage; }
            set { this.SetProperty(ref this._courseImage, value); }
        }

        private string _downloadingStatus;
        public string DownloadingStatus
        {
            get { return _downloadingStatus; }
            set { this.SetProperty(ref this._downloadingStatus, value); }
        }

        /// <summary>
        /// A collection for ItemViewModel objects.
        /// </summary>
        public ObservableCollection<WordViewModel> Words { get; private set; }

        public WordViewModel LearningWord { get; private set; }

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
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public ICommand CourseCommand { get; private set; }
        public ICommand TransformCommand { get; private set; }

        

        private async void ShowCourse(string courseId)
        {
            // todo: put the logic to data service
            //await _dataService.GetCourses(bookId);

            using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (storage.FileExists(courseId.Replace("/", "_")))
                {
                    NavigateToLearningWord(courseId);
                }
                else
                {
                    _navigationService.NavigateTo(new Uri("/DownloadVocaburary.xaml?courseId=" + courseId, UriKind.Relative));
                }
            }

        }

        private void NavigateToLearningWord(string courseId)
        {
            var appSettings = new AppSettingsViewModel();

            if (appSettings.LearningTypeSetting.Equals(Constants.WORD_LIST))
            {
                _navigationService.NavigateTo(new Uri("/WordList.xaml?courseId=" + courseId, UriKind.Relative));
            }
            else
            {
                _navigationService.NavigateTo(new Uri("/WordCard.xaml?courseId=" + courseId,
                               UriKind.Relative));
            }

        }
    }
}
