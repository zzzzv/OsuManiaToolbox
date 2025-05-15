using System.IO;
using OsuManiaToolbox.Settings;
using OsuParsers.Database.Objects;

namespace OsuManiaToolbox.Services;

public class OsuFileService(CommonSettings settings)
{
    private readonly CommonSettings _settings = settings;

    public string BeatmapDbPath => Path.Combine(_settings.OsuPath, "osu!.db");
    public string ScoreDbPath => Path.Combine(_settings.OsuPath, "scores.db");
    public string CollectionDbPath => Path.Combine(_settings.OsuPath, "collection.db");

    public string GetBeatmapPath(DbBeatmap beatmap)
    {
        return Path.Combine(_settings.OsuPath, "Songs", beatmap.FolderName, beatmap.FileName);
    }
}
