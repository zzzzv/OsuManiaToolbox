using CommunityToolkit.Mvvm.ComponentModel;

namespace OsuManiaToolbox.Settings;

public partial class StarRatingSettings : ObservableObject
{
    [ObservableProperty]
    private bool _forceUpdate = false;
}
