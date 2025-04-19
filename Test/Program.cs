using YSdk;

namespace Test;

static class Program
{
    static void Main(string[] args)
    {
        try
        {
            var cmd = EasyCommand.Parse("download /url fgtf /savePath 123");
            Console.WriteLine($"{cmd.Command} {cmd["url"]} {cmd["svePath"]}");
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }
}