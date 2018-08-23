using CallCenterSample.Controls;
using CallCenterSample.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace CallCenterSample
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>

    public sealed partial class MainPage : Page
    {
        public ObservableCollection<string> KeyPhrases { get; set; } = new ObservableCollection<string>();

        public MainPage()
        {
            this.InitializeComponent();

            this.STTControls.SpeechRecognitionAndSentimentProcessed += OnSpeechRecognitionAndSentimentProcessed;
            this.STTControls.SpeechRecognitionIsReset += OnSpeechRecognitionReset;
        }

        private void OnSpeechRecognitionAndSentimentProcessed(object sender, SpeechRecognitionAndSentimentResult e)
        {
            this.DetectedLanguageBox.Text = e.DetectedLanguage;
            this.CallerTextBox.Text = e.TranslatedText;
        }

        private void OnSpeechRecognitionReset(object sender, Boolean e)
        {
            if (e == true)
            {
                this.KeyPhrases.Clear();
                this.DetectedLanguageBox.Text = string.Empty;
                this.CallerTextBox.Text = string.Empty;
            }
        }

        private void SettingsClicked(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(SettingsPage));
        }

        private async void CallerTextBoxTextChanged(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(this.CallerTextBox.Text))
            {
                await GetKeyPhrasesAsync();
            }
        }

        private async Task GetKeyPhrasesAsync()
        {
            try
            {
                KeyPhrasesResult keyPhrasesResult = await TextAnalyticsHelper.GetKeyPhrasesAsync(this.CallerTextBox.Text);
                this.KeyPhrases.AddRange(keyPhrasesResult.KeyPhrases.OrderBy(i => i));
            }
            catch (Exception ex)
            {
                await Util.GenericApiCallExceptionHandler(ex, "Error during Text Analytics 'Key Phrases' call.");
            }
        }
    }
}
