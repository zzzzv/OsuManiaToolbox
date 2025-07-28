using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Navigation;

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

    public static void Navigate(RequestNavigateEventArgs e)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = e.Uri.AbsoluteUri,
            UseShellExecute = true
        });

        e.Handled = true;
    }

    public static Version GetVersion()
    {
        return Assembly.GetEntryAssembly()?.GetName().Version ?? new Version(0, 0, 0);
    }
}
