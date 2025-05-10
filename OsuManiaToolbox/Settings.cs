using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OsuParsers.Database;
using OsuParsers.Decoders;
using OsuParsers.Enums;
using OsuParsers.Enums.Database;
using OsuManiaToolbox.Regrade;
using OsuParsers.Database.Objects;
using OsuManiaToolbox.StarRating;

namespace OsuManiaToolbox;

public partial class Settings : ObservableObject
{
    public static string SettingsFilePath { get; set; } = "OsuManiaToolBox.json";

    [ObservableProperty]
    private string _osuPath = "c:\\osu!";

    [ObservableProperty]
    private bool _backupDb = true;

    public string OsuDbPath => Path.Combine(OsuPath, "osu!.db");
    public string ScoreDbPath => Path.Combine(OsuPath, "scores.db");
    public string CollectionDbPath => Path.Combine(OsuPath, "collection.db");

    public RegradeSettings Regrade { get; set; } = new();
    public StarRatingSettings StarRating { get; set; } = new();

    public string GetBeatmapPath(DbBeatmap beatmap)
    {
        return Path.Combine(OsuPath, "Songs", beatmap.FolderName, beatmap.FileName);
    }

    public void Save()
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            IgnoreReadOnlyProperties = true,
        };
        var json = JsonSerializer.Serialize(this, options);
        File.WriteAllText(SettingsFilePath, json);
    }

    public static Settings Load()
    {
        if (!File.Exists(SettingsFilePath))
        {
            return new Settings();
        }
        var json = File.ReadAllText(SettingsFilePath);
        return JsonSerializer.Deserialize<Settings>(json) ?? new Settings();
    }
}
