using Avalonia.Interactivity;
using ClassIsland.Core;
using ClassIsland.Core.Abstractions.Controls;
using ClassIsland.Core.Attributes;
using ClassIsland.Shared;
using UACHope.Settings;

namespace UACHope.SettingsPage;

[SettingsPageInfo("uachope.settings.main", "UACHope 设置", "\uEF6E", "\uEF6E")]
[HidePageTitle]
public partial class UACHopeSettingsPage : SettingsPageBase
{
    public UACHopeSettingsPage()
    {
        var configHandler = IAppHost.GetService<UACHopeConfigHandler>();
        ViewModel = new UACHopeSettingsViewModel(configHandler.Data, configHandler);
        DataContext = this;
        InitializeComponent();
    }

    public UACHopeSettingsViewModel ViewModel { get; }

    private void OnManageFeaturesClick(object? sender, RoutedEventArgs e)
    {
        // 重新刷新一次，确保与最新配置同步
        ViewModel.InitializeFeatureItems();
        ViewModel.FeatureDrawerContent = new object();
        ViewModel.IsFeatureDrawerOpen = true;
    }

    private void OnCloseDrawerClick(object? sender, RoutedEventArgs e)
    {
        ViewModel.IsFeatureDrawerOpen = false;
    }

    private void OnSaveFromDrawerClick(object? sender, RoutedEventArgs e)
    {
        ViewModel.SaveFeatureSettings();
        ViewModel.IsFeatureDrawerOpen = false;
        RequestRestart();
    }
}
