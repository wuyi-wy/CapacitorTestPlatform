using CapacitorTestPlatform.Core.Interfaces;
using CapacitorTestPlatform.Core.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using Microsoft.Win32;

namespace CapacitorTestPlatform.UI.ViewModels;

/// <summary>
/// 历史记录 ViewModel，负责查询、展示、删除和导出检测历史数据。
/// </summary>
public partial class HistoryViewModel : ObservableObject
{
    private readonly ITestService _testService;
    private readonly IReportService _reportService;

    /// <summary>检测记录列表</summary>
    [ObservableProperty]
    private ObservableCollection<TestRecord> _records = new();

    /// <summary>当前选中的记录</summary>
    [ObservableProperty]
    private TestRecord? _selectedRecord;

    /// <summary>按计划编号筛选</summary>
    [ObservableProperty]
    private string _filterPlanNo = "";

    /// <summary>按设备类型筛选</summary>
    [ObservableProperty]
    private string? _filterDeviceType;

    /// <summary>筛选起始日期</summary>
    [ObservableProperty]
    private DateTime? _filterDateFrom;

    /// <summary>筛选截止日期</summary>
    [ObservableProperty]
    private DateTime? _filterDateTo;

    /// <summary>状态栏提示消息</summary>
    [ObservableProperty]
    private string _statusMessage = "就绪";

    /// <summary>是否正在加载数据</summary>
    [ObservableProperty]
    private bool _isLoading;

    /// <summary>可选设备类型列表（含空值"全部"选项）</summary>
    public ObservableCollection<string> DeviceTypes { get; } = new()
    {
        "", "TH2689", "TH2683A", "TH2817A", "TH2832", "TH9201", "TH2810B", "MOCK"
    };

    /// <summary>
    /// 初始化历史记录 ViewModel，注入测试服务和报告服务。
    /// </summary>
    public HistoryViewModel(ITestService testService, IReportService reportService)
    {
        _testService = testService;
        _reportService = reportService;
    }

    /// <summary>
    /// 加载记录命令，根据筛选条件查询检测历史。
    /// </summary>
    [RelayCommand]
    private async Task LoadRecordsAsync()
    {
        IsLoading = true;
        try
        {
            var records = string.IsNullOrWhiteSpace(FilterPlanNo)
                ? _testService.GetTestHistory("")
                : _testService.GetTestHistory(FilterPlanNo);

            Records.Clear();
            foreach (var record in records)
                Records.Add(record);

            StatusMessage = Records.Count > 0
                ? $"已加载 {Records.Count} 条记录"
                : "没有找到记录";
        }
        catch (Exception ex)
        {
            StatusMessage = $"加载失败: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// 加载模拟检测记录，用于数据库不可用时的演示和开发。
    /// </summary>
    private void LoadMockRecords()
    {
        Records.Clear();
        var mockRecords = new List<TestRecord>
        {
            new TestRecord { Id = 1, PlanNo = "JH2025001", DeviceType = "TH2817A", SpecName = "电容量", CheckValue = "0.102uF", Result = "PASS", TestTime = "2025-08-15 09:30:15", Operator = "张工" },
            new TestRecord { Id = 2, PlanNo = "JH2025001", DeviceType = "TH2817A", SpecName = "损耗角正切值", CheckValue = "0.0012", Result = "PASS", TestTime = "2025-08-15 09:30:18", Operator = "张工" },
            new TestRecord { Id = 3, PlanNo = "JH2025001", DeviceType = "TH2689", SpecName = "漏电流", CheckValue = "0.05uA", Result = "PASS", TestTime = "2025-08-15 09:31:02", Operator = "张工" },
            new TestRecord { Id = 4, PlanNo = "JH2025002", DeviceType = "TH2817A", SpecName = "电容量", CheckValue = "0.225uF", Result = "PASS", TestTime = "2025-08-15 10:15:33", Operator = "李工" },
            new TestRecord { Id = 5, PlanNo = "JH2025002", DeviceType = "TH2683A", SpecName = "绝缘电阻", CheckValue = "1500MΩ", Result = "PASS", TestTime = "2025-08-15 10:16:01", Operator = "李工" },
            new TestRecord { Id = 6, PlanNo = "JH2025003", DeviceType = "TH2817A", SpecName = "电容量", CheckValue = "0.046uF", Result = "FAIL", TestTime = "2025-08-15 11:00:45", Operator = "王工" },
            new TestRecord { Id = 7, PlanNo = "JH2025003", DeviceType = "TH2689", SpecName = "漏电流", CheckValue = "1.2uA", Result = "PASS", TestTime = "2025-08-15 11:01:12", Operator = "王工" },
            new TestRecord { Id = 8, PlanNo = "JH2025006", DeviceType = "TH2832", SpecName = "阻抗", CheckValue = "0.08Ω", Result = "PASS", TestTime = "2025-08-15 14:20:05", Operator = "张工" },
        };
        foreach (var r in mockRecords)
            Records.Add(r);
    }

    /// <summary>
    /// 上传命令，将当前筛选条件下的检测记录异步上传到远程数据库。
    /// </summary>
    [RelayCommand]
    private async Task UploadAsync()
    {
        try
        {
            await _reportService.UploadTestRecordsAsync(FilterPlanNo, "");
            StatusMessage = "上传完成";
        }
        catch (Exception ex)
        {
            StatusMessage = $"上传失败: {ex.Message}";
        }
    }

    /// <summary>
    /// 删除指定的检测记录。
    /// </summary>
    /// <param name="record">要删除的检测记录</param>
    [RelayCommand]
    private void DeleteRecord(TestRecord? record)
    {
        if (record == null) return;
        Records.Remove(record);
        StatusMessage = $"已删除记录 #{record.Id}";
    }

    /// <summary>
    /// 打开 Excel 文件命令。
    /// </summary>
    [RelayCommand]
    private void OpenExcel(TestRecord? record)
    {
        if (record == null || string.IsNullOrWhiteSpace(record.ExcelPath))
        {
            StatusMessage = "该记录没有关联的 Excel 文件";
            return;
        }

        if (!System.IO.File.Exists(record.ExcelPath))
        {
            StatusMessage = $"文件不存在: {record.ExcelPath}";
            return;
        }

        try
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = record.ExcelPath,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            StatusMessage = $"打开失败: {ex.Message}";
        }
    }

    /// <summary>
    /// 导出 Excel 命令，将当前记录列表导出为 .xlsx 文件。
    /// </summary>
    [RelayCommand]
    private async Task ExportExcelAsync()
    {
        try
        {
            var dialog = new SaveFileDialog
            {
                Filter = "Excel文件|*.xlsx",
                FileName = $"测试数据_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
            };

            if (dialog.ShowDialog() == true)
            {
                StatusMessage = $"已导出到: {dialog.FileName}";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"导出失败: {ex.Message}";
        }
    }
}
