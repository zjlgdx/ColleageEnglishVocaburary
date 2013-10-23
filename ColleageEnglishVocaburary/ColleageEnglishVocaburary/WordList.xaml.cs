using System.Windows.Media;
using CaptainsLog;
using ColleageEnglishVocaburary.Model;
using ColleageEnglishVocaburary.ViewModels;
using Microsoft.Phone.BackgroundAudio;
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
using Microsoft.Phone.Shell;
using PlaylistFilePlaybackAgent;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;

namespace ColleageEnglishVocaburary
{
    public partial class WordList : PhoneApplicationPage
    {

        private CourseViewModel viewModel = null;

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

            ApplicationBar = (Microsoft.Phone.Shell.ApplicationBar)Resources["DefaultAppBar"];

            DataContext = ViewModel;
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            string courseId = NavigationContext.QueryString["courseId"];
            ViewModel.Id = courseId.Replace("/", "_");
            using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!storage.FileExists(ViewModel.Id))
                {

                    await downloadWord(courseId);
                }
            }

            await ViewModel.LoadData();

            base.OnNavigatedTo(e);
        }

        private async Task downloadWord(string courseId)
        {
            //status.Text = "Start to download words!";
            var bookId = courseId.Substring(0, 2);
            string baseUrl = "http://educenter.fudan.edu.cn/collegeenglish/new/integrated" + bookId;
            string url = "http://educenter.fudan.edu.cn/collegeenglish/new/integrated" + courseId;

            var client = new WebClient();
            client.Encoding = DBCSCodePage.DBCSEncoding.GetDBCSEncoding("gb2312");
            string response = await client.DownloadStringTaskAsync(new Uri(url));
            //xmld
            var course = new Course();
            course.Id = courseId;
            course.CourseName = courseId;
            var newWords = new List<NewWord>();

            // 参考：.NET正则基础之——平衡组(http://blog.csdn.net/lxcnn/article/details/4402808)
            var regexParagraph = new Regex(@"(?isx)                      #匹配模式，忽略大小写，“.”匹配任意字符
                      <p[^>]*>                      #开始标记“<p...>”
                          (?>                         #分组构造，用来限定量词“*”修饰范围
                              <p[^>]*>  (?<Open>)   #命名捕获组，遇到开始标记，入栈，Open计数加1
                          |                           #分支结构
                              </p>  (?<-Open>)      #狭义平衡组，遇到结束标记，出栈，Open计数减1
                          |                           #分支结构
                              <a\s+name=""nw\d+""\s*></a>(?:(?!</?p\b).)*      #右侧紧接着<a name=""nw2""></a>，之后右侧不为开始或结束标记的任意字符
                          )*                          #以上子串出现0次或任意多次
                          (?(Open)(?!))               #判断是否还有'OPEN'，有则说明不配对，什么都不匹配
                      </p>                          #结束标记“</p>”
                      ", RegexOptions.Compiled | RegexOptions.Singleline);
            var mc = regexParagraph.Matches(response);

            var regexMedia = new Regex(@"\(                         #普通字符“(”
                            (?>                     #分组构造，用来限定量词“*”修饰范围
                                [^()]+              #非括弧的其它任意字符
                            |                       #分支结构
                                \(  (?<Open>)       #命名捕获组，遇到开括弧Open计数加1
                            |                       #分支结构
                                \)  (?<-Open>)      #狭义平衡组，遇到闭括弧Open计数减1
                            )*                      #以上子串出现0次或任意多次
                            (?(Open)(?!))           #判断是否还有'OPEN'，有则说明不配对，什么都不匹配
                        \)                          #普通闭括弧
                       ", RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled | RegexOptions.Singleline);
            var wordId = 0;
            foreach (Match m in mc)
            {
                var expression = m.Value;
                var word = new NewWord();
                word.Id = (wordId++).ToString();
                var matches = regexMedia.Matches(expression);
                var index = 0;
                foreach (Match match in matches)
                {
                    // mp3 files
                    var mp3 = match.Value.Replace("('", "").Replace("')", "");
                    if (!mp3.EndsWith(".mp3"))
                    {
                        continue;
                    }
                    var mp3Path = course.CourseName + word.Id + mp3;
                    mp3Path = mp3Path.Replace("/", "");
                    if (index == 0)
                    {
                        word.WordVoice = mp3Path;
                    }
                    else
                    {
                        word.SentenceVoice = mp3Path;
                    }

                    index++;

                    Stream stream = await client.OpenReadTaskAsync(baseUrl + mp3);
                    await FileStorageOperations.SaveToLocalFolderAsync(mp3Path, stream);



                }

                // word and phase
                //<font color="#3366cc">
                string color = Regex.Escape("#3366cc");                    //动态获取id
                string pattern = @"(?isx)
                      <(font)\s+color=""#3366cc""\s*>                 #开始标记“<tag...>”
                          (?>                         #分组构造，用来限定量词“*”修饰范围
                              <\1[^>]*>  (?<Open>)    #命名捕获组，遇到开始标记，入栈，Open计数加1
                          |                           #分支结构
                              </\1>  (?<-Open>)       #狭义平衡组，遇到结束标记，出栈，Open计数减1
                          |                           #分支结构
                              (?:(?!</?\1\b).)*       #右侧不为开始或结束标记的任意字符
                          )*                          #以上子串出现0次或任意多次
                          (?(Open)(?!))               #判断是否还有'OPEN'，有则说明不配对，什么都不匹配
                      </\1>                           #结束标记“</tag>”
                     ";
                //pattern = @"(?<=<font\s+color=""#3366cc""\s*>)[^<]+(?=</font>)";
                var regexWord = new Regex(pattern, RegexOptions.Compiled | RegexOptions.Singleline);



                var matchWord = regexWord.Match(expression);

                if (matchWord.Success)
                {
                    var wordParaphrase = Regex.Replace(matchWord.Value, "\\s+", " ");
                    wordParaphrase = Regex.Replace(wordParaphrase, @"^<font\s+color=""#3366cc""\s*>", "");
                    wordParaphrase = Regex.Replace(wordParaphrase, @"</font>$", "");
                    wordParaphrase = Regex.Replace(wordParaphrase, @"<[^>]+>", "");
                    word.Word = wordParaphrase;

                    ViewModel.DownloadingItem = "downloading " + wordParaphrase;
                    //word.WordSavePath = lession.LessionName + word.WordId;
                    //await FileStorageOperations.SaveToLocalFolderAsync(word.WordSavePath, wordParaphrase);
                }

                // sentense 句子
                //<br>

                var regexMeaning = new Regex(@"(?isx)
                      <(br)\b[^>]*>                 #开始标记“<tag...>”
                          (?>                         #分组构造，用来限定量词“*”修饰范围
                              <\1[^>]*>  (?<Open>)    #命名捕获组，遇到开始标记，入栈，Open计数加1
                          |                           #分支结构
                              <\1>  (?<-Open>)       #狭义平衡组，遇到结束标记，出栈，Open计数减1
                          |                           #分支结构
                              (?:(?!<\1\b).)*       #右侧不为开始或结束标记的任意字符
                          )*                          #以上子串出现0次或任意多次
                          (?(Open)(?!))               #判断是否还有'OPEN'，有则说明不配对，什么都不匹配
                      <\1>                           #结束标记“</tag>”
                     ", RegexOptions.Compiled | RegexOptions.Singleline);
                var sentense = regexMeaning.Match(expression);
                if (sentense.Success)
                {
                    var wordParaphrase = Regex.Replace(sentense.Value, "\\s+|<br>", " ").Trim();
                    wordParaphrase = Regex.Replace(wordParaphrase, "<[^>]+>", "");
                    word.Meaning = wordParaphrase;
                    //await FileStorageOperations.SaveToLocalFolderAsync(wordParaphrase);
                }

                // full sentense
                var regexfullsentense = new Regex(Regex.Escape("&nbsp;&nbsp;e.g.") + ".*", RegexOptions.Singleline);
                var fullsentenseMatch = regexfullsentense.Match(expression);
                if (fullsentenseMatch.Success)
                {
                    var regexMark = new Regex("<[^>]+>");
                    var fullsentense = regexMark.Replace(fullsentenseMatch.Value, "");
                    fullsentense = fullsentense.Replace("&nbsp;&nbsp;e.g.", "e.g.");
                    fullsentense = Regex.Replace(fullsentense, "\\s+", " ");
                    word.Sentence = fullsentense;
                    // await FileStorageOperations.SaveToLocalFolderAsync(fullsentense);
                }
                newWords.Add(word);
            }

            course.NewWords = newWords;


            await MyDataSerializer<Course>.SaveObjectsAsync(course, ViewModel.Id);



            ViewModel.DownloadingItem = "DONE!";

            //status.Text = "DONE!";
        }


        private void Word_OnTap(object sender, GestureEventArgs gestureEventArgs)
        {
            var uiElement = sender as TextBlock;
            var voice = (string)uiElement.Tag;
            var word = uiElement.Text;
            AudioTrack audioTrack =
                new AudioTrack(new Uri(voice, UriKind.Relative),
                                word,
                                word,
                                word,
                                null,
                                null,
                                EnabledPlayerControls.Pause);
            BackgroundAudioPlayer.Instance.Track = audioTrack;
        }

        private void Sentence_OnTap(object sender, GestureEventArgs e)
        {
            var uiElement = sender as TextBlock;
            var voice = (string)uiElement.Tag;
            var sentence = uiElement.Text;
            AudioTrack audioTrack =
                new AudioTrack(new Uri(voice, UriKind.Relative),
                                sentence,
                                sentence,
                                sentence,
                                null,
                                null,
                                EnabledPlayerControls.Pause);
            BackgroundAudioPlayer.Instance.Track = audioTrack;
        }
        Playlist playlist;
        private void WordsList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (WordsList.SelectedItems.Count > 0)
            {
                ApplicationBar = (Microsoft.Phone.Shell.ApplicationBar)Resources["PlayAppBar"];

                // Create playlist
                playlist = new Playlist();

                // Build the playlist
                int count = 0;
                foreach (WordViewModel word in WordsList.SelectedItems)
                {
                    
                    EnabledPlayerControls playerControls =
                            EnabledPlayerControls.Pause |
                            (count != 0 ? EnabledPlayerControls.SkipPrevious : 0) |
                            (count != WordsList.SelectedItems.Count - 1 ? EnabledPlayerControls.SkipNext : 0);

                    PlaylistTrack track = new PlaylistTrack
                    {
                        Source = word.WordVoice,
                        Title = word.Word,
                        Artist = "College English",
                        Album = "College English Book",
                        PlayerControls = playerControls
                    };
                    playlist.Tracks.Add(track);

                    count++;
                }
                

                // Save it to isolated storage
                playlist.Save("ColleageEnglishVocaburaryPlaylist.xml");

                // Set fields of appbar buttons; otherwise they're null
                prevAppBarButton = this.ApplicationBar.Buttons[0] as ApplicationBarIconButton;
                playAppBarButton = this.ApplicationBar.Buttons[1] as ApplicationBarIconButton;
                pauseAppBarButton = this.ApplicationBar.Buttons[2] as ApplicationBarIconButton;
                nextAppBarButton = this.ApplicationBar.Buttons[3] as ApplicationBarIconButton;

                // Set PlayStateChanged handler
                BackgroundAudioPlayer.Instance.PlayStateChanged += OnBackgroundAudioPlayerPlayStateChanged;

                // Set CompositionTarget.Rendering handler
                //CompositionTarget.Rendering += OnCompositionTargetRendering;
            }
            else
            {
                ApplicationBar = (Microsoft.Phone.Shell.ApplicationBar)Resources["DefaultAppBar"];
                WordsList.EnforceIsSelectionEnabled = false;
            }
        }

        private void OnCompositionTargetRendering(object sender, EventArgs e)
        {
            // 
            AudioTrack audioTrack = null;
            TimeSpan position = TimeSpan.Zero;
            TimeSpan duration = TimeSpan.Zero;

            try
            {
                // Sometimes these property accesses will raise exceptions
                audioTrack = BackgroundAudioPlayer.Instance.Track;

                if (audioTrack != null)
                {
                    duration = audioTrack.Duration;
                    position = BackgroundAudioPlayer.Instance.Position;
                }
            }
            catch
            {
            }

            if (audioTrack != null)
            {
               // elapsedDuration.Text = position.ToString("m\\:ss\\.f");
                //totalDuration.Text = duration.ToString("m\\:ss\\.f");

                // Change the Slider position, but set a flag to 
                //  avoid the ValueChanged handler from being confused.
               // settingSliderFromPlay = true;

                //if (duration.Ticks > 0)
                //    positionSlider.Value = (double)position.Ticks / duration.Ticks;
                //else
                //    positionSlider.Value = 0;

                //settingSliderFromPlay = false;
            }
            else
            {
                //elapsedDuration.Text = null;
                //totalDuration.Text = null;
                //positionSlider.Value = 0;
            }
        }

        void UpdateScreen()
        {
            PlayState state = PlayState.Unknown;
            AudioTrack audioTrack = null;

            try
            {
                // Sometimes these property accesses will raise exceptions
                state = BackgroundAudioPlayer.Instance.PlayerState;
                audioTrack = BackgroundAudioPlayer.Instance.Track;
            }
            catch
            {
            }

            //playerState.Text = state != PlayState.Unknown ? state.ToString() : null;

            if (audioTrack != null)
            {
              //  albumText.Text = audioTrack.Album;
             //   artistText.Text = audioTrack.Artist;
             //   trackText.Text = audioTrack.Title;

           //     positionSlider.Visibility = Visibility.Visible;

                prevAppBarButton.IsEnabled = 0 != (audioTrack.PlayerControls & EnabledPlayerControls.SkipPrevious);
                nextAppBarButton.IsEnabled = 0 != (audioTrack.PlayerControls & EnabledPlayerControls.SkipNext);
                playAppBarButton.IsEnabled = BackgroundAudioPlayer.Instance.PlayerState == PlayState.Paused;
                pauseAppBarButton.IsEnabled = BackgroundAudioPlayer.Instance.PlayerState == PlayState.Playing;
            }
            else
            {
            //    albumText.Text = null;
            //    artistText.Text = null;
           //     trackText.Text = null;

           //     positionSlider.Visibility = Visibility.Collapsed;

                prevAppBarButton.IsEnabled = false;
                playAppBarButton.IsEnabled = true;
                pauseAppBarButton.IsEnabled = false;
                nextAppBarButton.IsEnabled = false;
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
            if (playlist != null && playlist.Tracks.Count > 0)
            {
                
            }
            BackgroundAudioPlayer.Instance.Play();
            playAppBarButton.IsEnabled = false;
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
    }
}