using CapacitorTestPlatform.Core.Interfaces;
using CapacitorTestPlatform.Core.Models;
using CapacitorTestPlatform.UI.ViewModels;
using CapacitorTestPlatform.UI.Views;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Controls;

namespace CapacitorTestPlatform.UI.Views;

/// <summary>
/// 主窗口，承载顶部导航栏和页面内容框架，管理各子页面的切换与初始化。
/// </summary>
public partial class MainWindow : Window
{
    /// <summary>主窗口 ViewModel</summary>
    private readonly MainWindowViewModel _viewModel;

    /// <summary>DI 服务提供者，用于按需获取页面实例</summary>
    private readonly IServiceProvider _serviceProvider;

    /// <summary>计划导入页面实例缓存</summary>
    private PlanImportView? _planImportPage;

    /// <summary>测试页面实例缓存</summary>
    private TestPageView? _testPage;

    /// <summary>历史记录页面实例缓存</summary>
    private HistoryView? _historyPage;

    /// <summary>
    /// 初始化主窗口，绑定 ViewModel 并注册 Loaded 事件。
    /// </summary>
    /// <param name="viewModel">主窗口视图模型。</param>
    /// <param name="serviceProvider">DI 服务提供者。</param>
    public MainWindow(MainWindowViewModel viewModel, IServiceProvider serviceProvider)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _serviceProvider = serviceProvider;
        DataContext = _viewModel;

        Loaded += MainWindow_Loaded;
    }

    /// <summary>
    /// 窗口加载完成后初始化子页面并导航到默认页面（计划导入）。
    /// </summary>
    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        _planImportPage = _serviceProvider.GetRequiredService<PlanImportView>();
        _testPage = _serviceProvider.GetRequiredService<TestPageView>();
        _historyPage = _serviceProvider.GetRequiredService<HistoryView>();

        NavigateTo("PlanImport");
    }

    /// <summary>
    /// 根据页面名称导航到对应的子页面。
    /// </summary>
    /// <param name="pageName">页面标识：PlanImport / Test / History。</param>
    public void NavigateTo(string pageName)
    {
        Page? page = pageName switch
        {
            "PlanImport" => _planImportPage ??= _serviceProvider.GetRequiredService<PlanImportView>(),
            "Test" => _testPage ??= _serviceProvider.GetRequiredService<TestPageView>(),
            "History" => _historyPage ??= _serviceProvider.GetRequiredService<HistoryView>(),
            _ => _planImportPage
        };

        if (page != null)
            ContentFrame.Navigate(page);
    }

    /// <summary>
    /// 导航到测试页面，并用指定计划和测试项初始化。
    /// </summary>
    /// <param name="plan">选中的测试计划。</param>
    /// <param name="testItem">测试项名称。</param>
    public void NavigateToTestPage(PlanInfo plan, string testItem)
    {
        _testPage = _serviceProvider.GetRequiredService<TestPageView>();
        var testVm = _testPage.DataContext as TestPageViewModel;
        testVm?.InitializeWithPlan(plan, testItem);
        ContentFrame.Navigate(_testPage);
    }

    /// <summary>
    /// 导航栏"计划导入"按钮点击事件。
    /// </summary>
    private void PlanImport_Selected(object sender, RoutedEventArgs e)
    {
        NavigateTo("PlanImport");
    }

    /// <summary>
    /// 导航栏"测试页面"按钮点击事件。
    /// </summary>
    private void TestPage_Selected(object sender, RoutedEventArgs e)
    {
        NavigateTo("Test");
    }

    /// <summary>
    /// 导航栏"历史记录"按钮点击事件。
    /// </summary>
    private void History_Selected(object sender, RoutedEventArgs e)
    {
        NavigateTo("History");
    }
}
