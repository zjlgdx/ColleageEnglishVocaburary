using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using ColleageEnglishVocaburary.Model;

namespace ColleageEnglishVocaburary.ViewModel
{
    public class BookViewModel : INotifyPropertyChanged
    {
        public BookViewModel()
        {
            this.Courses = new ObservableCollection<CourseViewModel>();
        }

        public async Task LoadData()
        {
            if (this.Courses.Count > 0)
            {
                return;
            }

            Book book = await MyDataSerializer<Book>.RestoreObjectsAsync(this.BookId);
            this.BookName = book.BookName;

            foreach (var course in book.Courses)
            {
                this.Courses.Add(new CourseViewModel
                {
                    CourseId = course.CourseId,
                    CourseName = course.CourseName,
                    CourseImage = course.CourseImage
                });
            }
        }

        private string _bookId;

        public string BookId
        {
            get { return _bookId; }
            set { this.SetProperty(ref this._bookId, value); }
        }

        public string _bookName;

        public string BookName
        {
            get { return _bookName; }
            set { this.SetProperty(ref this._bookName, value); }
        }

        private string _downloadingStatus;

        public string DownloadingStatus
        {
            get { return _downloadingStatus; }
            set
            {
                this.SetProperty(ref this._downloadingStatus, value);
            }
        }

        public ObservableCollection<CourseViewModel> Courses { get; private set; }

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

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
