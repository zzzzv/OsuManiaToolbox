using CommunityToolkit.Mvvm.ComponentModel;
using OsuParsers.Enums;
using OsuParsers.Enums.Database;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace OsuManiaToolbox.Settings;

public partial class GradeThreshold(Grade grade, double acc) : ObservableObject
{
    public Grade Grade { get; set; } = grade;

    [ObservableProperty]
    private double _acc = acc;
}

public partial class ModGradeStrategyIndex(Mods mod) : ObservableObject
{
    public Mods Mod { get; set; } = mod;
    public string ModAcronym => Mod.Acronym();

    [ObservableProperty]
    private int _index = 1;
}

public partial class RegradeSettings : ObservableObject
{
    public ObservableCollection<GradeThreshold> GradeThresholds { get; set; } = [
        new GradeThreshold(Grade.S, 99),
        new GradeThreshold(Grade.A, 98),
        new GradeThreshold(Grade.B, 96),
        new GradeThreshold(Grade.C, 93),
        new GradeThreshold(Grade.D, 88),
    ];

    public ObservableCollection<ModGradeStrategyIndex> ModGradeStrategyIndexes { get; set; } = [
        new ModGradeStrategyIndex(Mods.NoFail),
        new ModGradeStrategyIndex(Mods.Easy),
        new ModGradeStrategyIndex(Mods.HalfTime),
    ];

    [JsonIgnore]
    public readonly static IReadOnlyList<GradeStrategy> AvailableStrategies = [
        new GradeStrategy(),
        new FixedGradeStrategy(Grade.D),
        new FixedGradeStrategy(Grade.F),
        new IgnoreGradeStrategy(),
    ];

    public GradeStrategy GetGradeStrategy(Mods mods)
    {
        var maxIndex = ModGradeStrategyIndexes
            .Where(x => mods.HasFlag(x.Mod))
            .Select(x => x.Index)
            .DefaultIfEmpty(0)
            .Max();
        return AvailableStrategies[maxIndex];
    }
}
