using System.Text;

namespace YSdk;

public class EasyLog : ILog
{
    public enum LogLevel
    {
        Info,
        Warning,
        Error
    }

    private readonly object _lock = new();
    private readonly MemoryStream _logBuffer = new();
    private readonly string _logPath;

    public EasyLog(string logPath)
    {
        _logPath = logPath;
        Directory.CreateDirectory(_logPath);
    }

    public void Write(string message, LogLevel level)
    {
        var logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}{Environment.NewLine}";
        var bytes = Encoding.UTF8.GetBytes(logMessage);

        lock (_lock)
        {
            if (_logBuffer.Length >= 4 * 1024 * 1024)
            {
                FlushBuffer();
            }

            _logBuffer.Write(bytes, 0, bytes.Length);
        }
    }

    public void Close()
    {
        lock (_lock)
        {
            if (_logBuffer.Length > 0)
            {
                FlushBuffer();
            }
        }
    }

    private void FlushBuffer()
    {
        var filePath = Path.Combine(_logPath, "log.txt");
        using (var fs = new FileStream(filePath, FileMode.Append, FileAccess.Write))
        {
            fs.Write(_logBuffer.ToArray(), 0, (int)_logBuffer.Length);
        }

        _logBuffer.SetLength(0);
    }
}