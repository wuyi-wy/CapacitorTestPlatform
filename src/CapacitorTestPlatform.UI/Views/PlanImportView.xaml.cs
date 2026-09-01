using CapacitorTestPlatform.Core.Interfaces;
using CapacitorTestPlatform.Core.Models;
using CapacitorTestPlatform.UI.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Controls;

namespace CapacitorTestPlatform.UI.Views;

public partial class PlanImportView : Page
{
    private readonly PlanImportViewModel _viewModel;

    public PlanImportView(PlanImportViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = _viewModel;

        _viewModel.NavigateToTest += OnNavigateToTest;
        Loaded += async (s, e) => await _viewModel.LoadPlansCommand.ExecuteAsync(null);
    }

    private void OnNavigateToTest(object? sender, PlanInfo plan)
    {
        var mainWindow = Window.GetWindow(this) as MainWindow;
        mainWindow?.NavigateToTestPage(plan, plan.TestItemList.FirstOrDefault() ?? "");
    }
}
