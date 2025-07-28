using OsuManiaToolbox.Core.Services;
using OsuManiaToolbox.Settings;
using System.IO;
using System.Reflection;
using System.Text.Json;

namespace OsuManiaToolbox.Infrastructure.Services;

public class SettingsService : ISettingsService
{
    private readonly Dictionary<Type, PropertyInfo> _settingsProperties = new();
    private SettingsRoot _root = new();

    public string SettingsFilePath { get; set; } = "OsuManiaToolBox.json";
    public JsonSerializerOptions SerializerOptions { get; } = new()
    {
        WriteIndented = true,
        IgnoreReadOnlyProperties = true,
    };

    public SettingsService()
    {
        RegisterSettingsTypes();
    }

    private void RegisterSettingsTypes()
    {
        var rootType = typeof(SettingsRoot);
        var properties = rootType.GetProperties();

        foreach (var property in properties)
        {
            _settingsProperties[property.PropertyType] = property;
        }
    }

    public T GetSettings<T>() where T : class
    {
        if (_settingsProperties.TryGetValue(typeof(T), out var property))
        {
            return (T)property.GetValue(_root)!;
        }

        throw new InvalidOperationException($"未知的设置类型: {typeof(T).Name}");
    }

    public void Save()
    {
        _root.Version = Utils.GetVersion();

        var json = JsonSerializer.Serialize(_root, SerializerOptions);
        File.WriteAllText(SettingsFilePath, json);
    }

    public void Load()
    {
        if (File.Exists(SettingsFilePath))
        {
            var json = File.ReadAllText(SettingsFilePath);
            _root = JsonSerializer.Deserialize<SettingsRoot>(json) ?? new SettingsRoot();
        }
    }

    private class SettingsRoot
    {
        public Version Version { get; set; } = Utils.GetVersion();
        public CommonSettings Common { get; set; } = new();
        public RegradeSettings Regrade { get; set; } = new();
        public StarRatingSettings StarRating { get; set; } = new();
        public FilterSettings Filter { get; set; } = new();
    }
}
