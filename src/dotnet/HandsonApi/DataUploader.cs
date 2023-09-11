using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;

namespace HandsonApi;

public class DataUploader
{
    private const string DatabaseId = "handson";
    private const string ContainerId = "azure";

    private readonly CosmosClient _cosmosClient;

    public DataUploader(CosmosClient cosmosClient)
    {
        _cosmosClient = cosmosClient;
    }

    [FunctionName("upload")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "cosmos/upload"), FromBody] AzureInfo[] items,
        ILogger log)
    {
        var container = _cosmosClient.GetContainer(DatabaseId, ContainerId);

        // Cosmos DB ‚Ö Upsert
        var tasks = items.Select(item => container.UpsertItemAsync(item, new PartitionKey(item.Id))).ToArray();
        await Task.WhenAll(tasks);

        return new OkResult();
    }
}

public record AzureInfo(string Id, string Category, string Title, string Content);