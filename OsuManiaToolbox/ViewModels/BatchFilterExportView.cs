using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DynamicExpresso.Exceptions;
using OsuManiaToolbox.Core;
using OsuManiaToolbox.Core.Services;
using OsuManiaToolbox.Settings;
using OsuParsers.Enums;

namespace OsuManiaToolbox.ViewModels;

record FilterResult(FilterHistoryItem Filter, BeatmapData[] Beatmaps);

public partial class BatchFilterExportView : ObservableObject
{
    private readonly IBeatmapDbService _beatmapDb;
    private readonly ILogger _logger;
    private readonly IBeatmapFilterService _filterService;
    private readonly IExportService _exportService;

    public FilterSettings Settings { get; }

    public IRelayCommand BatchCreateCollection { get; }
    public IRelayCommand BatchWriteCsv { get; }

    public BatchFilterExportView(
        ISettingsService settingsService,
        IBeatmapDbService beatmapDb,
        ILogService logService,
        IBeatmapFilterService filterService,
        IExportService exportService)
    {
        Settings = settingsService.GetSettings<FilterSettings>();
        _beatmapDb = beatmapDb;
        _logger = logService.GetLogger(this);
        _filterService = filterService;
        _exportService = exportService;

        BatchCreateCollection = new RelayCommand(BatchCreateCollectionRun);
        BatchWriteCsv = new RelayCommand(BatchWriteCsvRun);
    }

    private List<FilterResult> FilterRun()
    {
        var mania = _beatmapDb.Items.Where(x => x.Ruleset == Ruleset.Mania).ToArray();
        var markedFilters = Settings.History.Where(x => x.MarkedAsBatch).ToArray();
        var resultAll = new List<FilterResult>();

        foreach (var filter in markedFilters)
        {
            try
            {
                var result = _filterService.Filter(mania, filter.Expression, filter.OrderBy).Skip(filter.Skip);
                if (filter.Take > 0)
                {
                    result = result.Take(filter.Take);
                }
                var arr = result.ToArray();
                _logger.Info($"符合条件的谱面有{arr.Length}张");
                resultAll.Add(new FilterResult(filter, arr));
            }
            catch (ParseException ex)
            {
                _logger.Error(filter.Expression);
                _logger.Error(new string('^', ex.Position + 1));
                _logger.Error(ex.Message);
            }
        }
        return resultAll;
    }

    private void BatchCreateCollectionRun()
    {
        _logger.Info("正在批量创建收藏夹...");
        var resultAll = FilterRun();

        foreach (var result in resultAll)
        {
            if (result.Beatmaps.Length == 0) continue;

            var hashes = result.Beatmaps.Select(x => x.Bm.MD5Hash).ToArray();
            var collectionName = string.IsNullOrWhiteSpace(result.Filter.CollectionName) ? result.Filter.Expression : result.Filter.CollectionName;
            _exportService.CreateCollection(hashes, collectionName);
        }
    }

    private void BatchWriteCsvRun()
    {
        _logger.Info("正在批量导出CSV...");
        var resultAll = FilterRun();

        foreach (var result in resultAll)
        {
            if (result.Beatmaps.Length == 0) continue;

            var fileName = string.IsNullOrWhiteSpace(result.Filter.CollectionName) ? result.Filter.Expression : result.Filter.CollectionName;
            _exportService.ExportToCsv(result.Beatmaps, $"{fileName}.csv");
        }
    }
}