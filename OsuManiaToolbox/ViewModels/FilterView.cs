using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OsuManiaToolbox.Core.Services;
using OsuManiaToolbox.Settings;
using OsuParsers.Database.Objects;
using OsuParsers.Enums;
using System.Data;

namespace OsuManiaToolbox.ViewModels;

public partial class FilterView : ObservableObject
{
    private readonly IBeatmapDbService _beatmapDb;
    private readonly ILogger _logger;
    private readonly IBeatmapFilterService _filterService;
    private readonly IExportService _exportService;

    public IRelayCommand CreateItem { get; }
    public IRelayCommand DeleteItem { get; }
    public IRelayCommand CreateCollection { get; }
    public IRelayCommand WriteCsv { get; }

    public FilterSettings Settings { get; }
    public DataView MetaTable { get; }
    public string DbBeatmapProperties => GetPropertyNames<DbBeatmap>();
    public string ScoreProperties => GetPropertyNames<Score>();

    [ObservableProperty]
    private FilterHistoryItem _selected;

    public FilterView(ISettingsService settingsService, IBeatmapDbService beatmapDb, ILogService logService,
        IBeatmapFilterService filterService, IExportService exportService)
    {
        Settings = settingsService.GetSettings<FilterSettings>();
        _beatmapDb = beatmapDb;
        _logger = logService.GetLogger(this);
        _filterService = filterService;
        MetaTable = _filterService.MetaTable;
        _exportService = exportService;

        if (!Settings.History.Any())
        {
            Settings.History.Add(new FilterHistoryItem());
        }
        Selected = Settings.History[0];

        CreateItem = new RelayCommand(CreateItemRun);
        DeleteItem = new RelayCommand(DeleteItemRun, CanDeleteItem);
        CreateCollection = new RelayCommand(CreateCollectionRun);
        WriteCsv = new RelayCommand(WriteCsvRun);
    }

    private void CreateItemRun()
    {
        var newItem = new FilterHistoryItem();
        Settings.History.Insert(0, newItem);
        Selected = newItem;
    }

    private void DeleteItemRun()
    {
        if (Selected != null && Settings.History.Contains(Selected))
        {
            var index = Settings.History.IndexOf(Selected);
            Settings.History.Remove(Selected);

            if (Settings.History.Count > 0)
            {
                index = Math.Min(index, Settings.History.Count - 1);
                Selected = Settings.History[index];
            }
            else
            {
                var newItem = new FilterHistoryItem();
                Settings.History.Add(newItem);
                Selected = newItem;
            }
        }
    }

    private bool CanDeleteItem()
    {
        return Selected != null && Settings.History.Count > 1;
    }

    private void CreateCollectionRun()
    {
        try
        {
            var result = Filter();
            var success = _exportService.CreateCollection(result, Selected.CollectionName);
            if (success)
            {
                Selected = Settings.MoveFirst(Selected);
            }
        }
        catch (Exception ex)
        {
            _logger.Exception(ex);
        }
    }

    private void WriteCsvRun()
    {
        try
        {
            var result = Filter();
            var success = _exportService.ExportToCsv(result, Selected.CsvName);
            if (success)
            {
                Selected = Settings.MoveFirst(Selected);
            }
        }
        catch (Exception ex)
        {
            _logger.Exception(ex);
        }
    }

    private DbBeatmap[] Filter()
    {
        var beatmaps = _beatmapDb.Items.Where(x => x.Ruleset == Ruleset.Mania).ToArray();
        _logger.Info($"共有{beatmaps.Length}张Mania谱面");
        var result = _filterService.Filter(Selected.Expression, beatmaps, Selected.OrderBy).Skip(Selected.Skip);
        if (Selected.Take != null)
        {
            result = result.Take(Selected.Take.Value);
        }
        var arr = result.ToArray();
        _logger.Info($"符合条件的谱面有{arr.Length}张");
        Selected = Settings.MoveFirst(Selected);
        return arr;
    }

    private static string GetPropertyNames<T>()
    {
        return string.Join(", ", typeof(T).GetProperties().Select(p => p.Name));
    }
}