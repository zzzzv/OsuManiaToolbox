namespace OsuManiaToolbox.Core.Services;

public interface ISettingsService
{
    T GetSettings<T>() where T : class;
    void Load();
    void Save();
}