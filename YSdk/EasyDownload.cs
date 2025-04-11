using System.Diagnostics;

namespace YSdk;

public class EasyDownload : IDownload
{
    private static readonly HttpClient HttpClient = new HttpClient();

    public async Task<(bool, string)> GetAsync(string url, string savePath)
        => await DownloadCoreAsync(url, savePath);

    public async Task<(bool success, long fileSize, bool acceptRanges)> GetInfoAsync(string url)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Head, url);
            using var response = await HttpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
                return (false, 0, false);

            var contentLength = response.Content.Headers.ContentLength ?? -1;
            var acceptRanges = response.Headers.Contains("Accept-Ranges");

            return (true, contentLength, acceptRanges);
        }
        catch
        {
            return (false, 0, false);
        }
    }

    // 单线程下载扩展
    public async Task<(bool, string)> GetAsync(string url, string savePath, IProgress<long> progress)
        => await DownloadCoreAsync(url, savePath, progress);

    public async Task<(bool, string)> GetAsync(string url, string savePath, int maxBytesPerSecond)
        => await DownloadCoreAsync(url, savePath, null, maxBytesPerSecond);

    public async Task<(bool, string)> GetAsync(string url, string savePath, IProgress<double> percentageProgress)
    {
        try
        {
            var (success, size, acceptRanges) = await GetInfoAsync(url);
            if (!success || size == -1 || !acceptRanges) return (false, "Content-Length required");

            return await DownloadCoreAsync(url, savePath,
                progress: new Progress<long>(bytes =>
                    percentageProgress.Report(bytes * 100.0 / size)));
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    private async Task<(bool, string)> DownloadCoreAsync(
        string url,
        string savePath,
        IProgress<long>? progress = null,
        int? maxBytesPerSecond = null)
    {
        try
        {
            using var response = await HttpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            if (!response.IsSuccessStatusCode)
                return (false, $"HTTP Error: {response.StatusCode}");

            var directoryPath = Path.GetDirectoryName(savePath);
            if (!string.IsNullOrEmpty(directoryPath))
                Directory.CreateDirectory(directoryPath);

            await using var stream = await response.Content.ReadAsStreamAsync();
            await using var fileStream = new FileStream(savePath, FileMode.Create, FileAccess.Write);

            var buffer = new byte[4096];
            long totalRead = 0;
            var sw = new Stopwatch();
            int bytesRead;

            if (maxBytesPerSecond.HasValue)
                sw.Start();

            while ((bytesRead = await stream.ReadAsync(buffer)) > 0)
            {
                if (maxBytesPerSecond.HasValue)
                {
                    var expectedTime = totalRead * 1000.0 / maxBytesPerSecond.Value;
                    if (expectedTime > sw.ElapsedMilliseconds)
                    {
                        await Task.Delay((int)(expectedTime - sw.ElapsedMilliseconds));
                    }
                }

                await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead));
                totalRead += bytesRead;
                progress?.Report(totalRead);
            }

            return (true, savePath);
        }
        catch (Exception ex)
        {
            return (false, $"{ex.GetType().Name}: {ex.Message}");
        }
    }
}

public interface IDownload
{
    Task<(bool, string)> GetAsync(string url, string savePath);

    // 新增文件信息接口
    Task<(bool success, long fileSize, bool acceptRanges)> GetInfoAsync(string url);
}