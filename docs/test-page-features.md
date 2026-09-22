---
name: test-page-features
description: 测试页面核心功能实现记录 - 多表格、切换视图、Excel导出、数据库存储
metadata:
  type: reference
---

# 测试页面功能实现记录

## 1. 多表格支持（设备采集→测试页面）

### 数据模型
- `TestDataTable`：动态列容器，`ColumnOrder` + `Rows`
- `TestDataRow`：索引器按列名读写，`ParentTable` 自动注册列
- `DeviceCategoryTab`：外层分组（Category + Tables）

### LCR 数据拆分逻辑
- `LcrTemplateColumns`：4 种模板（C(μF)+频率、损耗+频率、ESR+频率、阻抗+频率）
- `TemplateSubTitleMap`：列名→标签页副标题映射
- `ImportLcrData`：按模板匹配拆分，数据列全空则跳过模板和行

### TabControl DataGrid 列生成
- `DataGrid_Loaded` + `DataGrid_DataContextChanged` 双重处理
- WPF TabControl 复用 DataGrid 实例，切换时需 DataContextChanged 刷新列
- `BuildColumns`：序号列 + 数据列 + 删除按钮

## 2. 多选项卡/集中显示切换

### ViewModel
- `IsConcentratedMode`：bool 属性，默认 false

### XAML
- `BooleanToVisibilityConverter`（集中模式）+ `InverseBoolToVisibilityConverter`（多选项卡模式）
- 集中模式：`ScrollViewer` + `ItemsControl` 嵌套，每个 `TestDataTable` 一个 DataGrid
- `ConcentratedDataGrid_Loaded`：集中模式下列生成

## 3. Excel 导出（ClosedXML）

### 格式要求
- 字体：微软雅黑（全局 `sheet.Style.Font.FontName`）
- 边框：内外细线（`ApplyThinBorder`）
- 文字居中加粗（`ApplyCenterBold`）
- 频率列 C:D 合并（多加一列）
- 行高列宽自适应（`AutoFitSheet` 手动计算，中文2倍宽度）

### 判定标准区域
- **单标准**（损耗/ESR/阻抗/绝缘电阻）：2行，B:C合并
  - Row0: A=判定标准↕, B=标准名(merged B:C)
  - Row1: B=标准值(merged B:C)
- **多标准**（C(μF)）：8行，前2对B/C列，后2对C/D列
  - 前2对: B=label, C=value
  - 后2对: C=label, D=value(最后一列)
- **漏电流**：4对，左组B:C + 右组D:E

### 数据着色
- 上限标准（≤）：超标红，低于绿
- 下限标准（≥）：低于红，高于绿
- `IsLowerLimit` 判定：LC(μF)、IR(MΩ) 为下限

## 4. 数据库存储

### TestRecord 模型
- 新增 `ExcelPath` 属性
- `CheckData` 表新增 `ExcelPath` 列（ALTER TABLE 兼容旧库）

### 保存流程
1. 导出 Excel 文件
2. 创建汇总 TestRecord（PlanNo, DeviceType, CheckName, ExcelPath）
3. 调用 `_testService.SaveTestRecords` 入库

### 历史查询
- `GetByPlanNo("")` 返回全部记录（非空PlanNo精确匹配）
- "打开Excel"按钮：`IsEnabled` 绑定 ExcelPath，无路径时灰显
- 文件不存在时提示"文件不存在"

## 5. 关键文件清单

| 文件 | 改动内容 |
|------|---------|
| TestPageViewModel.cs | 多表格导入、切换视图、Excel导出、数据库保存 |
| TestPageView.xaml | TabControl + 集中模式模板、ToggleButton |
| TestPageView.xaml.cs | DataGrid_Loaded/ContextChanged、ConcentratedDataGrid_Loaded |
| DeviceCategoryTemplateSelector.cs | MultiTable vs SingleTable 选择 |
| TestDataTable.cs | Title/SubTitle/DeviceCategory/DisplayName |
| MockDriver.cs | LcrMockData 按 Function 返回不同列 |
| TestRecord.cs | 新增 ExcelPath |
| SQLiteContext.cs | CheckData 新增 ExcelPath 列 |
| TestRecordRepository.cs | INSERT 包含 ExcelPath |
| TestHistoryRepository.cs | GetByPlanNo 空值返回全部 |
| HistoryView.xaml | 打开Excel按钮 |
| HistoryViewModel.cs | OpenExcelCommand、去掉模拟数据 |
