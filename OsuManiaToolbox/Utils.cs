using OsuParsers.Database.Objects;
using System.IO;

namespace OsuManiaToolbox;

public static class Utils
{
    public static string BackupFile(string path)
    {
        int count = 0;
        string backupPath;
        do
        {
            count++;
            backupPath = path + $".bak{count}";
        } while (File.Exists(backupPath));
        File.Copy(path, backupPath, true);
        return backupPath;
    }
}
