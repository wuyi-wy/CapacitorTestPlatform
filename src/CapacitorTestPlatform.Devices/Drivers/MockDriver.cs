using CapacitorTestPlatform.Core.Interfaces;
using CapacitorTestPlatform.Core.Models;

namespace CapacitorTestPlatform.Devices.Drivers;

public class MockDriver : DeviceDriverBase
{
    private readonly Random _random = new();
    private Dictionary<string, string> _currentParams = new();
    private string _deviceCategory = "LCR";

    public override string ModelName => "MOCK";
    public override string DisplayName => "模拟设备";
    public override string Category => _deviceCategory;
    public override int DefaultBaudRate => 9600;

    public override List<string> ConfigurableParameters => new()
    {
        "Function", "Frequency", "Voltage", "Speed", "Range"
    };

    public MockDriver(ISerialPortService serialPortService) : base(serialPortService) { }

    public void SetDeviceCategory(string category)
    {
        _deviceCategory = category;
    }

    public override async Task<bool> ConnectAsync(string portName, int? baudRate = null)
    {
        await Task.Delay(200);
        IsConnected = true;
        Notify($"模拟设备已连接 ({portName})");
        return true;
    }

    public override async Task DisconnectAsync()
    {
        await Task.Delay(100);
        IsConnected = false;
        Notify("模拟设备已断开");
    }

    public override async Task<bool> ConfigureAsync(Dictionary<string, string> parameters)
    {
        await Task.Delay(100);
        _currentParams = new Dictionary<string, string>(parameters);
        Notify($"参数已配置: {string.Join(", ", parameters.Select(p => $"{p.Key}={p.Value}"))}");
        return true;
    }

    public override async Task<DeviceTestResult> MeasureAsync()
    {
        await Task.Delay(300 + _random.Next(200));

        var func = _currentParams.GetValueOrDefault("Function", "");
        var freq = _currentParams.GetValueOrDefault("Frequency", "120");

        Dictionary<string, object?> data;

        if (_deviceCategory == "漏电流测试仪")
        {
            data = new()
            {
                ["IL正向(μA)"] = (_random.NextDouble() * 2 + 0.01).ToString("F3"),
                ["HL正向(S)"] = (_random.NextDouble() * 0.01).ToString("F6"),
                ["IL反向(μA)"] = (_random.NextDouble() * 2 + 0.01).ToString("F3"),
                ["HL反向(S)"] = (_random.NextDouble() * 0.01).ToString("F6"),
            };
        }
        else if (_deviceCategory == "绝缘电阻测试仪")
        {
            data = new()
            {
                ["LC(μF)"] = (_random.NextDouble() * 0.5 + 0.01).ToString("F6"),
                ["IR(MΩ)"] = (_random.NextDouble() * 5000 + 500).ToString("F1"),
            };
        }
        else if (_deviceCategory == "极壳耐压")
        {
            var result = _random.Next(1, 6);
            var resultMap = new Dictionary<int, string> { [1] = "PASS", [2] = "HIGH FAIL", [3] = "LOW FAIL", [4] = "ARC FAIL", [5] = "RANGE FAIL" };
            data = new()
            {
                ["步骤"] = _currentParams.GetValueOrDefault("Step", "AC"),
                ["电压(V)"] = _currentParams.GetValueOrDefault("Volt", "1000"),
                ["结果"] = resultMap.GetValueOrDefault(result, result.ToString()),
            };
        }
        else // LCR 数字电桥
        {
            data = new()
            {
                ["C(μF)"] = (_random.NextDouble() * 900 + 800).ToString("F3"),
                ["频率(Hz)"] = freq,
                ["损耗(tgδ)"] = (_random.NextDouble() * 0.01 + 0.001).ToString("F6"),
                ["ESR(MΩ)"] = (_random.NextDouble() * 0.1 + 0.01).ToString("F4"),
                ["阻抗(MΩ)"] = (_random.NextDouble() * 50 + 5).ToString("F3"),
            };
        }

        return DeviceTestResult.Ok(data);
    }
}
