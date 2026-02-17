# CyreneMvvm

一个轻量级 MVVM 框架，通过源代码生成器自动实现属性变更通知。

[![NuGet](https://img.shields.io/nuget/v/CyreneMvvm.svg)](https://www.nuget.org/packages/CyreneMvvm/)
[![License](https://img.shields.io/github/license/CyreneLaboratory/CyreneMvvm)](https://github.com/CyreneLaboratory/CyreneMvvm/blob/main/LICENSE.txt)

## 特性

- **简单易用** - 与官方工具 `CommunityToolkit.Mvvm` 的属性标记方式完全相同
- **AOT 兼容** - 完全支持 Native AOT 编译
- **Observable 集合** - 提供 `ObList<T>` 和 `ObDictionary<TKey, TValue>`，并支持了绝大部分API
- **级联通知** - 子对象变更时自动通知父对象

## 安装

```bash
dotnet add package CyreneMvvm
```

## 快速开始

### 基本用法

- 继承 `ObObject` 并使用 `[ObProp]` 特性标记需要通知的属性
- 类和属性必须声明为 `partial`，否则不会报错，但也不会生成通知代码

```csharp
using CyreneMvvm.Model;
using CyreneMvvm.Attributes;

public partial class Test : ObObject
{
    [ObProp] public partial int Int { get; set; }
    [ObProp] public string? String { get; set; }
    [ObProp] public Internal? Object { get; set; }
}

public partial class Internal : ObObject;

```

### 级联通知

- 使用 `ObList<T>` 和 `ObDictionary<TKey, TValue>`
- 暂时没有支持针对ObDictionary中TKey的级联通知，在TKey中使用Ob集合无效

```csharp
public partial class Test : ObObject
{
    [ObProp] public ObList<int> List { get; set; } = [];
    [ObProp] public ObDictionary<int, int> Dict { get; set; } = [];
    [ObProp] public ObDictionary<int, ObList<ObDictionary<int, ObList<int>>>> Complex { get; set; } = [];

    public void Register()
    {
        Dict.PropertyChanged += (sender, e) => Console.WriteLine($"{e.PropertyName}");
    }
}

```

### Sql列标记 (定制)

- 为属性添加 `[ObColumn]` 以根据属性自动生成对应的列标记
- 这是一个高度定制化的功能，仅适用于以Json并使用SugarSql的项目, 暂时不支持自定义列标记规则

## 要求

- .NET 8.0 或更高版本
- C# 12 或更高版本

## 许可证

本项目采用 [MIT](LICENSE.txt)。

## 链接

- [NuGet](https://www.nuget.org/packages/CyreneMvvm/)
- [GitHub](https://github.com/CyreneLaboratory/CyreneMvvm)
- [作者](https://github.com/Letheriver2007)
