namespace YSdk;

public class EasyCommand
{
    public string Command { get; private set; } = "";
    private Dictionary<string, string> Arguments { get; } = new();

    public string this[string arg] =>
        Arguments.GetValueOrDefault(arg) ?? throw new KeyNotFoundException("Argument not found");

    public static EasyCommand Parse(string input, string splitChar = " ")
    {
        var segments = input.Split([splitChar], StringSplitOptions.RemoveEmptyEntries);

        var commandValue = segments.Length > 0
            ? segments[0]
            : throw new ArgumentException("Invalid command");

        var cmd = new EasyCommand
        {
            Command = commandValue
        };
        string? currentKey = null;
        var values = new List<string>();

        foreach (var segment in segments.Skip(1))
        {
            if (segment.StartsWith("/"))
            {
                if (currentKey is not null)
                {
                    cmd.Arguments[currentKey] = string.Join(splitChar, values);
                    values.Clear();
                }

                currentKey = segment[1..];
            }
            else
            {
                values.Add(segment);
            }
        }

        if (currentKey is not null)
        {
            cmd.Arguments[currentKey] = string.Join(splitChar, values);
        }

        return cmd;
    }
}