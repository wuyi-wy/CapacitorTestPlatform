using CapacitorTestPlatform.UI.Models;
using CapacitorTestPlatform.UI.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace CapacitorTestPlatform.UI.Views;

/// <summary>
/// 设备面板弹窗，提供设备连接、参数配置和数据采集功能。
/// 接收 testItem 和 plan 参数以支持推荐设备和模板列名。
/// </summary>
public partial class DevicePanelWindow : Window
{
    /// <summary>
    /// 初始化设备面板弹窗。
    /// </summary>
    /// <param name="viewModel">设备面板 ViewModel（已注入 testItem 和 plan）</param>
    public DevicePanelWindow(DevicePanelViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;

        // 导入数据事件：将结果写入测试页面的表格后关闭弹窗
        viewModel.DataImportRequested += (_, data) =>
        {
            this.Tag = data;   // 用 Tag 把采集结果带回去
            DialogResult = true;
            Close();
        };

        // 订阅采集结果行变更，自动刷新结果表格列
        viewModel.ResultRows.CollectionChanged += (_, _) =>
        {
            var columns = viewModel.ResultTable.GetColumnOrder();
            if (ResultDataGrid.Columns.Count != columns.Count + 2)
                BuildResultColumns(viewModel.ResultTable);
        };
    }

    /// <summary>
    /// 采集结果 DataGrid 加载时生成初始列。
    /// </summary>
    private void ResultDataGrid_Loaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is DevicePanelViewModel vm)
            BuildResultColumns(vm.ResultTable);
    }

    /// <summary>
    /// 为采集结果 DataGrid 动态生成列定义（序号列 + 数据列 + 删除按钮列）。
    /// </summary>
    private void BuildResultColumns(TestDataTable table)
    {
        var columns = table.GetColumnOrder();
        if (columns.Count == 0) return;

        ResultDataGrid.Columns.Clear();

        // 序号列
        ResultDataGrid.Columns.Add(new DataGridTextColumn
        {
            Header = "序号",
            Binding = new Binding("SeqNo") { Mode = BindingMode.OneWay },
            Width = 55,
            IsReadOnly = true
        });

        // 数据列
        foreach (var col in columns)
        {
            ResultDataGrid.Columns.Add(new DataGridTextColumn
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
            new Binding("DataContext.ClearResultsCommand") { Source = this });
        deleteCol.CellTemplate = new DataTemplate { VisualTree = factory };
        ResultDataGrid.Columns.Add(deleteCol);
    }
}
