using CapacitorTestPlatform.UI.ViewModels;
using System.Windows.Controls;

namespace CapacitorTestPlatform.UI.Views;

/// <summary>
/// 历史记录页面，展示已保存的测试记录，支持查询和筛选。
/// </summary>
public partial class HistoryView : Page
{
    /// <summary>历史记录视图模型</summary>
    private readonly HistoryViewModel _viewModel;

    /// <summary>
    /// 初始化历史记录页面，绑定 ViewModel 并在加载时自动查询记录。
    /// </summary>
    /// <param name="viewModel">历史记录视图模型实例。</param>
    public HistoryView(HistoryViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = _viewModel;

        Loaded += async (s, e) => await _viewModel.LoadRecordsCommand.ExecuteAsync(null);
    }
}
