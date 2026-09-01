# 本地数据库表名/字段重命名 + 代码注释

## Context

本地 SQLite 4 张表（TBL_PLANCACHE、TBL_CHECKDATA、TBL_DEVICE_CONFIG、TBL_CONNECTION_LOG）及其字段全部使用大写下划线命名（如 `PLAN_CODE`、`DEVICE_ID`），不符合 C# PascalCase 规范。同时发现 **Dapper 映射 bug**：`SELECT *` 返回的大写列名（`PLAN_CODE`）无法映射到 C# 属性（`PlanNo`），因为 Dapper 不做下划线去除。此外 `PlanInfo` 模型缺少 TBL_PLANCACHE 的多个字段（Lot、DeviceId 等），导致 INSERT 时大部分值为 null。

远程 SQL Server 表（`apl_contract_plan`、`TestData` 等）**不修改**。

---

## 改动范围

### 1. 表名重命名

| 旧表名 | 新表名 |
|--------|--------|
| TBL_PLANCACHE | PlanCache |
| TBL_CHECKDATA | CheckData |
| TBL_DEVICE_CONFIG | DeviceConfig |
| TBL_CONNECTION_LOG | ConnectionLog |

### 2. 字段重命名（以 CheckData 为例）

| 旧字段 | 新字段 | C# 属性 |
|--------|--------|---------|
| ID | Id | Id |
| PLAN_CODE | PlanNo | PlanNo |
| LOT | Lot | Lot |
| DEVICE_ID | DeviceId | DeviceId |
| DEVICE_TYPE | DeviceType | DeviceType |
| ITEM_NO | ItemNo | ItemNo |
| SPEC_NAME | SpecName | SpecName |
| CHECK_NAME | CheckName | CheckName |
| CHECK_VALUE | CheckValue | CheckValue |
| RESULT | Result | Result |
| TEST_DT | TestTime | TestTime |
| OPERATOR | Operator | Operator |
| REMARK | Remark | Remark |
| SYNC_STATUS | SyncStatus | SyncStatus |

PlanCache、DeviceConfig、ConnectionLog 同理全部转 PascalCase。

### 3. 涉及文件

| 文件 | 改动 |
|------|------|
| `Data/Contexts/SQLiteContext.cs` | DDL 表名+字段+索引名，加注释 |
| `Data/Repositories/PlanRepository.cs` | 6 条 SQL 语句，加注释 |
| `Data/Repositories/TestRecordRepository.cs` | 7 条 SQL 语句，加注释 |
| `Data/Repositories/TestHistoryRepository.cs` | 6 条 SQL 语句，加注释 |
| `Core/Models/PlanInfo.cs` | 补齐缺失属性（Lot、DeviceId、ItemNo、SpecName、SpecValue、SpecMin、SpecMax、SpecUnit、CreateTime），加注释 |
| `Core/Models/TestRecord.cs` | 属性名与新列名对齐确认，加注释 |
| `Core/Interfaces/IPlanRepository.cs` | 加注释 |
| `Core/Interfaces/ITestHistoryRepository.cs` | 加注释 |
| `Core/Interfaces/ITestRecordRepository.cs` | 加注释 |
| `CLAUDE.md` | 更新数据库表说明 |

### 4. 不改动的文件

- `RemotePlanRepository.cs` — 远程 SQL Server，表名不变
- `RemoteReportRepository.cs` — 远程 SQL Server，表名不变
- `SqlServerContext.cs` — 远程连接，无关
- 所有 ViewModel / View / Service 文件 — 不直接写 SQL，无需改动

---

## 执行步骤

1. 修改 `SQLiteContext.cs`：4 张表 DDL + 4 个索引，加中文注释
2. 补齐 `PlanInfo.cs`：添加 Lot、DeviceId、ItemNo、SpecName、SpecValue、SpecMin、SpecMax、SpecUnit、CreateTime 属性
3. 确认 `TestRecord.cs` 属性名与新列名一致，加注释
4. 修改 `PlanRepository.cs`：6 条 SQL 替换表名/字段名，加注释
5. 修改 `TestRecordRepository.cs`：7 条 SQL 替换，加注释
6. 修改 `TestHistoryRepository.cs`：6 条 SQL 替换，加注释
7. 修改3个 Interface 文件：加注释
8. 更新 `CLAUDE.md` 数据库表说明
9. `dotnet build` 验证编译通过
