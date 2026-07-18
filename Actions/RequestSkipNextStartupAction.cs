using System.Threading.Tasks;
using ClassIsland.Core;
using ClassIsland.Core.Abstractions.Automation;
using ClassIsland.Core.Attributes;
using Microsoft.Extensions.Logging;

namespace UACHope.Actions;

/// <summary>
/// 自动化行动：标记下一次 ClassIsland 启动时跳过自动提权（一次性标志，启动后由 OnAppStarted 自动清除）。
/// </summary>
[ActionInfo("uachope.requestSkipNextStartup", "UACHope 标记下次跳过提权", "\uE708")]
public class RequestSkipNextStartupAction : ActionBase
{
    protected override async Task OnInvoke()
    {
        await base.OnInvoke();
        var logger = IAppHost.GetService<ILoggerFactory>()?.CreateLogger<RequestSkipNextStartupAction>();
        logger?.LogInformation("UACHope 标记下次跳过提权 行动被触发");
        Plugin.RequestSkipNextStartup();
    }
}
