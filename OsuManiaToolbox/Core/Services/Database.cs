using OsuParsers.Database.Objects;
using System.Diagnostics.CodeAnalysis;

namespace OsuManiaToolbox.Core.Services;

public interface IBeatmapDbService : IReadOnlyList<DbBeatmap>
{
    public DbBeatmap this[string hash] { get; }
    public void Save();
    public void Reload();
}

public interface ICollectionDbService : IReadOnlyList<Collection>
{
    public Collection this[string name] { get; }
    public bool Contains(string name);
    public void Add(Collection collection);
    public void Remove(string name);
    public void Save();
    public void Reload();
}

public interface IScoreDbService : IReadOnlyList<List<Score>>
{
    public List<Score> this[string hash] { get; }
    public bool TryGetValue(string hash, [MaybeNullWhen(false)] out List<Score> scores);
    public void Reload();
}