using System.IO;

namespace UACHope;

public static class RestartGuard
{
    private const string MarkerArg = "--admin-restart";
    private const string CountFileName = "UACHope_restart_count";
    private const int MaxRestartCount = 3;
    private static readonly TimeSpan RestartWindow = TimeSpan.FromSeconds(60);

    /// <summary>
    /// 检查是否应该放弃重启。
    /// 如果命令行包含 --admin-restart 但仍非管理员，说明 UAC 被拒绝；
    /// 或者短时间内重启次数过多，也应放弃。
    /// </summary>
    public static bool ShouldAbortRestart()
    {
        // 如果带了 --admin-restart 标记但仍然不是管理员，说明 UAC 被用户拒绝
        var args = Environment.GetCommandLineArgs();
        if (args.Contains(MarkerArg))
            return true;

        // 检查重启计数是否超限
        return IsRestartCountExceeded();
    }

    /// <summary>
    /// 记录一次重启尝试
    /// </summary>
    public static void RecordRestartAttempt()
    {
        try
        {
            var countFilePath = GetCountFilePath();
            var entries = ReadEntries(countFilePath);
            entries.Add(DateTime.UtcNow);
            WriteEntries(countFilePath, entries);
        }
        catch
        {
            // 记录失败不影响主流程
        }
    }

    /// <summary>
    /// 成功以管理员身份启动后，清除重启计数
    /// </summary>
    public static void ClearRestartCount()
    {
        try
        {
            var countFilePath = GetCountFilePath();
            if (File.Exists(countFilePath))
                File.Delete(countFilePath);
        }
        catch
        {
            // 清除失败不影响主流程
        }
    }

    /// <summary>
    /// 获取 --admin-restart 标记参数名
    /// </summary>
    public static string GetMarkerArg() => MarkerArg;

    private static bool IsRestartCountExceeded()
    {
        try
        {
            var countFilePath = GetCountFilePath();
            var entries = ReadEntries(countFilePath);
            var cutoff = DateTime.UtcNow - RestartWindow;
            var recentCount = entries.Count(t => t > cutoff);
            return recentCount >= MaxRestartCount;
        }
        catch
        {
            return false;
        }
    }

    private static string GetCountFilePath()
    {
        return Path.Combine(Path.GetTempPath(), CountFileName);
    }

    private static List<DateTime> ReadEntries(string filePath)
    {
        var entries = new List<DateTime>();
        if (!File.Exists(filePath))
            return entries;

        foreach (var line in File.ReadAllLines(filePath))
        {
            if (DateTime.TryParse(line, out var dt))
                entries.Add(dt);
        }
        return entries;
    }

    private static void WriteEntries(string filePath, List<DateTime> entries)
    {
        var cutoff = DateTime.UtcNow - RestartWindow;
        var recent = entries.Where(t => t > cutoff).ToList();
        File.WriteAllLines(filePath, recent.Select(t => t.ToString("O")));
    }
}
