using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CsvHelper;
using System.Text;
using DynamicExpresso;
using OsuManiaToolbox.Core.Services;
using OsuManiaToolbox.Settings;
using OsuParsers.Database.Objects;
using OsuParsers.Enums;
using OsuParsers.Enums.Database;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace OsuManiaToolbox.ViewModels;

public partial class FilterView : ObservableObject
{
    private readonly IBeatmapDbService _beatmapDb;
    private readonly IScoreDbService _scoreDb;
    private readonly ICollectionDbService _collectionDb;
    private readonly ILogger _logger;
    private readonly IBeatmapFilterService _filterService;
    private readonly IExportService _exportService;

    public IRelayCommand CreateItem { get; }
    public IRelayCommand DeleteItem { get; }
    public IRelayCommand CreateCollection { get; }
    public IRelayCommand WriteCsv { get; }

    public FilterSettings Settings { get; }

    [ObservableProperty]
    private FilterHistoryItem _selected;

    public FilterView(ISettingsService settingsService, IBeatmapDbService beatmapDb, IScoreDbService scoreDb, 
        ICollectionDbService collectionDb, ILogService logService, IBeatmapFilterService filterService, IExportService exportService)
    {
        Settings = settingsService.GetSettings<FilterSettings>();
        _beatmapDb = beatmapDb;
        _scoreDb = scoreDb;
        _collectionDb = collectionDb;
        _logger = logService.GetLogger(this);
        _filterService = filterService;
        _exportService = exportService;

        _selected = Settings.History.FirstOrDefault() ?? new FilterHistoryItem();
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
        var result = _filterService.Filter(Selected.Expression, beatmaps).ToArray();
        _logger.Info($"符合条件的谱面有{result.Length}张");
        Selected = Settings.MoveFirst(Selected);
        return result;
    }
}