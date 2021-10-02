using FIAP.MachineLearning.QnAMaker.Bots;
using FIAP.MachineLearning.QnAMaker.Translator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.QnA;
using Microsoft.Bot.Builder.BotFramework;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FIAP.MachineLearning.QnAMaker
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpClient().AddControllers().AddNewtonsoftJson();

            services.AddHttpClient();

            services.AddSingleton<ICredentialProvider, ConfigurationCredentialProvider>();
            services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();
            services.AddSingleton(new QnAMakerEndpoint
            {
                KnowledgeBaseId = _configuration.GetValue<string>("QnAKnowledgebaseId"),
                EndpointKey = _configuration.GetValue<string>("QnAEndpointKey"),
                Host = _configuration.GetValue<string>("QnAEndpointHostName")
            });
            services.AddSingleton(new TranslatorConfiguration
            {
                SubscriptionKey = _configuration.GetValue<string>("TranslatorSubscriptionKey"),
                Location = _configuration.GetValue<string>("TranslatorLocation"),
                EndpointHostName = _configuration.GetValue<string>("TranslatorEndpointHostName")
            });

            services.AddTransient<IBot, QnABot>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseDefaultFiles()
                .UseStaticFiles()
                .UseRouting()
                .UseAuthorization()
                .UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                });
        }
    }
}