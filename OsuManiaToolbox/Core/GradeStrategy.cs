using OsuManiaToolbox.Settings;
using OsuParsers.Database.Objects;
using OsuParsers.Enums.Database;

namespace OsuManiaToolbox.Core;

public class GradeStrategy
{
    public virtual string Name => "等同于NM";
    public virtual Grade? GetGrade(Score score, IEnumerable<GradeThreshold> thresholds)
    {
        double acc = score.ManiaAcc();
        if (acc == 100) return Grade.X;
        foreach (var threshold in thresholds)
        {
            if (acc >= threshold.Acc)
            {
                return threshold.Grade;
            }
        }
        return Grade.F;
    }
}

public class FixedGradeStrategy(Grade? grade) : GradeStrategy
{
    public Grade? FixedGrade { get; } = grade;
    public override string Name => $"固定为{FixedGrade}";
    public override Grade? GetGrade(Score score, IEnumerable<GradeThreshold> thresholds)
    {
        return FixedGrade;
    }
}

public class IgnoreGradeStrategy : FixedGradeStrategy
{
    public IgnoreGradeStrategy() : base(null) { }

    public override string Name => "保持不变";
}
