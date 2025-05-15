using OsuManiaToolbox.Settings;
using OsuParsers.Database;
using OsuParsers.Database.Objects;
using System.Diagnostics.CodeAnalysis;

namespace OsuManiaToolbox.Services;

public interface IBeatmapDbService : IReadOnlyList<DbBeatmap>
{
    public DbBeatmap this[string hash] { get; }
    public void Save();
    public void Reload();
}

public class BeatmapDbService : LazyDb<OsuDatabase, DbBeatmap>, IBeatmapDbService
{
    public BeatmapDbService(OsuFileService fileService, CommonSettings settings, ILogger<BeatmapDbService> logger)
        : base(fileService.BeatmapDbPath, new BeatmapDbAdapter(), settings, logger)
    {
    }
}

public interface ICollectionDbService : IReadOnlyList<Collection>
{
    public void Add(Collection collection);
    public void Remove(string name);
    public void Save();
    public void Reload();
}

public class CollectionDbService : LazyDb<CollectionDatabase, Collection>, ICollectionDbService
{
    public CollectionDbService(OsuFileService fileService, CommonSettings settings, ILogger<CollectionDbService> logger)
        : base(fileService.CollectionDbPath, new CollectionDbAdapter(), settings, logger)
    {
    }

    public void Add(Collection collection)
    {
        Db.Collections.Add(collection);
        Db.CollectionCount++;
        Index.Add(collection.Name, collection);
    }

    public void Remove(string name)
    {
        Db.Collections.RemoveAll(c => c.Name == name);
        Db.CollectionCount--;
        Index.Remove(name);
    }
}

public interface IScoreDbService : IReadOnlyList<List<Score>>
{
    public List<Score> this[string hash] { get; }
    public bool TryGetValue(string hash, [MaybeNullWhen(false)] out List<Score> scores);
    public void Reload();
}

public class ScoreDbService : LazyDb<ScoresDatabase, List<Score>>, IScoreDbService
{
    public ScoreDbService(OsuFileService fileService, CommonSettings settings, ILogger<ScoreDbService> logger)
        : base(fileService.ScoreDbPath, new ScoreDbAdapter(), settings, logger)
    {
    }

    public bool TryGetValue(string hash, [MaybeNullWhen(false)] out List<Score> scores)
    {
        return Index.TryGetValue(hash, out scores);
    }
}
