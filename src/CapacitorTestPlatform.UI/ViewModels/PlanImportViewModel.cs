using CapacitorTestPlatform.Core.Interfaces;
using CapacitorTestPlatform.Core.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace CapacitorTestPlatform.UI.ViewModels;

/// <summary>
/// 计划导入 ViewModel，负责从远程数据库加载检测计划、筛选搜索并跳转到测试页面。
/// </summary>
public partial class PlanImportViewModel : ObservableObject
{
    private readonly IPlanService _planService;

    /// <summary>计划列表数据源</summary>
    [ObservableProperty]
    private ObservableCollection<PlanInfo> _plans = new();

    /// <summary>当前选中的计划</summary>
    [ObservableProperty]
    private PlanInfo? _selectedPlan;

    /// <summary>计划搜索关键字</summary>
    [ObservableProperty]
    private string _searchKeyword = "";

    /// <summary>当前选中的工站</summary>
    [ObservableProperty]
    private string? _selectedStation;

    /// <summary>是否正在加载数据</summary>
    [ObservableProperty]
    private bool _isLoading;

    /// <summary>状态栏提示消息</summary>
    [ObservableProperty]
    private string _statusMessage = "就绪";

    /// <summary>可选工站列表</summary>
    public ObservableCollection<string> Stations { get; } = new()
    {
        "电容性能台1#", "电容性能台2#", "电容性能台3#"
    };

    /// <summary>导航到测试页面时触发的事件</summary>
    public event EventHandler<PlanInfo>? NavigateToTest;

    /// <summary>
    /// 初始化计划导入 ViewModel，注入计划服务并设置默认工站。
    /// </summary>
    public PlanImportViewModel(IPlanService planService)
    {
        _planService = planService;
        SelectedStation = "电容性能台1#";
    }

    /// <summary>
    /// 加载计划命令：优先从远程拉取（同步保存到本地），连接异常时回退到本地/模拟数据，远程为空时提示当前台位没有测试中的单。
    /// </summary>
    [RelayCommand]
    private async Task LoadPlansAsync()
    {
        IsLoading = true;
        StatusMessage = "正在加载计划...";

        try
        {
            // 优先从远程 SQL Server 拉取当前工站的计划
            var remotePlans = await _planService.FetchFromRemoteAsync();

            if (remotePlans != null)
            {
                // 远程连接成功
                Plans.Clear();
                foreach (var plan in remotePlans)
                    Plans.Add(plan);

                if (Plans.Count == 0)
                {
                    StatusMessage = "当前台位没有测试中的单";
                }
                else
                {
                    StatusMessage = $"已从远程加载 {Plans.Count} 条计划";
                }
            }
            else
            {
                // 远程不可用，回退到本地数据
                var localPlans = await _planService.GetPlansAsync();
                Plans.Clear();
                foreach (var plan in localPlans)
                    Plans.Add(plan);

                if (Plans.Count == 0)
                {
                    LoadMockPlans();
                    StatusMessage = $"远程数据库未连接，已加载 {Plans.Count} 条模拟计划";
                }
                else
                {
                    StatusMessage = $"远程不可用，已加载 {Plans.Count} 条本地计划";
                }
            }
        }
        catch (Exception ex)
        {
            // 连接异常，显示模拟数据
            LoadMockPlans();
            StatusMessage = $"远程连接异常({ex.Message})，已加载 {Plans.Count} 条模拟计划";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// 加载模拟计划数据，用于远程数据库不可用时的演示和开发。
    /// </summary>
    private void LoadMockPlans()
    {
        Plans.Clear();
        // 模拟数据：每个实验项目拆分为独立记录（与远程拉取后的格式一致）
        var mockPlans = new List<PlanInfo>
        {
            new PlanInfo { ContractNumber = "JH2025001", SampleType = "CL21-104J-100V", TestItems = "电容量", InstrumentNumber = "电容性能台1#", StatusName = "测试中" },
            new PlanInfo { ContractNumber = "JH2025001", SampleType = "CL21-104J-100V", TestItems = "损耗角正切值(ESR)", InstrumentNumber = "电容性能台1#", StatusName = "测试中" },
            new PlanInfo { ContractNumber = "JH2025002", SampleType = "CL21-224J-100V", TestItems = "电容量", InstrumentNumber = "电容性能台1#", StatusName = "测试中" },
            new PlanInfo { ContractNumber = "JH2025002", SampleType = "CL21-224J-100V", TestItems = "绝缘外套的绝缘电阻", InstrumentNumber = "电容性能台1#", StatusName = "测试中" },
            new PlanInfo { ContractNumber = "JH2025003", SampleType = "CL21-473J-250V", TestItems = "电容量", InstrumentNumber = "电容性能台1#", StatusName = "录入中" },
            new PlanInfo { ContractNumber = "JH2025003", SampleType = "CL21-473J-250V", TestItems = "漏电流", InstrumentNumber = "电容性能台1#", StatusName = "录入中" },
            new PlanInfo { ContractNumber = "JH2025004", SampleType = "CBB22-105J-400V", TestItems = "电容量", InstrumentNumber = "电容性能台2#", StatusName = "测试中" },
            new PlanInfo { ContractNumber = "JH2025004", SampleType = "CBB22-105J-400V", TestItems = "损耗角正切值(ESR)", InstrumentNumber = "电容性能台2#", StatusName = "测试中" },
            new PlanInfo { ContractNumber = "JH2025004", SampleType = "CBB22-105J-400V", TestItems = "阻抗", InstrumentNumber = "电容性能台2#", StatusName = "测试中" },
            new PlanInfo { ContractNumber = "JH2025005", SampleType = "CBB22-225J-400V", TestItems = "电容量", InstrumentNumber = "电容性能台2#", StatusName = "测试中" },
            new PlanInfo { ContractNumber = "JH2025005", SampleType = "CBB22-225J-400V", TestItems = "绝缘外套的绝缘电阻", InstrumentNumber = "电容性能台2#", StatusName = "测试中" },
            new PlanInfo { ContractNumber = "JH2025006", SampleType = "CD110-1000uF-16V", TestItems = "电容量", InstrumentNumber = "电容性能台1#", StatusName = "测试中" },
            new PlanInfo { ContractNumber = "JH2025006", SampleType = "CD110-1000uF-16V", TestItems = "漏电流", InstrumentNumber = "电容性能台1#", StatusName = "测试中" },
            new PlanInfo { ContractNumber = "JH2025006", SampleType = "CD110-1000uF-16V", TestItems = "阻抗", InstrumentNumber = "电容性能台1#", StatusName = "测试中" },
            new PlanInfo { ContractNumber = "JH2025008", SampleType = "CT7-101K-1KV", TestItems = "电容量", InstrumentNumber = "电容性能台3#", StatusName = "测试中" },
            new PlanInfo { ContractNumber = "JH2025008", SampleType = "CT7-101K-1KV", TestItems = "绝缘外套的绝缘电阻", InstrumentNumber = "电容性能台3#", StatusName = "测试中" },
            new PlanInfo { ContractNumber = "JH2025008", SampleType = "CT7-101K-1KV", TestItems = "极壳耐压", InstrumentNumber = "电容性能台3#", StatusName = "测试中" },
            new PlanInfo { ContractNumber = "JH2025009", SampleType = "CT7-471K-1KV", TestItems = "电容量", InstrumentNumber = "电容性能台1#", StatusName = "测试中" },
            new PlanInfo { ContractNumber = "JH2025009", SampleType = "CT7-471K-1KV", TestItems = "绝缘外套的绝缘电阻", InstrumentNumber = "电容性能台1#", StatusName = "测试中" },
            new PlanInfo { ContractNumber = "JH2025010", SampleType = "CL21-333J-400V", TestItems = "电容量", InstrumentNumber = "电容性能台2#", StatusName = "测试中" },
            new PlanInfo { ContractNumber = "JH2025010", SampleType = "CL21-333J-400V", TestItems = "漏电流", InstrumentNumber = "电容性能台2#", StatusName = "测试中" },
            new PlanInfo { ContractNumber = "JH2025010", SampleType = "CL21-333J-400V", TestItems = "极壳耐压", InstrumentNumber = "电容性能台2#", StatusName = "测试中" },
            new PlanInfo { ContractNumber = "JH2025010", SampleType = "CL21-333J-400V", TestItems = "可靠性前性能实验", InstrumentNumber = "电容性能台2#", StatusName = "测试中" },
        };
        foreach (var plan in mockPlans)
            Plans.Add(plan);
    }

    /// <summary>
    /// 搜索命令，按关键字过滤计划列表，无匹配时回退到模拟数据筛选。
    /// </summary>
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
                p.ContractNumber.Contains(SearchKeyword, StringComparison.OrdinalIgnoreCase) ||
                (p.SampleType?.Contains(SearchKeyword, StringComparison.OrdinalIgnoreCase) ?? false)).ToList();
            Plans.Clear();
            foreach (var plan in filtered)
                Plans.Add(plan);
        }
    }

    /// <summary>
    /// 跳转到测试页面命令，将选中的计划传递给测试页面。
    /// </summary>
    /// <param name="plan">要进行测试的计划信息</param>
    [RelayCommand]
    private void GoToTest(PlanInfo? plan)
    {
        if (plan != null)
            NavigateToTest?.Invoke(this, plan);
    }
}
