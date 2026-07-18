using System.Runtime.Versioning;
using System.Security.Principal;

namespace UACHope;

[SupportedOSPlatform("windows")]
public static class AdminHelper
{
    public static bool IsRunningAsAdmin()
    {
        var identity = WindowsIdentity.GetCurrent();
        var principal = new WindowsPrincipal(identity);
        return principal.IsInRole(WindowsBuiltInRole.Administrator);
    }
}
