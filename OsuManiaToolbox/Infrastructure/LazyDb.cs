using OsuManiaToolbox.Settings;
using System.Collections;
using OsuManiaToolbox.Core.Services;

namespace OsuManiaToolbox.Infrastructure;

public class LazyDb<TDatabase, TItem>(string filePath, IDbAdapter<TDatabase, TItem> adapter, ISettingsService settingsService, ILogger logger)
    : IReadOnlyList<TItem> where TDatabase : class
{
    private readonly string _filePath = filePath;
    private readonly IDbAdapter<TDatabase, TItem> _adapter = adapter;
    private readonly CommonSettings _settings = settingsService.GetSettings<CommonSettings>();
    private readonly ILogger _logger = logger;

    private TDatabase? _db;
    private Dictionary<string, TItem>? _index;

    public TDatabase Db => LazyInitializer.EnsureInitialized(ref _db, Load);
    public Dictionary<string, TItem> Index => LazyInitializer.EnsureInitialized(ref _index, CreateIndex);

    public int Count => _adapter.GetList(Db).Count;
    public string FilePath => _filePath;

    public TItem this[int index] => _adapter.GetList(Db)[index];
    public TItem this[string key] => Index![key];

    protected virtual TDatabase Load()
    {
        _logger.Debug($"开始加载数据库: {FilePath}");
        var db = _adapter.Load(FilePath);
        _logger.Debug($"数据库加载完成: {FilePath}");
        return db;
    }

    public virtual void Save()
    {
        _logger.Debug($"开始保存数据库: {FilePath}");
        if (_settings.BackupDb)
        {
            var backup = Utils.BackupFile(FilePath);
            _logger.Info($"数据库备份到: {backup}");
        }
        _adapter.Save(Db, FilePath);
        _logger.Debug($"数据库保存完成: {FilePath}");
    }

    protected virtual Dictionary<string, TItem> CreateIndex()
    {
        _logger.Debug($"开始重建索引: {FilePath}");
        var index = _adapter.CreateIndex(Db);
        _logger.Debug($"索引重建完成: {FilePath}");
        return index;
    }

    public void Reload()
    {
        _db = Load();
        _index = CreateIndex();
    }

    public IEnumerator<TItem> GetEnumerator()
    {
        return _adapter.GetList(Db).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
