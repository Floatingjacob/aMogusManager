/*
 
 This code has been slightly modified by FloatingJacob.
 You can find the original code at https://webscraping.ai/faq/httpclient-c/is-it-possible-to-track-the-progress-of-a-download-using-httpclient-c

 */

using System.Threading.Tasks;

public class DownloadProgressTracker
{
    static void Main() { }
    private readonly HttpClient _httpClient;
    public DownloadProgressTracker()
    {
        _httpClient = new HttpClient();
    }
    public async Task DownloadFileAsync(string url, string filePath,
        IProgress<DownloadProgress> progress = null)
    {
        using var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"Download failed: {response.StatusCode}");
        var totalBytes = response.Content.Headers.ContentLength ?? -1L;
        var totalBytesRead = 0L;
        var buffer = new byte[65536];
        using var contentStream = await response.Content.ReadAsStreamAsync();
        using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write,
            FileShare.None, bufferSize: 65536, useAsync: true);
        int bytesRead;
        while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
        {
            await fileStream.WriteAsync(buffer, 0, bytesRead);
            totalBytesRead += bytesRead;
            progress?.Report(new DownloadProgress(totalBytesRead, totalBytes));
        }
    }
}

public class DownloadProgress
{
    public long BytesDownloaded { get; }
    public long TotalBytes { get; }
    public double? ProgressPercentage => TotalBytes > 0 ?
        (double)BytesDownloaded / TotalBytes * 100 : null;
    public DownloadProgress(long bytesDownloaded, long totalBytes)
    {
        BytesDownloaded = bytesDownloaded;
        TotalBytes = totalBytes;
    }
}

// I added this class here.
public class DownloadWithProgress
{
    public async Task Download(string url, string filePath)
    {
        int oldProgress = -1;
        var progress = new Progress<DownloadProgress>(p =>
        {
            if (!p.ProgressPercentage.HasValue) return;
            int newValue = (int)p.ProgressPercentage.Value;
            if (newValue == oldProgress) return;
            oldProgress = newValue;
            string text = $"Downloading... {newValue}%";
            Console.Write("\r" + text + "   ");
        });
        var downloadclient = new DownloadProgressTracker();
        await downloadclient.DownloadFileAsync(url, filePath, progress);
    }
}