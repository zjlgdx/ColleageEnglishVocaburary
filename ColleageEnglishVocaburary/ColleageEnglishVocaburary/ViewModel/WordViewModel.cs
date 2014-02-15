using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Cimbalino.Phone.Toolkit.Services;
using ColleageEnglishVocaburary.Model;
using GalaSoft.MvvmLight.Command;
using Microsoft.Phone.BackgroundAudio;

namespace ColleageEnglishVocaburary.ViewModel
{
    public class WordViewModel : INotifyPropertyChanged
    {


        public WordViewModel()
        {
            ReadWordCommand = new RelayCommand<string>(this.ReadVoice);
        }

        

        //private string _courseName;
        //public string CourseName
        //{
        //    get { return _courseName; }
        //    set { this.SetProperty(ref this._courseName, value); }
        //}

        public ICommand ReadWordCommand { get; private set; }

        private string _wordId;
        public string WordId
        {
            get { return _wordId; }
            set { this.SetProperty(ref this._wordId, value); }
        }

        private string _word;
        public string Word
        {
            get { return _word; }
            set { this.SetProperty(ref this._word, value); }
        }

        private string _wordVoice;
        public string WordVoice
        {
            get { return _wordVoice; }
            set { this.SetProperty(ref this._wordVoice, value); }
        }

        private string _wordPhrase;
        public string WordPhrase
        {
            get { return _wordPhrase; }
            set { this.SetProperty(ref this._wordPhrase, value); }
        }

        private string _sentence;
        public string Sentence
        {
            get { return _sentence; }
            set { this.SetProperty(ref this._sentence, value); }
        }
        private string _entenceVoice;
        public string SentenceVoice
        {
            get { return _entenceVoice; }
            set { this.SetProperty(ref this._entenceVoice, value); }
        }

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

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var eventHandler = this.PropertyChanged;
            if (eventHandler != null)
            {
                eventHandler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void ReadVoice(string voicePath)
        {
            var voice = voicePath;//_isForeground ? ViewModel.LearningWord.WordVoice : ViewModel.LearningWord.SentenceVoice;
            var text = voicePath;// todo // _isForeground ? ViewModel.LearningWord.Word : ViewModel.LearningWord.Sentence;
            if (string.IsNullOrWhiteSpace(voice))
            {
                return;
            }
            var audioTrack =
                new AudioTrack(new Uri(voice, UriKind.Relative),
                                text,
                                text,
                                text,
                                null,
                                null,
                                EnabledPlayerControls.Pause);
            audioTrack.BeginEdit();
            audioTrack.Tag = "S";
            audioTrack.EndEdit();
            BackgroundAudioPlayer.Instance.Stop();
            BackgroundAudioPlayer.Instance.Track = audioTrack;
            BackgroundAudioPlayer.Instance.Play();
        }
    }
}
