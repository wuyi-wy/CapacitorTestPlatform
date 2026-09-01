using CapacitorTestPlatform.Core.Interfaces;
using CapacitorTestPlatform.Data.Contexts;
using CapacitorTestPlatform.Data.Repositories;
using CapacitorTestPlatform.Devices;
using CapacitorTestPlatform.Services;
using CapacitorTestPlatform.UI.ViewModels;
using CapacitorTestPlatform.UI.Views;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Windows;

namespace CapacitorTestPlatform.UI;

public partial class App : Application
{
    private ServiceProvider? _serviceProvider;

    public IServiceProvider ServiceProvider => _serviceProvider!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        var services = new ServiceCollection();

        services.AddSingleton<IConfiguration>(configuration);

        var sqlitePath = configuration.GetConnectionString("SQLite") ?? "Data Source=test.db";
        var sqliteContext = new SQLiteContext(sqlitePath.Replace("Data Source=", ""));
        services.AddSingleton(sqliteContext);

        var readConn = configuration.GetConnectionString("SqlServerRead") ?? "";
        var writeConn = configuration.GetConnectionString("SqlServerWrite") ?? "";
        services.AddSingleton(new SqlServerContext(readConn, writeConn));

        services.AddSingleton<IPlanRepository, PlanRepository>();
        services.AddSingleton<TestRecordRepository>();
        services.AddSingleton<ITestHistoryRepository, TestHistoryRepository>();
        services.AddSingleton<RemotePlanRepository>();
        services.AddSingleton<RemoteReportRepository>();

        services.AddSingleton<ISerialPortService, SerialPortService>();
        services.AddSingleton<IDeviceFactory, DeviceFactory>();
        services.AddSingleton<IPlanService>(sp =>
            new PlanService(
                sp.GetRequiredService<IPlanRepository>(),
                sp.GetService<RemotePlanRepository>(),
                configuration["AppSettings:Station"] ?? "电容性能台1#"));
        services.AddSingleton<ITestService, TestService>();
        services.AddSingleton<IReportService, ReportService>();

        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<PlanImportViewModel>();
        services.AddTransient<TestPageViewModel>();
        services.AddTransient<HistoryViewModel>();
        services.AddTransient<DevicePanelViewModel>();

        services.AddTransient<MainWindow>();
        services.AddTransient<PlanImportView>();
        services.AddTransient<TestPageView>();
        services.AddTransient<HistoryView>();

        _serviceProvider = services.BuildServiceProvider();

        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _serviceProvider?.Dispose();
        base.OnExit(e);
    }
}
