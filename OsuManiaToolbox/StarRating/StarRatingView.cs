using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;
using OsuParsers.Decoders;
using OsuParsers.Enums;
using StarRatingRebirth;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;

namespace OsuManiaToolbox.StarRating;

public partial class StarRatingView : ObservableObject
{
    private readonly Settings _settings;
    private readonly Logger _logger;
    private CancellationTokenSource? _cancellationTokenSource;

    [ObservableProperty]
    private bool _isRunning;

    public ICommand StarRatingCommand { get; }
    public ICommand CancelCommand { get; }

    public StarRatingView(Settings settings, Logger logger)
    {
        _settings = settings;
        _logger = logger;
        StarRatingCommand = new AsyncRelayCommand(StarRatingRunAsync, () => !IsRunning);
        CancelCommand = new RelayCommand(CancelOperation, () => IsRunning);
    }

    private void CancelOperation()
    {
        _cancellationTokenSource?.Cancel();
        _logger.Warning("操作已取消");
    }

    private async Task StarRatingRunAsync()
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
            _logger.Warning("星级计算已取消");
        }
        catch (Exception ex)
        {
            _logger.Error(ex.Message);
            if (ex.StackTrace != null)
            {
                _logger.Error(ex.StackTrace);
            }
        }
        finally
        {
            IsRunning = false;
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
        }
    }

    private void StarRatingTask(CancellationToken token)
    {
        try
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
            int errorCount = 0;
            var options = new ParallelOptions
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount,
                CancellationToken = token
            };

            Parallel.ForEach(beatmaps, options, (bm) =>
            {
                try
                {
                    var data = ManiaData.FromFile(_settings.GetBeatmapPath(bm));
                    bm.ManiaStarRating[Mods.None] = SRCalculator.Calculate(data);
                    bm.ManiaStarRating[Mods.HalfTime] = SRCalculator.Calculate(data.HT());
                    bm.ManiaStarRating[Mods.DoubleTime] = SRCalculator.Calculate(data.DT());

                    int current = Interlocked.Increment(ref processedCount);
                    if (current % 500 == 0)
                    {
                        _logger.Info($"处理进度: {current}/{beatmaps.Count}");
                    }
                }
                catch (Exception ex)
                {
                    Interlocked.Increment(ref errorCount);
                    if (_settings.StarRating.ShowBeatmapError)
                    {
                        _logger.Error($"处理谱面 {bm.FolderName}/{bm.FileName} 时出错: {ex.Message}");
                    }
                }
            });
            _logger.Info($"已处理{processedCount}张谱面，{errorCount}张谱面出错");

            if (_settings.BackupDb)
            {
                var backupPath = Utils.BackupFile(_settings.OsuDbPath);
                _logger.Info($"已备份{Path.GetFileName(_settings.OsuDbPath)}到{backupPath}");
            }

            osuDb.Save(_settings.OsuDbPath);
            _logger.Info($"{Path.GetFileName(_settings.OsuDbPath)}已保存");
        }
        catch (FileNotFoundException ex)
        {
            _logger.Error($"文件未找到: {ex.FileName}");
        }
        catch (Exception ex)
        {
            _logger.Error(ex.Message);
            if (ex.StackTrace != null)
            {
                _logger.Error(ex.StackTrace);
            }
        }
    }
}
