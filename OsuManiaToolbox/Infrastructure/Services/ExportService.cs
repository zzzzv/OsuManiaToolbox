using CsvHelper;
using OsuManiaToolbox.Core.Services;
using OsuParsers.Database.Objects;
using OsuParsers.Enums;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;

namespace OsuManiaToolbox.Infrastructure.Services;

public class ExportService : IExportService
{
    private readonly IScoreDbService _scoreDb;
    private readonly ICollectionDbService _collectionDb;
    private readonly ILogger _logger;

    public ExportService(IScoreDbService scoreDb, ICollectionDbService collectionDb, ILogService logService)
    {
        _scoreDb = scoreDb;
        _collectionDb = collectionDb;
        _logger = logService.GetLogger(this);
    }

    public bool ExportToCsv(IEnumerable<DbBeatmap> beatmaps, string fileName)
    {
        try
        {
            if (string.IsNullOrEmpty(fileName))
            {
                _logger.Error("文件名不能为空");
                return false;
            }
            
            if (!beatmaps.Any())
            {
                _logger.Info("没有可导出的谱面");
                return false;
            }
            
            var file = fileName + ".csv";
            int count = 0;
            using (var stream = File.OpenWrite(file))
            using (var writer = new StreamWriter(stream, Encoding.UTF8))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteHeader<CsvData>();
                csv.NextRecord();
                foreach (var beatmap in beatmaps)
                {
                    csv.WriteRecord(new CsvData(beatmap, _scoreDb));
                    count++;
                    csv.NextRecord();
                }
            }
            
            _logger.Info($"已导出 {count} 行谱面信息到 {file}");

            var absolutePath = Path.GetFullPath(file);
            var process = new Process();
            process.StartInfo.FileName = absolutePath;
            process.StartInfo.UseShellExecute = true;
            process.Start();
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.Exception(ex);
            return false;
        }
    }

    public bool CreateCollection(IEnumerable<DbBeatmap> beatmaps, string collectionName)
    {
        try
        {
            if (string.IsNullOrEmpty(collectionName))
            {
                _logger.Error("收藏夹名称不能为空");
                return false;
            }
            
            if (!beatmaps.Any())
            {
                _logger.Info("没有符合条件的谱面");
                return false;
            }
            
            var collection = new Collection
            {
                Name = collectionName,
            };
            collection.MD5Hashes.AddRange(beatmaps.Select(x => x.MD5Hash));
            collection.Count = collection.MD5Hashes.Count;

            if (_collectionDb.Index.ContainsKey(collection.Name))
            {
                _logger.Warning($"收藏夹 {collection.Name} 已存在，自动覆盖");
                _collectionDb.Remove(collection.Name);
            }
            
            _collectionDb.Add(collection);
            _logger.Info($"创建收藏夹 {collection.Name}，包含 {collection.Count} 张谱面");
            _collectionDb.Save();
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.Exception(ex);
            return false;
        }
    }
}

public class CsvData
{
    private readonly DbBeatmap _bm;
    private readonly List<Score> _scores;
    private readonly Score? _maxAccScore;
    private readonly Score? _lastScore;

    public CsvData(DbBeatmap bm, IScoreDbService scoreDb)
    {
        _bm = bm;
        _scores = scoreDb.Index.TryGetValue(_bm.MD5Hash, out var scores) ? scores : [];
        _maxAccScore = _scores.OrderByDescending(x => x.ManiaAcc()).FirstOrDefault();
        _lastScore = _scores.OrderByDescending(x => x.ScoreTimestamp).FirstOrDefault();
    }

    public string Title => _bm.TitleUnicode;
    public string Artist => _bm.ArtistUnicode;
    public string Difficulty => _bm.Difficulty;
    public string XXY_SR => _bm.ManiaStarRating[Mods.None].ToString("F4");
    public string SR => _bm.ManiaStarRating[Mods.HardRock].ToString("F4");
    public int Key => (int)_bm.CircleSize;
    public string OD => _bm.OverallDifficulty.ToString("F1");
    public string HP => _bm.HPDrain.ToString("F1");
    public string LNRate => ((double)_bm.SlidersCount * 100 / (_bm.SlidersCount + _bm.CirclesCount)).ToString("F1");
    public int ScoreCount => _scores.Count;
    public string MaxAcc => _maxAccScore?.ManiaAcc().ToString("F2") ?? string.Empty;
    public string MaxAccMod => _maxAccScore?.Mods.Acronyms() ?? string.Empty;
    public string MaxAccTime => _maxAccScore?.ScoreTimestamp.ToString("g") ?? string.Empty;
    public string LastAcc => _lastScore?.ManiaAcc().ToString("F2") ?? string.Empty;
    public string LastAccMod => _lastScore?.Mods.Acronyms() ?? string.Empty;
    public string LastAccTime => _lastScore?.ScoreTimestamp.ToString("g") ?? string.Empty;
    public string Link => $"https://osu.ppy.sh/beatmapsets/{_bm.BeatmapSetId}#mania/{_bm.BeatmapId}";
}