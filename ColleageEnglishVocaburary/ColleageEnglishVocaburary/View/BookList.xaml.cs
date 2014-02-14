using System;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;

namespace ColleageEnglishVocaburary.View
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

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
           // NavigationService.g
        }
    }
}