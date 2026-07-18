using System.Threading.Tasks;
using ClassIsland.Core;
using ClassIsland.Core.Abstractions.Automation;
using ClassIsland.Core.Attributes;
using ClassIsland.Shared;
using Microsoft.Extensions.Logging;

namespace UACHope.Actions;

/// <summary>
/// 自动化行动：立即以管理员身份重启当前进程。
/// 若当前已是管理员身份，则跳过提权、不再触发 runas。
/// 触发后调用 <see cref="Plugin.ElevateNow"/>，由 runas 弹 UAC，成功后退出当前进程。
/// </summary>
[ActionInfo("uachope.elevateNow", "UACHope 立即提权", "\uEF6E")]
public class ElevateNowAction : ActionBase
{
    protected override async Task OnInvoke()
    {
        await base.OnInvoke();
        var logger = IAppHost.GetService<ILoggerFactory>()?.CreateLogger<ElevateNowAction>();
        if (AdminHelper.IsRunningAsAdmin())
        {
            logger?.LogInformation("当前已是管理员身份，跳过提权");
            return;
        }
        logger?.LogInformation("UACHope 立即提权 行动被触发");
        Plugin.ElevateNow(logger);
    }
}
