using CapacitorTestPlatform.Core.Interfaces;
using CapacitorTestPlatform.Core.Models;
using CapacitorTestPlatform.UI.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CapacitorTestPlatform.UI.ViewModels;

/// <summary>
/// 测试页面 ViewModel，管理计划选择、设备数据导入和检测记录保存/上传。
/// </summary>
public partial class TestPageViewModel : ObservableObject
{
    private readonly ITestService _testService;
    private readonly IReportService _reportService;

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

    /// <summary>测试数据动态表格容器</summary>
    public TestDataTable TestDataTable { get; } = new();

    /// <summary>请求打开设备选择弹窗时触发的事件</summary>
    public event EventHandler? RequestDeviceSelect;

    /// <summary>
    /// 初始化测试页面 ViewModel，注入测试服务和报告服务。
    /// </summary>
    public TestPageViewModel(ITestService testService, IReportService reportService)
    {
        _testService = testService;
        _reportService = reportService;
    }

    /// <summary>
    /// 使用计划信息初始化页面，设置计划编号、产品型号和测试项。
    /// </summary>
    /// <param name="plan">检测计划信息</param>
    /// <param name="testItem">测试项名称</param>
    public void InitializeWithPlan(PlanInfo plan, string testItem)
    {
        PlanNo = plan.PlanNo;
        ProductModel = plan.ProductModel ?? "";
        TestItem = testItem;
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
        RequestDeviceSelect?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// 将设备采集的数据导入到测试数据表中。
    /// </summary>
    /// <param name="deviceData">设备返回的测试数据表</param>
    public void ImportDeviceData(TestDataTable deviceData)
    {
        foreach (var row in deviceData.Rows)
        {
            TestDataTable.AddRow(row.Data.ToDictionary(kv => kv.Key, kv => kv.Value));
        }
        StatusMessage = $"已导入 {deviceData.Rows.Count} 条数据";
    }

    /// <summary>
    /// 删除指定的数据行。
    /// </summary>
    /// <param name="row">要删除的数据行</param>
    [RelayCommand]
    private void DeleteRow(TestDataRow? row)
    {
        if (row == null) return;
        TestDataTable.RemoveRow(row);
        StatusMessage = $"已删除第 {row.SeqNo} 行";
    }

    /// <summary>
    /// 保存数据命令，将测试数据导出为 CSV 文件（UTF-8 BOM 编码），保存后自动打开文件。
    /// </summary>
    [RelayCommand]
    private void SaveData()
    {
        if (TestDataTable.Rows.Count == 0)
        {
            StatusMessage = "没有数据可保存";
            return;
        }

        try
        {
            var columns = Models.TestDataRow.GetColumnOrder();
            var defaultName = $"测试数据_{PlanNo}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Title = "保存测试数据",
                Filter = "CSV文件|*.csv|Excel文件|*.xlsx|所有文件|*.*",
                FileName = defaultName,
                DefaultExt = ".csv"
            };

            if (dialog.ShowDialog() != true) return;

            var filePath = dialog.FileName;
            var lines = new List<string>();

            // 表头
            var headers = new List<string> { "序号" };
            headers.AddRange(columns);
            lines.Add(string.Join(",", headers));

            // 数据行
            foreach (var row in TestDataTable.Rows)
            {
                var values = new List<string> { row.SeqNo.ToString() };
                foreach (var col in columns)
                    values.Add(row[col]);
                lines.Add(string.Join(",", values));
            }

            // UTF-8 BOM 让 Excel 正确识别中文
            var utf8Bom = new System.Text.UTF8Encoding(true);
            System.IO.File.WriteAllText(filePath, string.Join("\n", lines), utf8Bom);

            StatusMessage = $"已保存到: {filePath}";

            // 自动打开文件
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = filePath,
                    UseShellExecute = true
                });
            }
            catch
            {
                // 打开失败不影响保存成功提示
            }
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
        if (TestDataTable.Rows.Count == 0)
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
    /// 清空数据命令，清除测试数据表中的所有行。
    /// </summary>
    [RelayCommand]
    private void ClearData()
    {
        TestDataTable.Clear();
        StatusMessage = "数据已清空";
    }
}
