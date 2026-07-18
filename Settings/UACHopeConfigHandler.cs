using System;
using System.ComponentModel;
using System.IO;
using ClassIsland.Shared.Helpers;

namespace UACHope.Settings;

/// <summary>
/// UACHope 插件配置的加载与持久化。
/// 任意属性变更都会自动写回 Main.json。
/// </summary>
public class UACHopeConfigHandler
{
    private readonly string _configPath;

    public UACHopeConfig Data { get; }

    public UACHopeConfigHandler(string pluginConfigFolder)
    {
        _configPath = Path.Combine(pluginConfigFolder, "Main.json");
        Data = ConfigureFileHelper.LoadConfig<UACHopeConfig>(_configPath);
        Data.PropertyChanged += OnDataPropertyChanged;
    }

    private void OnDataPropertyChanged(object? sender, PropertyChangedEventArgs e) => Save();

    public void Save()
    {
        ConfigureFileHelper.SaveConfig(_configPath, Data);
    }
}
