# testItems → 设备推荐 → 动态数据列 实施方案

## Context

当前系统存在3个断层：
1. DevicePanelWindow 不知道当前测试的是哪个 testItem，无法推荐设备
2. 数据列由设备驱动的 OutputFields 决定（如 "LC"、"IR"），不是模板中的友好列名（如 "C(μF)"、"绝缘电阻(MΩ)"）
3. PlanInfo 的规格标准（标称值、上限、下限）没有传递到测试页面用于判定

## 改动概览

### 1. 传递 testItem 和 PlanInfo 到 DevicePanelWindow

**TestPageViewModel.cs**
- 保留完整 `PlanInfo` 对象（新增 `_currentPlan` 字段）
- `AcquireData` 事件传递 `testItem` 和 `plan` 给 View

**TestPageView.xaml.cs**
- `OnRequestDeviceSelect` 创建 DevicePanelWindow 时传入 `testItem` 和 `plan`

**DevicePanelViewModel.cs**
- 构造函数新增 `string testItem` 和 `PlanInfo plan` 参数
- 存储 `_testItem` 和 `_plan` 用于推荐和判定

### 2. testItem 推荐设备映射

在 `DevicePanelViewModel` 中新增推荐逻辑：

```csharp
private static readonly Dictionary<string, string> TestItemDeviceMapping = new()
{
    ["电容量"] = "TH2817A",
    ["损耗角正切值(tgδ)"] = "TH2817A",
    ["损耗角正切"] = "TH2817A",
    ["等效串联电阻(ESR)"] = "TH2832",
    ["ESR"] = "TH2832",
    ["阻抗"] = "TH2832",
    ["绝缘外套的绝缘电阻"] = "TH2689",
    ["漏电流"] = "TH2683A",
    ["可靠性前性能实验"] = null,  // 不推荐，用户自选
    ["可靠性后性能实验"] = null,  // 不推荐，用户自选
};
```

`LoadProfiles()` 中：如果 testItem 有推荐设备，将推荐设备排在列表第一位并自动选中。

### 3. 数据列根据设备 + testItem 动态确定

**DeviceService.GetColumnsForTestItem()** 已有映射逻辑，需要激活使用。

**DevicePanelViewModel.MeasureAsync()**
- 测量前，根据 `_testItem` + 所选设备调用 `GetColumnsForTestItem` 获取列定义
- 调用 `ResultTable.RegisterColumns(columns)` 预注册列名
- 测量后，将设备返回的原始 Key 映射到模板列名

**MockDriver / 真实驱动**
- MeasureAsync 返回的 Key 统一改为模板列名（如 "C(μF)" 而非 "LC"）

### 4. PlanInfo 规格标准传递

**TestPageViewModel**
- 保留 `_currentPlan`，从中读取 SpecMin/SpecMax/SpecValue 用于判定
- 测试数据导入时，将规格标准附带到 TestDataTable 的列定义中

**判定逻辑**
- 采集值与 SpecMin/SpecMax 比较，自动生成 Result（PASS/FAIL）
- 模板中的"判定标准"行由 PlanInfo 的规格字段填充

## 涉及文件

| 文件 | 改动 |
|------|------|
| `UI/ViewModels/TestPageViewModel.cs` | 保留完整 PlanInfo，传递 testItem+plan 给设备弹窗 |
| `UI/Views/TestPageView.xaml.cs` | 传递 testItem+plan 给 DevicePanelWindow |
| `UI/ViewModels/DevicePanelViewModel.cs` | 接收 testItem+plan，推荐设备逻辑，列名映射 |
| `UI/Views/DevicePanelWindow.xaml.cs` | 构造函数接收 testItem+plan |
| `Services/DeviceService.cs` | GetColumnsForTestItem 补充完整映射 |
| `Devices/Drivers/MockDriver.cs` | MeasureAsync 返回模板列名 |
| `Core/Models/DeviceParameterConfig.cs` | OutputFields 改为模板列名 |
| `UI/Models/TestDataTable.cs` | RegisterColumns 支持列定义预注册 |

## 不改动

- 远程表/本地表结构不变
- PlanInfo 模型不变
- 真实设备驱动暂不改（后续接真实设备时按模板列名输出）
- CSV 导出逻辑不变

## 执行步骤

1. DevicePanelViewModel — 接收 testItem+plan，添加推荐设备映射
2. DevicePanelWindow — 构造函数签名更新
3. TestPageViewModel — 保留 PlanInfo，传递 testItem+plan
4. TestPageView — 传递参数给设备弹窗
5. DeviceService — 完善 GetColumnsForTestItem 映射
6. MockDriver — 返回模板列名
7. DeviceParameterConfig — OutputFields 改为模板列名
8. dotnet build 验证
