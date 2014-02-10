/* 
    Copyright (c) 2012 - 2013 Microsoft Corporation.  All rights reserved.
    Use of this sample source code is subject to the terms of the Microsoft license 
    agreement under which you licensed this sample source code and is provided AS-IS.
    If you did not accept the terms of the license agreement, you are not authorized 
    to use this sample source code.  For the terms of the license, please see the 
    license agreement between you and Microsoft.
  
    To see all Code Samples for Windows Phone, visit http://code.msdn.microsoft.com/wpapps
  
*/

using System;
using System.Diagnostics;
using System.IO.IsolatedStorage;

namespace ColleageEnglishVocaburary
{
    public class AppSettings
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
        public AppSettings()
        {
            try
            {
                // Get the settings for this application.
                settings = IsolatedStorageSettings.ApplicationSettings;

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
            settings.Save();
        }


        /// <summary>
        /// Property to get and set a CheckBox Setting Key.
        /// </summary>
        public string LearningTypeSetting
        {
            get
            {
                return GetValueOrDefault<string>(LearningTypeSettingKeyName, LearningTypeSettingDefault);
            }
            set
            {
                if (AddOrUpdateValue(LearningTypeSettingKeyName, value))
                {
                    Save();
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        public bool AutoReadingSetting
        {
            get
            {
                return GetValueOrDefault<bool>(AutoReadingSettingKeyName, AutoReadingSettingDefault);
            }
            set
            {
                if (AddOrUpdateValue(AutoReadingSettingKeyName, value))
                {
                    Save();
                }
            }
        }

    }
}
