using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Controls;

namespace CapacitorTestPlatform.UI.ViewModels;

/// <summary>
/// 主窗口 ViewModel，管理窗口标题、状态栏信息和页面导航。
/// </summary>
public partial class MainWindowViewModel : ObservableObject
{
    /// <summary>窗口标题</summary>
    [ObservableProperty]
    private string _title = "电容测试平台";

    /// <summary>状态栏提示文本</summary>
    [ObservableProperty]
    private string _statusText = "就绪";

    /// <summary>设备连接状态文本</summary>
    [ObservableProperty]
    private string _connectionStatus = "未连接";

    /// <summary>当前选中的计划编号</summary>
    [ObservableProperty]
    private string _currentPlan = "";

    /// <summary>设备状态描述</summary>
    [ObservableProperty]
    private string _deviceStatus = "无设备";

    /// <summary>侧边菜单当前选中项索引</summary>
    [ObservableProperty]
    private int _selectedMenuIndex;

    /// <summary>当前显示的页面</summary>
    [ObservableProperty]
    private Page? _currentPage;

    /// <summary>计划导入页面实例</summary>
    public Page? PlanImportPage { get; set; }

    /// <summary>测试页面实例</summary>
    public Page? TestPage { get; set; }

    /// <summary>历史记录页面实例</summary>
    public Page? HistoryPage { get; set; }

    /// <summary>设置页面实例</summary>
    public Page? SettingsPage { get; set; }

    /// <summary>
    /// 导航命令，根据页面名称切换当前显示的页面。
    /// </summary>
    /// <param name="pageName">页面标识名称（PlanImport/Test/History/Settings）</param>
    [RelayCommand]
    private void NavigateTo(string pageName)
    {
        CurrentPage = pageName switch
        {
            "PlanImport" => PlanImportPage,
            "Test" => TestPage,
            "History" => HistoryPage,
            "Settings" => SettingsPage,
            _ => PlanImportPage
        };
    }
}
