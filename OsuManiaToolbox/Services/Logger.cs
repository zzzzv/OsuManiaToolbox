using System.Collections.Concurrent;

namespace OsuManiaToolbox.Services;

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
    public string Category { get; }
    public void Log(string message, LogLevel level = LogLevel.Info);
    public void Debug(string message) => Log(message, LogLevel.Debug);
    public void Info(string message) => Log(message, LogLevel.Info);
    public void Warning(string message) => Log(message, LogLevel.Warning);
    public void Error(string message) => Log(message, LogLevel.Error);
    public void Exception(Exception ex)
    {
        Debug($"{ex.GetType().Name}: {ex.Message}");
        if (ex.StackTrace != null)
        {
            Debug(ex.StackTrace);
        }
    }
}

public interface ILogger<T> : ILogger { }

public class Logger<T>(ILogDispatcher logProvider) : ILogger<T>
{
    private readonly ILogDispatcher _logProvider = logProvider;

    public string Category => typeof(T).Name;

    public void Log(string message, LogLevel level = LogLevel.Info)
    {
        _logProvider.AppendLog(new LogMessage(Category, message, level));
    }
}

public interface ILogDispatcher
{
    event Action<IEnumerable<LogMessage>>? LogsReceived;
    public int Interval { get; set; }
    void AppendLog(LogMessage log);
}

public class LogDispatcher : ILogDispatcher, IDisposable
{
    public event Action<IEnumerable<LogMessage>>? LogsReceived;
    public int Interval { get; set; } = 200;

    private readonly ConcurrentQueue<LogMessage> _queue = new();
    private readonly Timer _flushTimer;
    private bool _disposed;

    public LogDispatcher()
    {
        _flushTimer = new Timer(_ => FlushQueue(), null, 0, Interval);
    }

    public void AppendLog(LogMessage log)
    {
        _queue.Enqueue(log);
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _flushTimer.Dispose();
            FlushQueue();
            _disposed = true;
        }
    }

    private void FlushQueue()
    {
        var logs = new List<LogMessage>();
        while (_queue.TryDequeue(out var message))
        {
            logs.Add(message);
        }

        if (logs.Count > 0)
        {
            LogsReceived?.Invoke(logs);
        }
    }
}