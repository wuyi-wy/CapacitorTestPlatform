using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Controls;

namespace CapacitorTestPlatform.UI.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty]
    private string _title = "电容测试平台";

    [ObservableProperty]
    private string _statusText = "就绪";

    [ObservableProperty]
    private string _connectionStatus = "未连接";

    [ObservableProperty]
    private string _currentPlan = "";

    [ObservableProperty]
    private string _deviceStatus = "无设备";

    [ObservableProperty]
    private int _selectedMenuIndex;

    [ObservableProperty]
    private Page? _currentPage;

    public Page? PlanImportPage { get; set; }
    public Page? TestPage { get; set; }
    public Page? HistoryPage { get; set; }
    public Page? SettingsPage { get; set; }

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
