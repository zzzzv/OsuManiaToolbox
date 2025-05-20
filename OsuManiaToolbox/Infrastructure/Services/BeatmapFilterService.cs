using DynamicExpresso;
using OsuManiaToolbox.Core.Services;
using OsuParsers.Database.Objects;
using OsuParsers.Enums;
using OsuParsers.Enums.Database;
using System.Text.RegularExpressions;

namespace OsuManiaToolbox.Infrastructure.Services;

public partial class BeatmapFilterService : IBeatmapFilterService
{
    private readonly IScoreDbService _scoreDb;
    private readonly ILogger _logger;

    public BeatmapFilterService(IScoreDbService scoreDb, ILogService logService)
    {
        _scoreDb = scoreDb;
        _logger = logService.GetLogger(this);
    }

    public IEnumerable<DbBeatmap> Filter(string expression, IEnumerable<DbBeatmap> beatmaps)
    {
        var checkedExpression = CheckExpression(expression);
        _logger.Info($"表达式为 {checkedExpression}");
        var func = new FilterInterpreter().Build(checkedExpression);
        return beatmaps.Where(p => func(p, _scoreDb));
    }

    private string CheckExpression(string expression)
    {
        var replaced = SingleEqualsRegex().Replace(expression, "==");
        if (replaced != expression)
        {
            _logger.Warning($"表达式 {expression} 应使用 == 而不是 =");
        }
        return replaced;
    }

    [GeneratedRegex(@"(?<![=<>])=(?![=<>])")]
    private static partial Regex SingleEqualsRegex();
}

public class FilterInterpreter
{
    public delegate bool FilterFunc(DbBeatmap bm, IScoreDbService scoreDb);

    public FilterFunc Build(string expression)
    {
        var interpreter = new Interpreter(InterpreterOptions.DefaultCaseInsensitive)
            .SetVariable("R", RankedStatus.Ranked)
            .SetVariable("L", RankedStatus.Loved)
            .SetVariable("Q", RankedStatus.Qualified)
            .SetVariable("P", RankedStatus.Pending)
            .SetVariable("HT", Mods.HalfTime)
            .SetVariable("DT", Mods.DoubleTime)
            .SetVariable("EZ", Mods.Easy)
            .SetVariable("HR", Mods.HardRock)
            .SetVariable("this", new FilterContext());
        expression = "Init(bm,scoreDb) && " + expression;
        return interpreter.ParseAsDelegate<FilterFunc>(expression, "bm", "scoreDb");
    }

    private class FilterContext
    {
#pragma warning disable CS8618
        private DbBeatmap _bm;
        private IScoreDbService _scoreDb;
#pragma warning restore CS8618

        public int Key => (int)_bm.CircleSize;
        public RankedStatus Status => _bm.RankedStatus;
        public double Acc => _scoreDb.Index.TryGetValue(_bm.MD5Hash, out var scores) ? scores.Max(x => x.ManiaAcc()) : 0;
        public int Score => _scoreDb.Index.TryGetValue(_bm.MD5Hash, out var scores) ? scores.Max(x => x.ReplayScore) : 0;

        public bool Init(DbBeatmap bm, IScoreDbService scores)
        {
            _bm = bm;
            _scoreDb = scores;
            return true;
        }

        public double SR(Mods mods = Mods.None)
        {
            return _bm.ManiaStarRating[mods];
        }
    }
}