namespace OsuManiaToolbox.Core.Services;

public enum LogLevel
{
    Debug,
    Info,
    Warning,
    Error,
}

public record LogMessage(string Source, string Message, LogLevel Level)
{
    public override string ToString() => $"[{DateTime.Now:HH:mm:ss}][{Source}]{Message}";
}

public interface ILogger
{
    string Category { get; }
    void Log(string message, LogLevel level = LogLevel.Info);
    void Debug(string message) => Log(message, LogLevel.Debug);
    void Info(string message) => Log(message, LogLevel.Info);
    void Warning(string message) => Log(message, LogLevel.Warning);
    void Error(string message) => Log(message, LogLevel.Error);
    void Exception(Exception ex)
    {
        Error($"{ex.GetType().Name}: {ex.Message}");
        if (ex.StackTrace != null)
        {
            Debug(ex.StackTrace);
        }
    }
}

public interface ILogDispatcher
{
    event Action<IEnumerable<LogMessage>>? LogsReceived;
    public int Interval { get; set; }
    void AppendLog(LogMessage log);
}

public interface ILogService
{
    ILogDispatcher LogDispatcher { get; }
    ILogger GetLogger(string category);
    ILogger GetLogger<T>()
    {
        return GetLogger(typeof(T).Name);
    }
    ILogger GetLogger(object context)
    {
        return GetLogger(context.GetType().Name);
    }
}