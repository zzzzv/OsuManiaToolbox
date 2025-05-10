using CommunityToolkit.Mvvm.Input;
using OsuParsers.Decoders;
using OsuParsers.Enums;
using StarRatingRebirth;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Reflection.Metadata.Ecma335;

namespace OsuManiaToolbox.StarRating;

public partial class StarRatingView : ObservableObject
{
    private readonly Settings _settings;
    private readonly Logger _logger;
    private CancellationTokenSource? _cancellationTokenSource;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RunCommand), nameof(CancelCommand))]
    private bool _isRunning = false;

    public IRelayCommand RunCommand { get; }
    public IRelayCommand CancelCommand { get; }

    public StarRatingView(Settings settings, Logger logger)
    {
        _settings = settings;
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
        if(!Path.Exists(_settings.OsuDbPath))
        {
            _logger.Error($"找不到文件 {_settings.OsuDbPath}");
            return;
        }

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
        var osuDb = DatabaseDecoder.DecodeOsu(_settings.OsuDbPath);
        var beatmapFilter = osuDb.ManiaBeatmaps().Where(x => x.ManiaStarRating[Mods.None] >= _settings.StarRating.MinSR);
        if (_settings.StarRating.Exclude4K)
        {
            beatmapFilter = beatmapFilter.Where(x => x.CircleSize != 4);
        }

        var beatmaps = beatmapFilter.ToList();
        _logger.Info($"共有{beatmaps.Count}张谱面");

        int processedCount = 0;
        int skipCount = 0;
        int errorCount = 0;
        var options = new ParallelOptions
        {
            MaxDegreeOfParallelism = Environment.ProcessorCount,
            CancellationToken = token
        };

        Parallel.ForEach(beatmaps, options, (bm) =>
        {
            token.ThrowIfCancellationRequested();

            if (!_settings.StarRating.ForceUpdate &&
                bm.ManiaStarRating[Mods.None] != bm.ManiaStarRating[Mods.Easy])
            {
                Interlocked.Increment(ref skipCount);
            }
            else
            {
                try
                {
                    var data = ManiaData.FromFile(_settings.GetBeatmapPath(bm));
                    bm.ManiaStarRating[Mods.None] = SRCalculator.Calculate(data);
                    bm.ManiaStarRating[Mods.HalfTime] = SRCalculator.Calculate(data.HT());
                    bm.ManiaStarRating[Mods.DoubleTime] = SRCalculator.Calculate(data.DT());

                    Interlocked.Increment(ref processedCount);
                }
                catch (Exception ex)
                {
                    Interlocked.Increment(ref errorCount);
                    if (_settings.StarRating.ShowBeatmapError)
                    {
                        _logger.Error($"处理谱面 {bm.FolderName}/{bm.FileName} 时出错: {ex.Message}");
                    }
                }
            }
            int current = processedCount + skipCount + errorCount;
            if (current % 500 == 0)
            {
                _logger.Info($"处理进度: {current}/{beatmaps.Count}");
            }
        });
        _logger.Info($"已处理{processedCount}张谱面，跳过{skipCount}张谱面, {errorCount}张谱面出错");

        if (_settings.BackupDb)
        {
            var backupPath = Utils.BackupFile(_settings.OsuDbPath);
            _logger.Info($"已备份{Path.GetFileName(_settings.OsuDbPath)}到{backupPath}");
        }

        osuDb.Save(_settings.OsuDbPath);
        _logger.Info($"{Path.GetFileName(_settings.OsuDbPath)}已保存");
    }
}
