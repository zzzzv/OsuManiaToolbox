using System.IO;
using OsuManiaToolbox.Core.Services;
using OsuManiaToolbox.Settings;
using OsuParsers.Database.Objects;

namespace OsuManiaToolbox.Core.Services;

public class OsuFileService(ISettingsService settingsService) : IOsuFileService
{
    private readonly CommonSettings _settings = settingsService.GetSettings<CommonSettings>();

    public string BeatmapDbPath => Path.Combine(_settings.OsuPath, "osu!.db");
    public string ScoreDbPath => Path.Combine(_settings.OsuPath, "scores.db");
    public string CollectionDbPath => Path.Combine(_settings.OsuPath, "collection.db");

    public string GetBeatmapPath(DbBeatmap beatmap)
    {
        return Path.Combine(_settings.OsuPath, "Songs", beatmap.FolderName, beatmap.FileName);
    }
}
