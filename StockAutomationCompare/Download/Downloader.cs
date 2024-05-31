namespace StockAutomationCompare.Download;

public static class Downloader
{
    private static string DownloadUrl { get; set; } =
        "https://ark-funds.com/wp-content/uploads/funds-etf-csv/ARK_INNOVATION_ETF_ARKK_HOLDINGS.csv";

    public static async Task<byte[]> DownloadToBytes(HttpClient client)
    {
        await using var streamResult = client.GetStreamAsync(DownloadUrl).Result;
        await using var fileBytes = new MemoryStream(100240); // should cover average snapshot size
        await streamResult.CopyToAsync(fileBytes);
        await fileBytes.FlushAsync();
        return fileBytes.ToArray();
    }
}
