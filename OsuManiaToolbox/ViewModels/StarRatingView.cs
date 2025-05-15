using CommunityToolkit.Mvvm.Input;
using OsuParsers.Enums;
using StarRatingRebirth;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using OsuManiaToolbox.Services;

namespace OsuManiaToolbox.StarRating;

public partial class StarRatingView : ObservableObject
{
    private readonly OsuFileService _fileService;
    private readonly IBeatmapDbService _beatmapDb;
    private readonly ILogger _logger;
    private CancellationTokenSource? _cancellationTokenSource;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RunCommand), nameof(CancelCommand))]
    private bool _isRunning = false;

    public IRelayCommand RunCommand { get; }
    public IRelayCommand CancelCommand { get; }

    public StarRatingSettings Settings { get; }

    public StarRatingView(StarRatingSettings settings, OsuFileService fileService, IBeatmapDbService beatmapDb, ILogger<StarRatingView> logger)
    {
        Settings = settings;
        _fileService = fileService;
        _beatmapDb = beatmapDb;
        _logger = logger;
        RunCommand = new AsyncRelayCommand(RunAsync, () => !IsRunning);
        CancelCommand = new RelayCommand(CancelOperation, () => IsRunning);
    }

    private void CancelOperation()
    {
        _cancellationTokenSource?.Cancel();
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
        var beatmapFilter = _beatmapDb.Where(x => x.Ruleset == Ruleset.Mania && x.ManiaStarRating[Mods.None] >= Settings.MinSR);
        if (Settings.Exclude4K)
        {
            beatmapFilter = beatmapFilter.Where(x => x.CircleSize != 4);
        }
        if (!Settings.ForceUpdate)
        {
            beatmapFilter = beatmapFilter.Where(beatmaps => beatmaps.ManiaStarRating[Mods.None] == beatmaps.ManiaStarRating[Mods.Easy]);
        }

        var beatmaps = beatmapFilter.ToList();
        _logger.Info($"共有{beatmaps.Count}张需要处理的谱面");

        int processedCount = 0;
        int notSupportedCount = 0;
        int invalidCount = 0;
        int errorCount = 0;
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
                Interlocked.Increment(ref invalidCount);
            }
            catch (Exception ex)
            {
                Interlocked.Increment(ref errorCount);
                _logger.Error($"处理谱面 {bm.FolderName}/{bm.FileName} 时出错: {ex.Message}");
                _logger.Exception(ex);
            }
            int current = Interlocked.Increment(ref totalProcessed);
            if (current % 500 == 0)
            {
                _logger.Info($"处理进度: {current}/{beatmaps.Count}");
            }
        });
        _logger.Info($"已处理{processedCount}张谱面，{notSupportedCount}张谱面不支持, {invalidCount}张谱面无效, {errorCount}张谱面出错");

        _beatmapDb.Save();
    }
}
