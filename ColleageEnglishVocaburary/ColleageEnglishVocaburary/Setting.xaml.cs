using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Navigation;
using CaptainsLog;
using ColleageEnglishVocaburary.ViewModels;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace ColleageEnglishVocaburary
{
    public partial class Setting : PhoneApplicationPage
    {
        private LearningTypeViewModel _learningTypeViewModel;
        public Setting()
        {
            InitializeComponent();

            _learningTypeViewModel = new LearningTypeViewModel();
            this.Loaded += Setting_Loaded;
           
        }

        void Setting_Loaded(object sender, RoutedEventArgs e)
        {
            DisplaySetting();
        }

        private void ToggleSwitch_OnChecked(object sender, RoutedEventArgs e)
        {
            ToggleSwitch.Content = "卡片式";
            SetLearningStyle("LearningStyle", "卡片式");
        }

        private void SetLearningStyle(string key, string learningStyle)
        {
            

            IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;
            if (!settings.Contains(key))
            {
                settings.Add(key, learningStyle);
            }
            else
            {
                settings[key] = learningStyle;
            }
            settings.Save();
        }

        private void ToggleSwitch_OnUnchecked(object sender, RoutedEventArgs e)
        {
            ToggleSwitch.Content = "列表式";
            SetLearningStyle("LearningStyle", "列表式");
        }

        private void ReadToggleSwitch_OnChecked(object sender, RoutedEventArgs e)
        {
            SetLearningStyle("ReadWord", "Yes");
        }

        private void ReadToggleSwitch_OnUnchecked(object sender, RoutedEventArgs e)
        {
            SetLearningStyle("ReadWord", "No");
        }

        private void DisplaySetting()
        {
            string learningStyle = "列表式";
            if (IsolatedStorageSettings.ApplicationSettings.Contains("LearningStyle"))
            {
                learningStyle =
                    IsolatedStorageSettings.ApplicationSettings["LearningStyle"] as string;
            }

            ToggleSwitch.Content = learningStyle;
            if (learningStyle == "列表式")
            {
                ToggleSwitch.IsChecked = false;
            }
            else
            {
                ToggleSwitch.IsChecked = true;
            }

            string readWord = "No";
            if (IsolatedStorageSettings.ApplicationSettings.Contains("ReadWord"))
            {
                readWord = IsolatedStorageSettings.ApplicationSettings["ReadWord"] as string;
            }

            ReadToggleSwitch.IsChecked = readWord == "Yes";
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
        }
    }

    public class AutoReadingConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var autoReading = (bool) value;
            return autoReading ? "开启" : "关闭";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}