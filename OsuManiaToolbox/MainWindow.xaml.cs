using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using OsuManiaToolbox.Regrade;

namespace OsuManiaToolbox;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public Settings Settings { get; }
    public RegradeView Regrade { get; }

    public MainWindow()
    {
        InitializeComponent();
        Settings = Settings.Load();
        Regrade = new RegradeView(Settings, new Logger(AppendLog, "Regrade"));

        DataContext = this;

        Closed += (s, e) => Settings.Save();
    }

    private void AppendLog(LogMessage log)
    {
        Dispatcher.Invoke(() =>
        {
            var paragraph = new Paragraph();
            var run = new Run($"[{DateTime.Now:HH:mm:ss}] [{log.Source}] {log.Message}");

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