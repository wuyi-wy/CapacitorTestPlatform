using CapacitorTestPlatform.UI.Models;
using CapacitorTestPlatform.UI.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace CapacitorTestPlatform.UI.Views;

public partial class TestPageView : Page
{
    private readonly TestPageViewModel _viewModel;

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
