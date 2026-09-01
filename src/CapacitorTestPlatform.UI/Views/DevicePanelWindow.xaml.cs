using CapacitorTestPlatform.UI.Models;
using CapacitorTestPlatform.UI.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace CapacitorTestPlatform.UI.Views;

/// <summary>
/// 设备面板弹窗，用于选择设备型号、配置参数、连接串口并采集测试数据。
/// </summary>
public partial class DevicePanelWindow : Window
{
    /// <summary>设备面板视图模型</summary>
    public DevicePanelViewModel ViewModel { get; }

    /// <summary>用户确认导入后返回的测试数据表，未确认时为 null。</summary>
    public TestDataTable? ImportedData { get; private set; }

    /// <summary>
    /// 初始化设备面板弹窗，绑定 ViewModel、注册数据导入和列刷新事件。
    /// </summary>
    /// <param name="viewModel">设备面板视图模型实例。</param>
    public DevicePanelWindow(DevicePanelViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;
        DataContext = ViewModel;

        ViewModel.DataImportRequested += (s, data) =>
        {
            ImportedData = data;
            DialogResult = true;
            Close();
        };

        // 监听数据变化，动态生成列
        ViewModel.ResultRows.CollectionChanged += (s, e) =>
        {
            if (ViewModel.ResultRows.Count > 0)
                UpdateResultColumns();
        };
    }

    /// <summary>
    /// 根据当前列信息动态重建结果 DataGrid 的列定义（序号列 + 数据列）。
    /// </summary>
    private void UpdateResultColumns()
    {
        var columns = TestDataRow.GetColumnOrder();
        if (columns.Count == 0 || ResultDataGrid == null) return;

        // 只在列数变化时重建
        if (ResultDataGrid.Columns.Count == columns.Count + 1) return;

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
    }
}
