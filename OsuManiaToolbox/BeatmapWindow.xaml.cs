using OsuManiaToolbox.ViewModels;
using System.Windows;

namespace OsuManiaToolbox;

public partial class BeatmapWindow : Window
{
    public FilterView Filter { get; }

    public BeatmapWindow(FilterView filterView)
    {
        InitializeComponent();

        Filter = filterView;

        DataContext = this;
    }
}