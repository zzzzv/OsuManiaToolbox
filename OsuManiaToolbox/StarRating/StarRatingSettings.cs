

using CommunityToolkit.Mvvm.ComponentModel;

namespace OsuManiaToolbox.StarRating;

public partial class StarRatingSettings : ObservableObject
{
    [ObservableProperty]
    private bool _exclude4K = true;

    [ObservableProperty]
    private double _minSR = 3;

    [ObservableProperty]
    private bool _forceUpdate = false;

    [ObservableProperty]
    private bool _showBeatmapError = false;
}
