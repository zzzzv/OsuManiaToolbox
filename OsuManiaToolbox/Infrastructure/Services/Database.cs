using OsuManiaToolbox.Settings;
using OsuParsers.Database;
using OsuParsers.Database.Objects;
using System.Diagnostics.CodeAnalysis;
using OsuManiaToolbox.Core.Services;

namespace OsuManiaToolbox.Infrastructure.Services;

public class BeatmapDbService : LazyDb<OsuDatabase, DbBeatmap>, IBeatmapDbService
{
    public BeatmapDbService(IOsuFileService fileService, ISettingsService settingsService, ILogger<BeatmapDbService> logger)
        : base(fileService.BeatmapDbPath, new BeatmapDbAdapter(), settingsService, logger)
    {
    }
}

public class CollectionDbService : LazyDb<CollectionDatabase, Collection>, ICollectionDbService
{
    public CollectionDbService(IOsuFileService fileService, ISettingsService settingsService, ILogger<CollectionDbService> logger)
        : base(fileService.CollectionDbPath, new CollectionDbAdapter(), settingsService, logger)
    {
    }

    public bool Contains(string name)
    {
        return Index.ContainsKey(name);
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



public class ScoreDbService : LazyDb<ScoresDatabase, List<Score>>, IScoreDbService
{
    public ScoreDbService(IOsuFileService fileService, ISettingsService settingsService, ILogger<ScoreDbService> logger)
        : base(fileService.ScoreDbPath, new ScoreDbAdapter(), settingsService, logger)
    {
    }

    public bool TryGetValue(string hash, [MaybeNullWhen(false)] out List<Score> scores)
    {
        return Index.TryGetValue(hash, out scores);
    }
}
