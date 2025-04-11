using YSdk;

namespace Test;

class Program
{
    static void Main()
    {
        var client = new EasyDownload();
        var progress = new Progress<double>(p => Console.Write($"\rProgress: {p:F2}%\r"));
        client.GetAsync(
            "http://officecdn.microsoft.com/pr/492350f6-3a01-4f97-b9c0-c7c6ddf67d60/media/zh-cn/ProPlus2021Retail.img",
            "ProPlus2021Retail.img", progress).GetAwaiter().GetResult();
    }
}