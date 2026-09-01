# CapacitorTestPlatform

WPF 上位机电容检测平台 — .NET 8.0，通过串口控制绝缘/漏电流/耐压/电容测试仪器，完成参数下发 → 数据采集 → 本地/远程存储的自动化流程。

---

## 技术栈

- **UI 框架**: WPF + HandyControl 3.5.1
- **MVVM**: CommunityToolkit.Mvvm 8.x
- **数据库**: SQLite (Dapper) + SQL Server (远程同步)
- **串口通信**: System.IO.Ports
- **DI**: Microsoft.Extensions.DependencyInjection

## 构建与运行

```bash
dotnet restore
dotnet build
# 运行 UI 项目（启动入口）
dotnet run --project src/CapacitorTestPlatform.UI
```

---

## 解决方案结构

```
CapacitorTestPlatform.sln
├── Core        (接口、模型、枚举 — 零依赖)
├── Data        (Dapper 仓储 — SQLiteContext / SqlServerContext)
├── Devices     (串口驱动 — MockDriver + 6 种真实驱动)
├── Services    (业务编排 — PlanService / TestService / ReportService)
└── UI          (WPF 入口 — Pages + ViewModels)
```

| 层            | 关键类                                                       |
| ------------- | ------------------------------------------------------------ |
| Core          | `IDeviceDriver`, `DeviceParameterConfig`, `TestRecord`, `PlanInfo` |
| Data          | `PlanRepository`, `TestRecordRepository`, `RemotePlanRepository` |
| Devices       | `DeviceDriverBase`, `MockDriver`, `TH2689Driver` … `TH9201Driver` |
| Services      | `PlanService`, `TestService`, `ReportService`, `DeviceService` |
| UI/Views      | `MainWindow`, `PlanImportView`, `TestPageView`, `HistoryView` |
| UI/ViewModels | `MainWindowViewModel`, `PlanImportViewModel`, `TestPageViewModel`, `HistoryViewModel`, `DevicePanelViewModel` |

---

## 核心业务流程

```
计划导入 (PlanImportView)
    ↓ 选择工站 → 拉取远程计划 → 选中保存到 SQLite
测试页面 (TestPageView)
    ↓ 选择计划 → 打开设备采集弹窗
    ↓ 设备弹窗 (DevicePanelWindow)
    ↓   选型号 → 配置参数 → 连接串口 → 采集
    ↓   测试数据存入 TestDataTable
    ↓ 导入回 TestPageView（动态列，按设备类型展开）
    ↓ 逐行保存/导出 CSV
历史记录 (HistoryView) — 查询 TBL_CHECKDATA
```

---

## 设备参数配置

**重要**: 设备参数必须与 `docs/功能指令说明.md` 严格一致（下拉选项、有效范围、默认值）。

| 设备型号 | 设备分类     | 关键参数                                          |
| -------- | ------------ | ------------------------------------------------- |
| TH2689   | 绝缘电阻     | Voltage(1~800V), Speed, Range, ChgTime(0~999s)    |
| TH2683A  | 漏电流       | Voltage, Current, Speed, Range, Bias, ChgTime     |
| TH2817A  | LCR 数字电桥 | Frequency, Speed, Level, Equivalent, Range        |
| TH2832   | LCR 数字电桥 | Frequency, Speed, Level, Equivalent, BiasV, Range |
| TH9201   | 极壳耐压     | Voltage, Current, Delay, TestTime                 |
| TH2810B  | LCR 数字电桥 | Frequency, Speed, Level, Equivalent, Range        |
| MOCK     | (开发模拟)   | 任何参数均可自由配置                              |

配置存储: `DeviceParameterConfig.DeviceProfiles[]`

---

## 测试数据模型

`TestDataTable` 是测试数据的动态表格容器:

- `TestDataTable.ColumnOrder`: `List<string>` — 列名有序集合
- `TestDataTable.Rows`: `ObservableCollection<TestDataRow>`
- `TestDataRow[string key]`: 索引器，按列名读写值
- `TestDataRow.SeqNo`: 带 PropertyChanged 通知，删除行后自动重编号

**设备 → 测试项映射**:

| 测试项   | 设备         | 数据列                                        |
| -------- | ------------ | --------------------------------------------- |
| 电容量   | LCR数字电桥  | C(μF), 频率(Hz), 损耗(tgδ), ESR(MΩ), 阻抗(MΩ) |
| 漏电流   | 漏电流测试仪 | IL正向(μA), HL正向(S), IL反向(μA), HL反向(S)  |
| 绝缘电阻 | 绝缘测试仪   | LC(μF), IR(MΩ)                                |
| 极壳耐压 | 耐压测试仪   | 步骤, 电压(V), 结果                           |

---

## 本地数据库表

- `TBL_CHECKDATA` — 主存储表（PLAN_CODE, LOT, DEVICE_ID, DEVICE_TYPE, CHECK_NAME, CHECK_VALUE, RESULT, TEST_DT…）
- `TBL_CHECKPROJECT` — 检测项目表
- `TBL_PRODUCTS` — 产品信息表

远程同步使用 `SqlServerContext`（ReadConn / WriteConn 双连接串）。

---

## 重要约定

### HandyControl 样式

HandyControl 主题会覆盖 DataGrid 默认样式。需要自定义 DataGrid 样式时，**必须**加 `BasedOn="{x:Null}"` 打断继承链，否则属性不生效。全局样式定义在 `App.xaml`。

### 串口通信协议

使用 SCPI 指令，换行符 `\n` 分隔命令。发送 `*IDN?` 识别设备，`FETCH?` 读取测量结果。

### 动态列生成

`TestPageView.xaml.cs` 和 `DevicePanelWindow.xaml.cs` 使用代码动态生成 DataGrid 列:

- `AutoGenerateColumns="False"` + `DataGridTemplateColumn`
- 通过 `Binding(string.Format("[{0}]", col))` 绑定 `TestDataRow` 索引器
- `TestDataTable.Rows.CollectionChanged` 触发列刷新

### CSV 导出

使用 `SaveFileDialog`，UTF-8 BOM 编码（`new UTF8Encoding(true)`），导出后自动打开文件。

### DI 注册

`App.xaml.cs` 中注册所有服务和 ViewModel。页面导航使用 `ContentControl` + `DataTemplate` 绑定 ViewModel 类型。

---

## 文档参考

- `docs/功能指令说明.md` — 所有设备的 SCPI 指令、参数选项、有效范围（**必须严格遵守**）
- `docs/远程插入表代码.md` — 远程数据库表结构和同步逻辑
- `docs/数据模板.xlsx` — 数据导出格式参考
