using Microsoft.Extensions.DependencyInjection;
using OsuManiaToolbox.Core.Services;
using OsuManiaToolbox.Infrastructure.Services;
using OsuManiaToolbox.ViewModels;
using System.Windows;

namespace OsuManiaToolbox;

public partial class App : Application
{
    public IServiceProvider Services { get; }
    public App()
    {
        Services = ConfigureServices();
        DispatcherUnhandledException += (s, e) =>
        {
            var logger = Services.GetRequiredService<ILogService>().GetLogger(this);
            logger.Exception(e.Exception);
            e.Handled = true;
        };
    }

    private static ServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ILogService, LogService>();
        services.AddSingleton<ISettingsService, SettingsService>();
        services.AddSingleton<IOsuFileService, OsuFileService>();
        services.AddSingleton<IBeatmapDbService, BeatmapDbService>();
        services.AddSingleton<IScoreDbService, ScoreDbService>();
        services.AddSingleton<ICollectionDbService, CollectionDbService>();
        services.AddSingleton<IExportService, ExportService>();
        services.AddSingleton<IBeatmapFilterService, BeatmapFilterService>();
        services.AddSingleton<IWindowService, WindowService>();

        services.AddSingleton<RegradeView>();
        services.AddSingleton<StarRatingView>();
        services.AddSingleton<FilterView>();

        services.AddTransient<BeatmapWindow>();
        services.AddTransient<MainWindow>();

        return services.BuildServiceProvider();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        try
        {
            base.OnStartup(e);
            try
            {
                Services.GetRequiredService<ISettingsService>().Load();
            }
            catch (Exception ex)
            {
                var logger = Services.GetRequiredService<ILogService>().GetLogger(this);
                logger.Warning($"加载设置失败: {ex.Message}, 使用默认设置");
            }

            var mainWindow = Services.GetRequiredService<MainWindow>();
            MainWindow = mainWindow;
            mainWindow.Show();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"应用程序启动失败: {ex.Message}\n\n{ex.StackTrace}", "启动错误",
                MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown();
        }
    }
}
