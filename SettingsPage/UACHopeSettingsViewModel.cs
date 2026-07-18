using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using UACHope.Settings;

namespace UACHope.SettingsPage;

public enum UACHopeFeatureType
{
    Rule,
    Function
}

public partial class UACHopeFeatureItem : ObservableObject
{
    [ObservableProperty] private string _id = string.Empty;
    [ObservableProperty] private string _displayName = string.Empty;
    [ObservableProperty] private bool _isEnabled = true;
    [ObservableProperty] private UACHopeFeatureType _itemType;
    [ObservableProperty] private string? _groupName;

    public string TypeDisplayName => ItemType switch
    {
        UACHopeFeatureType.Rule => "规则",
        UACHopeFeatureType.Function => "功能",
        _ => "未知"
    };
}

public partial class UACHopeSettingsViewModel : ObservableObject
{
    [ObservableProperty] private UACHopeConfig _config;
    [ObservableProperty] private UACHopeConfigHandler _configHandler;
    [ObservableProperty] private ObservableCollection<UACHopeFeatureItem> _featureItems = new();

    // Drawer 状态
    [ObservableProperty] private bool _isFeatureDrawerOpen;
    [ObservableProperty] private object? _featureDrawerContent;

    public UACHopeSettingsViewModel(UACHopeConfig config, UACHopeConfigHandler configHandler)
    {
        _config = config;
        _configHandler = configHandler;
        InitializeFeatureItems();
    }

    /// <summary>
    /// 注册的全部"功能"列表。新增 rule/feature 时在这里追加。
    /// </summary>
    public void InitializeFeatureItems()
    {
        FeatureItems.Clear();

        // 自动化行动
        var actions = new List<(string Id, string Name)>
        {
            ("uachope.elevateNow", "立即提权（行动）"),
            ("uachope.requestSkipNextStartup", "标记下次跳过提权（行动）"),
        };
        foreach (var (id, name) in actions)
        {
            FeatureItems.Add(new UACHopeFeatureItem
            {
                Id = id,
                DisplayName = name,
                IsEnabled = Config.IsActionEnabled(id),
                ItemType = UACHopeFeatureType.Function,
                GroupName = "自动化行动"
            });
        }

        // 规则
        var rules = new List<(string Id, string Name, string? Group)>
        {
            ("UACHope.AdminRunning", "当前为管理员身份运行", "规则"),
        };
        foreach (var (id, name, group) in rules)
        {
            FeatureItems.Add(new UACHopeFeatureItem
            {
                Id = id,
                DisplayName = name,
                IsEnabled = Config.IsRuleEnabled(id),
                ItemType = UACHopeFeatureType.Rule,
                GroupName = group
            });
        }
    }

    /// <summary>
    /// 将 Drawer 中的编辑结果回写到 Config 并落盘。功能类条目只读，不写回。
    /// </summary>
    public void SaveFeatureSettings()
    {
        foreach (var item in FeatureItems)
        {
            switch (item.ItemType)
            {
                case UACHopeFeatureType.Rule:
                    Config.EnabledRules[item.Id] = item.IsEnabled;
                    break;
                case UACHopeFeatureType.Function:
                    // 自动化行动的启用情况由 EnabledActions 维护
                    Config.EnabledActions[item.Id] = item.IsEnabled;
                    break;
            }
        }
        // 字典内部修改不会触发 PropertyChanged，强制落盘
        ConfigHandler.Save();
    }
}
