using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ColleageEnglishVocaburary.ViewModels
{
    public class LearningTypeViewModel: INotifyPropertyChanged
    {
        public string Type
        {
            get { return _type; }
            set
            {
                this.SetProperty(ref this._type, value);
            }
        }

        public bool AutoReading
        {
            get { return _autoReading; }
            set
            {
                this.SetProperty(ref this._autoReading, value);
            }
        }

        public v

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

        private string _type;
        private bool _autoReading;
    }
}
