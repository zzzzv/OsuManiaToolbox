using OsuParsers.Database;
using OsuParsers.Database.Objects;
using OsuParsers.Decoders;

namespace OsuManiaToolbox.Core;

public interface IDbAdapter<TDatabase, TItem> where TDatabase : class
{
    TDatabase Load(string filePath);
    void Save(TDatabase database, string filePath);
    Dictionary<string, TItem> CreateIndex(TDatabase database);
    List<TItem> GetList(TDatabase database);
}

public class BeatmapDbAdapter : IDbAdapter<OsuDatabase, DbBeatmap>
{
    public OsuDatabase Load(string filePath)
    {
        return DatabaseDecoder.DecodeOsu(filePath);
    }
    public void Save(OsuDatabase database, string filePath)
    {
        database.Save(filePath);
    }
    public Dictionary<string, DbBeatmap> CreateIndex(OsuDatabase database)
    {
        return database.Beatmaps.ToDictionary(x => x.MD5Hash, x => x);
    }
    public List<DbBeatmap> GetList(OsuDatabase database)
    {
        return database.Beatmaps;
    }
}

public class ScoreDbAdapter : IDbAdapter<ScoresDatabase, List<Score>>
{
    public ScoresDatabase Load(string filePath)
    {
        return DatabaseDecoder.DecodeScores(filePath);
    }
    public void Save(ScoresDatabase database, string filePath)
    {
        database.Save(filePath);
    }
    public Dictionary<string, List<Score>> CreateIndex(ScoresDatabase database)
    {
        return database.Scores.ToDictionary(x => x.Item1, x => x.Item2);
    }
    public List<List<Score>> GetList(ScoresDatabase database)
    {
        return database.Scores.Select(x => x.Item2).ToList();
    }
}

public class CollectionDbAdapter : IDbAdapter<CollectionDatabase, Collection>
{
    public CollectionDatabase Load(string filePath)
    {
        return DatabaseDecoder.DecodeCollection(filePath);
    }
    public void Save(CollectionDatabase database, string filePath)
    {
        database.Save(filePath);
    }
    public Dictionary<string, Collection> CreateIndex(CollectionDatabase database)
    {
        return database.Collections.ToDictionary(x => x.Name, x => x);
    }
    public List<Collection> GetList(CollectionDatabase database)
    {
        return database.Collections;
    }
}