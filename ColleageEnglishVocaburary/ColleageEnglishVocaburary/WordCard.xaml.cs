using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace ColleageEnglishVocaburary
{
    public partial class WordCard : PhoneApplicationPage
    {
        bool _isForeground = true;

        public WordCard()
        {
            InitializeComponent();
            //将反面翻转180°
            //this.myStoryboardX5.Begin();
            this.myStoryboardX1.Completed += new EventHandler(Completed_StoryBoard1);
            this.myStoryboardX3.Completed += new EventHandler(Completed_StoryBoard3);
        }

        private void OnTap_Click(object sender, GestureEventArgs e)
        {
            if (_isForeground)
            {
                this.myStoryboardX1.Begin();
                this._isForeground = false;

            }
            else
            {
                this.myStoryboardX3.Begin();
                this._isForeground = true;

            }
        }

        private void Completed_StoryBoard1(Object sender, EventArgs e)
        {
            this.WordStackPanel.Visibility = Visibility.Collapsed;
            this.WordPhraseStackPanel.Visibility = Visibility.Visible;
            this.myStoryboardX2.Begin();
        }

        private void Completed_StoryBoard3(Object sender, EventArgs e)
        {
            this.WordPhraseStackPanel.Visibility = Visibility.Collapsed;
            this.WordStackPanel.Visibility = Visibility.Visible;
            this.myStoryboardX4.Begin();
        }
    }
}