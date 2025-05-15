using OsuManiaToolbox.Settings;
using OsuManiaToolbox.StarRating;
using System.IO;
using System.Text.Json;

namespace OsuManiaToolbox.Services;

public class SettingsService
{
    public string SettingsFilePath { get; set; } = "OsuManiaToolBox.json";
    public CommonSettings Common { get; set; } = new();
    public RegradeSettings Regrade { get; set; } = new();
    public StarRatingSettings StarRating { get; set; } = new();

    public SettingsService()
    {
        Load();
    }

    public void Save()
    {
        var settingsRoot = new SettingsRoot
        {
            Common = Common,
            Regrade = Regrade,
            StarRating = StarRating,
        };
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            IgnoreReadOnlyProperties = true,
        };
        var json = JsonSerializer.Serialize(this, options);
        File.WriteAllText(SettingsFilePath, json);
    }

    public void Load()
    {
        if (File.Exists(SettingsFilePath))
        {
            var json = File.ReadAllText(SettingsFilePath);
            var settingsRoot = JsonSerializer.Deserialize<SettingsRoot>(json);
            if (settingsRoot != null)
            {
                Common = settingsRoot.Common;
                Regrade = settingsRoot.Regrade;
                StarRating = settingsRoot.StarRating;
            }
        }
    }

    private class SettingsRoot
    {
        public CommonSettings Common { get; set; } = new();
        public RegradeSettings Regrade { get; set; } = new();
        public StarRatingSettings StarRating { get; set; } = new();
    }
}
