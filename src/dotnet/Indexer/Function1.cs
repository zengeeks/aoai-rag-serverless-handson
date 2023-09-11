using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using Azure.AI.OpenAI;
using Azure.Search.Documents;
using System.Threading.Tasks;

namespace Indexer;

public class Function1
{
    // Azure OpenAI のモデル: text-embedding-ada-002 の Deployment Name (Model Name ではないので注意)
    private const string EmbeddingsDeploymentName = "text-embedding-ada-002";

    private readonly OpenAIClient _openAIClient;
    private readonly SearchClient _searchClient;

    public Function1(OpenAIClient openAIClient, SearchClient searchClient)
    {
        _openAIClient = openAIClient;
        _searchClient = searchClient;
    }

    [FunctionName("Function1")]
    public async Task Run([CosmosDBTrigger(
            databaseName: "handson",
            containerName: "azure",
            Connection = "CosmosConnection",
            CreateLeaseContainerIfNotExists = true,  // false 推奨
            LeaseContainerName = "leases")]IReadOnlyList<AzureInfo> input,
        ILogger log)
    {
        var documentsToUpload = new List<IndexDocument>();

        foreach (var azureInfo in input)
        {
            // content の値をベクター化
            var contentVector = await GetEmbeddingAsync(azureInfo.Content);

            // Cognitive Search のインデックスのスキーマへ変換
            documentsToUpload.Add(new IndexDocument
            {
                Id = azureInfo.Id,
                Category = azureInfo.Category,
                Title = azureInfo.Title,
                Content = azureInfo.Content,
                ContentVector = contentVector
            });
        }

        // Cognitive Search のインデックス更新
        await _searchClient.MergeOrUploadDocumentsAsync(documentsToUpload);
        log.LogInformation($"{documentsToUpload.Count} document(s) uploaded.");
    }

    private async Task<IReadOnlyList<float>> GetEmbeddingAsync(string text)
    {
        var response = await _openAIClient.GetEmbeddingsAsync(EmbeddingsDeploymentName, new EmbeddingsOptions(text));
        return response.Value.Data[0].Embedding;
    }
}

/// <summary>
/// Cosmos DB: azure コンテナーのスキーマ
/// </summary>
public record AzureInfo(string Id, string Category, string Title, string Content);

/// <summary>
/// Cognitive Search の Index 更新用スキーマ
/// </summary>
public class IndexDocument
{
    public string Id { get; set; }
    public string Category { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }
    public IReadOnlyList<float> ContentVector { get; set; }
}