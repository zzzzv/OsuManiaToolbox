using OsuManiaToolbox.Core;
using OsuManiaToolbox.Core.Services;
using OsuParsers.Database.Objects;
using OsuParsers.Enums;
using OsuParsers.Enums.Database;
using System.ComponentModel;
using System.Data;

namespace OsuManiaToolbox.Infrastructure;

public class FilterContext : BeatmapData
{
    public FilterContext(DbBeatmap bm, List<Score> scores) : base(bm, scores) { }

    public FilterContext(DbBeatmap bm, IScoreDbService scoreDb) : base(bm, scoreDb) { }

    public FilterContext(BeatmapData bm) : base(bm.Bm, bm.Scores.List) { }

    public Mods HT => Mods.HalfTime;
    public Mods DT => Mods.DoubleTime;
    public Mods EZ => Mods.Easy;
    public Mods HR => Mods.HardRock;
    public RankedStatus R => RankedStatus.Ranked;
    public RankedStatus L => RankedStatus.Loved;
    public RankedStatus Q => RankedStatus.Qualified;
    public RankedStatus P => RankedStatus.Pending;

    [Description("键数")]
    public int Key => (int)Bm.CircleSize;

    [Description("可用值: R, L, Q, P")]
    public RankedStatus Status => Bm.RankedStatus;

    [Description("米数量")]
    public double N => Bm.CirclesCount;

    [Description("面数量")]
    public double LN => Bm.SlidersCount;

    [Description("时长(秒)")]
    public double Length => Bm.TotalTime / 1000.0;

    [Description("谱面最后修改至今天数")]
    public double ModifyDays => (DateTime.Now - Bm.LastModifiedTime).TotalDays;

    [Description("最高Acc")]
    public double Acc => Scores.AccMax?.ManiaAcc() ?? double.NaN;

    [Description("最高Acc至今天数")]
    public double AccDays => (DateTime.Now - Scores.AccMax?.ScoreTimestamp)?.Days ?? double.NaN;

    [Description("最高Acc Mods")]
    public Mods AccMods => Scores.AccMax?.Mods ?? Mods.None;

    [Description("最高分数")]
    public double Score => Scores.ScoreMax?.ReplayScore ?? double.NaN;

    [Description("最高分数至今天数")]
    public double ScoreDays => (DateTime.Now - Scores.ScoreMax?.ScoreTimestamp)?.Days ?? double.NaN;

    [Description("最新Acc")]
    public double LastAcc => Scores.Last?.ManiaAcc() ?? double.NaN;

    [Description("最新分数")]
    public double LastScore => Scores.Last?.ReplayScore ?? double.NaN;

    [Description("最新成绩至今天数")]
    public double LastDays => (DateTime.Now - Scores.Last?.ScoreTimestamp)?.Days ?? double.NaN;

    [Description("最新成绩Mods")]
    public Mods LastMods => Scores.Last?.Mods ?? Mods.None;

    [Description("参数可用值: HT, DT, EZ, HR。支持|(按位或)运算符")]
    public double SR(Mods mod = Mods.None)
    {
        return Bm.ManiaStarRating[mod];
    }

    [Description("创建者名字包含")]
    public bool Creator(string name)
    {
        return Bm.Creator.Contains(name, StringComparison.OrdinalIgnoreCase);
    }

    [Description("谱面难度包含")]
    public bool Diff(string name)
    {
        return Bm.Difficulty.Contains(name, StringComparison.OrdinalIgnoreCase);
    }

    [Description("全文搜索(标题、作者、创建者、难度)")]
    public bool Text(string text)
    {
        return new string[] { Bm.Title, Bm.TitleUnicode, Bm.Artist, Bm.ArtistUnicode, Bm.Creator, Bm.Difficulty }
            .Any(x => x.Contains(text, StringComparison.OrdinalIgnoreCase));
    }

    public static DataView MetaTable
    {
        get
        {
            var table = MetaCreator.Create<FilterContext>();
            var scoresTable = MetaCreator.Create<ScoresData>();
            foreach (DataRow row in scoresTable.Rows)
            {
                row["Name"] = "Scores." + row["Name"];
            }
            table.Merge(scoresTable);
            return table.AsDataView();
        }
    }
}