using Microsoft.Extensions.DependencyInjection;
using OsuManiaToolbox.Core.Services;

namespace OsuManiaToolbox.Infrastructure.Services;

public class WindowService(IServiceProvider serviceProvider) : IWindowService
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly List<BeatmapWindow> _openWindows = [];
    private int _windowid = 0;

    public int NextWindowId => ++_windowid;

    public void ShowBeatmapWindow()
    {
        var window = _serviceProvider.GetRequiredService<BeatmapWindow>();
        _openWindows.Add(window);
        
        window.Closed += (s, args) =>
        {
            if (s is BeatmapWindow beatmapWindow)
            {
                _openWindows.Remove(beatmapWindow);
            }
        };
        
        window.Show();
    }

    public void CloseAllWindows()
    {
        foreach (var window in _openWindows.ToList())
        {
            window.Close();
        }
        _openWindows.Clear();
    }
}