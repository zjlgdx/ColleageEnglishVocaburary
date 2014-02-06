using System.Collections;
using CaptainsLog;
using ColleageEnglishVocaburary.Model;
using ColleageEnglishVocaburary.Resources;
using ColleageEnglishVocaburary.ViewModels;
using Microsoft.Phone.BackgroundAudio;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using PlaylistFilePlaybackAgent;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;

namespace ColleageEnglishVocaburary
{
    public partial class WordList : PhoneApplicationPage
    {
        #region regex expression

        // 参考：.NET正则基础之——平衡组(http://blog.csdn.net/lxcnn/article/details/4402808)
        // 提取单词段落表达式
        private const string PARAGRAPH_PATTERN = @"(?isx)   #匹配模式，忽略大小写，“.”匹配任意字符
                                                    <p[^>]*>  #开始标记“<p...>”
                                                    (?>       #分组构造，用来限定量词“*”修饰范围
                                                       <p[^>]*>(?<Open>)   #命名捕获组，遇到开始标记，入栈，Open计数加1
                                                    |         #分支结构
                                                       </p>(?<-Open>)      #狭义平衡组，遇到结束标记，出栈，Open计数减1
                                                    |                      #分支结构
                                                       <a\s+name=""nw\d+""\s*></a>(?:(?!</?p\b).)+      #右侧紧接着<a name=""nw2""></a>，之后右侧不为开始或结束标记的任意字符
                                                    )+                     #以上子串出现0次或任意多次
                                                    (?(Open)(?!))          #判断是否还有'OPEN'，有则说明不配对，什么都不匹配
                                                    </p>                   #结束标记“</p>”
                                                    ";
        // 提取mp3媒体文件正则表达式
        private const string MP3_MEDIA_PATTERN = @"(?isx)
                                                   \(              #普通字符“(”
                                                  (?>              #分组构造，用来限定量词“*”修饰范围
                                                    [^()]+         #非括弧的其它任意字符
                                                  |                #分支结构
                                                    \(  (?<Open>)  #命名捕获组，遇到开括弧Open计数加1
                                                  |                #分支结构
                                                    \)  (?<-Open>) #狭义平衡组，遇到闭括弧Open计数减1
                                                  )+               #以上子串出现0次或任意多次
                                                  (?(Open)(?!))    #判断是否还有'OPEN'，有则说明不配对，什么都不匹配
                                                  \)               #普通闭括弧
                                                 ";
        private const string WORD_PATTERN = @"(?isx)
                                                <(font)\s+color=""#3366cc""\s*>    #开始标记“<tag...>”
                                                (?>                         #分组构造，用来限定量词“*”修饰范围
                                                    <\1[^>]*>  (?<Open>)    #命名捕获组，遇到开始标记，入栈，Open计数加1
                                                |                           #分支结构
                                                    </\1>  (?<-Open>)       #狭义平衡组，遇到结束标记，出栈，Open计数减1
                                                |                           #分支结构
                                                    (?:(?!</?\1\b).)+       #右侧不为开始或结束标记的任意字符
                                                )+                          #以上子串出现0次或任意多次
                                                (?(Open)(?!))               #判断是否还有'OPEN'，有则说明不配对，什么都不匹配
                                                </\1>                       #结束标记“</tag>”
                                            ";

        private const string WORD_PHRASE_PATTERN =@"(?is)<(br)\b[^>]*>((?!<font\s+color=""#336600""\s*>).)*";
        private const string SENTENCE_PATTERN = @"(?<=<font\s+color=""#336600""\s*>).*";
        private const string _colleageenglishvocaburaryplaylistXml = "ColleageEnglishVocaburaryPlaylist.xml";

        #endregion

        private CourseViewModel viewModel = null;
        
        Playlist _playlist;

        /// <summary>
        /// A static ViewModel used by the views to bind against.
        /// </summary>
        /// <returns>The CourseViewModel object.</returns>
        public CourseViewModel ViewModel
        {
            get
            {
                // Delay creation of the view model until necessary
                if (viewModel == null)
                    viewModel = new CourseViewModel();

                return viewModel;
            }
        }

        public WordList()
        {
            InitializeComponent();

            ApplicationBar = (ApplicationBar)Resources["DefaultAppBar"];

            DataContext = ViewModel;

            // Set PlayStateChanged handler
            BackgroundAudioPlayer.Instance.PlayStateChanged += OnBackgroundAudioPlayerPlayStateChanged;
        }

       
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            string courseId = NavigationContext.QueryString["courseId"];
            ViewModel.CourseId = courseId.Replace("/", "_");
            using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!storage.FileExists(ViewModel.CourseId))
                {
                    downloadListStatus.Visibility = Visibility.Visible;
                    wordListItem.Visibility = Visibility.Collapsed;
                    progressBar1.Value = 0;
                    await DownloadWord(courseId);
                }
            }

            await ViewModel.LoadData();
            downloadListStatus.Visibility = Visibility.Collapsed;
            wordListItem.Visibility = Visibility.Visible;
            base.OnNavigatedTo(e);
        }

        #region FetchWords

        private async Task DownloadWord(string courseId)
        {
            var bookId = courseId.Substring(0, 2);
            string bookUrl = AppResources.COLLEGE_ENGLISH_ONLINE_BOOK_BASE_URL + bookId; 
            string courseUrl = AppResources.COLLEGE_ENGLISH_ONLINE_BOOK_BASE_URL + courseId; 

            var client = new WebClient { Encoding = DBCSCodePage.DBCSEncoding.GetDBCSEncoding("gb2312") };
            string response = await client.DownloadStringTaskAsync(new Uri(courseUrl));

            var courseName = GetCourseName(courseId);
            var course = new Course { CourseId = courseId, CourseName = courseName };
            var newWords = new List<NewWord>();
 
            var regexParagraph = new Regex(PARAGRAPH_PATTERN);
            
            var matchParagraphes = regexParagraph.Matches(response);

            var wordId = 0;
            double totalCount = matchParagraphes.Count;
            var percentage = 1d/totalCount*100d;
            progressBar1.LargeChange = percentage;

            foreach (Match matchParagraph in matchParagraphes)
            {
                var paragraph = matchParagraph.Value;
                var objWord = new NewWord();
                objWord.WordId = (wordId++).ToString();

                await FetchMedia(paragraph, course, objWord, client, bookUrl);

                // word and phase
                FetchWord(paragraph, objWord);

                FetchWordPhrase(paragraph, objWord);

                FetchSentence(paragraph, objWord);

                newWords.Add(objWord);

                progressBar1.Value += progressBar1.LargeChange;
            }

            course.NewWords = newWords;

            await MyDataSerializer<Course>.SaveObjectsAsync(course, ViewModel.CourseId);

            ViewModel.DownloadingStatus = Constants.DOWNLOAD_COMPLETE;
        }

        private static async Task FetchMedia(string paragraph, 
                                             Course course, 
                                             NewWord objWord, 
                                             WebClient client,
                                             string bookUrl)
        {
            var regexMedia = new Regex(MP3_MEDIA_PATTERN);
            var matchMedias = regexMedia.Matches(paragraph);
            var index = 0;
            foreach (Match matchMedia in matchMedias)
            {
                // 提取mp3文件并保存到独立存储中
                var mp3 = matchMedia.Value.Replace("('", "").Replace("')", "");
                if (!mp3.EndsWith(".mp3"))
                {
                    continue;
                }
                var mp3Path = course.CourseName + objWord.WordId + mp3;
                mp3Path = mp3Path.Replace("/", "");
                if (index == 0)
                {
                    objWord.WordVoice = mp3Path;
                }
                else
                {
                    objWord.SentenceVoice = mp3Path;
                }

                index++;
                try
                {
                    Stream stream = await client.OpenReadTaskAsync(bookUrl + mp3);
                    await FileStorageOperations.SaveToLocalFolderAsync(mp3Path, stream);
                }
                catch (Exception)
                {
                }
            }
        }

        private static void FetchSentence(string paragraph, NewWord objWord)
        {
            var regexSentence = new Regex(SENTENCE_PATTERN, RegexOptions.Singleline);
            var sentenceMatch = regexSentence.Match(paragraph);
            if (sentenceMatch.Success)
            {
                var regexMark = new Regex("<[^>]+>");
                var sentence = regexMark.Replace(sentenceMatch.Value, "");
                sentence = sentence.Replace("&nbsp;&nbsp;e.g.", "e.g.");
                sentence = Regex.Replace(sentence, "\\s+", " ");
                sentence = Regex.Replace(sentence, "&nbsp;$", "");
                objWord.Sentence = sentence;
            }
        }

        private static void FetchWordPhrase(string paragraph, NewWord objWord)
        {
            var regexWordPhrase = new Regex(WORD_PHRASE_PATTERN);
            var matchWordPhrase = regexWordPhrase.Match(paragraph);
            if (matchWordPhrase.Success)
            {
                var wordPhrase = Regex.Replace(matchWordPhrase.Value, "\\s+|<br>", " ").Trim();
                wordPhrase = Regex.Replace(wordPhrase, "<[^>]+>", "");
                wordPhrase = Regex.Replace(wordPhrase, "&nbsp;$", "");
                objWord.WordPhrase = wordPhrase;
            }
        }

        private void FetchWord(string paragraph, NewWord objWord)
        {
            var regexWord = new Regex(WORD_PATTERN);

            var matchWord = regexWord.Match(paragraph);

            if (matchWord.Success)
            {
                var word = Regex.Replace(matchWord.Value, "\\s+", " ");
                word = Regex.Replace(word, @"^<font\s+color=""#3366cc""\s*>", "");
                word = Regex.Replace(word, @"</font>$", "");
                word = Regex.Replace(word, @"<[^>]+>", "");
                objWord.Word = word;

                ViewModel.DownloadingStatus = "Downloading word : " + word;
            }
        }

        #endregion

        private void menuItem1_Click(object sender, EventArgs e)
        {
            SavePlaylist(ViewModel.Words);
            BackgroundAudioPlayer.Instance.Play();
        }

        private void Word_OnTap(object sender, GestureEventArgs gestureEventArgs)
        {
            var uiElement = sender as TextBlock;
            var voice = (string)uiElement.Tag;
            var word = uiElement.Text;
            var audioTrack =
                new AudioTrack(new Uri(voice, UriKind.Relative),
                                word,
                                word,
                                word,
                                null,
                                null,
                                EnabledPlayerControls.Pause);
            BackgroundAudioPlayer.Instance.Stop();
            BackgroundAudioPlayer.Instance.Track = audioTrack;
        }

        private void Sentence_OnTap(object sender, GestureEventArgs e)
        {
            var uiElement = sender as TextBlock;
            var voice = (string)uiElement.Tag;
            var sentence = uiElement.Text;
            var audioTrack =
                new AudioTrack(new Uri(voice, UriKind.Relative),
                                sentence,
                                sentence,
                                sentence,
                                null,
                                null,
                                EnabledPlayerControls.Pause);
            BackgroundAudioPlayer.Instance.Stop();
            BackgroundAudioPlayer.Instance.Track = audioTrack;
        }

        private void WordsList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (WordsList.SelectedItems.Count > 0)
            {
                ApplicationBar = (ApplicationBar)Resources["PlayAppBar"];

                // Set fields of appbar buttons; otherwise they're null
                prevAppBarButton = this.ApplicationBar.Buttons[0] as ApplicationBarIconButton;
                playAppBarButton = this.ApplicationBar.Buttons[1] as ApplicationBarIconButton;
                pauseAppBarButton = this.ApplicationBar.Buttons[2] as ApplicationBarIconButton;
                nextAppBarButton = this.ApplicationBar.Buttons[3] as ApplicationBarIconButton;
            }
            else
            {
                ApplicationBar = (ApplicationBar)Resources["DefaultAppBar"];
                WordsList.EnforceIsSelectionEnabled = false;
            }
        }

        void UpdateScreen()
        {
            if (BackgroundAudioPlayer.Instance.Track != null)
            {
                if (prevAppBarButton != null)
                {
                    prevAppBarButton.IsEnabled = 0 != (BackgroundAudioPlayer.Instance.Track.PlayerControls & EnabledPlayerControls.SkipPrevious);
                    nextAppBarButton.IsEnabled = 0 != (BackgroundAudioPlayer.Instance.Track.PlayerControls & EnabledPlayerControls.SkipNext);
                    playAppBarButton.IsEnabled = BackgroundAudioPlayer.Instance.PlayerState != PlayState.Playing;
                    pauseAppBarButton.IsEnabled = BackgroundAudioPlayer.Instance.PlayerState != PlayState.Paused;
                }
            }
            else
            {
                if (prevAppBarButton != null)
                {
                    prevAppBarButton.IsEnabled = false;
                    playAppBarButton.IsEnabled = true;
                    pauseAppBarButton.IsEnabled = false;
                    nextAppBarButton.IsEnabled = false;
                }
            }
        }

        private void OnBackgroundAudioPlayerPlayStateChanged(object sender, EventArgs e)
        {
            UpdateScreen();
        }

        private void mnuSelect_Click(object sender, EventArgs e)
        {
            WordsList.EnforceIsSelectionEnabled = !WordsList.EnforceIsSelectionEnabled;
        }

        private void OnPrevAppBarButtonClick(object sender, EventArgs e)
        {
            BackgroundAudioPlayer.Instance.SkipPrevious();
            prevAppBarButton.IsEnabled = false;
        }

        private void OnPlayAppBarButtonClick(object sender, EventArgs e)
        {
            if (WordsList.SelectedItems.Count > 0)
            {
                SavePlaylist(WordsList.SelectedItems);
            }
            BackgroundAudioPlayer.Instance.Play();
            playAppBarButton.IsEnabled = false;
        }

        private void SavePlaylist(IList words)
        {
            // Create playlist
            _playlist = new Playlist();

            // Build the playlist
            int count = 0;
            foreach (WordViewModel word in words)
            {
                EnabledPlayerControls playerControls =
                    EnabledPlayerControls.Pause |
                    EnabledPlayerControls.SkipPrevious |
                    EnabledPlayerControls.SkipNext;

                var track = new PlaylistTrack
                {
                    Source = word.WordVoice,
                    Title = word.Word,
                    Artist = "College English",
                    Album = "College English Book",
                    PlayerControls = playerControls
                };

                _playlist.Tracks.Add(track);

                count++;
                if (!string.IsNullOrEmpty(word.SentenceVoice))
                {
                    var playerControls2 =
                        EnabledPlayerControls.Pause |
                        EnabledPlayerControls.SkipPrevious |
                        EnabledPlayerControls.SkipNext;

                    PlaylistTrack track2 = new PlaylistTrack
                    {
                        Source = word.SentenceVoice,
                        Title = word.Sentence,
                        Artist = "College English",
                        Album = "College English Book",
                        PlayerControls = playerControls2
                    };
                    _playlist.Tracks.Add(track2);

                    count++;
                }
            }

            // Save it to isolated storage
            _playlist.Save(_colleageenglishvocaburaryplaylistXml);
        }

        private void OnPauseAppBarButtonClick(object sender, EventArgs e)
        {
            BackgroundAudioPlayer.Instance.Pause();
            pauseAppBarButton.IsEnabled = false;
        }

        private void OnNextAppBarButtonClick(object sender, EventArgs e)
        {
            BackgroundAudioPlayer.Instance.SkipNext();
            nextAppBarButton.IsEnabled = false;
        }

        private static string GetCourseName(string courseId)
        {
            var bookId = courseId.Substring(0, 1);
            string bookName;
            var unitId = courseId.Substring(2, 2);
            string unitName;
            switch (bookId)
            {
                case "1":
                    bookName = "第一册";
                    break;
                case "2":
                    bookName= "第二册";
                    break;
                case "3":
                    bookName= "第三册";
                    break;
                case "4":
                    bookName= "第四册";
                    break;
                default:
                    bookName= "Unknown";
                    break;
            }

            switch (unitId)
            {
                case "01":
                    unitName = "第一单元";
                    break;
                case "02":
                    unitName = "第二单元";
                    break;
                case "03":
                    unitName = "第三单元";
                    break;
                case "04":
                    unitName = "第四单元";
                    break;
                case "05":
                    unitName = "第五单元";
                    break;
                case "06":
                    unitName = "第六单元";
                    break;
                case "07":
                    unitName = "第七单元";
                    break;
                case "08":
                    unitName = "第八单元";
                    break;
                default:
                    unitName = "Unknown";
                    break;
            }

            return string.Format("{0} {1}", bookName, unitName);
        }
    }
}