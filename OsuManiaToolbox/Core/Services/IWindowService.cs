namespace OsuManiaToolbox.Core.Services;

public interface IWindowService
{
    void ShowBeatmapWindow();
    void CloseAllWindows();
    int NextWindowId { get; }
}