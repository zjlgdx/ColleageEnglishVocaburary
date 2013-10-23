using CaptainsLog;
using ColleageEnglishVocaburary.Model;
using Microsoft.Phone.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;

namespace ColleageEnglishVocaburary
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        public MainPage()
        {
            InitializeComponent();
        }
        
        private void goTohomepage_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/BookList.xaml", UriKind.Relative));
        }
    }
}