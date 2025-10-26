using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OsuManiaToolbox.Core.Services;
using OsuManiaToolbox.Settings;
using OsuParsers.Enums;
using StarRatingRebirth;
using System.Collections.Concurrent;
using System.IO;

namespace OsuManiaToolbox.ViewModels;

public partial class StarRatingView : ObservableObject
{
    private readonly IOsuFileService _fileService;
    private readonly IBeatmapDbService _beatmapDb;
    private readonly ILogger _logger;
    private readonly IExportService _exportService;
    private CancellationTokenSource? _cancellationTokenSource;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RunCommand), nameof(CancelCommand))]
    private bool _isRunning = false;

    public IRelayCommand RunCommand { get; }
    public IRelayCommand CancelCommand { get; }
    public IRelayCommand ResetCommand { get; }

    public StarRatingSettings Settings { get; }

    public StarRatingView(ISettingsService settingsService, IOsuFileService fileService,
        IBeatmapDbService beatmapDb, ILogService logService, IExportService exportService)
    {
        Settings = settingsService.GetSettings<StarRatingSettings>();
        _fileService = fileService;
        _beatmapDb = beatmapDb;
        _logger = logService.GetLogger(this);
        _exportService = exportService;
        RunCommand = new AsyncRelayCommand(RunAsync, () => !IsRunning);
        CancelCommand = new RelayCommand(CancelOperation, () => IsRunning);
        ResetCommand = new RelayCommand(Reset, () => !IsRunning);
    }

    private void CancelOperation()
    {
        _cancellationTokenSource?.Cancel();
    }

    public void Reset()
    {
        foreach (var bm in _beatmapDb.Items.Where(x => x.Ruleset == Ruleset.Mania))
        {
            bm.ManiaStarRating[Mods.None] = bm.ManiaStarRating[Mods.Easy];
            bm.ManiaStarRating[Mods.HalfTime] = bm.ManiaStarRating[Mods.Easy | Mods.HalfTime];
            bm.ManiaStarRating[Mods.DoubleTime] = bm.ManiaStarRating[Mods.Easy | Mods.DoubleTime];
        }
        _beatmapDb.Save();
        _logger.Info("恢复原始SR");
    }

    private async Task RunAsync()
    {
        IsRunning = true;
        _cancellationTokenSource = new CancellationTokenSource();
        var token = _cancellationTokenSource.Token;

        try
        {
            await Task.Run(() => StarRatingTask(token), token);
        }
        catch (OperationCanceledException)
        {
            _logger.Warning("SR计算已取消");
        }
        catch (Exception ex)
        {
            _logger.Exception(ex);
        }
        finally
        {
            IsRunning = false;
            _cancellationTokenSource?.Dispose();
        }
    }

    private void StarRatingTask(CancellationToken token)
    {
        var beatmapFilter = _beatmapDb.Items
            .Where(x => x.Ruleset == Ruleset.Mania)
            .Where(x =>
            {
                if (x.ManiaStarRating.Count == 0)
                {
                    _logger.Warning($"谱面 {x.FolderName}/{x.FileName} 的原始SR尚未计算, 跳过, 尝试游戏中F5刷新");
                    return false;
                }
                return true;
            });
        if (!Settings.ForceUpdate)
        {
            beatmapFilter = beatmapFilter.Where(beatmaps => beatmaps.ManiaStarRating[Mods.None] == beatmaps.ManiaStarRating[Mods.Easy]);
        }

        var beatmaps = beatmapFilter.ToList();
        _logger.Info($"共有{beatmaps.Count}张需要处理的谱面");

        int processedCount = 0;
        int notSupportedCount = 0;
        var invalidBag = new ConcurrentBag<string>();
        var errorBag = new ConcurrentBag<string>();
        int totalProcessed = 0;

        var options = new ParallelOptions
        {
            MaxDegreeOfParallelism = Math.Max(1, Environment.ProcessorCount - 1),
            CancellationToken = token
        };

        Parallel.ForEach(beatmaps, options, (bm) =>
        {
            token.ThrowIfCancellationRequested();

            try
            {
                var data = ManiaData.FromFile(_fileService.GetBeatmapPath(bm));
                bm.ManiaStarRating[Mods.None] = SRCalculator.Calculate(data);
                bm.ManiaStarRating[Mods.HalfTime] = SRCalculator.Calculate(data.HT());
                bm.ManiaStarRating[Mods.DoubleTime] = SRCalculator.Calculate(data.DT());

                Interlocked.Increment(ref processedCount);
            }
            catch (NotSupportedException)
            {
                Interlocked.Increment(ref notSupportedCount);
            }
            catch (InvalidDataException)
            {
                invalidBag.Add(bm.MD5Hash);
            }
            catch (Exception ex)
            {
                errorBag.Add(bm.MD5Hash);
                _logger.Error($"处理谱面 {bm.FolderName}/{bm.FileName} 时出错: {ex.Message}");
                _logger.Exception(ex);
            }
            int current = Interlocked.Increment(ref totalProcessed);
            if (current % 500 == 0)
            {
                _logger.Info($"处理进度: {current}/{beatmaps.Count}");
            }
        });
        _logger.Info($"已处理{processedCount}张谱面，{notSupportedCount}张谱面不支持"
            + $"{invalidBag.Count}张谱面无效, {errorBag.Count}张谱面出错");
        if (!invalidBag.IsEmpty)
        {
            _exportService.CreateCollection(invalidBag, "Invalid");
            _logger.Info("无效谱面已加入收藏夹 'Invalid'");
        }
        if (!errorBag.IsEmpty)
        {
            _exportService.CreateCollection(errorBag, "Error");
            _logger.Info("出错谱面已加入收藏夹 'Error'");
        }
        _beatmapDb.Save();
    }
}
