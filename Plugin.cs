using ClassIsland.Core;
using ClassIsland.Core.Abstractions;
using ClassIsland.Core.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace UACHope;

[PluginEntrance]
public class Plugin : PluginBase
{
    private ILogger<Plugin>? _logger;

    public override void Initialize(HostBuilderContext context, IServiceCollection services)
    {
        services.AddSingleton<ILogger<Plugin>>(sp => sp.GetRequiredService<ILoggerFactory>().CreateLogger<Plugin>());

        // 延迟到应用启动后执行提权，确保 ILogger 等服务已就绪
        AppBase.Current.AppStarted += OnAppStarted;
    }

    private void OnAppStarted(object? sender, EventArgs e)
    {
        // 已经是管理员，正常启动，清除重启计数
        if (AdminHelper.IsRunningAsAdmin())
        {
            _logger?.LogInformation("当前已是管理员身份，无需重启");
            RestartGuard.ClearRestartCount();
            return;
        }

        // 检查是否应该放弃重启（UAC被拒绝或重启次数过多）
        if (RestartGuard.ShouldAbortRestart())
        {
            _logger?.LogWarning("检测到 UAC 被拒绝或重启次数过多，放弃提权重启");
            return;
        }

        // 记录重启尝试
        RestartGuard.RecordRestartAttempt();

        try
        {
            _logger?.LogInformation("开始以管理员身份重启应用");

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
            _logger?.LogInformation("已启动管理员权限实例并退出当前进程");
            AppBase.Current.Stop();
            Environment.Exit(0);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "以管理员身份重启失败");
        }
    }
}
