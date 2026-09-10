using CapacitorTestPlatform.Core.Models;
using CapacitorTestPlatform.UI.Models;
using CapacitorTestPlatform.UI.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace CapacitorTestPlatform.UI.Views;

/// <summary>
/// 测试页面，展示测试计划数据表格（双层 TabControl：设备分类 → 测试项），支持动态列生成。
/// </summary>
public partial class TestPageView : Page
{
    /// <summary>测试页面视图模型</summary>
    private readonly TestPageViewModel _viewModel;

    /// <summary>
    /// 初始化测试页面，绑定 ViewModel 并注册设备选择事件。
    /// </summary>
    /// <param name="viewModel">测试页面视图模型实例。</param>
    public TestPageView(TestPageViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = _viewModel;

        _viewModel.RequestDeviceSelect += OnRequestDeviceSelect;
    }

    /// <summary>
    /// 响应设备选择请求，创建带 testItem 和 plan 参数的设备面板 ViewModel，打开弹窗。
    /// </summary>
    private void OnRequestDeviceSelect(object? sender, (string TestItem, PlanInfo Plan) args)
    {
        var app = (App)Application.Current;

        var vm = ActivatorUtilities.CreateInstance<DevicePanelViewModel>(
            app.ServiceProvider, args.TestItem, args.Plan);

        var dialog = new DevicePanelWindow(vm);
        var owner = Window.GetWindow(this);
        if (owner != null) dialog.Owner = owner;

        if (dialog.ShowDialog() == true && dialog.Tag is TestDataTable deviceData)
        {
            _viewModel.ImportDeviceData(deviceData);
        }
    }

    /// <summary>
    /// DataGrid 加载时动态生成列，并订阅行集合变更以自动刷新列。
    /// 支持两种 DataContext：TestDataTable（多表格模板）和 DeviceCategoryTab（单表格模板）。
    /// </summary>
    private void DataGrid_Loaded(object sender, RoutedEventArgs e)
    {
        if (sender is not DataGrid dataGrid) return;

        // 从 DataContext 解析出 TestDataTable
        TestDataTable? table = dataGrid.DataContext switch
        {
            TestDataTable dt => dt,
            DeviceCategoryTab tab => tab.Tables.FirstOrDefault(),
            _ => null
        };
        if (table == null) return;

        BuildColumns(dataGrid, table);

        // 订阅行集合变更，列数变化时重新生成列
        table.Rows.CollectionChanged += (_, _) =>
        {
            var columns = table.GetColumnOrder();
            if (dataGrid.Columns.Count != columns.Count + 2)
                BuildColumns(dataGrid, table);
        };
    }

    /// <summary>
    /// 为指定 DataGrid 动态生成列定义（序号列 + 数据列 + 删除按钮列）。
    /// 删除按钮绑定到 TestPageViewModel.DeleteRowCommand。
    /// </summary>
    private void BuildColumns(DataGrid dataGrid, TestDataTable table)
    {
        var columns = table.GetColumnOrder();
        if (columns.Count == 0) return;

        dataGrid.Columns.Clear();

        // 序号列
        dataGrid.Columns.Add(new DataGridTextColumn
        {
            Header = "序号",
            Binding = new Binding("SeqNo") { Mode = BindingMode.OneWay },
            Width = 55,
            IsReadOnly = true
        });

        // 数据列
        foreach (var col in columns)
        {
            dataGrid.Columns.Add(new DataGridTextColumn
            {
                Header = col,
                Binding = new Binding($"[{col}]") { Mode = BindingMode.TwoWay },
                Width = 110
            });
        }

        // 删除按钮列（绑定到 Page.DataContext.DeleteRowCommand）
        var deleteCol = new DataGridTemplateColumn { Header = "操作", Width = 60 };
        var factory = new FrameworkElementFactory(typeof(Button));
        factory.SetValue(Button.ContentProperty, "删除");
        factory.SetValue(Button.ForegroundProperty, System.Windows.Media.Brushes.Red);
        factory.SetValue(Button.BackgroundProperty, System.Windows.Media.Brushes.Transparent);
        factory.SetValue(Button.BorderThicknessProperty, new Thickness(0));
        factory.SetValue(Button.PaddingProperty, new Thickness(8, 2, 8, 2));
        factory.SetBinding(Button.CommandProperty, new Binding("DataContext.DeleteRowCommand")
        {
            RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Page), 1)
        });
        factory.SetBinding(Button.CommandParameterProperty, new Binding());
        deleteCol.CellTemplate = new DataTemplate { VisualTree = factory };
        dataGrid.Columns.Add(deleteCol);
    }
}
