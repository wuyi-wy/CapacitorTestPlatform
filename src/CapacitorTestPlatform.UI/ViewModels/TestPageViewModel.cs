using CapacitorTestPlatform.Core.Interfaces;
using CapacitorTestPlatform.Core.Models;
using CapacitorTestPlatform.UI.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CapacitorTestPlatform.UI.ViewModels;

public partial class TestPageViewModel : ObservableObject
{
    private readonly ITestService _testService;
    private readonly IReportService _reportService;

    [ObservableProperty]
    private string _planNo = "";

    [ObservableProperty]
    private string _productModel = "";

    [ObservableProperty]
    private string _testItem = "";

    [ObservableProperty]
    private string _specimenNumber = "";

    [ObservableProperty]
    private string _statusMessage = "就绪";

    public TestDataTable TestDataTable { get; } = new();

    public event EventHandler? RequestDeviceSelect;

    public TestPageViewModel(ITestService testService, IReportService reportService)
    {
        _testService = testService;
        _reportService = reportService;
    }

    public void InitializeWithPlan(PlanInfo plan, string testItem)
    {
        PlanNo = plan.PlanNo;
        ProductModel = plan.ProductModel ?? "";
        TestItem = testItem;
    }

    public void InitializeWithoutPlan()
    {
    }

    [RelayCommand]
    private void AcquireData()
    {
        RequestDeviceSelect?.Invoke(this, EventArgs.Empty);
    }

    public void ImportDeviceData(TestDataTable deviceData)
    {
        foreach (var row in deviceData.Rows)
        {
            TestDataTable.AddRow(row.Data.ToDictionary(kv => kv.Key, kv => kv.Value));
        }
        StatusMessage = $"已导入 {deviceData.Rows.Count} 条数据";
    }

    [RelayCommand]
    private void DeleteRow(TestDataRow? row)
    {
        if (row == null) return;
        TestDataTable.RemoveRow(row);
        StatusMessage = $"已删除第 {row.SeqNo} 行";
    }

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

    [RelayCommand]
    private void ClearData()
    {
        TestDataTable.Clear();
        StatusMessage = "数据已清空";
    }
}
