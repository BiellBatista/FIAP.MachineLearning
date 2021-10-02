using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace FIAP.MachineLearning.QnAMaker.Translator
{
    public class TranslatorResponse
    {
        [JsonPropertyName("detectedLanguage")]
        public DetectedLanguageResponse DetectedLanguage { get; set; }

        [JsonPropertyName("translations")]
        public IEnumerable<TranslationResponse> Translations { get; set; }
    }

    public class DetectedLanguageResponse
    {
        [JsonPropertyName("language")]
        public string Language { get; set; }

        [JsonPropertyName("score")]
        public double? Score { get; set; }
    }

    public class TranslationResponse
    {
        [JsonPropertyName("text")]
        public string Text { get; set; }

        [JsonPropertyName("to")]
        public string To { get; set; }
    }
}