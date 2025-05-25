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

    [Description("����")]
    public int Key => (int)Bm.CircleSize;

    [Description("����ֵ: R, L, Q, P")]
    public RankedStatus Status => Bm.RankedStatus;

    [Description("������")]
    public double N => Bm.CirclesCount;

    [Description("������")]
    public double LN => Bm.SlidersCount;

    [Description("ʱ��(��)")]
    public double Length => Bm.TotalTime / 1000.0;

    [Description("��������޸���������")]
    public double ModifyDays => (DateTime.Now - Bm.LastModifiedTime).TotalDays;

    [Description("���Acc")]
    public double Acc => Scores.AccMax?.ManiaAcc() ?? double.NaN;

    [Description("���Acc��������")]
    public double AccDays => (DateTime.Now - Scores.AccMax?.ScoreTimestamp)?.Days ?? double.NaN;

    [Description("���Acc Mods")]
    public Mods AccMods => Scores.AccMax?.Mods ?? Mods.None;

    [Description("��߷���")]
    public double Score => Scores.ScoreMax?.ReplayScore ?? double.NaN;

    [Description("��߷�����������")]
    public double ScoreDays => (DateTime.Now - Scores.ScoreMax?.ScoreTimestamp)?.Days ?? double.NaN;

    [Description("����Acc")]
    public double LastAcc => Scores.Last?.ManiaAcc() ?? double.NaN;

    [Description("���·���")]
    public double LastScore => Scores.Last?.ReplayScore ?? double.NaN;

    [Description("���³ɼ���������")]
    public double LastDays => (DateTime.Now - Scores.Last?.ScoreTimestamp)?.Days ?? double.NaN;

    [Description("���³ɼ�Mods")]
    public Mods LastMods => Scores.Last?.Mods ?? Mods.None;

    [Description("��������ֵ: HT, DT, EZ, HR��֧��|(��λ��)�����")]
    public double SR(Mods mod = Mods.None)
    {
        return Bm.ManiaStarRating[mod];
    }

    [Description("���������ְ���")]
    public bool Creator(string name)
    {
        return Bm.Creator.Contains(name, StringComparison.OrdinalIgnoreCase);
    }

    [Description("�����ѶȰ���")]
    public bool Diff(string name)
    {
        return Bm.Difficulty.Contains(name, StringComparison.OrdinalIgnoreCase);
    }

    [Description("ȫ������(���⡢���ߡ������ߡ��Ѷ�)")]
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