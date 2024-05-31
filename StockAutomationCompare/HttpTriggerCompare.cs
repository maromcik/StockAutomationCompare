using System.Net;
using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using StockAutomationCompare.Diff;
using StockAutomationCompare.DiffFormat;
using StockAutomationCompare.Download;
using StockAutomationCompare.Parser;

namespace StockAutomationCompare;

public class HttpTriggerCompare(
    ILoggerFactory loggerFactory,
    BlobServiceClient blobServiceClient,
    ServiceBusClient serviceBusClient)
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<HttpTriggerCompare>();
    private const string QueueName = "pa200-hw3-snapshots";


    [Function("HttpTriggerCompare")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req,
        FunctionContext executionContext)
    {
        _logger.LogInformation("Processing snapshots");

        var http = new HttpClient();
        http.DefaultRequestHeaders.Add("User-Agent", "StockAutomationCore/1.0");
        var newFile = await Downloader.DownloadToBytes(http);
        var oldFile = await FetchOldFileFromBlobStorageAsync();

        if (oldFile.Length == 0)
        {
            oldFile = newFile;
        }

        await UploadNewFileToBlobStorageAsync(newFile);
        var newLines = HoldingSnapshotLineParser.ParseLinesFromBytes(newFile);
        var oldLines = HoldingSnapshotLineParser.ParseLinesFromBytes(oldFile);

        var diff = new HoldingsDiff(oldLines, newLines);
        var diffResult = TextDiffFormatter.Format(diff);
        await SendMessageToServiceBusQueueAsync(diffResult);
        var response = req.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
        await response.WriteStringAsync(diffResult);
        return response;
    }


    private async Task<byte[]> FetchOldFileFromBlobStorageAsync()
    {
        const string containerName = "snapshots";

        var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
        BlobItem? mostRecentBlob = null;
        DateTimeOffset? mostRecentTime = null;

        await foreach (var blobItem in containerClient.GetBlobsAsync())
        {
            var blobClient = containerClient.GetBlobClient(blobItem.Name);
            BlobProperties properties = await blobClient.GetPropertiesAsync();

            if (mostRecentTime != null && !(properties.CreatedOn > mostRecentTime)) continue;
            mostRecentTime = properties.CreatedOn;
            mostRecentBlob = blobItem;
        }

        if (mostRecentBlob == null)
        {
            _logger.LogWarning("No initial snapshot found, using the same snapshot");
            return [];
        }

        var mostRecentBlobClient = containerClient.GetBlobClient(mostRecentBlob?.Name);
        BlobDownloadInfo download = await mostRecentBlobClient.DownloadAsync();
        using var ms = new MemoryStream();
        await download.Content.CopyToAsync(ms);
        return ms.ToArray();
    }

    private async Task UploadNewFileToBlobStorageAsync(byte[] newFile)
    {
        var containerName = "snapshots";
        var blobName = $"snapshot-{DateTime.UtcNow:yyyyMMddHHmmss}.csv";

        BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);
        BlobClient blobClient = containerClient.GetBlobClient(blobName);

        using (var ms = new MemoryStream(newFile))
        {
            await blobClient.UploadAsync(ms, overwrite: true);
        }

        _logger.LogInformation("Uploaded new snapshot to blob storage: {BlobName}", blobName);
    }

    private async Task SendMessageToServiceBusQueueAsync(string messageBody)
    {
        var sender = serviceBusClient.CreateSender(QueueName);
        var message = new ServiceBusMessage(messageBody);
        await sender.SendMessageAsync(message);
        await sender.DisposeAsync();
    }
}
