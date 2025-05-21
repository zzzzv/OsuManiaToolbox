using CommunityToolkit.Mvvm.ComponentModel;
using OsuManiaToolbox.Core.Services;
using System.Text.Json.Serialization;

namespace OsuManiaToolbox.Settings;

public partial class CommonSettings : ObservableObject
{
    [ObservableProperty]
    private string _osuPath = "c:\\osu!";

    [ObservableProperty]
    private bool _backupDb = true;

    [ObservableProperty]
    private LogLevel _logLevel = LogLevel.Info;

    [JsonIgnore]
    public IReadOnlyList<LogLevel> LogLevelAll => Enum.GetValues(typeof(LogLevel)).Cast<LogLevel>().ToArray();
}
