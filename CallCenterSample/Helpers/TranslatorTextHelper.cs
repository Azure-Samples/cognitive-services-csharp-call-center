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

using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net;
using Newtonsoft.Json.Linq;

namespace CallCenterSample.Helpers
{
    public class TranslatorTextHelper
    {
        private static HttpClient httpClient { get; set; }

        private static string apiKey;

        public static string ApiKey
        {
            get { return apiKey; }
            set
            {
                var changed = apiKey != value;
                apiKey = value;
                if (changed)
                {
                    InitializeTranslatorTextClient();
                }
            }
        }

        private static void InitializeTranslatorTextClient()
        {
            if (!string.IsNullOrEmpty(ApiKey))
            {
                httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", ApiKey);
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.BaseAddress = new Uri("https://api.cognitive.microsofttranslator.com");
            }
        }

        public static async Task<TranslateTextResult> GetTranslatedTextAsync(string input, string fromLanguage, string toLanguage = "en")
        {
            TranslateTextResult translationResult = new TranslateTextResult() { TranslatedText = string.Empty };

            if ((input != null) && (fromLanguage != null))
            {
                // Request body. 
                string requestString = "[{\"Text\":\"" + input + "\"}]";
                byte[] byteData = Encoding.UTF8.GetBytes(requestString);

                // get translation; omit the "from" param to use language auto-detection
                string uri = string.Format("/translate?api-version=3.0&from={0}&to={1}", fromLanguage, toLanguage);
                var response = await CallEndpoint(httpClient, uri, byteData);
                string content = await response.Content.ReadAsStringAsync();

                // the JSON array returned is enclosed in [], which throws off Parse()
                content = content.TrimStart('[');
                content = content.TrimEnd(']');
                dynamic data = JObject.Parse(content);

                string translation = string.Empty;
                if (data.translations != null)
                {
                    translation = (string) data.translations[0].text;
                }

                if (data.error != null)
                {
                    translation = "<Error: unable to translate input text>";
                }

                translationResult = new TranslateTextResult { TranslatedText = translation };
            }

            return translationResult;
        }

        static async Task<HttpResponseMessage> CallEndpoint(HttpClient client, string uri, byte[] byteData)
        {
            using (var content = new ByteArrayContent(byteData))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                return await client.PostAsync(uri, content);
            }
        }
    }

    /// Class to hold result of Translate call
    /// </summary>
    public class TranslateTextResult
    {
        public string TranslatedText { get; set; }
    }
}
