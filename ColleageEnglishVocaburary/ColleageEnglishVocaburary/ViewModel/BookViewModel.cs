﻿using CaptainsLog;
using Cimbalino.Phone.Toolkit.Services;
using ColleageEnglishVocaburary.Model;
using ColleageEnglishVocaburary.Resources;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.IO.IsolatedStorage;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ColleageEnglishVocaburary.ViewModel
{
    public class BookViewModel : INotifyPropertyChanged
    {
        private readonly IDataService _dataService;
        /// <summary>
        /// The navigation service.
        /// </summary>
        private readonly INavigationService _navigationService;

        private const string COURSE_ID = "{0}/0{1}p2newword1.htm";
        private const string COLLEAGE_ENGLISH_VOCABURARY_BOOK_ID = "Colleage_English_Vocaburary_book_{0}";

        /// <summary>
        /// * 参考：http://blog.csdn.net/lxcnn/article/details/4402808
        ///     * .NET正则基础之——平衡组
        ///     * 3.1.3  捕获组
        ///       这里面主要涉及到了两个捕获组“(?<Open>\()”和“(?<-Open>\))”，而在平衡组的应用中，我是只关心它是否匹配了，而对于匹配到的内容是不关心的。
        ///     * 对于这样一种需求，可以用以下方式实现
        ///        \( (?<Open>)
        ///        \)(?<-Open>)
        ///       “(?<Open>)”和“(?<-Open>)”这两种方式只是使用了命名捕获组，捕获的是一个位置，它总是能够匹配成功的，而匹配的内容是空的，分配的内存空间是固定的，
        ///     * 可以有效的节省资源，这在单字符嵌套结构中并不明显，但是在字符序列嵌套结构中就比较明显了。
        ///     * 由于捕获组是直接跟在开始或结束标记之后的，所以只要开始或结束标记匹配成功，命名捕获组自然就会匹配成功，对于功能是没有任何影响的。
        ///     * 备注：由于是匹配link标签，所以完全可以使用如下表达式：@"<(a)(?:(?!\bhref\b)[^<>])*href=([""']?){0}\2[^>]*>(?>(?:(?!</?\1).)*)</\1>"。
        ///     * 代码中的表达式只是更通用，并不仅限于a锚点标签。
        /// </summary>
        private const string COURSE_HYPER_LINK_PATTERN = @"<([a-z]+)(?:(?!\bhref\b)[^<>])*href=([""']?){0}\2[^>]*>(?><\1[^>]*>(?<o>)|</\1>(?<-o>)|(?:(?!</?\1).)*)*(?(o)(?!))</\1>";

        public BookViewModel(IDataService dataService, INavigationService navigationService)
        {
            _dataService = dataService;
            _navigationService = navigationService;
            this.Courses = new ObservableCollection<CourseViewModel>();
            CourseListCommand = new RelayCommand<string>(this.ShowCourseList);

        }

        private async Task Initial(string bookId)
        {
            this.BookId = string.Format(COLLEAGE_ENGLISH_VOCABURARY_BOOK_ID, bookId);
            using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!storage.FileExists(this.BookId))
                {

                    await DownloadCourseUnit(bookId);
                }
            }

            await this.LoadData();
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
                this.Courses.Add(new CourseViewModel(null,null)
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

                if (this._downloadingStatus == Constants.DOWNLOAD_COMPLETE)
                {
                    DisplayDownloadProcess = Visibility.Collapsed;
                    DisplayCourseList = Visibility.Visible;
                }
                else
                {
                    DisplayDownloadProcess = Visibility.Visible;
                    DisplayCourseList = Visibility.Collapsed;
                }
            }
        }

        private double _downloadingProcessPercentage;

        public double DownloadingProcessPercentage
        {
            get { return _downloadingProcessPercentage; }
            set
            {
                this.SetProperty(ref this._downloadingProcessPercentage, value);
            }
        }

        private Visibility _displayCourseList;

        public Visibility DisplayCourseList
        {
            get
            {
                return _displayCourseList;
            }
            set
            {
                this.SetProperty(ref this._displayCourseList, value);
            }
        }

        private Visibility _displayDownloadProcess;

        public Visibility DisplayDownloadProcess
        {
            get
            {
                return _displayDownloadProcess;
            }
            set
            {
                this.SetProperty(ref this._displayDownloadProcess, value);
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

        public ICommand CourseListCommand { get; private set; }

        private async void ShowCourseList(string bookId)
        {
            // todo: put the logic to data service
            //await _dataService.GetCourses(bookId);
            await Initial(bookId);

            _navigationService.NavigateTo(new Uri("/View/CourseList.xaml?bookId=" + bookId, UriKind.Relative));
        }

        /// <summary>
        /// 下载每册的单元目录
        /// </summary>
        /// <param name="bookId"></param>
        /// <returns></returns>
        private async Task DownloadCourseUnit(string bookId)
        {
            string url = string.Format(AppResources.COLLEGE_ENGLISH_ONLINE_BOOK_URL, bookId);
            var client = new WebClient { Encoding = DBCSCodePage.DBCSEncoding.GetDBCSEncoding("gb2312") };
            string response = await client.DownloadStringTaskAsync(new Uri(url));
            var book = new Book();
            book.BookName = GetBookName(bookId);
            var courses = new List<Course>();
            string[] hrefList = { "u1-p1-d.htm", "u2-p1-d.htm", "u3-p1-d.htm", "u4-p1-d.htm", "u5-p1-d.htm", "u6-p1-d.htm", "u7-p1-d.htm", "u8-p1-d.htm" };
            var courseMapping = new Dictionary<string, string> { 
                { "u1-p1-d.htm", "Unit One" }, 
                { "u2-p1-d.htm", "Unit Two" }, 
                { "u3-p1-d.htm", "Unit Three" }, 
                { "u4-p1-d.htm", "Unit Four" }, 
                { "u5-p1-d.htm", "Unit Five" }, 
                { "u6-p1-d.htm", "Unit Six" } ,
                { "u7-p1-d.htm", "Unit Seven" },
                { "u8-p1-d.htm", "Unit Eight" } 
            };
            var index = 0;

            foreach (string href in hrefList)
            {
                Match match = Regex.Match(response, string.Format(COURSE_HYPER_LINK_PATTERN, Regex.Escape(href)),
                               RegexOptions.Singleline | RegexOptions.IgnoreCase);

                if (match.Success)
                {
                    var regexHref = Regex.Match(match.Value, "(?<=src=\")images/home_\\d+.(gif|jpg)");
                    if (regexHref.Success)
                    {
                        this.DownloadingStatus = "Downloading unit : " + courseMapping[href];
                        var image = regexHref.Value;
                        var imageUrl = url + image;
                        var imagePath = (bookId + image).Replace("/", "_");
                        courses.Add(new Course { CourseId = string.Format(COURSE_ID, bookId, ++index), CourseName = imagePath, CourseImage = imagePath });

                        Stream stream = await client.OpenReadTaskAsync(imageUrl);
                        await FileStorageOperations.SaveToLocalFolderAsync(imagePath, stream);

                        //progressBar1.Value += progressBar1.LargeChange;
                        DownloadingProcessPercentage += DownloadingProcessPercentage;
                    }
                }
            }

            book.Courses = courses;
            await MyDataSerializer<Book>.SaveObjectsAsync(book, string.Format(COLLEAGE_ENGLISH_VOCABURARY_BOOK_ID, bookId));
            this.DownloadingStatus = Constants.DOWNLOAD_COMPLETE;
        }

        private string GetBookName(string bookId)
        {
            switch (bookId)
            {
                case "1":
                    return "第一册";
                case "2":
                    return "第二册";
                case "3":
                    return "第三册";
                case "4":
                    return "第四册";
                default:
                    return "Unknown";
            }
        }
    }
}