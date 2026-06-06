using ClassIsland.Core;
using ClassIsland.Core.Abstractions;
using ClassIsland.Core.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Diagnostics;

namespace UACHope;

[PluginEntrance]
public class Plugin : PluginBase
{
    public override void Initialize(HostBuilderContext context, IServiceCollection services)
    {
        // 已经是管理员，正常启动，清除重启计数
        if (AdminHelper.IsRunningAsAdmin())
        {
            RestartGuard.ClearRestartCount();
            return;
        }

        // 检查是否应该放弃重启（UAC被拒绝或重启次数过多）
        if (RestartGuard.ShouldAbortRestart())
        {
            return;
        }

        // 记录重启尝试
        RestartGuard.RecordRestartAttempt();

        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = Environment.ProcessPath?.Replace(".dll", ".exe"),
                Verb = "runas",
                UseShellExecute = true
            };

            // 添加 -m 参数（ClassIsland 主启动参数）
            startInfo.ArgumentList.Add("-m");

            // 添加管理员重启标记，用于防止无限重启
            startInfo.ArgumentList.Add(RestartGuard.GetMarkerArg());

            // 透传原始命令行参数（跳过第一个，它是可执行文件路径）
            var args = Environment.GetCommandLineArgs();
            for (var i = 1; i < args.Length; i++)
            {
                startInfo.ArgumentList.Add(args[i]);
            }

            Process.Start(startInfo);
            AppBase.Current.Stop();
        }
        catch
        {
            // runas 启动失败（如用户取消 UAC），不做任何操作，当前进程继续运行
        }
    }
}
