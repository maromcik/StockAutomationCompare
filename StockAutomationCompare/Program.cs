using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services =>
    {
        var blobServiceConnectionString = Environment.GetEnvironmentVariable("CUSTOMCONNSTR_BLOB_STORAGE");
        services.AddSingleton(new BlobServiceClient(blobServiceConnectionString));

        var serviceBusConnectionString = Environment.GetEnvironmentVariable("CUSTOMCONNSTR_SERVICE_BUS");
        services.AddSingleton(new ServiceBusClient(serviceBusConnectionString));

        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
    })
    .Build();

host.Run();
