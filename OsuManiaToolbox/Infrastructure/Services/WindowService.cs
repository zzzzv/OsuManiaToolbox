using Microsoft.Extensions.DependencyInjection;
using OsuManiaToolbox.Core.Services;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace OsuManiaToolbox.Core.Services;

public class WindowService : IWindowService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly List<BeatmapWindow> _openWindows = new List<BeatmapWindow>();

    public IReadOnlyCollection<BeatmapWindow> OpenWindows => _openWindows.AsReadOnly();

    public WindowService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

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