using FIAP.MachineLearning.QnAMaker.Shared;
using FIAP.MachineLearning.QnAMaker.Translator;
using Flurl.Http;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.QnA;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Polly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace FIAP.MachineLearning.QnAMaker.Bots
{
    public class QnABot : ActivityHandler
    {
        private const int TIMEOUT_RETRY = 2;

        private readonly ILogger<QnABot> _logger;

        private readonly List<HttpStatusCode> _httpStatusCodesWorthRetrying;
        private readonly Microsoft.Bot.Builder.AI.QnA.QnAMaker _qnaMaker;
        private readonly TranslatorConfiguration _translatorConfiguration;

        private string _language;

        public QnABot(ILogger<QnABot> logger, QnAMakerEndpoint endpoint, TranslatorConfiguration translatorConfiguration)
        {
            _logger = logger;

            _httpStatusCodesWorthRetrying = new List<HttpStatusCode>
            {
                HttpStatusCode.RequestTimeout,
                HttpStatusCode.BadGateway,
                HttpStatusCode.ServiceUnavailable,
                HttpStatusCode.GatewayTimeout
            };

            _qnaMaker = new Microsoft.Bot.Builder.AI.QnA.QnAMaker(endpoint);
            _translatorConfiguration = translatorConfiguration;
            _language = "pt-br";
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Chamando QnA Maker");

            var question = turnContext.Activity.Text;
            var translatedQuestion = await AccessTranslatorAsync(question);

            turnContext.Activity.Text = translatedQuestion;

            var response = await AccessQnAMakerAsync(turnContext, cancellationToken);
            var translatedResponse = await AccessTranslatorAsync(response);

            await turnContext.SendActivityAsync(MessageFactory.Text(translatedResponse), cancellationToken);
        }

        private async Task<string> AccessTranslatorAsync(string text)
        {
            object[] body = new object[] { new { Text = text } };
            var requestBody = JsonConvert.SerializeObject(body);
            var url = $"{ _translatorConfiguration.EndpointHostName}&to={_language}";

            using (var httpResponse = await Policy
                .Handle<FlurlHttpException>(x => x.Call?.Response
                                                is not null && _httpStatusCodesWorthRetrying.Contains((HttpStatusCode)x.Call.Response.StatusCode))
                .WaitAndRetryAsync(new[]
                {
                    TimeSpan.FromSeconds(TIMEOUT_RETRY),
                    TimeSpan.FromSeconds(TIMEOUT_RETRY)
                })
                .ExecuteAsync(() =>
                   url
                   .WithHeaders(new
                   {
                       Content_type = "application/json; charset=UTF-8",
                       Ocp_Apim_Subscription_Key = _translatorConfiguration.SubscriptionKey,
                       Ocp_Apim_Subscription_Region = _translatorConfiguration.Location
                   })
                    .PostStringAsync(requestBody)
                ))
            {
                var response = await httpResponse.ResponseMessage.Content.ReadAsStreamAsync();
                var result = StreamSerializer.Deserialize<List<TranslatorResponse>>(response);

                _language = result.FirstOrDefault()?.DetectedLanguage.Language;

                return result.FirstOrDefault()?.Translations.FirstOrDefault()?.Text;
            }
        }

        private async Task<string> AccessQnAMakerAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var options = new QnAMakerOptions { Top = 1 };
            var results = await _qnaMaker.GetAnswersAsync(turnContext, options);

            if (results.Any()) return results.First().Answer;
            else return "Desculpe, eu não sei a resposta.";
        }
    }
}