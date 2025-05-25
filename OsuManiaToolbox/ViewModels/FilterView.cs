using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OsuManiaToolbox.Core;
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
    private readonly ITableService _tableService;

    public IRelayCommand CreateItem { get; }
    public IRelayCommand DeleteItem { get; }
    public IRelayCommand FilterCommand { get; }
    public IRelayCommand CreateCollection { get; }
    public IRelayCommand WriteCsv { get; }

    public FilterSettings Settings { get; }
    public DataView MetaTable { get; }
    public string DbBeatmapProperties => GetPropertyNames<DbBeatmap>();
    public string ScoreProperties => GetPropertyNames<Score>();

    [ObservableProperty]
    private FilterHistoryItem _selected;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Table))]
    private BeatmapData[] _data = [];

    public DataView Table => _tableService.Create(Data).DefaultView;

    public FilterView(ISettingsService settingsService, IBeatmapDbService beatmapDb, ILogService logService,
        IBeatmapFilterService filterService, IExportService exportService, ITableService tableService)
    {
        Settings = settingsService.GetSettings<FilterSettings>();
        _beatmapDb = beatmapDb;
        _logger = logService.GetLogger(this);
        _filterService = filterService;
        MetaTable = _filterService.MetaTable;
        _exportService = exportService;
        _tableService = tableService;

        if (!Settings.History.Any())
        {
            Settings.History.Add(new FilterHistoryItem());
        }
        Selected = Settings.History[0];

        CreateItem = new RelayCommand(CreateItemRun);
        DeleteItem = new RelayCommand(DeleteItemRun, CanDeleteItem);
        FilterCommand = new RelayCommand(FilterRun);
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

    private void FilterRun()
    {
        var mania = _beatmapDb.Items.Where(x => x.Ruleset == Ruleset.Mania).ToArray();
        _logger.Info($"共有{mania.Length}张Mania谱面");
        var result = _filterService.Filter(mania, Selected.Expression, Selected.OrderBy).Skip(Selected.Skip);
        if (Selected.Take > 0)
        {
            result = result.Take(Selected.Take);
        }
        var arr = result.ToArray();
        _logger.Info($"符合条件的谱面有{arr.Length}张");
        Selected = Settings.MoveFirst(Selected);
        Data = arr;
    }

    private void CreateCollectionRun()
    {
        try
        {
            var success = _exportService.CreateCollection(Data.Select(p => p.Bm.MD5Hash), Selected.CollectionName);
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
            var success = _exportService.ExportToCsv(Data, Selected.CsvName);
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

    private static string GetPropertyNames<T>()
    {
        return string.Join(", ", typeof(T).GetProperties().Select(p => p.Name));
    }
}