using ColleageEnglishVocaburary.Model;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.IO.IsolatedStorage;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace ColleageEnglishVocaburary.ViewModels
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

            Book book = await MyDataSerializer<Book>.RestoreObjectsAsync(this.Id);
            this.BookName = book.BookName;

            foreach (var course in book.Courses)
            {
                this.Courses.Add(new CourseViewModel
                {
                    Id = course.Id,
                    CourseName = course.CourseName,
                    CourseImage = course.CourseImage
                });
            }
        }

        private string _id;

        public string Id
        {
            get { return _id; }
            set { this.SetProperty(ref this._id, value); }
        }

        public string _bookName;

        public string BookName
        {
            get { return _bookName; }
            set { this.SetProperty(ref this._bookName, value); }
        }

        private string _downloadingItem;

        public string DownloadingItem
        {
            get { return _downloadingItem; }
            set { this.SetProperty(ref this._downloadingItem, value); }
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

    public class IsoImageConverter : IValueConverter
    {
        //Convert Data to Image when Loading Data
        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            var bitmap = new BitmapImage();
            try
            {
                var path = (string)value;
                if (!String.IsNullOrEmpty(path))
                {
                    using (var file = LoadFile(path))
                    {
                        bitmap.SetSource(file);
                    }
                }
            }
            catch
            {
            }
            return bitmap;
        }

        private Stream LoadFile(string file)
        {
            using (var isoStore = IsolatedStorageFile.GetUserStoreForApplication())
            {
                return isoStore.OpenFile(file, FileMode.Open, FileAccess.Read);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
