# 电容检测平台开发技能手册

## 一、项目架构速查

```
CapacitorTestPlatform.sln
├── Core        零依赖层 — 接口、模型、枚举
├── Data        数据层 — Dapper 仓储（SQLite + SQL Server）
├── Devices     设备层 — 串口驱动（MockDriver + 6种真实驱动）
├── Services    业务层 — PlanService / TestService / ReportService / DeviceService
└── UI          展示层 — WPF Pages + ViewModels（MVVM）
```

**依赖方向**: UI → Services → Data → Core（不可反向）

---

## 二、数据流全景

```
┌─────────────────────────────────────────────────────────────────┐
│  远程 SQL Server (apl_contract_plan)                            │
│       │  RemotePlanRepository.FetchPlansAsync()                 │
│       │  SELECT Id, contractNumber AS ContractNumber, ...       │
│       ▼                                                         │
│  PlanService.FetchFromRemoteAsync()                             │
│       │  拆分 testItems 分号 → 多条 PlanInfo                     │
│       │  InsertBatch → 本地 SQLite                               │
│       ▼                                                         │
│  PlanCache 表 (ContractNumber, SampleType, TestItems, ...)      │
│       │                                                         │
│       ▼                                                         │
│  PlanImportView → 选择计划 → TestPageView                       │
│       │                                                         │
│       ▼                                                         │
│  DevicePanelWindow → 选设备 → 配置参数 → 串口采集               │
│       │  MockDriver / TH2689Driver / ...                         │
│       │  返回 DeviceTestResult { Data = { "C(μF)"→852.864 } }   │
│       ▼                                                         │
│  TestDataTable (动态行/列) → 导入回 TestPageView                 │
│       │                                                         │
│       ▼                                                         │
│  保存 → CheckData 表 (PlanNo, CheckName, CheckValue, ...)       │
│  导出 → CSV (UTF-8 BOM)                                         │
└─────────────────────────────────────────────────────────────────┘
```

---

## 三、加远程字段的操作步骤

**场景**: 远程 `apl_contract_plan` 表新增了字段 `priority`，需要在本地使用。

### 步骤

| # | 文件 | 操作 |
|---|------|------|
| 1 | `Core/Models/PlanInfo.cs` | 添加属性 `public string? Priority { get; set; }` |
| 2 | `Data/Repositories/RemotePlanRepository.cs` | SELECT 列表加 `priority AS Priority`（3处） |
| 3 | `Data/Contexts/SQLiteContext.cs` | PlanCache DDL 加 `Priority TEXT` |
| 4 | `Data/Repositories/PlanRepository.cs` | INSERT 列表和 VALUES 加 `Priority` / `@Priority` |
| 5 | UI XAML/ViewModel | 按需绑定 `{Binding Priority}` |

**关键**: AS 别名必须和 PlanInfo 属性名**完全一致**（大小写不敏感但字符要对），否则 Dapper 静默映射为 null。

---

## 四、加新设备型号的操作步骤

**场景**: 新增设备 TH2816A（LCR 数字电桥）。

### 步骤

| # | 文件 | 操作 |
|---|------|------|
| 1 | `Core/Models/DeviceParameterConfig.cs` | `DeviceProfiles[]` 加新配置（参数、下拉选项、默认值） |
| 2 | `Core/Enums/DeviceModel.cs` | 枚举加 `TH2816A` |
| 3 | `Devices/Drivers/TH2816ADriver.cs` | 继承 `DeviceDriverBase`，实现 `ConfigureAsync` / `MeasureAsync` |
| 4 | `Devices/DeviceFactory.cs` | 注册新驱动 |

**参数配置必须严格遵循** `docs/功能指令说明.md` 中的下拉选项、有效范围、默认值。

**MeasureAsync 返回格式**:
```csharp
return DeviceTestResult.Ok(new Dictionary<string, object?>
{
    ["C(μF)"] = 0.102,
    ["频率(Hz)"] = 1000,
    ["损耗(tgδ)"] = 0.0012,
    // ...
});
```

---

## 五、加新测试数据列的操作步骤

**场景**: LCR 设备需要新增 "Q值" 列。

### 步骤

| # | 文件 | 操作 |
|---|------|------|
| 1 | `Devices/Drivers/MockDriver.cs` | `MeasureAsync` 返回值加 `["Q值"] = 150.5` |
| 2 | 对应真实驱动 | `MeasureAsync` 返回值加 `["Q值"] = ...` |

**无需改 UI**: `TestDataTable` 是动态表格，列名从 `DeviceTestResult.Data` 的 Key 自动生成。DataGrid 列通过 `CollectionChanged` 事件自动刷新。

---

## 六、Dapper 映射规则

### 本地 SQLite（PlanCache / CheckData）
- 表名和列名采用 **PascalCase**（如 `ContractNumber`、`PlanNo`）
- C# 属性名与列名**完全一致** → `SELECT *` 自动映射
- INSERT 必须**显式列出列名和参数**，参数名加 `@` 前缀

### 远程 SQL Server（apl_contract_plan）
- 远程表名和列名保持原样（camelCase 如 `contractNumber`）
- 使用 `SELECT xxx AS PropertyName` 显式映射到 C# 属性
- **不要用 `SELECT *`**（远程字段多，只查需要的列）

### 映射失败特征
- 属性值为 `null` / `0` / `false` → 大概率是列名/属性名不匹配
- 检查 AS 别名是否和 PlanInfo 属性名一致

---

## 七、HandyControl 样式陷阱

HandyControl 主题会**覆盖** DataGrid 默认样式。自定义样式必须：

```xml
<Style TargetType="DataGridRow" BasedOn="{x:Null}">
    <!-- 不加 BasedOn="{x:Null}" 则属性不生效 -->
</Style>
```

全局样式定义在 `App.xaml`，影响所有页面的 DataGrid。

---

## 八、动态列绑定模式

`TestPageView` 和 `DevicePanelWindow` 使用代码动态生成 DataGrid 列：

```csharp
// 1. 数据源实现 INotifyPropertyChanged（TestDataRow）
// 2. 索引器绑定
var binding = new Binding(string.Format("[{0}]", col));
// 3. 监听 CollectionChanged 刷新列
TestDataTable.Rows.CollectionChanged += (_, _) => UpdateTestColumns();
```

**不要用** `AutoGenerateColumns="True"` — 会生成额外列，且无法控制样式。

---

## 九、CSV 导出规范

```csharp
// UTF-8 BOM — Excel 打开中文不乱码
var encoding = new UTF8Encoding(true);
using var writer = new StreamWriter(filePath, false, encoding);
// 导出后自动打开
Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
```

---

## 十、DI 注册位置

`App.xaml.cs` → `OnStartup` 方法中：
- 仓储: `services.AddSingleton<IPlanRepository, PlanRepository>()`
- 服务: `services.AddSingleton<IPlanService, PlanService>()`
- ViewModel: `services.AddTransient<PlanImportViewModel>()`
- 页面: `services.AddTransient<PlanImportView>()`

导航: `ContentControl` + `DataTemplate` 绑定 ViewModel 类型，`MainWindowViewModel.CurrentPage` 切换。

---

## 十一、数据库表清单

### 本地 SQLite（4张，PascalCase 命名）

| 表名 | 用途 | 关键字段 |
|------|------|---------|
| `PlanCache` | 远程计划本地缓存 | ContractNumber, SampleType, TestItems, InstrumentNumber |
| `CheckData` | 检测数据主表 | PlanNo, Lot, DeviceId, CheckName, CheckValue, Result |
| `DeviceConfig` | 设备参数配置（预留） | DeviceType, ParamName, ParamValue |
| `ConnectionLog` | 串口连接日志（预留） | DeviceType, Port, BaudRate, Status |

### 远程 SQL Server（保持原样不修改）

| 表名 | 用途 |
|------|------|
| `apl_contract_plan` | 检测计划主表 |
| `TestData` | 测试数据表 |

---

## 十二、构建命令

```bash
dotnet restore          # 还原 NuGet 包
dotnet build            # 编译（如有文件锁定先 taskkill 进程再 dotnet clean）
dotnet run --project src/CapacitorTestPlatform.UI  # 启动 UI
```

**常见构建问题**: WPF 进程锁定 DLL → `taskkill //F //IM CapacitorTestPlatform.UI.exe` → `dotnet clean` → `dotnet build`

---

## 十三、数据模板与测试项映射（核心业务规则）

### 数据模板结构

`docs/数据模板.xlsx` 定义了导出格式，每个 Sheet = 一种设备类型，Sheet 内可包含多个独立表格：

**Sheet: LCR数字电桥**（4个独立表格）

| 表格 | 数据列 | 判定标准（来源:远程表） |
|------|--------|----------------------|
| 电容量 | 序号, C(μF), 频率(HZ) | 标称值, 容量正偏差(±20%), 容量上限, 容量下限 |
| 损耗 | 序号, 损耗, 频率(HZ) | 损耗标准(≤0.2) |
| ESR | 序号, ESR(MΩ), 频率(HZ) | ESR标准(≤0.50) |
| 阻抗 | 序号, 阻抗(MΩ), 频率(HZ) | 阻抗标准(≤8) |

**Sheet: 漏电流测试仪**（1个表格）

| 数据列 | 判定标准（来源:远程表） |
|--------|----------------------|
| 序号, IL正向(μA), HL(S), 反向(μA), HL(S) | 漏电流标准(≤2012) |

**Sheet: 绝缘电阻测试仪**（1个表格）

| 数据列 | 判定标准（来源:远程表） |
|--------|----------------------|
| 序号, 绝缘电阻(MΩ) | 绝缘电阻判定值(≥100) |

### testItems → 设备 → 数据列 完整映射

| # | testItems（实验项目） | 推荐设备 | 数据列（设备采集） | 判定标准（远程表） |
|---|---------------------|---------|-------------------|------------------|
| 1 | 电容量 | LCR数字电桥 | C(μF), 频率(HZ) | 标称值, 容量正偏差, 容量上限, 容量下限 |
| 2 | 损耗角正切值(tgδ) | LCR数字电桥 | 损耗, 频率(HZ) | 损耗标准 |
| 3 | 等效串联电阻(ESR) | LCR数字电桥 | ESR(MΩ), 频率(HZ) | ESR标准 |
| 4 | 阻抗 | LCR数字电桥 | 阻抗(MΩ), 频率(HZ) | 阻抗标准 |
| 5 | 绝缘外套的绝缘电阻 | 绝缘电阻测试仪 | 绝缘电阻(MΩ) | 绝缘电阻判定值 |
| 6 | 漏电流 | 漏电流测试仪 | IL正向(μA), HL(S), IL反向(μA), HL(S) | 漏电流标准 |
| 7 | 可靠性前性能实验 | **用户自选** | 按所选设备决定 | 按所选设备决定 |
| 8 | 可靠性后性能实验 | **用户自选** | 按所选设备决定 | 按所选设备决定 |

### 业务规则

1. **设备可选，系统不限制**: testItems 只做推荐设备，用户可自由选择其他设备
2. **用户选什么设备，就用什么参数和数据列**: 系统根据所选设备的 `DeviceParameterConfig` 设置参数，根据设备驱动的 `MeasureAsync` 返回值确定数据列
3. **判定标准来自远程表**: 标称值、容量正偏差、上限、下限、损耗标准等字段从远程 `apl_contract_plan` 表获取，不是设备采集的
4. **可靠性实验可选多个设备**: 可靠性前/后性能实验允许用户选择多个设备，每个设备生成一个独立表格（Sheet）
5. **同一设备多个 testItems**: 如电容量+损耗+ESR+阻抗都用 LCR数字电桥，在同一个 Sheet 内生成多个独立表格（每个 testItem 一个表格）

### 数据来源分类

| 数据类型 | 来源 | 示例 |
|---------|------|------|
| 采集数据 | 设备驱动 MeasureAsync | C(μF)=852.864, 绝缘电阻=658.68MΩ |
| 规格标准 | 远程表 apl_contract_plan | 标称值=1000, 容量上限=1200, 容量下限=800 |
| 判定结果 | 系统比较采集值与规格标准 | PASS / FAIL |
| 序号 | 系统自增 | 1, 2, 3, ... |

---

## 十四、PlanInfo 字段对照表

| C# 属性 | 远程字段 | PlanCache 列 | 说明 |
|---------|---------|-------------|------|
| ContractNumber | contractNumber | ContractNumber | 合同号 |
| SampleType | sampleType | SampleType | 样品型号 |
| TestItems | testItems | TestItems | 实验项目（拆分后单值） |
| InstrumentNumber | instrumentNumber | InstrumentNumber | 工站编号 |
| StatusName | statusName | StatusName | 计划状态 |
| Lot | — | Lot | 批次号（本地扩展） |
| DeviceId | — | DeviceId | 设备编号（本地扩展） |
| SpecMin/SpecMax | — | SpecMin/SpecMax | 规格上下限（本地扩展） |

---

## 十五、常用 SCPI 指令

```
*IDN?           → 识别设备（返回型号、序列号等）
CONF:VOLT 500   → 配置测试电压 500V
CONF:SPED FAST  → 配置测试速度
FETCH?          → 读取测量结果
INIT            → 触发测试
*RST            → 复位设备
```

换行符 `\n` 分隔命令，响应以 `\n` 结尾。
