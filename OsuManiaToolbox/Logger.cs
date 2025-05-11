namespace OsuManiaToolbox;

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

public class Logger(Action<LogMessage> logAction, string source = "Main")
{
    private readonly Action<LogMessage> _logAction = logAction;
    public string Source { get; } = source;

    public void Log(string message, LogLevel level = LogLevel.Info)
    {
        _logAction(new LogMessage(Source, message, level));
    }

    public void Debug(string message) => Log(message, LogLevel.Debug);
    public void Info(string message) => Log(message, LogLevel.Info);
    public void Warning(string message) => Log(message, LogLevel.Warning);
    public void Error(string message) => Log(message, LogLevel.Error);
    public void Exception(Exception ex)
    {
        Log($"{ex.GetType().Name}: {ex.Message}", LogLevel.Error);
        if (ex.StackTrace != null)
        {
            Log(ex.StackTrace, LogLevel.Error);
        }
    }
}