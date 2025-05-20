using OsuManiaToolbox.Core.Services;
using OsuParsers.Database.Objects;
using OsuParsers.Enums;
using OsuParsers.Enums.Database;
using System.Data;

namespace OsuManiaToolbox.Infrastructure;

public class FilterContext(DbBeatmap bm, IScoreDbService scoreDb)
{
    public Mods HT => Mods.HalfTime;
    public Mods DT => Mods.DoubleTime;
    public Mods EZ => Mods.Easy;
    public Mods HR => Mods.HardRock;
    public RankedStatus R => RankedStatus.Ranked;
    public RankedStatus L => RankedStatus.Loved;
    public RankedStatus Q => RankedStatus.Qualified;
    public RankedStatus P => RankedStatus.Pending;

    public DbBeatmap Bm { get; } = bm;
    public List<Score> Scores { get; } = scoreDb.Index.TryGetValue(bm.MD5Hash, out var scoreList) ? scoreList : [];

    public int Key => (int)Bm.CircleSize;
    public RankedStatus Status => Bm.RankedStatus;
    public double N => Bm.CirclesCount;
    public double LN => Bm.SlidersCount;
    public double MaxAcc => Scores.Max(x => x.ManiaAcc());
    public int MaxScore => Scores.Max(x => x.ReplayScore);
    public double LastAcc => Scores.LastOrDefault()?.ManiaAcc() ?? 0.0;
    public int LastScore => Scores.LastOrDefault()?.ReplayScore ?? 0;
    public int ScoreCount => Scores.Count;

    public double SR(Mods mods = Mods.None)
    {
        return Bm.ManiaStarRating[mods];
    }

    public static DataView DescriptionTable
    {
        get
        {
            var table = new DataTable();
            table.Columns.Add("Name", typeof(string));
            table.Columns.Add("Type", typeof(string));
            table.Columns.Add("Comment", typeof(string));
            table.Rows.Add("Key", "int", "");
            table.Rows.Add("SR()", "double", "参数可用值: HT, DT, EZ, HR。支持&运算符");
            table.Rows.Add("Status", "RankedStatus", "可用值: R, L, Q, P");
            table.Rows.Add("N", "double", "米数量");
            table.Rows.Add("LN", "double", "面数量");
            table.Rows.Add("MaxAcc", "double", "最高Acc");
            table.Rows.Add("MaxScore", "int", "最高分数");
            table.Rows.Add("LastAcc", "double", "最新Acc");
            table.Rows.Add("LastScore", "int", "最新分数");
            table.Rows.Add("ScoreCount", "int", "成绩数量");
            table.Rows.Add("BM", "DbBeatmap", "当前谱面对象");
            table.Rows.Add("Scores", "List<Score>", "当前谱面成绩列表");
            return table.AsDataView();
        }
    }

    public static string DbBeatmapDescription
    {
        get
        {
            var props = typeof(DbBeatmap).GetProperties();
            return string.Join(", ", props.Select(p => p.Name));
        }
    }

    public static string ScoreDescription
    {
        get
        {
            var props = typeof(Score).GetProperties();
            return string.Join(", ", props.Select(p => p.Name));
        }
    }
}