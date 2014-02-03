using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace ColleageEnglishVocaburary.ViewModels
{
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