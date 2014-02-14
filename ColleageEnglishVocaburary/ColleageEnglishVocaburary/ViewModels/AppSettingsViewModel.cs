
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO.IsolatedStorage;
using System.Runtime.CompilerServices;

namespace ColleageEnglishVocaburary.ViewModels
{
    public class AppSettingsViewModel: INotifyPropertyChanged
    {

        // Our isolated storage settings
        IsolatedStorageSettings settings;

        // The isolated storage key names of our settings
        const string LearningTypeSettingKeyName = "LearningTypeSetting";
        const string AutoReadingSettingKeyName = "AutoReadingSetting";
        

        // The default value of our settings
        const string LearningTypeSettingDefault = "¿¨Æ¬Ê½";
        const bool AutoReadingSettingDefault = true;

        /// <summary>
        /// Constructor that gets the application settings.
        /// </summary>
        public AppSettingsViewModel()
        {
            try
            {
                // Get the settings for this application.
                settings = IsolatedStorageSettings.ApplicationSettings;
                LearningTypeSetting = GetValueOrDefault<string>(LearningTypeSettingKeyName, LearningTypeSettingDefault);
                AutoReadingSetting = GetValueOrDefault<bool>(AutoReadingSettingKeyName, AutoReadingSettingDefault);

            }
            catch (Exception e)
            {
                Debug.WriteLine("Exception while using IsolatedStorageSettings: " + e.ToString());
            }
        }

        /// <summary>
        /// Update a setting value for our application. If the setting does not
        /// exist, then add the setting.
        /// </summary>
        /// <param name="Key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool AddOrUpdateValue(string Key, Object value)
        {
            bool valueChanged = false;

            // If the key exists
            if (settings.Contains(Key))
            {
                // If the value has changed
                if (settings[Key] != value)
                {
                    // Store the new value
                    settings[Key] = value;
                    valueChanged = true;
                }
            }
            // Otherwise create the key.
            else
            {
                settings.Add(Key, value);
                valueChanged = true;
            }

            return valueChanged;
        }


        /// <summary>
        /// Get the current value of the setting, or if it is not found, set the 
        /// setting to the default setting.
        /// </summary>
        /// <typeparam name="valueType"></typeparam>
        /// <param name="Key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public valueType GetValueOrDefault<valueType>(string Key, valueType defaultValue)
        {
            valueType value;

            // If the key exists, retrieve the value.
            if (settings.Contains(Key))
            {
                value = (valueType)settings[Key];
            }
            // Otherwise, use the default value.
            else
            {
                value = defaultValue;
            }

            return value;
        }


        /// <summary>
        /// Save the settings.
        /// </summary>
        public void Save()
        {
            AddOrUpdateValue(LearningTypeSettingKeyName, LearningTypeSetting);
            AddOrUpdateValue(AutoReadingSettingKeyName, AutoReadingSetting);
            settings.Save();
        }


        private string _learningTypeSetting;
        public string LearningTypeSetting
        {
            get { return _learningTypeSetting; }
            set { this.SetProperty(ref this._learningTypeSetting, value); }
        }

        /// <summary>
        /// Property to get and set a CheckBox Setting Key.
        /// </summary>
        //public string LearningTypeSetting
        //{
        //    get
        //    {
        //        return GetValueOrDefault<string>(LearningTypeSettingKeyName, LearningTypeSettingDefault);
        //    }
        //    set
        //    {
        //        if (AddOrUpdateValue(LearningTypeSettingKeyName, value))
        //        {
        //            Save();
        //        }
        //    }
        //}


        private bool _autoReadingSetting;
        public bool AutoReadingSetting
        {
            get { return _autoReadingSetting; }
            set { this.SetProperty(ref this._autoReadingSetting, value); }
        }

        ///// <summary>
        ///// 
        ///// </summary>
        //public bool AutoReadingSetting
        //{
        //    get
        //    {
        //        return GetValueOrDefault<bool>(AutoReadingSettingKeyName, AutoReadingSettingDefault);
        //    }
        //    set
        //    {
        //        if (AddOrUpdateValue(AutoReadingSettingKeyName, value))
        //        {
        //            Save();
        //        }
        //    }
        //}

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
    }
}
