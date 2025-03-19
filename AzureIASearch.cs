using Azure;
using Azure.AI.OpenAI;
using Azure.AI.OpenAI.Chat;
using IacontactoApi.Application.Common.Interfaces;
using IacontactoApi.Domain.Exceptions;
using Microsoft.Extensions.Configuration;
using OpenAI.Chat;
using Polly;
using Polly.Retry;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IacontactoApi.Infrastructure.Services.IaSearch
{
    public class AzureIASearch : IAzureIASearch
    {
        private readonly string _azureOpenAIEndpoint;
        private readonly string _azureOpenAIKey;
        private readonly string _deploymentName;

        private readonly string _azureAISearchIndex;
        private readonly string _azureAISearchEndpoint;
        private readonly string _azureAISearchKey;

        private readonly AsyncRetryPolicy _retryPolicy;

        public AzureIASearch(IConfiguration configuration)
        {
            _azureOpenAIEndpoint = configuration["OpenAI:Endpoint"];
            _azureOpenAIKey = configuration["OpenAI:ApiKey"];
            _deploymentName = configuration["OpenAI:DeploymentName"];
            _azureAISearchEndpoint = configuration["AzureAISearch:Endpoint"];
            _azureAISearchKey = configuration["AzureAISearch:ApiKey"];
            _azureAISearchIndex = configuration["AzureAISearch:Index"];
            _retryPolicy = CreateRetryPolicy();
        }

        private AsyncRetryPolicy CreateRetryPolicy()
        {
            return Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(2, retryAttempt => TimeSpan.FromSeconds(2),
                    (result, timeSpan, retryCount, context) =>
                    {
                        Console.WriteLine($"Intento de reintento {retryCount} después de {timeSpan.Seconds} segundos debido a: {result.Message}");
                    });
        }

        public async Task<string> SearchAsync(string promptquery)
        {
#pragma warning disable AOAI001
            AzureOpenAIClient azureClient = new(
                new Uri(_azureOpenAIEndpoint),
                new AzureKeyCredential(_azureOpenAIKey));
            ChatClient chatClient = azureClient.GetChatClient(_deploymentName);

            ChatCompletionOptions options = new();
            options.AddDataSource(new AzureSearchChatDataSource()
            {
                Endpoint = new Uri(_azureAISearchEndpoint),
                IndexName = _azureAISearchIndex,
                Authentication = DataSourceAuthentication.FromApiKey(_azureAISearchKey),
            });
            options.Temperature = 0.3f;

            var systemMessage = new SystemChatMessage("Debes utilizar el documento que tenga la fecha más actual disponible. Necesito asegurarme de que se utilice la versión más reciente para obtener la información más precisa y actualizada. Asegúrate de verificar las fechas de cada documento antes de proceder");
            var userMessage = new UserChatMessage($"{promptquery}, utiliza el documento que tenga la fecha más actual");

            var resultmessege = await _retryPolicy.ExecuteAsync(() =>
              {
                  ChatCompletion completion = chatClient.CompleteChat(
                      new List<ChatMessage> { systemMessage, userMessage }
                      , options);

                  var result = completion.Content[0].Text;
                  if (result.Contains("La información solicitada no está disponible en los datos recuperados. Por favor, intenta con otra consulta o tema."))
                  {
                      throw new CustomRetryException("Información no disponible, reintentando...");
                  }
                  return Task.FromResult(result);
              });

            return resultmessege;
        }
    }
}