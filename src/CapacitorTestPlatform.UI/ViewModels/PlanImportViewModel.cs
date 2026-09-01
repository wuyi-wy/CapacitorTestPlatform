using CapacitorTestPlatform.Core.Interfaces;
using CapacitorTestPlatform.Core.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace CapacitorTestPlatform.UI.ViewModels;

public partial class PlanImportViewModel : ObservableObject
{
    private readonly IPlanService _planService;

    [ObservableProperty]
    private ObservableCollection<PlanInfo> _plans = new();

    [ObservableProperty]
    private PlanInfo? _selectedPlan;

    [ObservableProperty]
    private string _searchKeyword = "";

    [ObservableProperty]
    private string? _selectedStation;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _statusMessage = "就绪";

    public ObservableCollection<string> Stations { get; } = new()
    {
        "电容性能台1#", "电容性能台2#", "电容性能台3#"
    };

    public event EventHandler<PlanInfo>? NavigateToTest;

    public PlanImportViewModel(IPlanService planService)
    {
        _planService = planService;
        SelectedStation = "电容性能台1#";
    }

    [RelayCommand]
    private async Task LoadPlansAsync()
    {
        IsLoading = true;
        StatusMessage = "正在加载计划...";

        try
        {
            var plans = await _planService.GetPlansAsync();
            Plans.Clear();
            foreach (var plan in plans)
                Plans.Add(plan);

            if (Plans.Count == 0)
            {
                LoadMockPlans();
                StatusMessage = $"远程数据库未连接，已加载 {Plans.Count} 条模拟计划";
            }
            else
            {
                StatusMessage = $"已加载 {Plans.Count} 条计划";
            }
        }
        catch (Exception ex)
        {
            LoadMockPlans();
            StatusMessage = $"加载失败({ex.Message})，已加载 {Plans.Count} 条模拟计划";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void LoadMockPlans()
    {
        Plans.Clear();
        var mockPlans = new List<PlanInfo>
        {
            new PlanInfo { PlanNo = "JH2025001", ProductModel = "CL21-104J-100V", TestItems = "电容量;损耗角正切值(ESR)", Station = "电容性能台1#", Status = "测试中" },
            new PlanInfo { PlanNo = "JH2025002", ProductModel = "CL21-224J-100V", TestItems = "电容量;绝缘外套的绝缘电阻", Station = "电容性能台1#", Status = "测试中" },
            new PlanInfo { PlanNo = "JH2025003", ProductModel = "CL21-473J-250V", TestItems = "电容量;漏电流", Station = "电容性能台1#", Status = "录入中" },
            new PlanInfo { PlanNo = "JH2025004", ProductModel = "CBB22-105J-400V", TestItems = "电容量;损耗角正切值(ESR);阻抗", Station = "电容性能台2#", Status = "测试中" },
            new PlanInfo { PlanNo = "JH2025005", ProductModel = "CBB22-225J-400V", TestItems = "电容量;绝缘外套的绝缘电阻", Station = "电容性能台2#", Status = "测试中" },
            new PlanInfo { PlanNo = "JH2025006", ProductModel = "CD110-1000uF-16V", TestItems = "电容量;漏电流;阻抗", Station = "电容性能台1#", Status = "测试中" },
            new PlanInfo { PlanNo = "JH2025007", ProductModel = "CD110-470uF-25V", TestItems = "电容量;损耗角正切值(ESR);漏电流", Station = "电容性能台3#", Status = "录入中" },
            new PlanInfo { PlanNo = "JH2025008", ProductModel = "CT7-101K-1KV", TestItems = "电容量;绝缘外套的绝缘电阻;极壳耐压", Station = "电容性能台3#", Status = "测试中" },
            new PlanInfo { PlanNo = "JH2025009", ProductModel = "CT7-471K-1KV", TestItems = "电容量;绝缘外套的绝缘电阻", Station = "电容性能台1#", Status = "测试中" },
            new PlanInfo { PlanNo = "JH2025010", ProductModel = "CL21-333J-400V", TestItems = "电容量;漏电流;极壳耐压", Station = "电容性能台2#", Status = "测试中" },
        };
        foreach (var plan in mockPlans)
            Plans.Add(plan);
    }

    [RelayCommand]
    private void Search()
    {
        if (string.IsNullOrWhiteSpace(SearchKeyword))
        {
            LoadPlansCommand.Execute(null);
            return;
        }

        var results = _planService.Search(SearchKeyword);
        Plans.Clear();
        foreach (var plan in results)
            Plans.Add(plan);

        if (Plans.Count == 0)
        {
            LoadMockPlans();
            var filtered = Plans.Where(p =>
                p.PlanNo.Contains(SearchKeyword, StringComparison.OrdinalIgnoreCase) ||
                (p.ProductModel?.Contains(SearchKeyword, StringComparison.OrdinalIgnoreCase) ?? false)).ToList();
            Plans.Clear();
            foreach (var plan in filtered)
                Plans.Add(plan);
        }
    }

    [RelayCommand]
    private void GoToTest(PlanInfo? plan)
    {
        if (plan != null)
            NavigateToTest?.Invoke(this, plan);
    }
}
