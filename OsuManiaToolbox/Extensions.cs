using OsuParsers.Database.Objects;
using OsuParsers.Enums;

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
            _ => mod.ToString(),
        };
    }
}
