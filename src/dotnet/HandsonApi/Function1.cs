using Azure.AI.OpenAI;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HandsonApi;

public class Function1
{
    // Azure OpenAI のモデル: text-embedding-ada-002 の Deployment Name (Model Name ではないので注意)
    private static readonly string EmbeddingsDeploymentName = "text-embedding-ada-002";
    private static readonly string VectorField = "contentVector";
    private static readonly int DataCount = 3;

    private readonly OpenAIClient _openAIClient;
    private readonly SearchClient _searchClient;

    public Function1(OpenAIClient openAIClient, SearchClient searchClient)
    {
        _openAIClient = openAIClient;
        _searchClient = searchClient;
    }

    [FunctionName("vector-search")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
        ILogger log)
    {
        // クエリ文字列で `query` の値を取得
        var query = req.Query["query"];

        if (string.IsNullOrWhiteSpace(query))
        {
            return new BadRequestResult();
        }

        // ベクター化
        var embedding = await GetEmbeddingAsync(query);
        // ベクター検索の実行
        var searchResults = await ExecuteVectorSearchAsync(embedding);

        if (searchResults.Any())
        {
            return new OkObjectResult(searchResults);
        }

        return new NotFoundResult();
    }

    private async Task<IReadOnlyList<float>> GetEmbeddingAsync(string text)
    {
        var response = await _openAIClient.GetEmbeddingsAsync(EmbeddingsDeploymentName, new EmbeddingsOptions(text));
        return response.Value.Data[0].Embedding;
    }

    private async Task<List<IndexDocument>> ExecuteVectorSearchAsync(IReadOnlyList<float> embedding)
    {
        var searchOptions = new SearchOptions
        {
            Vectors = { new SearchQueryVector { Value = embedding.ToArray(), KNearestNeighborsCount = 3, Fields = { VectorField } } },
            Size = DataCount,
            Select = { "title", "content", "category" }
        };

        // `Azure.Search.Documents.Models.SearchDocument` を `SearchAsync()` の Generics に定義してコールしているが、
        // 独自の class を Generics として定義も可能。
        SearchResults<SearchDocument> response = await _searchClient.SearchAsync<SearchDocument>(null, searchOptions);
        var searchResults = new List<IndexDocument>(DataCount);
        await foreach (var result in response.GetResultsAsync())
        {
            searchResults.Add(new IndexDocument
            {
                Title = result.Document["title"].ToString(),
                Category = result.Document["category"].ToString(),
                Content = result.Document["content"].ToString(),
                Score = result.Score
            });
        }

        return searchResults;
    }
}

/// <summary>
/// 検索結果のモデル
/// </summary>
/// <remarks>検索結果の API の response body には List で出力</remarks>>
public class IndexDocument
{
    public string Title { get; set; }
    public string Category { get; set; }
    public string Content { get; set; }
    public double? Score { get; set; }
}