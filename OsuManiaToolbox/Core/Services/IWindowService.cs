using System.Collections.Generic;

namespace OsuManiaToolbox.Core.Services;

public interface IWindowService
{
    void ShowBeatmapWindow();
    void CloseAllWindows();
    IReadOnlyCollection<BeatmapWindow> OpenWindows { get; }
}