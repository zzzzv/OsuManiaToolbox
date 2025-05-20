using OsuManiaToolbox.Settings;
using OsuManiaToolbox.Core.Services;
using OsuManiaToolbox.ViewModels;
using System.Reflection;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;

namespace OsuManiaToolbox;

public partial class MainWindow : Window
{
    private readonly ILogDispatcher _logDispatcher;
    private readonly ISettingsService _settingsService;

    public CommonSettings Settings => _settingsService.GetSettings<CommonSettings>();
    public RegradeView Regrade { get; }
    public StarRatingView StarRating { get; }
    public FilterView Filter { get; }

    public MainWindow(
        ILogService logService,
        ISettingsService settingsService,
        RegradeView regradeView,
        StarRatingView starRatingView,
        FilterView filterView)
    {
        InitializeComponent();

        _logDispatcher = logService.LogDispatcher;
        _settingsService = settingsService;
        Regrade = regradeView;
        StarRating = starRatingView;
        Filter = filterView;

        DataContext = this;

        var version = Assembly.GetExecutingAssembly().GetName().Version ?? new Version(1, 0, 0);
        Title = $"osu!mania工具箱 v{version.Major}.{version.Minor}.{version.Build}";

        logTextBox.Document.Blocks.Clear();

        _logDispatcher.LogsReceived += AppendLog;

        Closed += MainWindow_Closed;
    }

    private void MainWindow_Closed(object? sender, EventArgs e)
    {
        _settingsService.Save();
        _logDispatcher.LogsReceived -= AppendLog;
    }

    private void AppendLog(IEnumerable<LogMessage> logs)
    {
        var filteredLogs = logs.Where(p => p.Level >= Settings.LogLevel).ToList();
        if (filteredLogs.Count == 0) return;

        Dispatcher.BeginInvoke(() =>
        {
            logTextBox.BeginChange();

            foreach (var log in filteredLogs)
            {
                var paragraph = new Paragraph();
                var run = new Run(log.ToString());

                switch (log.Level)
                {
                    case LogLevel.Debug:
                        run.Foreground = Brushes.Gray;
                        break;
                    case LogLevel.Info:
                        run.Foreground = Brushes.Black;
                        break;
                    case LogLevel.Warning:
                        run.Foreground = Brushes.Orange;
                        break;
                    case LogLevel.Error:
                        run.Foreground = Brushes.Red;
                        break;
                }
                paragraph.Inlines.Add(run);

                logTextBox.Document.Blocks.Add(paragraph);
            }

            logTextBox.EndChange();
            logTextBox.ScrollToEnd();
        }, DispatcherPriority.Background);
    }
}