using OsuParsers.Database.Objects;

namespace OsuManiaToolbox.Core.Services;

public interface IDbService<TItem>
{
    IReadOnlyList<TItem> Items { get; }
    IReadOnlyDictionary<string, TItem> Index { get; }
    void Save();
    void Reload();
}

public interface IBeatmapDbService : IDbService<DbBeatmap>
{
    void Add(DbBeatmap item);
    void Remove(string key);
}

public interface ICollectionDbService : IDbService<Collection>
{
    void Add(Collection item);
    void Remove(string key);
}

public interface IScoreDbService : IDbService<List<Score>>
{

}