using OsuManiaToolbox.Core.Services;
using OsuManiaToolbox.ViewModels;
using System.Windows;

namespace OsuManiaToolbox;

public partial class BeatmapWindow : Window
{
    public FilterView Filter { get; }

    public BeatmapWindow(FilterView filterView, ITableService tableService)
    {
        InitializeComponent();

        Filter = filterView;
        Title = $"Beatmap Filter #{Filter.WindowId}";

        DataContext = this;

        tableService.SetupColumns(BeatmapDataGrid);
    }

    private void Window_Closed(object sender, EventArgs e)
    {
        Filter.Dispose();
    }

}