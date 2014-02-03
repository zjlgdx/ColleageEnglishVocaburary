using ColleageEnglishVocaburary.Model;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace ColleageEnglishVocaburary.ViewModels
{
    public class CourseViewModel : INotifyPropertyChanged
    {
        public CourseViewModel()
        {
            this.Words = new ObservableCollection<WordViewModel>();
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
                        Id = word.Id,
                        Word = word.Word,
                        Meaning = word.Meaning,
                        Sentence = word.Sentence,
                        SentenceVoice = word.SentenceVoice,
                        WordVoice = word.WordVoice
                    });
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

        private string _downloadingItem;
        public string DownloadingItem
        {
            get { return _downloadingItem; }
            set { this.SetProperty(ref this._downloadingItem, value); }
        }

        /// <summary>
        /// A collection for ItemViewModel objects.
        /// </summary>
        public ObservableCollection<WordViewModel> Words { get; private set; }

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
