using DynamicExpresso;
using OsuParsers.Database.Objects;
using OsuManiaToolbox.Core.Services;
using OsuManiaToolbox.Core;
using System.Data;
using System.Text.RegularExpressions;

namespace OsuManiaToolbox.Infrastructure.Services;

public partial class BeatmapFilterService : IBeatmapFilterService
{
    public delegate bool? FilterFunc(FilterContext context);
    public delegate object OrderFunc(FilterContext context);

    private readonly IScoreDbService _scoreDb;
    private readonly ILogger _logger;

    public BeatmapFilterService(IScoreDbService scoreDb, ILogService logService)
    {
        _scoreDb = scoreDb;
        _logger = logService.GetLogger(this);
    }

    public DataView MetaTable => FilterContext.MetaTable;

    public IEnumerable<BeatmapData> Filter(IEnumerable<DbBeatmap> beatmaps, string expression, string order)
    {
        return Filter(beatmaps.Select(p => new FilterContext(p, _scoreDb)), expression, order);
    }

    public IEnumerable<BeatmapData> Filter(IEnumerable<BeatmapData> beatmaps, string expression, string order)
    {
        expression = CheckExpression(expression);
        _logger.Info($"表达式为 {expression}");
        var interpreter = new Interpreter(InterpreterOptions.DefaultCaseInsensitive);
        var func = interpreter.ParseAsDelegate<FilterFunc>(expression, "this");
        var result = beatmaps.Select(p => p as FilterContext ?? new FilterContext(p)).Where(p => func(p) ?? false);
        if (order != string.Empty)
        {
            var orderFunc = interpreter.ParseAsDelegate<OrderFunc>(order, "this");
            result = result.OrderBy(p => orderFunc(p));
        }
        return result;
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