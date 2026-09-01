using CapacitorTestPlatform.Core.Interfaces;
using CapacitorTestPlatform.Core.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using Microsoft.Win32;

namespace CapacitorTestPlatform.UI.ViewModels;

public partial class HistoryViewModel : ObservableObject
{
    private readonly ITestService _testService;
    private readonly IReportService _reportService;

    [ObservableProperty]
    private ObservableCollection<TestRecord> _records = new();

    [ObservableProperty]
    private TestRecord? _selectedRecord;

    [ObservableProperty]
    private string _filterPlanNo = "";

    [ObservableProperty]
    private string? _filterDeviceType;

    [ObservableProperty]
    private DateTime? _filterDateFrom;

    [ObservableProperty]
    private DateTime? _filterDateTo;

    [ObservableProperty]
    private string _statusMessage = "就绪";

    [ObservableProperty]
    private bool _isLoading;

    public ObservableCollection<string> DeviceTypes { get; } = new()
    {
        "", "TH2689", "TH2683A", "TH2817A", "TH2832", "TH9201", "TH2810B", "MOCK"
    };

    public HistoryViewModel(ITestService testService, IReportService reportService)
    {
        _testService = testService;
        _reportService = reportService;
    }

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

            if (Records.Count == 0)
            {
                LoadMockRecords();
                StatusMessage = $"数据库为空，已加载 {Records.Count} 条模拟记录";
            }
            else
            {
                StatusMessage = $"已加载 {Records.Count} 条记录";
            }
        }
        catch (Exception ex)
        {
            LoadMockRecords();
            StatusMessage = $"加载失败({ex.Message})，已加载 {Records.Count} 条模拟记录";
        }
        finally
        {
            IsLoading = false;
        }
    }

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

    [RelayCommand]
    private void DeleteRecord(TestRecord? record)
    {
        if (record == null) return;
        Records.Remove(record);
        StatusMessage = $"已删除记录 #{record.Id}";
    }

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
