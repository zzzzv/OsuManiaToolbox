using OsuManiaToolbox.Core;
using OsuManiaToolbox.Core.Services;
using OsuParsers.Enums;
using System.Data;
using System.Windows.Controls;
using System.Windows.Data;

namespace OsuManiaToolbox.Infrastructure.Services;

public class TableService : ITableService
{
    private record BeatmapColumn(string Name, Func<BeatmapData, object?> Func, Type Type, string Format);

    private readonly List<BeatmapColumn> _columns = [];

    public TableService()
    {
        AddStringColumn("Title", x => x.Bm.TitleUnicode);
        AddStringColumn("Artist", x => x.Bm.ArtistUnicode);
        AddStringColumn("Creator", x => x.Bm.Creator);
        AddStringColumn("Difficulty", x => x.Bm.Difficulty);
        AddIntColumn("Key", x => (int)x.Bm.CircleSize);
        AddIntColumn("N", x => x.Bm.CirclesCount);
        AddIntColumn("LN", x => x.Bm.SlidersCount);
        AddDoubleColumn("LN%", x => (double)x.Bm.SlidersCount * 100 / (x.Bm.SlidersCount + x.Bm.CirclesCount));
        AddTimeSpanColumn("Length", x => TimeSpan.FromMilliseconds(x.Bm.TotalTime));
        AddDoubleColumn("SR", x => x.Bm.ManiaStarRating[Mods.None]);
        AddDoubleColumn("SR(HT)", x => x.Bm.ManiaStarRating[Mods.HalfTime]);
        AddDoubleColumn("SR(DT)", x => x.Bm.ManiaStarRating[Mods.DoubleTime]);
        AddDoubleColumn("SR(EZ)", x => x.Bm.ManiaStarRating[Mods.Easy]);
        AddDoubleColumn("SR(HT&EZ)", x => x.Bm.ManiaStarRating[Mods.HalfTime | Mods.Easy]);
        AddDoubleColumn("SR(DT&EZ)", x => x.Bm.ManiaStarRating[Mods.DoubleTime | Mods.Easy]);
        AddDateTimeColumn("LastModify", x => x.Bm.LastModifiedTime);
        AddDoubleColumn("MaxAcc", x => x.Scores.AccMax?.ManiaAcc());
        AddDateTimeColumn("MaxAccTime", x => x.Scores.AccMax?.ScoreTimestamp);
        AddStringColumn("MaxAccMods", x => x.Scores.AccMax?.Mods.Acronyms());
        AddIntColumn("MaxScore", x => x.Scores.ScoreMax?.ReplayScore);
        AddDateTimeColumn("MaxScoreTime", x => x.Scores.ScoreMax?.ScoreTimestamp);
        AddDoubleColumn("LastAcc", x => x.Scores.Last?.ManiaAcc());
        AddIntColumn("LastScore", x => x.Scores.Last?.ReplayScore);
        AddDateTimeColumn("LastTime", x => x.Scores.Last?.ScoreTimestamp);
        AddStringColumn("LastMods", x => x.Scores.Last?.Mods.Acronyms());
        AddIntColumn("ScoreCount", x => x.Scores.Count);
        AddStringColumn("Link", x => $"https://osu.ppy.sh/beatmapsets/{x.Bm.BeatmapSetId}#mania/{x.Bm.BeatmapId}");
    }

    public DataTable Create(IEnumerable<BeatmapData> beatmaps)
    {
        var table = new DataTable();
        foreach (var column in _columns)
        {
            table.Columns.Add(column.Name, column.Type);
        }
        foreach (var beatmap in beatmaps)
        {
            var row = table.NewRow();
            foreach (var column in _columns)
            {
                row[column.Name] = column.Func(beatmap) ?? DBNull.Value;
            }
            table.Rows.Add(row);
        }
        return table;
    }

    public void SetupColumns(DataGrid dataGridView)
    {
        dataGridView.Columns.Clear();

        foreach (var column in _columns)
        {
            var gridColumn = new DataGridTextColumn
            {
                Header = column.Name,
                Binding = new Binding(column.Name)
                {
                    StringFormat = column.Format,
                }
            };
            dataGridView.Columns.Add(gridColumn);
        }
    }

    private void AddStringColumn(string name, Func<BeatmapData, string?> func)
    {
        _columns.Add(new BeatmapColumn(name, func, typeof(string), string.Empty));
    }

    private void AddIntColumn(string name, Func<BeatmapData, object?> func)
    {
        _columns.Add(new BeatmapColumn(name, func, typeof(int), "N0"));
    }

    private void AddDoubleColumn(string name, Func<BeatmapData, object?> func)
    {
        _columns.Add(new BeatmapColumn(name, func, typeof(double), "F2"));
    }

    private void AddDateTimeColumn(string name, Func<BeatmapData, object?> func)
    {
        _columns.Add(new BeatmapColumn(name, func, typeof(DateTime), "yy-MM-dd HH:mm"));
    }

    private void AddTimeSpanColumn(string name, Func<BeatmapData, object?> func)
    {
        _columns.Add(new BeatmapColumn(name, func, typeof(TimeSpan), @"hh\:mm\:ss"));
    }
}
