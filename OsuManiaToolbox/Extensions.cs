using OsuParsers.Database.Objects;
using OsuParsers.Enums;
using OsuParsers.Enums.Database;

namespace OsuManiaToolbox;

public static class Extensions
{
    public static double ManiaAcc(this Score score)
    {
        return (score.CountGeki * 300.0 + score.Count300 * 300.0 + score.CountKatu * 200.0 + score.Count100 * 100.0 + score.Count50 * 50.0)
            / (score.CountGeki + score.Count300 + score.CountKatu + score.Count100 + score.Count50 + score.CountMiss) / 300.0 * 100.0;
    }

    public static string Acronym(this Mods mod)
    {
        return mod switch
        {
            Mods.None => "NM",
            Mods.HalfTime => "HT",
            Mods.DoubleTime => "DT",
            Mods.Easy => "EZ",
            Mods.HardRock => "HR",
            Mods.NoFail => "NF",
            Mods.Mirror => "MR",
            _ => string.Empty,
        };
    }

    public static string Acronyms(this Mods mods)
    {
        return string.Join('&', Enum.GetValues<Mods>()
            .Where(x => x != Mods.None && mods.HasFlag(x))
            .Select(x => x.Acronym()));
    }

    public static string Acronym(this RankedStatus status)
    {
        return status switch
        {
            RankedStatus.Ranked => "R",
            RankedStatus.Loved => "L",
            RankedStatus.Qualified => "Q",
            RankedStatus.Pending => "P",
            _ => string.Empty,
        };
    }
}
