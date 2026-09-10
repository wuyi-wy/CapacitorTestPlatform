# 电容测试平台 WPF 上位机 — Claude Code 生成 Prompt

> **使用方式**：将此文件内容作为单次或分步 Prompt 发给 Claude Code，建议按"生成顺序"分 8 步执行，每步编译通过后再下一步。

---

## 项目概述

生成一个 **电容测试平台 WPF 上位机**，用于电容器各项性能指标的测试数据采集、管理和上报四代系统。

---

## 技术栈

| 项目 | 选型 |
|------|------|
| 框架 | .NET 6.0 WPF |
| 远程数据库 | SQL Server（计划导入 + 数据上报） |
| 本地数据库 | SQLite（离线缓存 + 测试数据 + 历史记录） |
| 依赖注入 | Microsoft.Extensions.DependencyInjection |
| MVVM | CommunityToolkit.Mvvm |
| ORM | Dapper（轻量，适合本场景） |
| UI 控件库 | HandyControl（NuGet: `HandyControl`） |
| 串口通信 | System.IO.Ports |

---

## 项目结构

```
CapacitorTestPlatform/
├── src/
│   ├── CapacitorTestPlatform.Core/              # 领域模型、接口、枚举
│   │   ├── Models/                              # 实体模型
│   │   ├── Interfaces/                          # 服务接口
│   │   └── Enums/                               # 枚举定义
│   ├── CapacitorTestPlatform.Data/              # 数据库上下文、仓储
│   │   ├── Contexts/                            # SQLiteContext, SqlServerContext
│   │   ├── Repositories/                        # 仓储实现
│   │   └── Migrations/                          # 数据库迁移
│   ├── CapacitorTestPlatform.Services/          # 业务逻辑服务
│   │   ├── PlanService.cs                       # 计划导入服务
│   │   ├── TestService.cs                       # 测试数据服务
│   │   ├── DeviceService.cs                     # 设备管理服务
│   │   └── ReportService.cs                     # 远程上报服务
│   ├── CapacitorTestPlatform.Devices/           # 设备通信驱动层
│   │   ├── SerialPortService.cs                 # 串口通信封装
│   │   ├── DeviceFactory.cs                     # 设备工厂
│   │   ├── IDriver.cs                           # 设备驱动接口
│   │   └── Drivers/                             # 各设备驱动实现
│   │       ├── TH2689Driver.cs
│   │       ├── TH2683ADriver.cs
│   │       ├── TH2817ADriver.cs
│   │       ├── TH2832Driver.cs
│   │       ├── TH9201Driver.cs
│   │       └── TH2810BDriver.cs
│   └── CapacitorTestPlatform.UI/                # WPF 界面
│       ├── App.xaml.cs                          # DI 容器配置
│       ├── ViewModels/                          # ViewModel 层
│       ├── Views/                               # View 层
│       ├── Converters/                          # 值转换器
│       └── Controls/                            # 自定义控件
├── docs/
│   ├── 功能指令说明.md                           # 设备 SCPI 指令集（已提供）
│   ├── 远程插入表代码.md                         # 远程上报逻辑（已提供）
│   └── 数据模板.xlsx                             # 各实验项目数据列模板（已提供）
└── CapacitorTestPlatform.sln
```

---

## 模块一：计划导入（Plan Import）

### 数据源
- **远程 SQL Server**：`Server=10.2.4.195;Database=GreeTestCenter;User Id=ilims;Password=***`
- **主表**：`apl_contract_plan`
- **关键字段**：`testItems`（实验项目集合，分号分隔，如 `外形尺寸;漏电流;电容量;损耗角正切;阻抗或ESR(有需要时)`）

### 功能
1. 拉取远程 SQL Server 的 `apl_contract_plan` 表数据
2. 按台位筛选（如 `电容性能台1#`），使用 `instrumentNumber` 字段
3. 拆分 `testItems` 为多条独立实验项目记录
4. 产品信息（标称值、容量正偏差、上限/下限、损耗标准等）来源于产品信息表，字段待定，**先预留接口 `IProductInfoService`**

### 离线模式
- 无网络时使用本地 SQLite 缓存的计划数据
- 内置 Mock 数据（`MockPlanDataProvider`），包含以下实验项目：
  - 电容量、损耗角正切值（tgδ）、等效串联电阻（ESR）、阻抗
  - 绝缘外套的绝缘电阻、漏电流
  - 可靠性前性能实验、可靠性后性能实验

---

## 模块二：计划列表（Plan List）

### UI
- HandyControl `DataGrid` 展示计划
- 列：计划编号、产品型号、实验项目、台位、状态、操作
- 最后一列 **"测试" 按钮**（`hc:ButtonGroup`），点击跳转测试页面
- 支持台位筛选下拉框 + 关键字搜索框

---

## 模块三：测试页面（Test Page）

### 核心交互
1. 顶部显示当前计划信息（计划号、产品型号、实验项目）
2. 中部是测试数据列表（`DataGrid`，可编辑）
3. 设备每采集一次 → 自动新增一行（序号递增）
4. 每个单元格可手动修改
5. 最右边有 **删除按钮**（每行一个）
6. 底部操作栏：
   - **"设备采集" 按钮** → 弹出设备选择窗口
   - **"保存数据" 按钮** → 保存到本地 SQLite
   - **"上报数据" 按钮** → 同时写入远程 SQL Server

### 数据列（根据实验项目动态变化）

**电容量测试示例：**
| 序号 | C（μF） | 频率（Hz） |
|------|---------|-----------|
| 1    | 852.864 | 120       |
| 2    | 850.751 | 120       |

**漏电流测试示例：**
| 序号 | IL正向（μA） | HL（S） | IL反向（μA） | HL（S） |
|------|-------------|---------|-------------|---------|

**损耗角正切值示例：**
| 序号 | 损耗 | 频率（Hz） |
|------|------|-----------|

**ESR 示例：**
| 序号 | ESR（MΩ） | 频率（Hz） |
|------|----------|-----------|

**阻抗示例：**
| 序号 | 阻抗（MΩ） | 频率（Hz） |
|------|----------|-----------|

**绝缘电阻示例：**
| 序号 | 绝缘电阻（MΩ） |
|------|----------------|

---

## 模块四：设备管理（Device Management）

### 设备列表（分区显示）

| 分区           | 设备型号             |
|---------------|---------------------|
| LCR数字电桥    | TH2817A、TH2816B+（即TH2810B+） |
| 漏电流测试仪    | TH2689              |
| 绝缘电阻测试仪  | TH2683A             |
| 极壳耐压       | TH9201              |

### 实验项目 → 设备推荐映射

| 实验项目                  | 推荐设备            | 采集数据列                                        |
|--------------------------|--------------------|-------------------------------------------------|
| 电容量                    | LCR数字电桥         | C（μF）、频率（Hz）                                |
| 损耗角正切值（tgδ）        | LCR数字电桥         | 损耗                                               |
| 等效串联电阻（ESR）        | LCR数字电桥         | ESR（MΩ）                                         |
| 阻抗                     | LCR数字电桥         | 阻抗（MΩ）                                         |
| 绝缘外套的绝缘电阻         | 绝缘电阻测试仪       | 绝缘电阻（MΩ）                                     |
| 漏电流                    | 漏电流测试仪         | IL正向（μA）、HL（S）、IL反向（μA）、HL（S）         |
| 可靠性前/后性能实验         | 多设备可选           | 根据所选设备动态显示列                                |

**注意**：系统不做硬限制，用户可自由选择设备。系统根据用户选择的设备类型，动态设置参数和显示数据列。

### 设备通信

#### 串口公共参数
- 数据位：8，校验位：None，停止位：One
- 指令结尾：`\n`（换行符）
- 读写超时：3000ms
- 支持波特率：9600 / 19200 / 38400 / 57600 / 115200

#### 各设备默认波特率
| 设备     | 默认波特率 |
| -------- | ---------- |
| TH2689   | 19200      |
| TH2683A  | 9600       |
| TH2817A  | 9600       |
| TH2832   | 115200     |
| TH9201   | 19200      |
| TH2810B+ | 19200      |

#### 每个设备驱动需实现的功能模块
1. **连接**：打开/关闭串口，异常时弹窗提醒（超时、断开、错误码）
2. **控制**：发送控制指令（触发测试、停止等）
3. **参数配置**：向设备发送 SCPI 配置指令
4. **数据采集**：触发测量并读取结果
5. **结果解析**：解析设备返回的数据

#### 各设备 SCPI 指令集（详见 `docs/功能指令说明.md`）

**TH2689 - 漏电流测试仪**
```
可配参数: Voltage(1~800V), Current(0.5~500), Function(SEQ/STEP/CONT), Speed(FAST/MEDium/SLOW),
         Range(2uA/20uA/200uA/2mA/20mA), ChgTime(0~999s), Dweli(0.2~999s), RangeAuto(OFF/ON)
指令:
  :DISPlay:STATe?                    # 查询显示状态
  :DISPlay:LCTest                    # 切换到LC测试页面
  :LCTest:SOURce:VOLTage {V}         # 设置测试电压
  :LCTest:SOURce:CURRent {C}         # 设置测试电流
  :LCTest:CONFigure:FUNCtion {F}     # 设置测试模式
  :LCTest:CONFigure:SPEed {S}        # 设置测试速度
  :LCTest:CONFigure:RANGe {R}        # 设置电流量程索引
  :LCTest:CONFigure:CHGTime {T}      # 设置充电时间
  :LCTest:CONFigure:DWELl {D}        # 设置延迟时间
  :LCTest:CONFigure:RANGe:AUTO {A}   # 设置自动量程
  :LCTest:MEASure:IR?                # 读取绝缘电阻值
  :LCTest:MEASure:LC?                # 读取绝缘电容值
输出: LC, LCf, IR, IRf
```

**TH2683A - 绝缘电阻测试仪**
```
可配参数: Voltage(100v/500v), CurrentTime, CheckTime, CheckSpeed(FAST/SLOW),
         WaitTime, Mode(SINGle/CONTinuous), FreeTime
前置检查: SYSTem:STSTus? → 若"DISCharging"或"TESTing"则提示等待
指令:
  SYSTem:STSTus?                     # 查询设备状态
  DISPlay:PAGE MSETup                # 切换到测量设置页面
  DISP:PAGE MEAS                     # 切换至测量页面
  :FUNCtion:OVOLTage {V}             # 设置测试电压
  :FUNCtion:CTIMe {T}                # 设置加压时间
  :FUNCtion:MTIMe {T}                # 设置检测时间
  :FUNCtion:MSPeed {S}               # 设置检测速度
  :FUNCtion:MMode {M}                # 设置测试模式
  :FUNCtion:WTIMe {T}                # 设置等待时间
  :FUNCtion:DTIMe {T}                # 设置放电时间
  fetc?                              # 获取测量结果
输出: LC(μF, ×1000000), IR(MΩ, /1000000)
```

**TH2817A - LCR数字电桥**
```
可配参数: Function(CPD/CSD/CSRS/LPQ/LSRS/ZTD/ZTR/RX), Frequency(MIN~100kHz, 18档),
         ListFrequency(逗号分隔)
指令:
  DISPlay:PAGE?                      # 查询当前页面
  DISPlay:PAGE MeAsurement           # 切换到测量页面
  FUNC:IMP {F}                       # 设置阻抗测试功能
  FREQUENCY {F}                      # 设置测试频率
  DISPlay:PAGE LIST                  # 切换到列表页面
  LIST:FREQ {F}                      # 设置扫频频率列表
  TRIG:Source BUS                    # 设置触发源为总线
  TRIG                               # 触发测试
输出: LC(电容值), IR(绝缘电阻值)
```

**TH2832 - LCR数字电桥（增强版）**
```
可配参数: Function(CPD/CSD/CSRS/LPQ/LSRS/ZTD/ZTR/RX), Frequency(MIN~100kHz),
         Voltage(10mV~2V), Speed(FAST/MED/SLOW), Range(3~100000/AUTO),
         ListFrequency, ListVoltage, Mode
指令:
  DISPlay:PAGE MeAsurement           # 切换到测量页面
  FUNC:IMP {F}                       # 设置阻抗测试功能
  FREQUENCY {F}                      # 设置测试频率
  VOLTage {V}                        # 设置测试电压
  APER {S}                           # 设置测量速率
  FUNC:IMP:RANG:AUTO ON              # 开启自动量程
  FUNC:IMP:RANG {R}                  # 设置固定量程
  DISPlay:PAGE LIST                  # 切换到列表页面
  LIST:FREQ {F}                      # 设置扫频频率列表
  LIST:VOLT1 {V}                     # 设置扫频电压列表
  DISPlay:PAGE MEASurement           # 读取时切换到测量页面
  TRIG                               # 触发单次测试
  FETC?                              # 获取测量结果
  rs232:print on                     # 开启列表模式串口打印
输出: LC(μF, ×1000000, F3), IR(MΩ, ×1, F5)
```

**TH9201 - 安规测试仪**
```
可配参数: Step(AC/DC/IR/OS), Volt, Upper, Lower(默认0), Time
结果映射: 1=PASS, 2=HIGH FAIL, 3=LOW FAIL, 4=ARC FAIL, 5=RANGE FAIL
指令:
  :SOUR:SAFE:STEP {N}:FUNC {F}              # 设置步骤功能(AC=1/DC=2/IR=3/OS=4)
  :SOUR:SAFE:STEP {N}:{Step}:LEV {V}        # 设置步骤电压等级
  :SOUR:SAFE:STEP {N}:{Step}:LIM:HIGH {U}   # 设置上限
  :SOUR:SAFE:STEP {N}:{Step}:LIM:LOW {L}    # 设置下限
  :SOUR:SAFE:STEP {N}:{Step}:TIME:TEST {T}  # 设置测试时间
  :TEST:FETCH?                               # 获取测试结果
输出: PASS/FAIL状态
```

**TH2810B+ - LCR数字电桥**
```
可配参数: Function(CSD), Frequency(100/120/1k/10kHz), Voltage(10mV~1V),
         Range(3~100000/AUTO), Speed(FAST/MED/SLOW), ListFrequency, ListVoltage, Mode(SEQ/STEP)
指令:
  DISPlay:PAGE MeAsurement           # 切换到测量页面
  FUNC:IMP {F}                       # 设置阻抗测试功能
  FREQUENCY {F}                      # 设置测试频率
  VOLTage {V}                        # 设置测试电压
  FUNC:IMP:RANG:AUTO ON              # 开启自动量程
  FUNC:IMP:RANG {R}                  # 设置固定量程
  APER {S}                           # 设置测量速率
  DISPlay:PAGE LIST                  # 切换到列表页面
  LIST:FREQ {F}                      # 设置扫频频率列表
  LIST:VOLT {V}                      # 设置扫频电压列表
  LIST:MODE {M}                      # 设置列表模式(SEQ/STEP)
  DISPlay:PAGE MEASurement           # 读取时切换到测量页面
  TRIG:Source BUS                    # 设置触发源为总线
  TRIG                               # 触发测试
输出: LC(μF, ×1000000, F4), IR(MΩ, ×1, F6)
列表模式: i%5==0→LC, i%5==1→IR, 其他忽略
```

---

## 模块五：数据存储

### 本地 SQLite 表设计

```sql
-- 计划缓存（离线用）
CREATE TABLE PlanCache (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    PlanNo TEXT NOT NULL,
    ProductModel TEXT,
    TestItems TEXT,              -- 原始分号分隔
    Station TEXT,                -- 台位，如"电容性能台1#"
    Status TEXT,
    ProductInfoJson TEXT,        -- 产品信息JSON（标称值、偏差等）
    SyncTime DATETIME
);

-- 测试记录（每次保存一条）
CREATE TABLE TestRecord (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    PlanNo TEXT NOT NULL,
    TestItem TEXT NOT NULL,      -- 实验项目名
    DeviceModel TEXT NOT NULL,   -- 使用的设备型号
    DeviceParamsJson TEXT,       -- 设备配置参数JSON
    TestDataJson TEXT NOT NULL,  -- 采集数据JSON（序号+数据列）
    SpecimenNumber TEXT,         -- 样机编号
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    IsUploaded INTEGER DEFAULT 0,
    RemoteTestDataId TEXT        -- 远程TestData表的ID，用于关联
);

-- 历史快照（每次保存同时生成）
CREATE TABLE TestHistory (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    TestRecordId INTEGER NOT NULL,
    SnapshotJson TEXT NOT NULL,  -- 完整数据快照
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (TestRecordId) REFERENCES TestRecord(Id)
);
```

### 远程 SQL Server 上报

- **连接字符串**：`Server=10.2.25.196;Database=TraceSource;User Id=sa;Password=***`
- **上报流程**（参照 `docs/远程插入表代码.md`）：
  1. 参数校验（contractNumber, specimenNumber, inspectionName, expId, labId, fileName）
  2. 获取申请单信息，校验状态为"测试中"或"录入中"
  3. 检查台位是否开启自动报告
  4. 获取检测项信息（apl_contract_inspection）
  5. 构建并保存 TestData
  6. 更新计划状态
  7. 更新样机信息
  8. 根据测试项目类型分发处理
  9. 计算综合结论
- 上报成功后标记本地记录 `IsUploaded = 1`，保存远程 `TestDataId`

---

## 模块六：历史数据（History）

- DataGrid 展示所有 TestRecord
- 支持按计划号、实验项目、日期范围筛选
- 每行可点击查看详细数据（弹窗或侧边栏）
- 未上报记录显示 **"重新上报" 按钮**
- 支持数据导出 Excel（EPPlus 或 ClosedXML）

---

## 模块七：页面导航

主窗口左侧导航栏（HandyControl `SideMenu`）：

| 图标 | 页面     | 说明                    |
|------|---------|------------------------|
| 📋   | 计划导入  | 拉取/筛选/查看计划          |
| 🧪   | 测试页面  | 从计划列表跳转，数据采集主页面 |
| 📊   | 历史数据  | 查看所有保存记录            |
| ⚙️   | 设置     | 设备管理、连接配置、系统设置   |

---

## 界面风格

- 使用 HandyControl 控件库（主题色可自定义）
- 现代扁平风格，圆角卡片布局
- 导航栏左侧固定（宽度 200px），内容区右侧填充
- 状态栏底部显示：连接状态、当前计划、设备在线状态
- DataGrid 斑马纹、hover 高亮
- 按钮使用 HandyControl 的 `ButtonGroup`、`ProgressBarButton` 等

---

## 生成顺序（建议分步执行）

### 第 1 步：解决方案骨架 + DI 配置
- 创建 .sln + 4 个项目（Core, Data, Services, Devices, UI）
- 配置 NuGet 包引用
- App.xaml.cs 配置 DI 容器
- 主窗口 + 导航框架

### 第 2 步：数据层
- SQLite 表创建（PlanCache, TestRecord, TestHistory）
- SQL Server 连接服务（GreeTestCenter 读取 + TraceSource 写入）
- Repository 模式实现

### 第 3 步：计划导入页面
- 远程拉取 + 本地缓存
- 离线 Mock 数据
- 台位筛选 + testItems 拆分
- 计划列表 DataGrid + "测试"按钮

### 第 4 步：测试页面 + 设备选择弹窗
- 动态列生成（根据实验项目）
- 可编辑 DataGrid（新增行、删除行、手动编辑）
- 设备选择弹窗（分区显示，推荐标记）
- 保存/上报按钮

### 第 5 步：设备通信驱动
- SerialPortService 封装
- 6 个设备驱动（TH2689, TH2683A, TH2817A, TH2832, TH9201, TH2810B+）
- 连接异常提醒（弹窗 + 状态栏）

### 第 6 步：数据保存 + 历史记录
- 保存到 SQLite（TestRecord + TestHistory）
- 历史数据页面

### 第 7 步：远程上报
- 实现 processAutoReport 逻辑
- 上报状态管理

### 第 8 步：界面美化 + 导航完善
- HandyControl 主题定制
- 动画过渡
- 状态栏实时信息

---

## 关键约束

1. **密码等敏感信息**使用 `appsettings.json` + `IConfiguration` 管理，不要硬编码
2. **MVVM 严格分离**：View 不要有 code-behind 逻辑，ViewModel 不要引用 WPF 程序集
3. **设备通信异步化**：所有串口操作使用 `async/await`，不阻塞 UI 线程
4. **错误处理**：设备连接超时、断开、数据解析失败都要有用户友好的提示
5. **离线优先**：无网络时所有功能可用（使用 Mock + 本地缓存）
