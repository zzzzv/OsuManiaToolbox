using OsuParsers.Decoders;
using OsuParsers.Enums.Database;
using OsuParsers.Database.Objects;
using System.IO;
using CommunityToolkit.Mvvm.Input;

namespace OsuManiaToolbox.Regrade;

public partial class RegradeView
{
    private readonly Settings _settings;
    private readonly Logger _logger;

    public IRelayCommand RegradeCommand { get; }

    public RegradeView(Settings settings, Logger logger)
    {
        _settings = settings;
        _logger = logger;
        RegradeCommand = new RelayCommand(RegradeRun);
    }

    private void RegradeRun()
    {
        if (!Path.Exists(_settings.OsuDbPath))
        {
            _logger.Error($"找不到文件 {_settings.OsuDbPath}");
            return;
        }

        try
        {
            var scoreDb = DatabaseDecoder.DecodeScores(_settings.ScoreDbPath);
            var scoreDict = scoreDb.Scores.ToDictionary(x => x.Item1, x => x.Item2);
            var osuDb = DatabaseDecoder.DecodeOsu(_settings.OsuDbPath);
            var beatmaps = osuDb.ManiaBeatmaps().ToArray();
            _logger.Info($"共有{beatmaps}张谱面");

            int count = 0;
            foreach (var beatmap in beatmaps)
            {
                if (beatmap.MD5Hash == null) continue;

                var scores = scoreDict.GetValueOrDefault(beatmap.MD5Hash);
                if (scores == null) continue;

                var grade = GetGrade(scores, _settings.Regrade);
                if (grade != null)
                {
                    beatmap.ManiaGrade = grade.Value;
                    count++;
                }
            }
            _logger.Info($"已处理{count}张谱面");

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
            _logger.Exception(ex);
        }
    }

    private static Grade? GetGrade(IEnumerable<Score> scores, RegradeSettings settings)
    {
        Grade? grade = null;
        foreach(var score in scores)
        {
            var strategy = settings.GetGradeStrategy(score.Mods);
            var curGrade = strategy.GetGrade(score, settings.GradeThresholds);
            if (grade == null || curGrade < grade)
            {
                grade = curGrade;
            }
        }
        return grade;
    }
}
