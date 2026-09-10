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

    /// <summary>
    /// 将设备采集的数据导入到测试页面。
    /// 按设备分类（DeviceCategory）分组，同分类下按 Title（testItem）匹配：
    /// - 匹配已有表格 → 追加数据
    /// - 不匹配 → 创建新表格（LCR数字电桥下会有多个子表格）
    /// - 新设备分类 → 创建新选项卡
    /// </summary>
    /// <param name="deviceData">设备返回的测试数据表（含 Title 和 DeviceCategory）</param>
    public void ImportDeviceData(TestDataTable deviceData)
    {
        var category = deviceData.DeviceCategory;
        if (string.IsNullOrEmpty(category))
            category = "其他";

        // 查找或创建设备分类选项卡
        var deviceTab = DeviceTabs.FirstOrDefault(t => t.Category == category);
        if (deviceTab == null)
        {
            deviceTab = new DeviceCategoryTab { Category = category };
            DeviceTabs.Add(deviceTab);
        }

        // 在该分类下查找匹配的数据表（同 DisplayName 且列兼容）
        var incomingColumns = deviceData.GetColumnOrder();
        TestDataTable? targetTable = null;

        foreach (var table in deviceTab.Tables)
        {
            if (table.DisplayName == deviceData.DisplayName)
            {
                // 同名表格：检查列是否兼容
                if (table.GetColumnOrder().Count == 0 ||
                    (table.HasColumns(incomingColumns) && table.GetColumnOrder().Count == incomingColumns.Count))
                {
                    targetTable = table;
                    break;
                }
            }
        }

        // 没有匹配的表格，创建新表格
        if (targetTable == null)
        {
            targetTable = new TestDataTable
            {
                Title = deviceData.Title,
                SubTitle = deviceData.SubTitle,
                DeviceCategory = category
            };
            deviceTab.Tables.Add(targetTable);
        }

        // 导入数据行
        foreach (var row in deviceData.Rows)
        {
            targetTable.AddRow(row.Data.ToDictionary(kv => kv.Key, kv => kv.Value));
        }

        ActiveDeviceTab = deviceTab;
        ActiveTable = targetTable;
        StatusMessage = $"已导入 {deviceData.Rows.Count} 条数据到「{category} - {targetTable.Title}」";
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
    /// 保存数据命令，将所有表格导出为 CSV 文件。
    /// 按设备分类 → 表格名 分段输出。
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
            var defaultName = $"测试数据_{PlanNo}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Title = "保存测试数据",
                Filter = "CSV文件|*.csv|所有文件|*.*",
                FileName = defaultName,
                DefaultExt = ".csv"
            };

            if (dialog.ShowDialog() != true) return;

            var filePath = dialog.FileName;
            var lines = new List<string>();

            foreach (var deviceTab in DeviceTabs)
            {
                foreach (var table in deviceTab.Tables)
                {
                    if (table.Rows.Count == 0) continue;

                    // 表格标题行：设备分类 - 显示名
                    lines.Add($"【{deviceTab.Category} - {table.DisplayName}】");

                    // 表头
                    var columns = table.GetColumnOrder();
                    var headers = new List<string> { "序号" };
                    headers.AddRange(columns);
                    lines.Add(string.Join(",", headers));

                    // 数据行
                    foreach (var row in table.Rows)
                    {
                        var values = new List<string> { row.SeqNo.ToString() };
                        foreach (var col in columns)
                            values.Add(row[col]);
                        lines.Add(string.Join(",", values));
                    }

                    lines.Add(""); // 空行分隔
                }
            }

            var utf8Bom = new System.Text.UTF8Encoding(true);
            System.IO.File.WriteAllText(filePath, string.Join("\n", lines), utf8Bom);

            StatusMessage = $"已保存到: {filePath}";

            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = filePath,
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
