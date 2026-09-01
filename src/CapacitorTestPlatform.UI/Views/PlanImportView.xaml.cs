using CapacitorTestPlatform.Core.Interfaces;
using CapacitorTestPlatform.Core.Models;
using CapacitorTestPlatform.UI.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Controls;

namespace CapacitorTestPlatform.UI.Views;

/// <summary>
/// 计划导入页面，负责从远程拉取测试计划并选择保存。
/// </summary>
public partial class PlanImportView : Page
{
    /// <summary>计划导入视图模型</summary>
    private readonly PlanImportViewModel _viewModel;

    /// <summary>
    /// 初始化计划导入页面，绑定 ViewModel、注册导航事件并在加载时自动拉取计划列表。
    /// </summary>
    /// <param name="viewModel">计划导入视图模型实例。</param>
    public PlanImportView(PlanImportViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = _viewModel;

        _viewModel.NavigateToTest += OnNavigateToTest;
        Loaded += async (s, e) => await _viewModel.LoadPlansCommand.ExecuteAsync(null);
    }

    /// <summary>
    /// 响应 ViewModel 的导航请求，跳转到测试页面。
    /// </summary>
    private void OnNavigateToTest(object? sender, PlanInfo plan)
    {
        var mainWindow = Window.GetWindow(this) as MainWindow;
        mainWindow?.NavigateToTestPage(plan, plan.TestItemList.FirstOrDefault() ?? "");
    }
}
