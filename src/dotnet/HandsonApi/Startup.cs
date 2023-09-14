using Azure;
using Azure.AI.OpenAI;
using Azure.Core.Serialization;
using Azure.Search.Documents;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Text.Json;

[assembly: FunctionsStartup(typeof(HandsonApi.Startup))]

namespace HandsonApi;

internal class Startup : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
        // OpenAIClient を登録
        builder.Services.AddSingleton(_ =>
        {
            var endpoint = new Uri(Environment.GetEnvironmentVariable("AzureOpenAIOptions:Endpoint") ?? throw new NullReferenceException("AzureOpenAIOptions:Endpoint"));
            var apiKey = Environment.GetEnvironmentVariable("AzureOpenAIOptions:ApiKey") ?? throw new NullReferenceException("AzureOpenAIOptions:ApiKey");

            return new OpenAIClient(endpoint, new AzureKeyCredential(apiKey));
        });

        // SearchClient の登録
        builder.Services.AddSingleton(_ =>
        {
            var endpoint = new Uri(Environment.GetEnvironmentVariable("CognitiveSearchOptions:Endpoint") ?? throw new NullReferenceException("CognitiveSearchOptions:Endpoint"));
            var queryKey = Environment.GetEnvironmentVariable("CognitiveSearchOptions:queryKey") ?? throw new NullReferenceException("CognitiveSearchOptions:queryKey");
            var indexName = Environment.GetEnvironmentVariable("CognitiveSearchOptions:IndexName") ?? throw new NullReferenceException("CognitiveSearchOptions:IndexName");

            var jsonSerializerOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            // C# の class (pascal-case) と CognitiveSearch index schema (camel-case) を補完するためのオプション
            var searchClientOptions = new SearchClientOptions { Serializer = new JsonObjectSerializer(jsonSerializerOptions) };

            return new SearchClient(endpoint, indexName, new AzureKeyCredential(queryKey), searchClientOptions);
        });

        // CosmosClient の登録
        builder.Services.AddSingleton(_ =>
        {
            var connectionString = Environment.GetEnvironmentVariable("CosmosConnectionString") ?? throw new NullReferenceException("CosmosConnectionString");
            var options = new CosmosClientOptions
            {
                SerializerOptions = new CosmosSerializationOptions { PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase }
            };
            return new CosmosClient(connectionString, options);
        });

    }
}