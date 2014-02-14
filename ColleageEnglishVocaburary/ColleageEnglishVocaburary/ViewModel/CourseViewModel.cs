using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using ColleageEnglishVocaburary.Model;

namespace ColleageEnglishVocaburary.ViewModel
{
    public class CourseViewModel : INotifyPropertyChanged
    {
        public CourseViewModel()
        {
            this.Words = new ObservableCollection<WordViewModel>();
            this.LearningWord = new WordViewModel();
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
    }
}
