using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DynamicExpresso;
using OsuManiaToolbox.Core.Services;
using OsuManiaToolbox.Settings;
using OsuParsers.Database.Objects;
using OsuParsers.Enums;
using System.Text.RegularExpressions;
using OsuParsers.Enums.Database;

namespace OsuManiaToolbox.ViewModels;

public partial class FilterView : ObservableObject
{
    private readonly IBeatmapDbService _beatmapDb;
    private readonly IScoreDbService _scoreDb;
    private readonly ICollectionDbService _collectionDb;
    private readonly ILogger _logger;

    public IRelayCommand CreateItem { get; }
    public IRelayCommand DeleteItem { get; }
    public IRelayCommand CreateCollection { get; }

    public FilterSettings Settings { get; }

    [ObservableProperty]
    private FilterHistoryItem _selected;

    public FilterView(ISettingsService settingsService, IBeatmapDbService beatmapDb, IScoreDbService scoreDb, 
        ICollectionDbService collectionDb, ILogger<RegradeView> logger)
    {
        Settings = settingsService.GetSettings<FilterSettings>();
        _beatmapDb = beatmapDb;
        _scoreDb = scoreDb;
        _collectionDb = collectionDb;
        _logger = logger;
        _selected = Settings.History.FirstOrDefault() ?? new FilterHistoryItem();
        CreateItem = new RelayCommand(CreateItemRun);
        DeleteItem = new RelayCommand(DeleteItemRun, CanDeleteItem);
        CreateCollection = new RelayCommand(CreateCollectionRun);
    }

    private void CreateItemRun()
    {
        var newItem = new FilterHistoryItem();
        Settings.History.Insert(0, newItem);
        Selected = newItem;
    }

    private void DeleteItemRun()
    {
        if (Selected != null && Settings.History.Contains(Selected))
        {
            var index = Settings.History.IndexOf(Selected);
            Settings.History.Remove(Selected);

            if (Settings.History.Count > 0)
            {
                index = Math.Min(index, Settings.History.Count - 1);
                Selected = Settings.History[index];
            }
            else
            {
                var newItem = new FilterHistoryItem();
                Settings.History.Add(newItem);
                Selected = newItem;
            }
        }
    }

    private bool CanDeleteItem()
    {
        return Selected != null && Settings.History.Count > 1;
    }

    private void CreateCollectionRun()
    {
        try
        {
            if (string.IsNullOrEmpty(Selected.CollectionName))
            {
                _logger.Error("收藏夹名称不能为空");
                return;
            }
            var result = Filter().ToArray();
            if (result.Length == 0)
            {
                _logger.Info("没有符合条件的谱面");
                return;
            }
            var collection = new Collection
            {
                Name = Selected.CollectionName,
                Count = result.Length,
            };
            collection.MD5Hashes.AddRange(result.Select(x => x.MD5Hash));
            if (_collectionDb.Contains(collection.Name))
            {
                _logger.Warning($"收藏夹 {collection.Name} 已存在，自动覆盖");
                _collectionDb.Remove(collection.Name);
            }
            _collectionDb.Add(collection);
            _logger.Info($"创建收藏夹 {collection.Name}，包含 {collection.Count} 张谱面");
            _collectionDb.Save();
        }
        catch (Exception ex)
        {
            _logger.Exception(ex);
        }
    }

    private DbBeatmap[] Filter()
    {
        var beatmaps = _beatmapDb.Where(x => x.Ruleset == Ruleset.Mania).ToArray();
        _logger.Info($"共有{beatmaps.Length}张Mania谱面");
        var expression = Check(Selected.Expression);
        _logger.Info($"表达式为 {expression}");

        var func = new FilterInterpreter().Build(expression);
        var result = beatmaps.Where(p => func(p, _scoreDb)).ToArray();
        _logger.Info($"符合条件的谱面有{result.Length}张");
        Selected = Settings.MoveFirst(Selected);
        return result;
    }


    private string Check(string expression)
    {
        var replaced = Regex.Replace(expression, @"(?<![=<>])=(?![=<>])", "==");
        if (replaced != expression)
        {
            _logger.Warning($"表达式 {expression} 应使用 == 而不是 =");
        }
        return replaced;
    }
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
        public double Acc => _scoreDb.TryGetValue(_bm.MD5Hash, out var scores) ? scores.Max(x => x.ManiaAcc()) : 0;
        public int Score => _scoreDb.TryGetValue(_bm.MD5Hash, out var scores) ? scores.Max(x => x.ReplayScore) : 0;

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

