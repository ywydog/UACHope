using ClassIsland.Core;
using ClassIsland.Core.Abstractions;
using ClassIsland.Core.Attributes;
using ClassIsland.Core.Extensions.Registry;
using ClassIsland.Shared;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using UACHope.Actions;
using UACHope.Settings;
using UACHope.SettingsPage;

namespace UACHope;

[PluginEntrance]
public partial class Plugin : PluginBase
{
    private ILogger<Plugin>? _logger;

    /// <summary>
    /// 插件全局配置访问点。在 Initialize 中初始化，供 OnAppStarted 等非 DI 路径使用。
    /// </summary>
    internal static UACHopeConfig Config { get; private set; } = null!;

    public override void Initialize(HostBuilderContext context, IServiceCollection services)
    {
        services.AddSingleton<ILogger<Plugin>>(sp => sp.GetRequiredService<ILoggerFactory>().CreateLogger<Plugin>());

        // 初始化配置
        var configHandler = new UACHopeConfigHandler(PluginConfigFolder);
        Config = configHandler.Data;
        services.AddSingleton(configHandler);

        // 注册设置页
        services.AddSettingsPage<UACHopeSettingsPage>();

        // 按功能开关条件式注册自动化行动
        if (Config.IsActionEnabled("uachope.elevateNow"))
        {
            services.AddAction<ElevateNowAction>();
        }
        if (Config.IsActionEnabled("uachope.requestSkipNextStartup"))
        {
            services.AddAction<RequestSkipNextStartupAction>();
        }

        // 按功能开关条件式注册规则
        if (Config.IsRuleEnabled("UACHope.AdminRunning"))
        {
            services.AddRule<UACHope.Rules.AdminRunningRuleSettings, UACHope.Controls.AdminRunningRuleSettingsControl>(
                "UACHope.AdminRunning", "当前为管理员身份运行", "\uEF6E", HandleAdminRunningRule);
        }

        // 延迟到应用启动后执行提权，确保 ILogger 等服务已就绪
        AppBase.Current.AppStarted += OnAppStarted;
    }

    private void OnAppStarted(object? sender, EventArgs e)
    {
        // 延迟到 AppStarted 后再从 DI 取 logger，确保 ILogger 服务已就绪
        _logger ??= IAppHost.GetService<ILogger<Plugin>>();
        _logger?.LogInformation("UACHope 启动钩子已触发");

        // 总闸：用户关闭了自动提权 → 整个插件静默
        if (!Config.IsAutoElevateEnabled)
        {
            _logger?.LogInformation("用户已关闭自动提权，跳过全部提权逻辑");
            return;
        }

        // 一次性开关：下次启动跳过提权 → 跳过并自动重置
        if (Config.SkipElevationNextStartup)
        {
            _logger?.LogInformation("用户已勾选【下次启动跳过提权】，跳过本次提权并自动重置开关");
            Config.SkipElevationNextStartup = false;
            RestartGuard.ClearRestartCount();
            return;
        }

        // 如果 ClassIsland 下次启动会切换到新部署，跳过本次提权，避免与部署/启动器切换流程冲突
        if (IsUpdatePending())
        {
            _logger?.LogInformation("检测到 ClassIsland 即将在下一次启动时更新，跳过本次提权");
            return;
        }

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

        // 自动提权路径
        ElevateNow(_logger);
    }

    /// <summary>
    /// 以管理员身份重启当前进程（runas）。供 OnAppStarted 自动路径与 ElevateNowAction 自动化行动共用。
    /// 失败时仅记录日志，不抛出。
    /// </summary>
    public static void ElevateNow(ILogger? logger = null)
    {
        // 记录重启尝试
        RestartGuard.RecordRestartAttempt();

        try
        {
            logger?.LogInformation("开始以管理员身份重启应用");

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
            logger?.LogInformation("已启动管理员权限实例并退出当前进程");
            AppBase.Current.Stop();
            Environment.Exit(0);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "以管理员身份重启失败");
        }
    }

    /// <summary>
    /// 标记下一次启动跳过提权（一次性标志，启动后会被自动重置）。供 RequestSkipNextStartupAction 自动化行动调用。
    /// </summary>
    public static void RequestSkipNextStartup()
    {
        Config.SkipElevationNextStartup = true;
        IAppHost.GetService<ILoggerFactory>()?.CreateLogger<Plugin>()
            ?.LogInformation("已置位 SkipElevationNextStartup，下次启动将跳过提权");
    }

    /// <summary>
    /// 检测 ClassIsland 下次启动是否会切换到新部署。
    /// 依据本体 [UpdateService] 写入的 .destroy / .current 标记判断。
    /// </summary>
    private static bool IsUpdatePending()
    {
        try
        {
            // 信号 1：当前运行目录被标记为待销毁，说明本体已完成部署，下次启动器会切走
            if (File.Exists(Path.Combine(Environment.CurrentDirectory, ".destroy")))
            {
                return true;
            }

            // 信号 2：包根下存在与当前目录不同的、且带有 .current 标记的 appN 部署
            var root = CommonDirectories.AppPackageRoot;
            if (string.IsNullOrEmpty(root) || !Directory.Exists(root))
            {
                return false;
            }

            var currentDir = Path.GetFullPath(Environment.CurrentDirectory);
            foreach (var dir in Directory.GetDirectories(root))
            {
                if (!Path.GetFileName(dir).StartsWith("app", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
                if (Path.GetFullPath(dir) == currentDir)
                {
                    continue;
                }
                if (File.Exists(Path.Combine(dir, ".current")))
                {
                    return true;
                }
            }
        }
        catch
        {
            // 检测失败保守放行，不影响主流程
        }
        return false;
    }
}
