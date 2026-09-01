using CapacitorTestPlatform.UI.Models;
using CapacitorTestPlatform.UI.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace CapacitorTestPlatform.UI.Views;

public partial class DevicePanelWindow : Window
{
    public DevicePanelViewModel ViewModel { get; }
    public TestDataTable? ImportedData { get; private set; }

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
