namespace UACHope;

public partial class Plugin
{
    private static bool HandleAdminRunningRule(object? settings)
    {
        // settings 类型为 AdminRunningRuleSettings，但此规则无配置项，无需读取
        return AdminHelper.IsRunningAsAdmin();
    }
}
