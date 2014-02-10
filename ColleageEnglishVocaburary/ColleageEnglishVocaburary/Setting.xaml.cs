using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using System;
using System.Globalization;
using System.Windows.Data;

namespace ColleageEnglishVocaburary
{
    public partial class Setting : PhoneApplicationPage
    {
        public Setting()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            if (ToggleSwitch.IsChecked.HasValue && ToggleSwitch.IsChecked.Value)
            {
                NavigationService.Navigate(new Uri("/WordCard.xaml?courseId=" + NavigationContext.QueryString["courseId"],
                               UriKind.Relative));
            }
            else
            {
                NavigationService.Navigate(new Uri("/WordList.xaml?courseId=" + NavigationContext.QueryString["courseId"], UriKind.Relative));
            }
        }
    }

    public class LearningTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || value.ToString().Equals(""))
            {
                return false;
            }

            return value.ToString().Equals(Constants.WORD_CARD, StringComparison.CurrentCultureIgnoreCase);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var isChecked = (bool) value;
            return isChecked ? Constants.WORD_CARD : Constants.WORD_LIST;
        }
    }

}