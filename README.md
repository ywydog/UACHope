<div align="center">

# UACHope

[![GitHub](https://img.shields.io/badge/GitHub-%23121011.svg?logo=github&logoColor=white)](https://github.com/Programmer-MrWang/UACHope)
![GitHub License](https://img.shields.io/github/license/Programmer-MrWang/UACHope)
![GitHub Release](https://img.shields.io/github/v/release/Programmer-MrWang/UACHope?include_prereleases)

**ClassIsland 自动管理员提权插件 — 启动即提权，无需手动操作**

</div>

---

## 功能

- 启动 ClassIsland 时自动检测管理员权限，非管理员则自动以管理员身份重启
- 透传所有原始命令行参数，不影响 ClassIsland 正常启动流程
- 双重防无限重启保护机制
- UAC 被用户拒绝时安全降级，当前进程继续以普通权限运行
- 绿色无痕，不依赖计划任务或系统服务
- 零第三方依赖，仅依赖 ClassIsland 插件 SDK

## 防无限重启机制

| 保护层 | 机制 | 说明 |
|--------|------|------|
| 第一层 | `--admin-restart` 标记 | 重启时附加此参数，若新实例仍非管理员则说明 UAC 被拒绝，立即放弃 |
| 第二层 | 重启计数文件 | 60 秒内重启超过 3 次则自动放弃，防止极端情况下的循环重启 |

## 安装

在 ClassIsland 的插件市场搜索 **UACHope** 并安装，或将构建产物 `.cipx` 文件手动导入。

## 构建

需要 .NET 8 SDK，在 Windows 上执行：

```bash
dotnet restore
dotnet publish -p:CreateCipx=true --configuration Release
```

构建产物位于 `cipx/UACHope.cipx`。

## 声明

- 此插件仅适用于 Windows 平台。
- 此插件适用于 ClassIsland 2.x（≥2.0.0.1）版本。
- LGPLv3 许可。[LICENSE](./LICENSE)
