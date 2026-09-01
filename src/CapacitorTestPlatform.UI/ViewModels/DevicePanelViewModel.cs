using CapacitorTestPlatform.Core.Interfaces;
using CapacitorTestPlatform.Core.Models;
using CapacitorTestPlatform.Devices.Drivers;
using CapacitorTestPlatform.UI.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace CapacitorTestPlatform.UI.ViewModels;

/// <summary>
/// 可绑定的设备参数项，用于在 UI 中动态渲染参数输入控件。
/// </summary>
public partial class BindableParameter : ObservableObject
{
    /// <summary>参数名称（SCPI 对应键名）</summary>
    public string ParameterName { get; set; } = string.Empty;

    /// <summary>参数显示名称（中文）</summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>输入控件类型（TextBox / ComboBox）</summary>
    public string InputType { get; set; } = "TextBox";

    /// <summary>下拉选项列表（仅 InputType 为 ComboBox 时使用）</summary>
    public List<string> Options { get; set; } = new();

    /// <summary>参数单位</summary>
    public string? Unit { get; set; }

    /// <summary>输入框占位提示文本</summary>
    public string? Placeholder { get; set; }

    /// <summary>参数当前值</summary>
    [ObservableProperty]
    private string _value = "";

    /// <summary>用于 SCPI 指令发送的参数值</summary>
    public string ScpiValue => string.IsNullOrEmpty(Value) ? "" : Value;
}

/// <summary>
/// 设备面板 ViewModel，管理设备选择、串口连接、参数配置、数据采集和结果导入。
/// </summary>
public partial class DevicePanelViewModel : ObservableObject
{
    private readonly IDeviceFactory _deviceFactory;
    private readonly ISerialPortService _serialPortService;
    private IDeviceDriver? _currentDriver;
    private MockDriver? _mockDriver;

    /// <summary>设备配置方案列表</summary>
    [ObservableProperty]
    private ObservableCollection<DeviceConfigProfile> _deviceProfiles = new();

    /// <summary>当前选中的设备配置方案</summary>
    [ObservableProperty]
    private DeviceConfigProfile? _selectedProfile;

    /// <summary>当前设备的可绑定参数列表</summary>
    [ObservableProperty]
    private ObservableCollection<BindableParameter> _parameters = new();

    /// <summary>可用串口列表</summary>
    public ObservableCollection<string> AvailablePorts { get; } = new();

    /// <summary>当前选中的串口</summary>
    [ObservableProperty]
    private string? _selectedPort;

    /// <summary>当前选中的波特率</summary>
    [ObservableProperty]
    private int _selectedBaudRate = 9600;

    /// <summary>可选波特率列表</summary>
    public ObservableCollection<int> BaudRates { get; } = new() { 9600, 19200, 38400, 57600, 115200 };

    /// <summary>设备是否已连接</summary>
    [ObservableProperty]
    private bool _isConnected;

    /// <summary>连接状态文本</summary>
    [ObservableProperty]
    private string _connectionStatus = "未连接";

    /// <summary>连接状态指示颜色（十六进制）</summary>
    [ObservableProperty]
    private string _statusColor = "#7F8C8D";

    /// <summary>错误提示消息</summary>
    [ObservableProperty]
    private string _errorMessage = "";

    /// <summary>操作日志文本</summary>
    [ObservableProperty]
    private string _logText = "";

    /// <summary>采集次数</summary>
    [ObservableProperty]
    private int _acquireCount = 1;

    /// <summary>采集结果数据表</summary>
    public TestDataTable ResultTable { get; } = new();

    /// <summary>采集结果行集合（用于 DataGrid 绑定）</summary>
    public ObservableCollection<TestDataRow> ResultRows => ResultTable.Rows;

    /// <summary>请求将采集数据导入测试页面时触发的事件</summary>
    public event EventHandler<TestDataTable>? DataImportRequested;

    /// <summary>
    /// 初始化设备面板 ViewModel，注入设备工厂和串口服务，并加载端口和配置方案。
    /// </summary>
    public DevicePanelViewModel(IDeviceFactory deviceFactory, ISerialPortService serialPortService)
    {
        _deviceFactory = deviceFactory;
        _serialPortService = serialPortService;
        RefreshPorts();
        LoadProfiles();
    }

    /// <summary>
    /// 加载所有设备配置方案到列表。
    /// </summary>
    private void LoadProfiles()
    {
        DeviceProfiles.Clear();
        foreach (var profile in DeviceConfigProfile.GetAllProfiles())
            DeviceProfiles.Add(profile);
    }

    /// <summary>
    /// 刷新串口命令，扫描系统可用串口并更新列表，无可用端口时加载默认 COM1-COM8。
    /// </summary>
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

    /// <summary>
    /// 设备配置方案变更回调，更新波特率、参数列表并清空采集结果。
    /// </summary>
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

    /// <summary>
    /// 连接命令，根据设备配置方案连接真实设备或模拟驱动。
    /// </summary>
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

    /// <summary>
    /// 断开连接命令，释放当前设备驱动资源并重置连接状态。
    /// </summary>
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

    /// <summary>
    /// 参数配置命令，将 UI 中的参数下发到设备驱动。
    /// </summary>
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

    /// <summary>
    /// 数据采集命令，按设定次数循环执行测量并将结果添加到结果表。
    /// </summary>
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

    /// <summary>
    /// 导入数据命令，将采集结果通过事件发送到测试页面。
    /// </summary>
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

    /// <summary>
    /// 清空结果命令，清除采集结果表中的所有数据。
    /// </summary>
    [RelayCommand]
    private void ClearResults()
    {
        ResultTable.Clear();
        OnPropertyChanged(nameof(ResultRows));
        Log("已清空结果");
    }

    /// <summary>
    /// 显示错误信息并记录到日志。
    /// </summary>
    /// <param name="msg">错误消息</param>
    private void ShowError(string msg)
    {
        ErrorMessage = msg;
        StatusColor = "#F44336";
        Log($"[错误] {msg}");
    }

    /// <summary>
    /// 追加一行带时间戳的操作日志。
    /// </summary>
    /// <param name="msg">日志消息</param>
    private void Log(string msg)
    {
        LogText += $"[{DateTime.Now:HH:mm:ss}] {msg}\n";
    }
}
