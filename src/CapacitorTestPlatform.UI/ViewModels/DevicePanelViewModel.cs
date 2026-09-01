using CapacitorTestPlatform.Core.Interfaces;
using CapacitorTestPlatform.Core.Models;
using CapacitorTestPlatform.Devices.Drivers;
using CapacitorTestPlatform.UI.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace CapacitorTestPlatform.UI.ViewModels;

public partial class BindableParameter : ObservableObject
{
    public string ParameterName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string InputType { get; set; } = "TextBox";
    public List<string> Options { get; set; } = new();
    public string? Unit { get; set; }
    public string? Placeholder { get; set; }

    [ObservableProperty]
    private string _value = "";

    public string ScpiValue => string.IsNullOrEmpty(Value) ? "" : Value;
}

public partial class DevicePanelViewModel : ObservableObject
{
    private readonly IDeviceFactory _deviceFactory;
    private readonly ISerialPortService _serialPortService;
    private IDeviceDriver? _currentDriver;
    private MockDriver? _mockDriver;

    [ObservableProperty]
    private ObservableCollection<DeviceConfigProfile> _deviceProfiles = new();

    [ObservableProperty]
    private DeviceConfigProfile? _selectedProfile;

    [ObservableProperty]
    private ObservableCollection<BindableParameter> _parameters = new();

    public ObservableCollection<string> AvailablePorts { get; } = new();

    [ObservableProperty]
    private string? _selectedPort;

    [ObservableProperty]
    private int _selectedBaudRate = 9600;

    public ObservableCollection<int> BaudRates { get; } = new() { 9600, 19200, 38400, 57600, 115200 };

    [ObservableProperty]
    private bool _isConnected;

    [ObservableProperty]
    private string _connectionStatus = "未连接";

    [ObservableProperty]
    private string _statusColor = "#7F8C8D";

    [ObservableProperty]
    private string _errorMessage = "";

    [ObservableProperty]
    private string _logText = "";

    [ObservableProperty]
    private int _acquireCount = 1;

    public TestDataTable ResultTable { get; } = new();
    public ObservableCollection<TestDataRow> ResultRows => ResultTable.Rows;

    public event EventHandler<TestDataTable>? DataImportRequested;

    public DevicePanelViewModel(IDeviceFactory deviceFactory, ISerialPortService serialPortService)
    {
        _deviceFactory = deviceFactory;
        _serialPortService = serialPortService;
        RefreshPorts();
        LoadProfiles();
    }

    private void LoadProfiles()
    {
        DeviceProfiles.Clear();
        foreach (var profile in DeviceConfigProfile.GetAllProfiles())
            DeviceProfiles.Add(profile);
    }

    [RelayCommand]
    private void RefreshPorts()
    {
        AvailablePorts.Clear();
        var ports = System.IO.Ports.SerialPort.GetPortNames();
        foreach (var port in ports)
            AvailablePorts.Add(port);

        if (AvailablePorts.Count == 0)
        {
            for (int i = 1; i <= 8; i++)
                AvailablePorts.Add($"COM{i}");
            Log("未检测到串口设备，已加载默认端口列表");
        }
        else
        {
            Log($"刷新串口: 发现 {AvailablePorts.Count} 个");
        }

        if (SelectedPort == null && AvailablePorts.Count > 0)
            SelectedPort = AvailablePorts[0];
    }

    partial void OnSelectedProfileChanged(DeviceConfigProfile? value)
    {
        if (value == null) return;

        SelectedBaudRate = value.DefaultBaudRate;

        Parameters.Clear();
        foreach (var p in value.Parameters)
        {
            Parameters.Add(new BindableParameter
            {
                ParameterName = p.ParameterName,
                DisplayName = p.DisplayName,
                InputType = p.InputType,
                Options = p.Options,
                Unit = p.Unit,
                Placeholder = p.Placeholder,
                Value = p.DefaultValue ?? ""
            });
        }

        ResultTable.Clear();
        Log($"已选择: {value.DisplayName}");
    }

    [RelayCommand]
    private async Task ConnectAsync()
    {
        if (SelectedProfile == null) return;

        ErrorMessage = "";
        try
        {
            if (SelectedProfile.IsMock)
            {
                _mockDriver = new MockDriver(_serialPortService);
                _mockDriver.SetDeviceCategory(SelectedProfile.Category);
                _mockDriver.StatusChanged += (s, msg) => Log(msg);
                var success = await _mockDriver.ConnectAsync("MOCK", 9600);
                _currentDriver = _mockDriver;

                IsConnected = success;
                ConnectionStatus = success ? "已连接(模拟)" : "连接失败";
                StatusColor = success ? "#4CAF50" : "#F44336";
            }
            else
            {
                _currentDriver = _deviceFactory.Create(SelectedProfile.DeviceModel);
                if (_currentDriver == null)
                {
                    ShowError("无法创建设备驱动");
                    return;
                }

                _currentDriver.StatusChanged += (s, msg) => Log(msg);

                if (string.IsNullOrEmpty(SelectedPort))
                {
                    ShowError("请选择串口");
                    return;
                }

                var success = await _currentDriver.ConnectAsync(SelectedPort, SelectedBaudRate);
                IsConnected = success;
                ConnectionStatus = success ? "已连接" : "连接失败";
                StatusColor = success ? "#4CAF50" : "#F44336";
            }
        }
        catch (Exception ex)
        {
            ShowError($"连接异常: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task DisconnectAsync()
    {
        if (_currentDriver != null)
        {
            await _currentDriver.DisconnectAsync();
            _currentDriver = null;
            _mockDriver = null;
        }
        IsConnected = false;
        ConnectionStatus = "未连接";
        StatusColor = "#7F8C8D";
        Log("已断开");
    }

    [RelayCommand]
    private async Task ConfigureAsync()
    {
        if (_currentDriver == null || !IsConnected)
        {
            ShowError("请先连接设备");
            return;
        }

        ErrorMessage = "";
        try
        {
            var paramDict = Parameters.ToDictionary(p => p.ParameterName, p => p.ScpiValue);
            var success = await _currentDriver.ConfigureAsync(paramDict);
            Log(success ? "参数配置成功" : "参数配置失败");
        }
        catch (Exception ex)
        {
            ShowError($"配置异常: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task MeasureAsync()
    {
        if (_currentDriver == null || !IsConnected)
        {
            ShowError("请先连接设备");
            return;
        }

        ErrorMessage = "";
        var count = Math.Max(1, AcquireCount);

        for (int i = 0; i < count; i++)
        {
            try
            {
                var result = await _currentDriver.MeasureAsync();
                if (result.Success)
                {
                    var rowData = new Dictionary<string, string>();
                    foreach (var kv in result.Data)
                        rowData[kv.Key] = kv.Value?.ToString() ?? "";

                    ResultTable.AddRow(rowData);
                    OnPropertyChanged(nameof(ResultRows));
                    Log($"采集 #{ResultTable.Rows.Count} 完成");
                }
                else
                {
                    ShowError($"采集失败: {result.ErrorMessage}");
                }
            }
            catch (Exception ex)
            {
                ShowError($"采集异常: {ex.Message}");
            }

            if (i < count - 1)
                await Task.Delay(500);
        }
    }

    [RelayCommand]
    private void ImportData()
    {
        if (ResultTable.Rows.Count == 0)
        {
            ShowError("没有数据可导入");
            return;
        }
        DataImportRequested?.Invoke(this, ResultTable);
        Log($"已导入 {ResultTable.Rows.Count} 条数据到测试页面");
    }

    [RelayCommand]
    private void ClearResults()
    {
        ResultTable.Clear();
        OnPropertyChanged(nameof(ResultRows));
        Log("已清空结果");
    }

    private void ShowError(string msg)
    {
        ErrorMessage = msg;
        StatusColor = "#F44336";
        Log($"[错误] {msg}");
    }

    private void Log(string msg)
    {
        LogText += $"[{DateTime.Now:HH:mm:ss}] {msg}\n";
    }
}
