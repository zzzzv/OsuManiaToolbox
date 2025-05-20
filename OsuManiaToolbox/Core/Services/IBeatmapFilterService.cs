using OsuParsers.Database.Objects;

namespace OsuManiaToolbox.Core.Services;

public interface IBeatmapFilterService
{
    IEnumerable<DbBeatmap> Filter(string expression, IEnumerable<DbBeatmap> beatmaps);
}