using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace UACHope.Settings;

/// <summary>
/// UACHope 插件的配置数据。
/// </summary>
public class UACHopeConfig : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private bool _isAutoElevateEnabled = true;

    /// <summary>
    /// 是否在 ClassIsland 启动时自动提权。关闭后插件将完全静默，不执行任何提权操作。
    /// </summary>
    [JsonPropertyName("isAutoElevateEnabled")]
    public bool IsAutoElevateEnabled
    {
        get => _isAutoElevateEnabled;
        set
        {
            if (value == _isAutoElevateEnabled) return;
            _isAutoElevateEnabled = value;
            OnPropertyChanged();
        }
    }

    private bool _skipElevationNextStartup = false;

    /// <summary>
    /// 下一次启动跳过提权。一次性开关，使用后会自动重置为 false。
    /// </summary>
    [JsonPropertyName("skipElevationNextStartup")]
    public bool SkipElevationNextStartup
    {
        get => _skipElevationNextStartup;
        set
        {
            if (value == _skipElevationNextStartup) return;
            _skipElevationNextStartup = value;
            OnPropertyChanged();
        }
    }

    private Dictionary<string, bool> _enabledRules = new()
    {
        ["UACHope.AdminRunning"] = true
    };

    /// <summary>
    /// 各规则的启用情况。Key 为规则 id，Value 为是否启用。
    /// 修改后需要重启插件才生效。
    /// </summary>
    [JsonPropertyName("enabledRules")]
    public Dictionary<string, bool> EnabledRules
    {
        get => _enabledRules;
        set
        {
            _enabledRules = value;
            OnPropertyChanged();
        }
    }

    public bool IsRuleEnabled(string id) => EnabledRules.GetValueOrDefault(id, true);

    private Dictionary<string, bool> _enabledActions = new()
    {
        ["uachope.elevateNow"] = true,
        ["uachope.requestSkipNextStartup"] = true
    };

    /// <summary>
    /// 各自动化行动的启用情况。Key 为行动 id，Value 为是否启用。
    /// 修改后需要重启插件才生效。
    /// </summary>
    [JsonPropertyName("enabledActions")]
    public Dictionary<string, bool> EnabledActions
    {
        get => _enabledActions;
        set
        {
            _enabledActions = value;
            OnPropertyChanged();
        }
    }

    public bool IsActionEnabled(string id) => EnabledActions.GetValueOrDefault(id, true);

    private void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
