using Microsoft.Phone.Controls;
using System;
using System.Windows;

namespace ColleageEnglishVocaburary
{
    public partial class WordCard : PhoneApplicationPage
    {
        bool _isForeground = true;

        public WordCard()
        {
            InitializeComponent();
            this.myStoryboardX1.Completed += new EventHandler(Completed_StoryBoard1);
            this.myStoryboardX3.Completed += new EventHandler(Completed_StoryBoard3);
        }

        private void OnTap_Click(object sender, GestureEventArgs e)
        {
            if (_isForeground)
            {
                this.myStoryboardX1.Begin();
            }
            else
            {
                this.myStoryboardX3.Begin();
            }

            this._isForeground = !this._isForeground;
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