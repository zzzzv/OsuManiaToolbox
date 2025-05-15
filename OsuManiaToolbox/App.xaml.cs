using Microsoft.Extensions.DependencyInjection;
using OsuManiaToolbox.Services;
using OsuManiaToolbox.Settings;
using OsuManiaToolbox.StarRating;
using OsuManiaToolbox.ViewModels;
using System.Windows;

namespace OsuManiaToolbox;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public static new App Current => (App)Application.Current;
    public IServiceProvider Services { get; }
    public App()
    {
        Services = ConfigureServices();
        DispatcherUnhandledException += (s, e) =>
        {
            var logger = Services.GetRequiredService<ILogger<App>>();
            logger.Exception(e.Exception);
            e.Handled = true;
        };
    }

    private static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ILogDispatcher, LogDispatcher>();
        services.AddTransient(typeof(ILogger<>), typeof(Logger<>));

        services.AddSingleton<SettingsService>();
        services.AddSingleton(sp => sp.GetRequiredService<SettingsService>().Common);
        services.AddSingleton(sp=> sp.GetRequiredService<SettingsService>().Regrade);
        services.AddSingleton(sp => sp.GetRequiredService<SettingsService>().StarRating);

        services.AddSingleton<OsuFileService>();
        services.AddSingleton<IBeatmapDbService, BeatmapDbService>();
        services.AddSingleton<IScoreDbService, ScoreDbService>();

        services.AddSingleton<RegradeView>();
        services.AddSingleton<StarRatingView>();

        return services.BuildServiceProvider();
    }
}
