# CyreneMvvm

一个轻量级 MVVM 框架，通过源代码生成器自动实现属性变更通知。

[![NuGet](https://img.shields.io/nuget/v/CyreneMvvm.svg)](https://www.nuget.org/packages/CyreneMvvm/)
[![License](https://img.shields.io/github/license/CyreneLaboratory/CyreneMvvm)](https://github.com/CyreneLaboratory/CyreneMvvm/blob/main/LICENSE.txt)

## 特性

- **零标记** - 继承 `ObObject` 后，所有 `partial` 自动属性自动生成变更通知，无需逐个标记
- **编译期校验** - 漏写 `partial` 或使用不受支持的类型会直接编译报错，避免静默错误
- **AOT 兼容** - 完全支持 Native AOT 编译
- **Observable 集合** - 提供 `ObList<T>` 和 `ObDictionary<TKey, TValue>`，并支持了绝大部分API
- **级联通知** - 子对象变更时自动通知父对象

## 安装

```bash
dotnet add package CyreneMvvm
```

## 快速开始

### 基本用法

- 继承 `ObObject`，类和属性均声明为 `partial`，生成器会自动为每个自动属性生成通知代码，无需任何特性
- 属性类型必须是以下四类之一：基元类型（含 `enum`、`string`）、`ObObject` 子类、`ObList<T>`、`ObDictionary<TKey, TValue>`
- 不希望被生成的属性，用 `[ObIgnore]` 显式排除

```csharp
using CyreneMvvm.Model;
using CyreneMvvm.Attributes;

public partial class Test : ObObject
{
    public partial int Int { get; set; }
    public partial string? String { get; set; }
    public partial Internal? Object { get; set; }
    [ObIgnore] public Test? Parent { get; set; } // 显式排除，不生成通知
}

public partial class Internal : ObObject;

```

### 编译期校验

生成器会对 `ObObject` 子类里的自动属性强制校验，违规直接报错（而非静默跳过）：

- **CYM001** - 属性类型不受支持（例如原生 `List<T>`、`Dictionary<TKey,TValue>`、数组）。原生集合无法监控内部变更，必须替换为 `ObList`/`ObDictionary`（或用 `[ObIgnore]` 退出）

### 级联通知

- 使用 `ObList<T>` 和 `ObDictionary<TKey, TValue>`
- 暂时没有支持针对ObDictionary中TKey的级联通知，在TKey中使用Ob集合无效

```csharp
public partial class Test : ObObject
{
    public partial ObList<int> List { get; set; } = [];
    public partial ObDictionary<int, int> Dict { get; set; } = [];
    public partial ObDictionary<int, ObList<ObDictionary<int, ObList<int>>>> Complex { get; set; } = [];

    public void Register()
    {
        Dict.PropertyChanged += (sender, e) => Console.WriteLine($"{e.PropertyName}");
    }
}

```

### Sql列标记 (定制)

- 在类上添加 `[ObShadow]` 特性，生成器会为非基元属性自动生成对应的 FreeSql 列影子属性（Json 序列化存储）
- 这是一个高度定制化的功能，仅适用于以Json并使用FreeSql的项目, 暂时不支持自定义列标记规则

## 要求

- .NET 8.0 或更高版本
- C# 12 或更高版本

## 许可证

本项目采用 [MIT](LICENSE.txt)。

## 链接

- [NuGet](https://www.nuget.org/packages/CyreneMvvm/)
- [GitHub](https://github.com/CyreneLaboratory/CyreneMvvm)
- [作者](https://github.com/Letheriver2007)
