using OsuManiaToolbox.Settings;
using OsuParsers.Database;
using OsuParsers.Database.Objects;
using OsuManiaToolbox.Core.Services;

namespace OsuManiaToolbox.Infrastructure.Services;

public abstract class DbService<TDatabase, TItem> : IDbService<TItem> where TDatabase : class
{
    protected IDbAdapter<TDatabase, TItem> _adapter;
    protected readonly string _filePath;
    protected readonly CommonSettings _settings;
    protected readonly ILogger _logger;
    protected Lazy<TDatabase> _db;
    protected Lazy<Dictionary<string, TItem>> _index;

    protected DbService(IDbAdapter<TDatabase, TItem> adapter, string filePath, CommonSettings settings, ILogger logger)
    {
        _adapter = adapter;
        _filePath = filePath;
        _settings = settings;
        _logger = logger;
        _db = new Lazy<TDatabase>(() => _adapter.Load(_filePath), LazyThreadSafetyMode.ExecutionAndPublication);
        _index = new Lazy<Dictionary<string, TItem>>(() => _adapter.CreateIndex(_db.Value), LazyThreadSafetyMode.ExecutionAndPublication);
    }

    public IReadOnlyList<TItem> Items => _adapter.GetList(_db.Value);
    public IReadOnlyDictionary<string, TItem> Index => _index.Value;
    public virtual void Add(TItem item)
    {
        throw new NotImplementedException($"{GetType().Name} 不支持添加");
    }
    public virtual void Remove(string key)
    {
        throw new NotImplementedException($"{GetType().Name} 不支持删除");
    }
    public void Save()
    {
        _logger.Debug($"开始保存数据库: {_filePath}");
        if (_settings.BackupDb)
        {
            var backup = Utils.BackupFile(_filePath);
            _logger.Info($"数据库备份到: {backup}");
        }
        _adapter.Save(_db.Value, _filePath);
        _logger.Debug($"数据库保存完成: {_filePath}");
    }
    protected TDatabase Load()
    {
        _logger.Debug($"开始加载数据库: {_filePath}");
        var db = _adapter.Load(_filePath);
        _logger.Debug($"数据库加载完成: {_filePath}");
        return db;
    }
    protected Dictionary<string, TItem> CreateIndex()
    {
        _logger.Debug($"开始重建索引: {_filePath}");
        var index = _adapter.CreateIndex(_db.Value);
        _logger.Debug($"索引重建完成: {_filePath}");
        return index;
    }
}

public class BeatmapDbService : DbService<OsuDatabase, DbBeatmap>, IBeatmapDbService
{
    public BeatmapDbService(IOsuFileService fileService, ISettingsService settingsService, ILogService logService)
        : base(new BeatmapDbAdapter(), fileService.BeatmapDbPath, settingsService.GetSettings<CommonSettings>(), logService.GetLogger<BeatmapDbService>())
    {
    }

    public override void Add(DbBeatmap item)
    {
        _db.Value.Beatmaps.Add(item);
        _db.Value.BeatmapCount++;
        _index.Value.Add(item.MD5Hash, item);
    }

    public override void Remove(string key)
    {
        _db.Value.Beatmaps.RemoveAll(b => b.MD5Hash == key);
        _db.Value.BeatmapCount--;
        _index.Value.Remove(key);
    }
}

public class CollectionDbService : DbService<CollectionDatabase, Collection>, ICollectionDbService
{
    public CollectionDbService(IOsuFileService fileService, ISettingsService settingsService, ILogService logService)
        : base(new CollectionDbAdapter(), fileService.CollectionDbPath, settingsService.GetSettings<CommonSettings>(), logService.GetLogger<CollectionDbService>())
    {
    }

    public override void Add(Collection collection)
    {
        _db.Value.Collections.Add(collection);
        _db.Value.CollectionCount++;
        _index.Value.Add(collection.Name, collection);
    }

    public override void Remove(string name)
    {
        _db.Value.Collections.RemoveAll(c => c.Name == name);
        _db.Value.CollectionCount--;
        _index.Value.Remove(name);
    }
}

public class ScoreDbService : DbService<ScoresDatabase, List<Score>>, IScoreDbService
{
    public ScoreDbService(IOsuFileService fileService, ISettingsService settingsService, ILogService logService)
        : base(new ScoreDbAdapter(), fileService.ScoreDbPath, settingsService.GetSettings<CommonSettings>(), logService.GetLogger<ScoreDbService>())
    {
    }
}
