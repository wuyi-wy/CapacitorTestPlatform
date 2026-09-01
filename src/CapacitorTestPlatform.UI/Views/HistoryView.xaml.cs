using CapacitorTestPlatform.UI.ViewModels;
using System.Windows.Controls;

namespace CapacitorTestPlatform.UI.Views;

public partial class HistoryView : Page
{
    private readonly HistoryViewModel _viewModel;

    public HistoryView(HistoryViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = _viewModel;

        Loaded += async (s, e) => await _viewModel.LoadRecordsCommand.ExecuteAsync(null);
    }
}
