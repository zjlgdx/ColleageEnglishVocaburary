using CaptainsLog;
using ColleageEnglishVocaburary.Model;
using ColleageEnglishVocaburary.ViewModels;
using Microsoft.Phone.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;

namespace ColleageEnglishVocaburary
{
    public partial class CourseList : PhoneApplicationPage
    {
        private BookViewModel viewModel = null;

        /// <summary>
        /// A static ViewModel used by the views to bind against.
        /// </summary>
        /// <returns>The CourseViewModel object.</returns>
        public BookViewModel ViewModel
        {
            get
            {
                // Delay creation of the view model until necessary
                if (viewModel == null)
                    viewModel = new BookViewModel();

                return viewModel;
            }
        }

        public CourseList()
        {
            InitializeComponent();

            DataContext = ViewModel;
        }

        private async Task downloadWord(string bookId)
        {
            string url = string.Format("http://educenter.fudan.edu.cn/collegeenglish/new/integrated{0}/", bookId);
            var client = new WebClient();
            client.Encoding = DBCSCodePage.DBCSEncoding.GetDBCSEncoding("gb2312");
            string response = await client.DownloadStringTaskAsync(new Uri(url));
            var book = new Book();
            var courses = new List<Course>();
            string[] hrefList = { "u1-p1-d.htm", "u2-p1-d.htm", "u3-p1-d.htm", "u4-p1-d.htm", "u5-p1-d.htm", "u6-p1-d.htm", "u7-p1-d.htm", "u8-p1-d.htm" };
            var index = 0;
            string pattern = @"<([a-z]+)(?:(?!\bhref\b)[^<>])*href=([""']?){0}\2[^>]*>(?><\1[^>]*>(?<o>)|</\1>(?<-o>)|(?:(?!</?\1).)*)*(?(o)(?!))</\1>";
            foreach (string href in hrefList)
            {
                Match match = Regex.Match(response, string.Format(pattern, Regex.Escape(href)),
                               RegexOptions.Singleline | RegexOptions.IgnoreCase);

                if (match.Success)
                {
                    var regexhref = Regex.Match(match.Value, "(?<=src=\")images/home_\\d+.(gif|jpg)");
                    if (regexhref.Success)
                    {
                        ViewModel.DownloadingItem = "Downloading unit :" + regexhref.Value;
                        var image = regexhref.Value;
                        var imageUrl = url + image;
                        var imagePath = (bookId + image).Replace("/", "_");
                        courses.Add(new Course { Id = string.Format("{0}/0{1}p2newword1.htm", bookId, ++index), CourseName = imagePath, CourseImage = imagePath });

                        Stream stream = await client.OpenReadTaskAsync(imageUrl);
                        await FileStorageOperations.SaveToLocalFolderAsync(imagePath, stream);
                    }
                }


            }
            book.Courses = courses;
            await MyDataSerializer<Book>.SaveObjectsAsync(book, "Colleage_English_Vocaburary_book_" + bookId);
            ViewModel.DownloadingItem = "DONE!";
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            string bookId = NavigationContext.QueryString["bookId"];

            ViewModel.Id = "Colleage_English_Vocaburary_book_" + bookId;
            using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!storage.FileExists(ViewModel.Id))
                {

                    await downloadWord(bookId);
                }
            }

            await ViewModel.LoadData();

            base.OnNavigatedTo(e);
        }

        private void Course_OnTap(object sender, GestureEventArgs gestureEventArgs)
        {
            var ui = sender as Image;
            NavigationService.Navigate(new Uri("/WordList.xaml?courseId=" + (string)ui.Tag, UriKind.Relative));
        }
    }
}