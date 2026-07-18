using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using ClassIsland.Core.Abstractions.Controls;
using UACHope.Rules;

namespace UACHope.Controls;

/// <summary>
/// "当前为管理员身份运行"规则的设置控件。
/// 此规则无需配置，仅展示当前进程身份状态以便用户验证。
/// </summary>
public class AdminRunningRuleSettingsControl : RuleSettingsControlBase<AdminRunningRuleSettings>
{
    private readonly TextBlock _statusText;
    private readonly TextBlock _detailText;

    public AdminRunningRuleSettingsControl()
    {
        var panel = new StackPanel
        {
            Spacing = 10,
            Margin = new Thickness(12)
        };

        var title = new TextBlock
        {
            Text = "此规则无需配置",
            FontWeight = FontWeight.Bold
        };
        panel.Children.Add(title);

        _statusText = new TextBlock
        {
            FontSize = 14,
            TextWrapping = TextWrapping.Wrap
        };
        panel.Children.Add(_statusText);

        _detailText = new TextBlock
        {
            FontSize = 12,
            TextWrapping = TextWrapping.Wrap,
            Foreground = Brushes.Gray
        };
        panel.Children.Add(_detailText);

        var refreshButton = new Button
        {
            Content = "刷新当前状态",
            Width = 140,
            HorizontalAlignment = HorizontalAlignment.Left,
            Margin = new Thickness(0, 6, 0, 0)
        };
        refreshButton.Click += (_, _) => UpdateStatus();
        panel.Children.Add(refreshButton);

        Content = panel;
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        UpdateStatus();
    }

    private void UpdateStatus()
    {
        var isAdmin = AdminHelper.IsRunningAsAdmin();
        _statusText.Text = isAdmin
            ? "✓ 当前进程以管理员身份运行"
            : "✗ 当前进程未以管理员身份运行";
        _statusText.Foreground = isAdmin ? Brushes.SeaGreen : Brushes.IndianRed;
        _detailText.Text = "规则会在每次规则集重算时调用 AdminHelper.IsRunningAsAdmin() 实时判定。";
    }
}
