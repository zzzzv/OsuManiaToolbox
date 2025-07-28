using CommunityToolkit.Mvvm.ComponentModel;
using OsuManiaToolbox.Core;
using OsuParsers.Enums;
using OsuParsers.Enums.Database;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
using System.ComponentModel;

namespace OsuManiaToolbox.Settings;

public partial class ModGradeStrategyIndex(Mods mod) : ObservableObject
{
    public Mods Mod { get; set; } = mod;
    public string ModAcronym => Mod.Acronym();

    [ObservableProperty]
    private int _index = 1;

    [JsonIgnore]
    public IReadOnlyList<GradeStrategy> All => GradeStrategy.All;
}

public enum LastPlayedSelection
{
    [Description("保持不变")]
    NoChange,
    [Description("最高Acc时间")]
    MaxAcc,
    [Description("最高分数时间")]
    MaxScore,
}

public partial class RegradeSettings : ObservableObject
{
    public ObservableCollection<GradeThreshold> GradeThresholds { get; set; } = [
        new(Grade.S, 99),
        new(Grade.A, 98),
        new(Grade.B, 96),
        new(Grade.C, 93),
        new(Grade.D, 88),
    ];

    public ObservableCollection<ModGradeStrategyIndex> ModGradeStrategyIndexes { get; set; } = [
        new(Mods.NoFail),
        new(Mods.Easy),
        new(Mods.HalfTime),
    ];

    [ObservableProperty]
    private LastPlayedSelection _lastPlayedSelection = LastPlayedSelection.NoChange;

    [ObservableProperty]
    private bool _unplayedIfNoScore = false;

    public GradeStrategy GetGradeStrategy(Mods mods)
    {
        var maxIndex = ModGradeStrategyIndexes
            .Where(x => mods.HasFlag(x.Mod))
            .Select(x => x.Index)
            .DefaultIfEmpty(0)
            .Max();
        return GradeStrategy.All[maxIndex];
    }
}
