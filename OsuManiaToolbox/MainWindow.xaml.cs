using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using OsuManiaToolbox.Regrade;
using OsuManiaToolbox.StarRating;
using System.Reflection;

namespace OsuManiaToolbox;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public string AppVersion => Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0";

    public Settings Settings { get; }
    public RegradeView Regrade { get; }
    public StarRatingView StarRating { get; }

    public MainWindow()
    {
        InitializeComponent();
        Settings = Settings.Load();
        Regrade = new RegradeView(Settings, new Logger(AppendLog, "Regrade"));
        StarRating = new StarRatingView(Settings, new Logger(AppendLog, "StarRating"));

        DataContext = this;

        var version = Assembly.GetExecutingAssembly().GetName().Version ?? new Version(1, 0, 0);
        Title = $"osu!mania工具箱 v{version.Major}.{version.Minor}.{version.Build}";

        logTextBox.Document.Blocks.Clear();

        Closed += (s, e) => Settings.Save();
    }

    private void AppendLog(LogMessage log)
    {
        Dispatcher.Invoke(() =>
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
            logTextBox.ScrollToEnd();
        });
    }
}