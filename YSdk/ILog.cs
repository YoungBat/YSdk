namespace YSdk;

public interface ILog
{
    void Write(string message, EasyLog.LogLevel level);
    void Close();
}