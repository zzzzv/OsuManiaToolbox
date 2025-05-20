using DynamicExpresso;
using OsuManiaToolbox.Core.Services;
using OsuParsers.Database.Objects;
using System.Data;
using System.Text.RegularExpressions;

namespace OsuManiaToolbox.Infrastructure.Services;

public partial class BeatmapFilterService : IBeatmapFilterService
{
    public delegate bool FilterFunc(FilterContext context);

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
        var func = Build(checkedExpression);
        return beatmaps.Where(p => func(new FilterContext(p, _scoreDb)));
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

    private FilterFunc Build(string expression)
    {
        var interpreter = new Interpreter(InterpreterOptions.DefaultCaseInsensitive);
        return interpreter.ParseAsDelegate<FilterFunc>(expression, "this");
    }

    [GeneratedRegex(@"(?<![=<>])=(?![=<>])")]
    private static partial Regex SingleEqualsRegex();
}