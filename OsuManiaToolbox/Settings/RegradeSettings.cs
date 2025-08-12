using CommunityToolkit.Mvvm.ComponentModel;
using OsuParsers.Enums;
using OsuParsers.Enums.Database;
using System.ComponentModel;

namespace OsuManiaToolbox.Settings;

public enum ModGradeStrategyType
{
    [Description("等同于NM")]
    Normal,
    [Description("固定为D")]
    FixedD,
    [Description("删除评级")]
    FixedN,
    [Description("保持不变")]
    Ignore,
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

public partial class GradeThresholds : ObservableObject
{
    [ObservableProperty]
    private double _s = 99;

    [ObservableProperty]
    private double _a = 98;

    [ObservableProperty]
    private double _b = 96;

    [ObservableProperty]
    private double _c = 93;

    [ObservableProperty]
    private double _d = 88;

    public IEnumerable<(Grade, double)> All()
    {
        yield return (Grade.S, S);
        yield return (Grade.A, A);
        yield return (Grade.B, B);
        yield return (Grade.C, C);
        yield return (Grade.D, D);
    }
}

public partial class ModGradeStrategies : ObservableObject
{
    [ObservableProperty]
    private ModGradeStrategyType _nf = ModGradeStrategyType.FixedD;

    [ObservableProperty]
    private ModGradeStrategyType _ez = ModGradeStrategyType.FixedD;

    [ObservableProperty]
    private ModGradeStrategyType _ht = ModGradeStrategyType.FixedD;

    public IEnumerable<(Mods, ModGradeStrategyType)> All()
    {
        yield return (Mods.NoFail, Nf);
        yield return (Mods.Easy, Ez);
        yield return (Mods.HalfTime, Ht);
    }
}

public partial class RegradeSettings : ObservableObject
{
    public GradeThresholds GradeThresholds { get; set; } = new();

    public ModGradeStrategies ModGradeStrategies { get; set; } = new();

    [ObservableProperty]
    private LastPlayedSelection _lastPlayedSelection = LastPlayedSelection.NoChange;

    [ObservableProperty]
    private bool _unplayedIfNoScore = false;
}
