using CapacitorTestPlatform.Core.Interfaces;
using CapacitorTestPlatform.Core.Models;
using CapacitorTestPlatform.UI.ViewModels;
using CapacitorTestPlatform.UI.Views;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Controls;

namespace CapacitorTestPlatform.UI.Views;

public partial class MainWindow : Window
{
    private readonly MainWindowViewModel _viewModel;
    private readonly IServiceProvider _serviceProvider;

    private PlanImportView? _planImportPage;
    private TestPageView? _testPage;
    private HistoryView? _historyPage;

    public MainWindow(MainWindowViewModel viewModel, IServiceProvider serviceProvider)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _serviceProvider = serviceProvider;
        DataContext = _viewModel;

        Loaded += MainWindow_Loaded;
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        _planImportPage = _serviceProvider.GetRequiredService<PlanImportView>();
        _testPage = _serviceProvider.GetRequiredService<TestPageView>();
        _historyPage = _serviceProvider.GetRequiredService<HistoryView>();

        NavigateTo("PlanImport");
    }

    public void NavigateTo(string pageName)
    {
        Page? page = pageName switch
        {
            "PlanImport" => _planImportPage ??= _serviceProvider.GetRequiredService<PlanImportView>(),
            "Test" => _testPage ??= _serviceProvider.GetRequiredService<TestPageView>(),
            "History" => _historyPage ??= _serviceProvider.GetRequiredService<HistoryView>(),
            _ => _planImportPage
        };

        if (page != null)
            ContentFrame.Navigate(page);
    }

    public void NavigateToTestPage(PlanInfo plan, string testItem)
    {
        _testPage = _serviceProvider.GetRequiredService<TestPageView>();
        var testVm = _testPage.DataContext as TestPageViewModel;
        testVm?.InitializeWithPlan(plan, testItem);
        ContentFrame.Navigate(_testPage);
    }

    private void PlanImport_Selected(object sender, RoutedEventArgs e)
    {
        NavigateTo("PlanImport");
    }

    private void TestPage_Selected(object sender, RoutedEventArgs e)
    {
        NavigateTo("Test");
    }

    private void History_Selected(object sender, RoutedEventArgs e)
    {
        NavigateTo("History");
    }
}
