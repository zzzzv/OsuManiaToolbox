using OsuParsers.Database.Objects;

namespace OsuManiaToolbox.Core.Services;

public interface IOsuFileService
{
    string BeatmapDbPath { get; }
    string CollectionDbPath { get; }
    string ScoreDbPath { get; }

    string GetBeatmapPath(DbBeatmap beatmap);
}