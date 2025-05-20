using OsuParsers.Database.Objects;
using System.Diagnostics.CodeAnalysis;

namespace OsuManiaToolbox.Core.Services;

public interface IDbService<TItem>
{
    public IReadOnlyList<TItem> Items { get; }
    public IReadOnlyDictionary<string, TItem> Index { get; }
    public void Save();
}

public interface IBeatmapDbService : IDbService<DbBeatmap>
{
    public void Add(DbBeatmap item);
    public void Remove(string key);
}

public interface ICollectionDbService : IDbService<Collection>
{
    public void Add(Collection item);
    public void Remove(string key);
}

public interface IScoreDbService : IDbService<List<Score>>
{

}