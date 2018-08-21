using CallCenterSample.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.SpeechRecognition;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace CallCenterSample.Controls
{
    public class SpeechRecognitionAndSentimentResult
    {
        public string DetectedLanguage { get; set; }
        public string SpeechRecognitionText { get; set; }
        public double TextAnalysisSentiment { get; set; }
        public string TranslatedText { get; set; }
    }

    public sealed partial class SpeechToTextControl : UserControl
    {
        public event EventHandler<SpeechRecognitionAndSentimentResult> SpeechRecognitionAndSentimentProcessed;
        public event EventHandler<Boolean> SpeechRecognitionIsReset;

        private SpeechRecognizer speechRecognizer;
        private bool isCapturingSpeech;
        private static uint HResultPrivacyStatementDeclined = 0x80045509;
        private string[] detectedLanguage;
        private string translatedText;

        // Keep track of existing text that we've accepted in ContinuousRecognitionSession_ResultGenerated(), so
        // that we can combine it and Hypothesized results to show in-progress dictation mid-sentence.
        private StringBuilder dictatedTextBuilder;

        public SpeechToTextControl()
        {
            this.InitializeComponent();
        }

        #region Speech Recognizer and Text Analytics

        public async Task InitializeSpeechRecognizerAsync()
        {
            if (this.speechRecognizer != null)
            {
                this.DisposeSpeechRecognizer();
            }

            this.dictatedTextBuilder = new StringBuilder();
            this.speechRecognizer = new SpeechRecognizer();

            var dictationConstraint = new SpeechRecognitionTopicConstraint(SpeechRecognitionScenario.Dictation, "dictation");
            speechRecognizer.Constraints.Add(dictationConstraint);
            SpeechRecognitionCompilationResult result = await speechRecognizer.CompileConstraintsAsync();

            if (result.Status != SpeechRecognitionResultStatus.Success)
            {
                await new MessageDialog("CompileConstraintsAsync returned " + result.Status, "Error initializing SpeechRecognizer").ShowAsync();
                return;
            }

            this.speechRecognizer.ContinuousRecognitionSession.ResultGenerated += ContinuousRecognitionSession_ResultGenerated; ;
            this.speechRecognizer.ContinuousRecognitionSession.Completed += ContinuousRecognitionSession_Completed;
            this.speechRecognizer.HypothesisGenerated += SpeechRecognizer_HypothesisGenerated;
        }

        private async void ContinuousRecognitionSession_ResultGenerated(SpeechContinuousRecognitionSession sender, SpeechContinuousRecognitionResultGeneratedEventArgs args)
        {
            if (args.Result.Confidence == SpeechRecognitionConfidence.Medium ||
                args.Result.Confidence == SpeechRecognitionConfidence.High)
            {
                dictatedTextBuilder.Append(args.Result.Text + " ");

                await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    this.speechRecognitionTextBox.Text = dictatedTextBuilder.ToString();
                });
            }
        }

        private async void ContinuousRecognitionSession_Completed(SpeechContinuousRecognitionSession sender, SpeechContinuousRecognitionCompletedEventArgs args)
        {
            if (args.Status != SpeechRecognitionResultStatus.Success)
            {
                // If TimeoutExceeded occurs, the user has been silent for too long. We can use this to 
                // cancel recognition if the user in dictation mode and walks away from their device, etc.
                // In a global-command type scenario, this timeout won't apply automatically.
                // With dictation (no grammar in place) modes, the default timeout is 20 seconds.
                if (args.Status == SpeechRecognitionResultStatus.TimeoutExceeded)
                {
                    await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        this.speechRecognitionControlButtonSymbol.Symbol = Symbol.Refresh;
                        this.speechRecognitionTextBox.PlaceholderText = "";
                        this.speechRecognitionTextBox.Text = dictatedTextBuilder.ToString();
                        this.isCapturingSpeech = false;
                    });
                }
                else
                {
                    await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        this.speechRecognitionControlButtonSymbol.Symbol = Symbol.Refresh;
                        this.speechRecognitionTextBox.PlaceholderText = "";
                        this.isCapturingSpeech = false;
                    });
                }
            }
        }

        public void DisposeSpeechRecognizer()
        {
            if (this.speechRecognizer != null)
            {
                try
                {
                    this.speechRecognizer.ContinuousRecognitionSession.ResultGenerated -= ContinuousRecognitionSession_ResultGenerated;
                    this.speechRecognizer.ContinuousRecognitionSession.Completed -= ContinuousRecognitionSession_Completed;
                    this.speechRecognizer.HypothesisGenerated -= SpeechRecognizer_HypothesisGenerated;
                    this.speechRecognizer.Dispose();
                    this.speechRecognizer = null;
                }
                catch (Exception) { }
            }
        }

        private async void SpeechRecognizer_HypothesisGenerated(SpeechRecognizer sender, SpeechRecognitionHypothesisGeneratedEventArgs args)
        {
            string hypothesis = args.Hypothesis.Text;

            // Update the textbox with the currently confirmed text, and the hypothesis combined.
            string textboxContent = dictatedTextBuilder.ToString() + " " + hypothesis + " ...";

            await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                this.speechRecognitionTextBox.Text = textboxContent;
            });
        }

        private async void MicrophoneButtonClick(object sender, object e)
        {
            try
            {
                this.speechRecognitionControlButton.Focus(FocusState.Programmatic);
                await StartSpeechRecognition();
            }
            catch (Exception ex)
            {
                if ((uint)ex.HResult == HResultPrivacyStatementDeclined)
                {
                    await Util.ConfirmActionAndExecute(
                        "The Speech Privacy settings need to be enabled. Under 'Settings->Privacy->Speech, inking and typing', ensure you have viewed the privacy policy, and 'Get To Know You' is enabled. Want to open the settings now?",
                        async () =>
                        {
                            // Open the privacy/speech, inking, and typing settings page.
                            await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-settings:privacy-speechtyping"));
                        });
                }
                else
                {
                    await Util.GenericApiCallExceptionHandler(ex, "Error starting SpeechRecognizer.");
                }
            }
        }

        private async Task StartSpeechRecognition()
        {
            this.isCapturingSpeech = true;
            this.speechRecognitionControlButtonSymbol.Symbol = Symbol.Stop;

            if (this.speechRecognizer == null)
            {
                await this.InitializeSpeechRecognizerAsync();

            }

            this.speechRecognitionTextBox.Text = "";
            this.speechRecognitionTextBox.PlaceholderText = "Listening...";
            this.dictatedTextBuilder.Clear();
            this.sentimentControl.Sentiment = 0.5;

            await this.speechRecognizer.ContinuousRecognitionSession.StartAsync();
        }

        private async void SpeechRecognitionButtonClick(object sender, RoutedEventArgs e)
        {
            if (this.isCapturingSpeech)
            {
                this.isCapturingSpeech = false;
                this.OnSpeechRecognitionReset(false);
                this.speechRecognitionControlButtonSymbol.Symbol = Symbol.Refresh;
                this.speechRecognitionTextBox.PlaceholderText = "";

                if (speechRecognizer.State != SpeechRecognizerState.Idle)
                {
                    // Cancelling recognition prevents any currently recognized speech from
                    // generating a ResultGenerated event. StopAsync() will allow the final session to 
                    // complete.
                    try
                    {
                        await speechRecognizer.ContinuousRecognitionSession.StopAsync();
                        string dictatedTextAfterStop = dictatedTextBuilder.ToString();

                        // Ensure we don't leave any hypothesis text behind
                        if (!string.IsNullOrEmpty(dictatedTextAfterStop))
                        {
                            this.speechRecognitionTextBox.Text = dictatedTextAfterStop;
                        }
                        else if (!string.IsNullOrEmpty(this.speechRecognitionTextBox.Text) && this.speechRecognitionTextBox.Text.EndsWith(" ..."))
                        {
                            this.speechRecognitionTextBox.Text = this.speechRecognitionTextBox.Text.Replace(" ...", ".");
                        }
                    }
                    catch (Exception exception)
                    {
                        await Util.GenericApiCallExceptionHandler(exception, "Error stopping SpeechRecognizer.");
                    }
                }

                this.translatedText = this.speechRecognitionTextBox.Text;  // we don't translate English input, so set "translated text" to the input to cover that scenario

                await this.DetectLanguageAsync();
                if (this.detectedLanguage[0] != "en")
                {
                    await this.TranslateTextAsync();
                }
                await this.AnalyzeTextAsync();
            }
            else
            {
                this.OnSpeechRecognitionReset(true);    // this path means that the speechRecognitionControlButtonSymbol button was showing Symbol.Refresh when clicked
                await this.StartSpeechRecognition();
            }
        }

        private async Task DetectLanguageAsync()
        {
            try
            {
                if (!string.IsNullOrEmpty(this.speechRecognitionTextBox.Text))
                {
                    DetectLanguageResult textDetectResult = TextAnalyticsHelper.GetDetectedLanguageAsync(this.speechRecognitionTextBox.Text);
                    this.detectedLanguage = new string[] { textDetectResult.Language["iso6391Name"], textDetectResult.Language["name"] };
                }
                else
                {
                    this.detectedLanguage = new string[] { "en", string.Empty };
                }
            }
            catch (Exception ex)
            {
                await Util.GenericApiCallExceptionHandler(ex, "Error during Text Analytics 'Detect' call.");
            }
        }

        private async Task TranslateTextAsync()
        {
            try
            {
                if (!string.IsNullOrEmpty(this.speechRecognitionTextBox.Text))
                {
                    TranslateTextResult textTranslateResult = await TranslatorTextHelper.GetTranslatedTextAsync(this.speechRecognitionTextBox.Text, this.detectedLanguage[0]);
                    this.translatedText = textTranslateResult.TranslatedText;
                }
                else
                {
                    this.translatedText = string.Empty;
                }
            }
            catch (Exception ex)
            {
                await Util.GenericApiCallExceptionHandler(ex, "Error during Translation Text call.");
            }
        }

        private async Task AnalyzeTextAsync()
        {
            try
            {
                if (!string.IsNullOrEmpty(this.speechRecognitionTextBox.Text))
                {
                    SentimentResult textAnalysisResult = TextAnalyticsHelper.GetTextSentimentAsync(this.translatedText, this.detectedLanguage[0]);
                    this.sentimentControl.Sentiment = textAnalysisResult.Score;
                }
                else
                {
                    this.sentimentControl.Sentiment = 0.5;
                }

                this.OnSpeechRecognitionAndSentimentProcessed(new SpeechRecognitionAndSentimentResult { SpeechRecognitionText = this.speechRecognitionTextBox.Text, TextAnalysisSentiment = this.sentimentControl.Sentiment, DetectedLanguage = this.detectedLanguage[1], TranslatedText = this.translatedText });
            }
            catch (Exception ex)
            {
                await Util.GenericApiCallExceptionHandler(ex, "Error during Text Analytics 'Sentiment' call.");
            }
        }

        private void OnSpeechRecognitionReset(Boolean result)
        {
            this.SpeechRecognitionIsReset(this, result);
        }

        private void OnSpeechRecognitionAndSentimentProcessed(SpeechRecognitionAndSentimentResult result)
        {
            if (this.SpeechRecognitionAndSentimentProcessed != null)
            {
                this.SpeechRecognitionAndSentimentProcessed(this, result);
            }
        }

        #endregion

    }
}
