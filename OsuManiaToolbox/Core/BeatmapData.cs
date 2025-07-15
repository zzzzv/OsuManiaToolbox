using OsuManiaToolbox.Core.Services;
using OsuParsers.Database.Objects;
using System.ComponentModel;

namespace OsuManiaToolbox.Core;

public class BeatmapData(DbBeatmap beatmap, List<Score> scores)
{
    [Description("谱面对象")]
    public DbBeatmap Bm { get; } = beatmap;

    public ScoresData Scores { get; } = new(scores);

    public BeatmapData(DbBeatmap beatmap, IScoreDbService scoreDb)
        : this(beatmap, scoreDb.Index.TryGetValue(beatmap.MD5Hash, out var scoreList) ? scoreList : []) { }
}

public class ScoresData(List<Score> scores)
{
    [Description("成绩列表")]
    public List<Score> List { get; } = scores;

    [Description("可空。Acc最高的成绩")]
    public Score? AccMax => List.MaxBy(x => x.ManiaAcc());

    [Description("可空。分数最高的成绩")]
    public Score? ScoreMax => List.MaxBy(x => x.ReplayScore);

    [Description("可空。最新成绩")]
    public Score? Last => List.MaxBy(x => x.ScoreTimestamp);

    [Description("成绩数量")]
    public int Count => List.Count;
}