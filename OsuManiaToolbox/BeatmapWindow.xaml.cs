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

        DataContext = this;

        tableService.SetupColumns(BeatmapDataGrid);
    }
}