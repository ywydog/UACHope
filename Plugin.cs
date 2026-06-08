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
            var processStartInfo = new ProcessStartInfo()
            {
                FileName = Environment.ProcessPath?.Replace(".dll", ".exe"),
                Verb = "runas",
                UseShellExecute = true
            };

            processStartInfo.ArgumentList.Add("-m");

            var args = Environment.GetCommandLineArgs().ToList();
            args.RemoveAt(0);
            foreach (var i in args)
            {
                processStartInfo.ArgumentList.Add(i);
            }

            Process.Start(processStartInfo);
            AppBase.Current.Stop();
            Environment.Exit(0);
        }
        catch
        {
            // runas 启动失败（如用户取消 UAC），不做任何操作，当前进程继续运行
        }
    }
}
