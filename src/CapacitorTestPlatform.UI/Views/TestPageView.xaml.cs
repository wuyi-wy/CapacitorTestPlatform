using CapacitorTestPlatform.UI.Models;
using CapacitorTestPlatform.UI.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace CapacitorTestPlatform.UI.Views;

/// <summary>
/// 测试页面，展示测试计划数据表格，支持动态列生成和设备数据导入。
/// </summary>
public partial class TestPageView : Page
{
    /// <summary>测试页面视图模型</summary>
    private readonly TestPageViewModel _viewModel;

    /// <summary>
    /// 初始化测试页面，绑定 ViewModel 并注册数据行集合变更事件以刷新列。
    /// </summary>
    /// <param name="viewModel">测试页面视图模型实例。</param>
    public TestPageView(TestPageViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = _viewModel;

        _viewModel.RequestDeviceSelect += OnRequestDeviceSelect;
        _viewModel.TestDataTable.Rows.CollectionChanged += (s, e) =>
        {
            UpdateTestColumns();
        };
    }

    /// <summary>
    /// 响应设备选择请求，打开设备面板弹窗，导入采集数据。
    /// </summary>
    private void OnRequestDeviceSelect(object? sender, EventArgs e)
    {
        var app = (App)Application.Current;
        var vm = app.ServiceProvider.GetRequiredService<DevicePanelViewModel>();
        var dialog = new DevicePanelWindow(vm);

        var owner = Window.GetWindow(this);
        if (owner != null) dialog.Owner = owner;

        if (dialog.ShowDialog() == true && dialog.ImportedData != null)
        {
            _viewModel.ImportDeviceData(dialog.ImportedData);
        }
    }

    /// <summary>
    /// 根据当前列信息动态重建 DataGrid 的列定义（序号列 + 数据列 + 删除按钮列）。
    /// </summary>
    private void UpdateTestColumns()
    {
        var columns = TestDataRow.GetColumnOrder();
        if (columns.Count == 0 || TestDataGrid == null) return;

        if (TestDataGrid.Columns.Count == columns.Count + 2) return;

        TestDataGrid.Columns.Clear();

        // 序号列
        TestDataGrid.Columns.Add(new DataGridTextColumn
        {
            Header = "序号",
            Binding = new Binding("SeqNo") { Mode = BindingMode.OneWay },
            Width = 55,
            IsReadOnly = true
        });

        // 数据列
        foreach (var col in columns)
        {
            TestDataGrid.Columns.Add(new DataGridTextColumn
            {
                Header = col,
                Binding = new Binding($"[{col}]") { Mode = BindingMode.TwoWay },
                Width = 110
            });
        }

        // 删除按钮列
        var deleteCol = new DataGridTemplateColumn { Header = "操作", Width = 60 };
        var factory = new FrameworkElementFactory(typeof(Button));
        factory.SetValue(Button.ContentProperty, "删除");
        factory.SetValue(Button.ForegroundProperty, System.Windows.Media.Brushes.Red);
        factory.SetValue(Button.BackgroundProperty, System.Windows.Media.Brushes.Transparent);
        factory.SetValue(Button.BorderThicknessProperty, new Thickness(0));
        factory.SetValue(Button.PaddingProperty, new Thickness(8, 2, 8, 2));
        factory.SetBinding(Button.CommandProperty,
            new Binding("DataContext.DeleteRowCommand") { Source = this });
        factory.SetBinding(Button.CommandParameterProperty, new Binding());
        deleteCol.CellTemplate = new DataTemplate { VisualTree = factory };
        TestDataGrid.Columns.Add(deleteCol);
    }
}
