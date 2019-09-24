---
page_type: sample
languages:
- csharp
products:
- azure
description: "Mock-up of a call center app. Take a customer's request, convert it to text, translate it, gauge the emotion behind the text, and parse key phrases."
urlFragment: call-center-first-contact-sample
---

# Call Center First Contact

This sample is a basic mock-up for a call center app that takes a customer's spoken request, converts it to text, translates it to English (if necessary), gauges the emotion behind the text, and then parses it for key phrases. This data could be used to assist in routing the call to a:

* Speaker of the customer's preferred language
* Senior Customer Service Representative to work with an especially upset caller
* Subject matter expert for the subject of the call

## Features

This project framework provides the following features:

* Text translation: Translation of text is provided by Translator Text via JSON returned from a request sent to the [Translate](https://docs.microsoft.com/azure/cognitive-services/translator/reference/v3-0-translate) method
* Language detection: Language detection of text is provided by Text Analytics via the [Microsoft.Azure.CognitiveServices.Language.TextAnalytics](https://www.nuget.org/packages/Microsoft.Azure.CognitiveServices.Language.TextAnalytics/) NuGet package
* Emotion detection: Emotion underlying text is provided by Text Analytics via the [Microsoft.Azure.CognitiveServices.Language.TextAnalytics](https://www.nuget.org/packages/Microsoft.Azure.CognitiveServices.Language.TextAnalytics/) NuGet package
* Key phrases: Parsing text for key phrases is provided by Text Analytics via the [Microsoft.Azure.CognitiveServices.Language.TextAnalytics](https://www.nuget.org/packages/Microsoft.Azure.CognitiveServices.Language.TextAnalytics/) NuGet package
* Speech-to-text: In the context of the scenario of this sample, where customers are telephoning a call center, the "speech to text" functionality could be handled via any number of methods. For the sake of convenience and simplicity, we are using the built-in voice input features of Windows 10 as a placeholder for those methods.

## Getting Started

### Prerequisites

You'll need [Visual Studio](https://www.visualstudio.com/downloads/) to run the sample code and an [Azure account](https://azure.microsoft.com/free/) with subscriptions to both the Translator Text and Text Analytics services.

Due to the use of Windows 10's voice input features, you'll also need to make a few changes to your Windows installation in order to test the sample:

* To test the translation feature, you'll need to install one or more non-English language packs. You can do so by selecting **Settings** > **Time & Language** > **Region & language**, and then **Add a language**.
* To select the language you'll use when speaking, select **Settings** > **Time & Language** > **Speech**, and then the desired language under the **Speech language** setting.
* To allow voice input, select **Settings** > **Privacy** > **Speech, inking, & typing**, and then **Turn on speech services and typing suggestions**. Select **Turn on** in the confirmation dialog box.

### Quickstart

1. From a shell or command line:
    1. `git clone https://github.com/Azure-Samples/cognitive-services-dotnet-call-center.git`
    1. `cd cognitive-services-dotnet-call-center`
1. Double-click `CallCenterSample.sln` to open the solution in Visual Studio
1. Ensure that the platform the solution is building for is set to `x64`

## Demo

To run the sample, follow these steps:

1. Run the project from Visual Studio
2. On the main page, click **Settings** so that you can enter your API keys and region info, and then click **Close**
3. On the main page, click the microphone button so that the app begins listening for speech; when you're done speaking, click the stop button so that the text representation of your speech can be tested for source language (and translated to English if necessary) and emotion, and parsed for key terms and phrases
4. Click the reset button to begin the process again

## Resources

* [Translator Text API](https://docs.microsoft.com/azure/cognitive-services/translator/)
* [Text Analytics API](https://docs.microsoft.com/azure/cognitive-services/text-analytics/)
