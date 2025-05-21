using OsuParsers.Database.Objects;
using System.Data;

namespace OsuManiaToolbox.Core.Services;

public interface IBeatmapFilterService
{
    IEnumerable<DbBeatmap> Filter(string expression, IEnumerable<DbBeatmap> beatmaps, string order);
    DataView MetaTable { get; }
}