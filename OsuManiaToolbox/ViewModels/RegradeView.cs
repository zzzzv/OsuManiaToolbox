using CommunityToolkit.Mvvm.Input;
using OsuManiaToolbox.Core.Services;
using OsuManiaToolbox.Settings;
using OsuParsers.Database.Objects;
using OsuParsers.Enums;
using OsuParsers.Enums.Database;

namespace OsuManiaToolbox.ViewModels;

public class RegradeView
{
    private readonly IBeatmapDbService _beatmapDb;
    private readonly IScoreDbService _scoreDb;
    private readonly ILogger _logger;

    public IRelayCommand RegradeCommand { get; }

    public RegradeSettings Settings { get; }

    public RegradeView(ISettingsService settingsService, IBeatmapDbService beatmapDb, IScoreDbService scoreDb, ILogService logService)
    {
        Settings = settingsService.GetSettings<RegradeSettings>();
        _beatmapDb = beatmapDb;
        _scoreDb = scoreDb;
        _logger = logService.GetLogger(this);
        RegradeCommand = new RelayCommand(RegradeRun);
    }

    private void RegradeRun()
    {
        try
        {
            var beatmaps = _beatmapDb.Items.Where(x => x.Ruleset == Ruleset.Mania).ToArray();
            _logger.Info($"共有{beatmaps.Length}张Mania谱面");

            int count = 0;
            foreach (var beatmap in beatmaps)
            {
                if (beatmap.MD5Hash == null) continue;

                if (_scoreDb.Index.TryGetValue(beatmap.MD5Hash, out var scores))
                {
                    var grade = GetGrade(scores, Settings);
                    if (grade != null)
                    {
                        beatmap.ManiaGrade = grade.Value;
                        count++;
                    }
                    if (Settings.LastPlayedSelection == LastPlayedSelection.MaxAcc)
                    {
                        var maxAccScore = scores.MaxBy(x => x.ManiaAcc());
                        if (maxAccScore != null)
                        {
                            beatmap.LastPlayed = maxAccScore.ScoreTimestamp;
                        }
                    }
                    else if (Settings.LastPlayedSelection == LastPlayedSelection.MaxScore)
                    {
                        var maxScore = scores.MaxBy(x => x.ReplayScore);
                        if (maxScore != null)
                        {
                            beatmap.LastPlayed = maxScore.ScoreTimestamp;
                        }
                    }
                }
                else if (Settings.UnplayedIfNoScore)
                {
                    beatmap.IsUnplayed = true;
                    beatmap.LastPlayed = DateTime.MinValue;
                }
            }
            _logger.Info($"已处理{count}张谱面");

            _beatmapDb.Save();
        }
        catch (Exception ex)
        {
            _logger.Exception(ex);
        }
        finally
        {

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
