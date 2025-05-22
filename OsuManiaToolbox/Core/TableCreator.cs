using OsuParsers.Enums;
using System.Data;

namespace OsuManiaToolbox.Core;

public class BeatmapColumn(string name, Func<BeatmapData, string> func)
{
    public string Name { get; } = name;
    public Func<BeatmapData, string> Func { get; } = func;
    public string Description { get; set; } = string.Empty;
    public bool Show { get; set; } = true;
}

public class TableCreator
{
    private readonly List<BeatmapColumn> _columns;

    public TableCreator()
    {
        _columns =
        [
            new("Title", x => x.Bm.TitleUnicode),
            new("Artist", x => x.Bm.ArtistUnicode),
            new("Creator", x => x.Bm.Creator),
            new("Difficulty", x => x.Bm.Difficulty),
            new("Key", x => x.Bm.CircleSize.ToString("0")),
            new("N", x => x.Bm.CirclesCount.ToString()),
            new("LN", x => x.Bm.SlidersCount.ToString()),
            new("LN%", x => ((double)x.Bm.SlidersCount * 100 / (x.Bm.SlidersCount + x.Bm.CirclesCount)).ToString("F1")),
            new("Length", x => TimeSpan.FromMilliseconds(x.Bm.TotalTime).ToString("g")),
            new("SR", x => x.Bm.ManiaStarRating[Mods.None].ToString("F2")),
            new("SR(HT)", x => x.Bm.ManiaStarRating[Mods.HalfTime].ToString("F2")),
            new("SR(DT)", x => x.Bm.ManiaStarRating[Mods.DoubleTime].ToString("F2")),
            new("SR(EZ)", x => x.Bm.ManiaStarRating[Mods.Easy].ToString("F2")),
            new("SR(HT&EZ)", x => x.Bm.ManiaStarRating[Mods.HalfTime | Mods.Easy].ToString("F2")),
            new("SR(DT&EZ)", x => x.Bm.ManiaStarRating[Mods.DoubleTime | Mods.Easy].ToString("F2")),
            new("LastModify", x => x.Bm.LastModifiedTime.ToString("g")),
            new("MaxAcc", x => x.Scores.AccMax?.ManiaAcc().ToString("F2") ?? string.Empty),
            new("MaxAccTime", x => x.Scores.AccMax?.ScoreTimestamp.ToString("g") ?? string.Empty),
            new("MaxScore", x => x.Scores.ScoreMax?.ReplayScore.ToString("N0") ?? string.Empty),
            new("MaxScoreTime", x => x.Scores.ScoreMax?.ScoreTimestamp.ToString("g") ?? string.Empty),
            new("LastAcc", x => x.Scores.Last?.ManiaAcc().ToString("F2") ?? string.Empty),
            new("LastScore", x => x.Scores.Last?.ReplayScore.ToString("N0") ?? string.Empty),
            new("LastTime", x => x.Scores.Last?.ScoreTimestamp.ToString("g") ?? string.Empty),
            new("ScoreCount", x => x.Scores.Count.ToString()),
            new("Link", x => $"https://osu.ppy.sh/beatmapsets/{x.Bm.BeatmapSetId}#mania/{x.Bm.BeatmapId}"),
        ];
    }

    public DataTable Create(IEnumerable<BeatmapData> beatmaps)
    {
        var table = new DataTable();
        foreach (var column in _columns)
        {
            table.Columns.Add(column.Name, typeof(string));
        }
        foreach (var beatmap in beatmaps)
        {
            var row = table.NewRow();
            foreach (var column in _columns)
            {
                row[column.Name] = column.Func(beatmap);
            }
            table.Rows.Add(row);
        }
        return table;
    }
}
