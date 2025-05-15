using CommunityToolkit.Mvvm.ComponentModel;
using OsuManiaToolbox.Services;

namespace OsuManiaToolbox.Settings;

public partial class CommonSettings : ObservableObject
{
    [ObservableProperty]
    private string _osuPath = "c:\\osu!";

    [ObservableProperty]
    private bool _backupDb = true;

    public static LogLevel[] LogLevels => Enum.GetValues(typeof(LogLevel)).Cast<LogLevel>().ToArray();

    [ObservableProperty]
    private LogLevel _logLevel = LogLevel.Info;
}
