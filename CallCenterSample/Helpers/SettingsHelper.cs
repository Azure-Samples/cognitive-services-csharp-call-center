// 
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.
// 
// Microsoft Cognitive Services: http://www.microsoft.com/cognitive
// 
// Microsoft Cognitive Services Github:
// https://github.com/Microsoft/Cognitive
// 
// Copyright (c) Microsoft Corporation
// All rights reserved.
// 
// MIT License:
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// 

using System;
using System.ComponentModel;
using System.IO;
using Windows.Storage;

namespace CallCenterSample.Helpers
{
    internal class SettingsHelper : INotifyPropertyChanged
    {
        public event EventHandler SettingsChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        private static SettingsHelper instance;

        static SettingsHelper()
        {
            instance = new SettingsHelper();
        }

        public void Initialize()
        {
            LoadRoamingSettings();
            Windows.Storage.ApplicationData.Current.DataChanged += RoamingDataChanged;
        }

        private void RoamingDataChanged(ApplicationData sender, object args)
        {
            LoadRoamingSettings();
            instance.OnSettingsChanged();
        }

        private void OnSettingsChanged()
        {
            if (instance.SettingsChanged != null)
            {
                instance.SettingsChanged(instance, EventArgs.Empty);
            }
        }

        private void OnSettingChanged(string propertyName, object value)
        {
            ApplicationData.Current.RoamingSettings.Values[propertyName] = value;

            instance.OnSettingsChanged();
            instance.OnPropertyChanged(propertyName);
        }

        private void OnPropertyChanged(string propertyName)
        {
            if (instance.PropertyChanged != null)
            {
                instance.PropertyChanged(instance, new PropertyChangedEventArgs(propertyName));
            }
        }

        public static SettingsHelper Instance
        {
            get
            {
                return instance;
            }
        }

        private void LoadRoamingSettings()
        {
            object value = ApplicationData.Current.RoamingSettings.Values["TextAnalyticsApiKey"];
            if (value != null)
            {
                this.TextAnalyticsApiKey = value.ToString();
            }

            value = ApplicationData.Current.RoamingSettings.Values["TextAnalyticsApiKeyRegion"];
            if (value != null)
            {
                this.TextAnalyticsApiKeyRegion = value.ToString();
            }

            value = ApplicationData.Current.RoamingSettings.Values["TranslatorTextApiKey"];
            if (value != null)
            {
                this.TranslatorTextApiKey = value.ToString();
            }

            value = ApplicationData.Current.RoamingSettings.Values["ShowDebugInfo"];
            if (value != null)
            {
                bool booleanValue;
                if (bool.TryParse(value.ToString(), out booleanValue))
                {
                    this.ShowDebugInfo = booleanValue;
                }
            }
        }

        public void RestoreAllSettings()
        {
            ApplicationData.Current.RoamingSettings.Values.Clear();
        }

        private string textAnalyticsApiKey = string.Empty;
        public string TextAnalyticsApiKey
        {
            get { return textAnalyticsApiKey; }
            set
            {
                this.textAnalyticsApiKey = value;
                this.OnSettingChanged("TextAnalyticsApiKey", value);
            }
        }

        private string textAnalyticsApiKeyRegion = "westus";
        public string TextAnalyticsApiKeyRegion
        {
            get { return textAnalyticsApiKeyRegion; }
            set
            {
                this.textAnalyticsApiKeyRegion = value;
                this.OnSettingChanged("TextAnalyticsApiKeyRegion", value);
            }
        }

        private string translatorTextApiKey = string.Empty;
        public string TranslatorTextApiKey
        {
            get { return this.translatorTextApiKey; }
            set
            {
                this.translatorTextApiKey = value;
                this.OnSettingChanged("TranslatorTextApiKey", value);
            }
        }

        private bool showDebugInfo = false;
        public bool ShowDebugInfo
        {
            get { return showDebugInfo; }
            set
            {
                this.showDebugInfo = value;
                this.OnSettingChanged("ShowDebugInfo", value);
            }
        }

        public string[] AvailableApiRegions
        {
            get
            {
                return new string[]
                {
                    "westus",
                    "westus2",
                    "eastus",
                    "eastus2",
                    "westcentralus",
                    "southcentralus",
                    "westeurope",
                    "northeurope",
                    "southeastasia",
                    "eastasia",
                    "australiaeast",
                    "brazilsouth"
                };
            }
        }
    }
}