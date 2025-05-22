using OsuParsers.Database.Objects;
using System.Data;

namespace OsuManiaToolbox.Core.Services;

public interface IBeatmapFilterService
{
    IEnumerable<BeatmapData> Filter(IEnumerable<DbBeatmap> beatmaps, string expression, string order);
    IEnumerable<BeatmapData> Filter(IEnumerable<BeatmapData> beatmaps, string expression, string order);
    DataView MetaTable { get; }
}