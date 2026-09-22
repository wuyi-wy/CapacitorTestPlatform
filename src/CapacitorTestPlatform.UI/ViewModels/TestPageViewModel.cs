using CapacitorTestPlatform.Core.Interfaces;
using CapacitorTestPlatform.Core.Models;
using CapacitorTestPlatform.UI.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace CapacitorTestPlatform.UI.ViewModels;

/// <summary>
/// 设备分类选项卡，用于测试页面外层 TabControl 分组。
/// 每个设备分类（如 LCR数字电桥）对应一个选项卡，内含该分类下的所有数据表。
/// </summary>
public class DeviceCategoryTab
{
    /// <summary>设备分类名称（如"LCR数字电桥"、"漏电流测试仪"）</summary>
    public string Category { get; set; } = "";

    /// <summary>该分类下的数据表集合（内层 TabControl 绑定）</summary>
    public ObservableCollection<TestDataTable> Tables { get; } = new();
}

/// <summary>
/// 测试页面 ViewModel，管理计划选择、设备数据导入和检测记录保存/上传。
/// 支持多表格：按设备分类分组，LCR数字电桥内部按 testItem 分子表格。
/// </summary>
public partial class TestPageViewModel : ObservableObject
{
    private readonly ITestService _testService;
    private readonly IReportService _reportService;

    /// <summary>当前完整的计划信息（含规格标准，用于传递给设备面板）</summary>
    private PlanInfo? _currentPlan;

    /// <summary>当前计划编号</summary>
    [ObservableProperty]
    private string _planNo = "";

    /// <summary>产品型号</summary>
    [ObservableProperty]
    private string _productModel = "";

    /// <summary>当前测试项名称</summary>
    [ObservableProperty]
    private string _testItem = "";

    /// <summary>试品编号</summary>
    [ObservableProperty]
    private string _specimenNumber = "";

    /// <summary>状态栏提示消息</summary>
    [ObservableProperty]
    private string _statusMessage = "就绪";

    /// <summary>设备分类选项卡集合（外层 TabControl 绑定）</summary>
    public ObservableCollection<DeviceCategoryTab> DeviceTabs { get; } = new();

    /// <summary>当前选中的设备分类选项卡</summary>
    [ObservableProperty]
    private DeviceCategoryTab? _activeDeviceTab;

    /// <summary>是否为集中显示模式（false 为多选项卡模式）</summary>
    [ObservableProperty]
    private bool _isConcentratedMode;

    /// <summary>当前选中的数据表（内层 TabControl 绑定）</summary>
    [ObservableProperty]
    private TestDataTable? _activeTable;

    /// <summary>请求打开设备选择弹窗时触发的事件，携带 testItem 和 plan 信息</summary>
    public event EventHandler<(string TestItem, PlanInfo Plan)>? RequestDeviceSelect;

    /// <summary>
    /// 初始化测试页面 ViewModel，注入测试服务和报告服务。
    /// </summary>
    public TestPageViewModel(ITestService testService, IReportService reportService)
    {
        _testService = testService;
        _reportService = reportService;
    }

    /// <summary>
    /// 使用计划信息初始化页面，创建第一个设备分类选项卡和数据表。
    /// </summary>
    /// <param name="plan">检测计划信息</param>
    /// <param name="testItem">测试项名称</param>
    public void InitializeWithPlan(PlanInfo plan, string testItem)
    {
        _currentPlan = plan;
        PlanNo = plan.ContractNumber;
        ProductModel = plan.SampleType ?? "";
        TestItem = testItem;

        // 清空并创建初始结构（设备分类在导入数据时动态创建）
        DeviceTabs.Clear();
        ActiveDeviceTab = null;
        ActiveTable = null;
    }

    /// <summary>
    /// 无计划模式初始化，手动录入场景使用。
    /// </summary>
    public void InitializeWithoutPlan()
    {
    }

    /// <summary>
    /// 数据采集命令，触发打开设备选择弹窗。
    /// </summary>
    [RelayCommand]
    private void AcquireData()
    {
        if (_currentPlan == null) return;
        RequestDeviceSelect?.Invoke(this, (TestItem, _currentPlan));
    }

    /// <summary>LCR 数据模板：每组列名对应一个内层 Tab</summary>
    private static readonly List<List<string>> LcrTemplateColumns = new()
    {
        new() { "C(μF)", "频率(Hz)" },
        new() { "损耗", "频率(Hz)" },
        new() { "ESR(MΩ)", "频率(Hz)" },
        new() { "阻抗(MΩ)", "频率(Hz)" },
    };

    /// <summary>模板列 → 内层 Tab 子标题映射</summary>
    private static readonly Dictionary<string, string> TemplateSubTitleMap = new()
    {
        ["C(μF)"] = "C(μF)",
        ["损耗"] = "损耗",
        ["ESR(MΩ)"] = "ESR(MΩ)",
        ["阻抗(MΩ)"] = "阻抗(MΩ)",
    };

    /// <summary>
    /// 将设备采集的数据导入到测试页面。
    /// - LCR 数字电桥：按数据模板拆分成多个子表格（C(μF)、损耗、ESR(MΩ)、阻抗(MΩ)）
    /// - 非 LCR 设备：整体作为一个表格导入
    /// </summary>
    public void ImportDeviceData(TestDataTable deviceData)
    {
        var category = deviceData.DeviceCategory;
        if (string.IsNullOrEmpty(category))
            category = "其他";

        var deviceTab = DeviceTabs.FirstOrDefault(t => t.Category == category);
        if (deviceTab == null)
        {
            deviceTab = new DeviceCategoryTab { Category = category };
            DeviceTabs.Add(deviceTab);
        }

        if (category == "LCR数字电桥")
            ImportLcrData(deviceTab, deviceData);
        else
            ImportSingleTable(deviceTab, deviceData);

        ActiveDeviceTab = deviceTab;
    }

    /// <summary>
    /// LCR 数据导入：按模板拆分列到多个子表格。
    /// 例如 CPD 返回 C(μF)+损耗+频率，拆分后：C(μF) 表和 损耗 表各一份数据。
    /// 导入前清空目标表的旧数据，确保行数与源数据一致。
    /// </summary>
    private void ImportLcrData(DeviceCategoryTab deviceTab, TestDataTable source)
    {
        var sourceColumns = source.GetColumnOrder();
        int imported = 0;

        foreach (var templateCols in LcrTemplateColumns)
        {
            // 数据列（去掉频率）必须全部在源数据中才创建该子表格
            var dataCols = templateCols.Where(c => c != "频率(Hz)").ToList();
            if (!dataCols.All(c => sourceColumns.Contains(c)))
                continue;

            var subTitle = TemplateSubTitleMap.GetValueOrDefault(dataCols.First(), dataCols.First());

            // 如果数据列（不含频率）全部为空，则跳过不创建
            bool allEmpty = source.Rows.All(srcRow =>
                dataCols.All(c => string.IsNullOrWhiteSpace(srcRow.Data.GetValueOrDefault(c, ""))));
            if (allEmpty)
                continue;

            var target = deviceTab.Tables.FirstOrDefault(t => t.DisplayName == subTitle);
            if (target == null)
            {
                target = new TestDataTable
                {
                    Title = source.Title,
                    SubTitle = subTitle,
                    DeviceCategory = "LCR数字电桥"
                };
                deviceTab.Tables.Add(target);
            }

            // 导入前清空旧数据，避免重复追加
            target.Clear();

            foreach (var srcRow in source.Rows)
            {
                // 跳过数据列（不含频率）全部为空的行
                if (dataCols.All(c => string.IsNullOrWhiteSpace(srcRow.Data.GetValueOrDefault(c, ""))))
                    continue;

                var rowData = new Dictionary<string, string>();
                foreach (var col in templateCols)
                    rowData[col] = srcRow.Data.GetValueOrDefault(col, "");
                target.AddRow(rowData);
            }

            imported = source.Rows.Count;
            ActiveTable = target;
        }

        StatusMessage = $"已导入 {imported} 条数据到「LCR数字电桥」";
    }

    /// <summary>
    /// 非 LCR 数据导入：整体作为一个表格。
    /// </summary>
    private void ImportSingleTable(DeviceCategoryTab deviceTab, TestDataTable source)
    {
        var incomingColumns = source.GetColumnOrder();
        TestDataTable? target = null;

        foreach (var table in deviceTab.Tables)
        {
            if (table.DisplayName == source.DisplayName)
            {
                if (table.GetColumnOrder().Count == 0 ||
                    (table.HasColumns(incomingColumns) && table.GetColumnOrder().Count == incomingColumns.Count))
                {
                    target = table;
                    break;
                }
            }
        }

        if (target == null)
        {
            target = new TestDataTable
            {
                Title = source.Title,
                SubTitle = source.SubTitle,
                DeviceCategory = source.DeviceCategory
            };
            deviceTab.Tables.Add(target);
        }

        foreach (var row in source.Rows)
            target.AddRow(row.Data.ToDictionary(kv => kv.Key, kv => kv.Value));

        ActiveTable = target;
        StatusMessage = $"已导入 {source.Rows.Count} 条数据到「{target.DisplayName}」";
    }

    /// <summary>
    /// 删除活动表格中的指定数据行。
    /// </summary>
    /// <param name="row">要删除的数据行</param>
    [RelayCommand]
    private void DeleteRow(TestDataRow? row)
    {
        if (row == null || ActiveTable == null) return;
        ActiveTable.RemoveRow(row);
        StatusMessage = $"已删除第 {row.SeqNo} 行";
    }

    /// <summary>
    /// 保存数据命令，将所有表格导出为格式化的 Excel 文件（匹配数据模板格式）。
    /// 按设备分类分 Sheet，每个子表格纵向排列，含表头、数据行、判定标准行。
    /// </summary>
    [RelayCommand]
    private void SaveData()
    {
        if (DeviceTabs.Count == 0 || DeviceTabs.All(t => t.Tables.All(tb => tb.Rows.Count == 0)))
        {
            StatusMessage = "没有数据可保存";
            return;
        }

        try
        {
            var defaultName = $"测试数据_{PlanNo}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Title = "保存测试数据",
                Filter = "Excel文件|*.xlsx|所有文件|*.*",
                FileName = defaultName,
                DefaultExt = ".xlsx"
            };

            if (dialog.ShowDialog() != true) return;

            using var workbook = new ClosedXML.Excel.XLWorkbook();

            foreach (var deviceTab in DeviceTabs)
            {
                var sheet = workbook.Worksheets.Add(deviceTab.Category);
                sheet.Style.Font.FontName = "微软雅黑";
                var row = 1;

                foreach (var table in deviceTab.Tables)
                {
                    if (table.Rows.Count == 0) continue;

                    var columns = table.GetColumnOrder();
                    var freqColIdx = columns.IndexOf("频率(Hz)");
                    // 有频率列时多加一列（频率 C:D 合并显示）
                    var lastCol = freqColIdx >= 0 ? 1 + columns.Count + 1 : 1 + columns.Count;
                    var threshold = GetNumericThreshold(table.DisplayName);

                    // 表头
                    sheet.Cell(row, 1).Value = "序号";
                    for (int c = 0; c < columns.Count; c++)
                        sheet.Cell(row, c + 2).Value = columns[c];
                    // 频率列合并 C:D
                    if (freqColIdx >= 0)
                        sheet.Range(row, freqColIdx + 2, row, lastCol).Merge();
                    ApplyCenterBold(sheet.Range(row, 1, row, lastCol));
                    ApplyThinBorder(sheet.Range(row, 1, row, lastCol));
                    row++;

                    // 数据行
                    foreach (var dataRow in table.Rows)
                    {
                        sheet.Cell(row, 1).Value = dataRow.SeqNo;
                        for (int c = 0; c < columns.Count; c++)
                        {
                            var val = dataRow[columns[c]];
                            if (double.TryParse(val, out var numVal))
                            {
                                var cell = sheet.Cell(row, c + 2);
                                cell.Value = numVal;
                                if (threshold.HasValue && c != freqColIdx)
                                {
                                    var isLower = IsLowerLimit(table.DisplayName);
                                    if (isLower ? numVal < threshold.Value : numVal > threshold.Value)
                                        cell.Style.Font.FontColor = ClosedXML.Excel.XLColor.Red;
                                    else if (isLower ? numVal > threshold.Value : numVal < threshold.Value)
                                        cell.Style.Font.FontColor = ClosedXML.Excel.XLColor.Green;
                                }
                            }
                            else
                                sheet.Cell(row, c + 2).Value = val;
                        }
                        // 频率列合并
                        if (freqColIdx >= 0)
                            sheet.Range(row, freqColIdx + 2, row, lastCol).Merge();
                        sheet.Range(row, 1, row, lastCol).Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;
                        ApplyThinBorder(sheet.Range(row, 1, row, lastCol));
                        row++;
                    }

                    row++; // 空行

                    // 判定标准区域（匹配数据模板格式）
                    var pairs = GetCriteriaRows(table.DisplayName);
                    if (pairs.Count > 0)
                    {
                        var cr = row; // criteria start row
                        int criteriaRows;

                        if (pairs.Count == 1)
                        {
                            // 单标准（损耗/ESR/阻抗/绝缘电阻）：2行，B:C合并
                            //   Row0: A=判定标准, B=标准名 (merged B:C)
                            //   Row1: A=空,       B=标准值 (merged B:C)
                            criteriaRows = 2;
                            sheet.Cell(cr, 1).Value = "判定标准";
                            sheet.Cell(cr, 1).Style.Font.Bold = true;
                            sheet.Cell(cr, 1).Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;
                            sheet.Range(cr, 1, cr + 1, 1).Merge();
                            sheet.Cell(cr, 2).Value = pairs[0].Label;
                            sheet.Range(cr, 2, cr, 3).Merge();
                            sheet.Cell(cr + 1, 2).Value = pairs[0].Value;
                            sheet.Range(cr + 1, 2, cr + 1, 3).Merge();
                        }
                        else
                        {
                            // 多标准（C(μF)/漏电流）：每对占2行
                            // 偶数对在B列，奇数对在C列；第2对起用C:D列
                            criteriaRows = pairs.Count * 2;
                            sheet.Cell(cr, 1).Value = "判定标准";
                            sheet.Cell(cr, 1).Style.Font.Bold = true;
                            sheet.Cell(cr, 1).Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;
                            sheet.Range(cr, 1, cr + criteriaRows - 1, 1).Merge();

                            for (int i = 0; i < pairs.Count; i++)
                            {
                                var (label, value) = pairs[i];
                                int lr = cr + i * 2;
                                if (i < 2)
                                {
                                    // 前2对：B=i*2+2, C=i*2+3
                                    sheet.Cell(lr, i * 2 + 2).Value = label;
                                    sheet.Cell(lr + 1, i * 2 + 2).Value = value;
                                }
                                else
                                {
                                    // 后续对：C=3, D=4 (lastCol)
                                    sheet.Cell(lr, 3).Value = label;
                                    sheet.Cell(lr + 1, lastCol).Value = value;
                                }
                            }
                        }

                        ApplyThinBorder(sheet.Range(cr, 1, cr + criteriaRows - 1, lastCol));
                        row = cr + criteriaRows;
                    }

                    row += 2; // 空行间隔
                }

                AutoFitSheet(sheet);
            }

            workbook.SaveAs(dialog.FileName);

            // 保存到本地数据库（一条汇总记录，关联 Excel 文件路径）
            try
            {
                var records = new List<TestRecord>();
                foreach (var deviceTab in DeviceTabs)
                {
                    foreach (var table in deviceTab.Tables)
                    {
                        if (table.Rows.Count == 0) continue;
                        records.Add(new TestRecord
                        {
                            PlanNo = PlanNo,
                            DeviceType = deviceTab.Category,
                            CheckName = table.DisplayName,
                            CheckValue = $"{table.Rows.Count}条",
                            Result = "导出",
                            TestTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                            Remark = SpecimenNumber,
                            ExcelPath = dialog.FileName
                        });
                    }
                }
                if (records.Count > 0)
                    _testService.SaveTestRecords(records);
            }
            catch { }

            StatusMessage = $"已保存到: {dialog.FileName}";

            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = dialog.FileName,
                    UseShellExecute = true
                });
            }
            catch { }
        }
        catch (Exception ex)
        {
            StatusMessage = $"保存失败: {ex.Message}";
        }
    }

    /// <summary>
    /// 根据表格显示名获取判定标准行（当前使用固定值，后续可从计划信息中读取）。
    /// 返回 (标准名, 标准值) 列表，每项占一行。
    /// </summary>
    private static List<(string Label, string Value)> GetCriteriaRows(string displayName) => displayName switch
    {
        "C(μF)" => [("标称值", "1000"), ("容量正偏差", "±20%"), ("容量上限", "1200"), ("容量下限", "800")],
        "损耗" => [("损耗标准", "≤0.2")],
        "ESR(MΩ)" => [("ESR标准", "≤0.50")],
        "阻抗(MΩ)" => [("阻抗标准", "≤8")],
        "IL正向(μA)" or "HL正向(S)" => [("漏电流标准", "≤2012"), ("漏电流标准", "≤2012")],
        "IL反向(μA)" or "HL反向(S)" => [("漏电流标准", "≤2012"), ("漏电流标准", "≤2012")],
        "LC(μF)" or "IR(MΩ)" => [("绝缘电阻判定值", "≥100")],
        _ => []
    };

    /// <summary>
    /// 获取判定标准的数值阈值（用于数据着色比对，当前使用固定值）。
    /// </summary>
    private static double? GetNumericThreshold(string displayName) => displayName switch
    {
        "C(μF)" => 1200,      // 容量上限
        "损耗" => 0.2,
        "ESR(MΩ)" => 0.50,
        "阻抗(MΩ)" => 8,
        "IL正向(μA)" or "IL反向(μA)" => 2012,
        "LC(μF)" => 100,
        "IR(MΩ)" => 100,
        _ => null
    };

    /// <summary>
    /// 是否为下限判定标准（值越大越好，低于阈值标红，高于标绿）。
    /// </summary>
    private static bool IsLowerLimit(string displayName) => displayName is "LC(μF)" or "IR(MΩ)";

    private static void ApplyThinBorder(ClosedXML.Excel.IXLRange range)
    {
        range.Style.Border.OutsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
        range.Style.Border.InsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
    }

    private static void ApplyCenterBold(ClosedXML.Excel.IXLRange range)
    {
        range.Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;
        range.Style.Font.Bold = true;
    }

    /// <summary>
    /// 手动自适应列宽和行高（ClosedXML 的 AdjustToContents 对合并单元格无效）。
    /// 中文字符按2倍宽度计算。
    /// </summary>
    private static void AutoFitSheet(ClosedXML.Excel.IXLWorksheet sheet)
    {
        var usedRange = sheet.RangeUsed();
        if (usedRange == null) return;

        int lastRow = usedRange.LastRow().RowNumber();
        int lastCol = usedRange.LastColumn().ColumnNumber();

        // 计算每列最大字符宽度
        var colWidths = new double[lastCol + 1];
        for (int col = 1; col <= lastCol; col++)
        {
            double maxWidth = 4; // 最小宽度
            for (int r = 1; r <= lastRow; r++)
            {
                var cell = sheet.Cell(r, col);
                var text = cell.GetString();
                if (string.IsNullOrEmpty(text)) continue;
                // 中文字符按2倍宽度
                double width = 0;
                foreach (var ch in text)
                    width += ch > 127 ? 2.1 : 1.0;
                width += 2; // padding
                if (width > maxWidth) maxWidth = width;
            }
            colWidths[col] = maxWidth;
        }

        // 应用列宽
        for (int col = 1; col <= lastCol; col++)
            sheet.Column(col).Width = colWidths[col];

        // 行高自适应（基于字号）
        for (int r = 1; r <= lastRow; r++)
        {
            var row = sheet.Row(r);
            var fontSize = row.Style.Font.FontSize > 0 ? row.Style.Font.FontSize : sheet.Style.Font.FontSize > 0 ? sheet.Style.Font.FontSize : 11;
            row.Height = fontSize * 1.8;
        }
    }

    /// <summary>
    /// 上传数据命令，将测试记录异步上传到远程数据库。
    /// </summary>
    [RelayCommand]
    private async Task UploadDataAsync()
    {
        if (DeviceTabs.All(dt => dt.Tables.All(t => t.Rows.Count == 0)))
        {
            StatusMessage = "没有数据可上传";
            return;
        }

        StatusMessage = "正在上传...";
        try
        {
            await _reportService.UploadTestRecordsAsync(PlanNo, SpecimenNumber);
            StatusMessage = "上传完成";
        }
        catch (Exception ex)
        {
            StatusMessage = $"上传失败: {ex.Message}";
        }
    }

    /// <summary>
    /// 清空数据命令，清除所有表格和选项卡。
    /// </summary>
    [RelayCommand]
    private void ClearData()
    {
        foreach (var deviceTab in DeviceTabs)
        {
            foreach (var table in deviceTab.Tables)
                table.Clear();
        }
        DeviceTabs.Clear();
        ActiveDeviceTab = null;
        ActiveTable = null;
        StatusMessage = "数据已清空";
    }
}
