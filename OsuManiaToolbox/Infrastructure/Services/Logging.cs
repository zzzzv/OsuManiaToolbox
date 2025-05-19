using System.Collections.Concurrent;
using OsuManiaToolbox.Core.Services;

namespace OsuManiaToolbox.Infrastructure.Services;

public class Logging<T>(ILogDispatcher logProvider) : ILogger<T>
{
    private readonly ILogDispatcher _logProvider = logProvider;

    public string Category => typeof(T).Name;

    public void Log(string message, LogLevel level = LogLevel.Info)
    {
        _logProvider.AppendLog(new LogMessage(Category, message, level));
    }
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