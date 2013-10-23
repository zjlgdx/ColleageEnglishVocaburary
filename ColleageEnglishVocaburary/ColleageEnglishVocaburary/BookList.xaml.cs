using Microsoft.Phone.Controls;
using System;
using System.Windows.Input;
using System.Windows.Navigation;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;

namespace ColleageEnglishVocaburary
{
    public partial class BookList : PhoneApplicationPage
    {
        public BookList()
        {
            InitializeComponent();
        }

        private void BookOne_OnTap(object sender, GestureEventArgs gestureEventArgs)
        {
            NavigationService.Navigate(new Uri("/CourseList.xaml?bookId=1", UriKind.Relative));
        }

        private void BookTwo_OnTap(object sender, GestureEventArgs e)
        {
            NavigationService.Navigate(new Uri("/CourseList.xaml?bookId=2", UriKind.Relative));
        }

        private void BookThree_OnTap(object sender, GestureEventArgs e)
        {
            NavigationService.Navigate(new Uri("/CourseList.xaml?bookId=3", UriKind.Relative));
        }

        private void BookFour_OnTap(object sender, GestureEventArgs e)
        {
            NavigationService.Navigate(new Uri("/CourseList.xaml?bookId=4", UriKind.Relative));
        }
    }
}