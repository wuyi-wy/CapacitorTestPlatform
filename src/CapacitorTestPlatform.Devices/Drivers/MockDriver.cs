using CapacitorTestPlatform.Core.Interfaces;
using CapacitorTestPlatform.Core.Models;

namespace CapacitorTestPlatform.Devices.Drivers;

/// <summary>
/// Mock 驱动，开发调试用，模拟设备测量结果，无需真实硬件。
/// </summary>
public class MockDriver : DeviceDriverBase
{
    /// <summary>随机数生成器，用于产生模拟数据</summary>
    private readonly Random _random = new();
    /// <summary>当前配置的参数</summary>
    private Dictionary<string, string> _currentParams = new();
    /// <summary>设备分类，决定返回哪类模拟数据</summary>
    private string _deviceCategory = "LCR";

    /// <summary>设备型号标识</summary>
    public override string ModelName => "MOCK";
    /// <summary>设备显示名称</summary>
    public override string DisplayName => "模拟设备";
    /// <summary>设备分类</summary>
    public override string Category => _deviceCategory;
    /// <summary>默认波特率</summary>
    public override int DefaultBaudRate => 9600;

    /// <summary>可配置参数列表</summary>
    public override List<string> ConfigurableParameters => new()
    {
        "Function", "Frequency", "Voltage", "Speed", "Range"
    };

    /// <summary>
    /// 初始化 Mock 驱动。
    /// </summary>
    /// <param name="serialPortService">串口通信服务</param>
    public MockDriver(ISerialPortService serialPortService) : base(serialPortService) { }

    /// <summary>
    /// 设置模拟设备分类，影响 MeasureAsync 返回的数据结构。
    /// </summary>
    /// <param name="category">设备分类（如 LCR、漏电流测试仪、绝缘电阻测试仪、极壳耐压）</param>
    public void SetDeviceCategory(string category)
    {
        _deviceCategory = category;
    }

    /// <summary>
    /// 模拟连接设备，延迟后返回成功。
    /// </summary>
    /// <param name="portName">端口名称</param>
    /// <param name="baudRate">波特率</param>
    /// <returns>始终返回 true</returns>
    public override async Task<bool> ConnectAsync(string portName, int? baudRate = null)
    {
        await Task.Delay(200);
        IsConnected = true;
        Notify($"模拟设备已连接 ({portName})");
        return true;
    }

    /// <summary>
    /// 模拟断开设备连接。
    /// </summary>
    public override async Task DisconnectAsync()
    {
        await Task.Delay(100);
        IsConnected = false;
        Notify("模拟设备已断开");
    }

    /// <summary>
    /// 模拟配置设备参数，保存参数字典备用。
    /// </summary>
    /// <param name="parameters">参数名值对</param>
    /// <returns>始终返回 true</returns>
    public override async Task<bool> ConfigureAsync(Dictionary<string, string> parameters)
    {
        await Task.Delay(100);
        _currentParams = new Dictionary<string, string>(parameters);
        Notify($"参数已配置: {string.Join(", ", parameters.Select(p => $"{p.Key}={p.Value}"))}");
        return true;
    }

    /// <summary>
    /// 模拟一次测量，根据设备分类返回对应的随机测试数据。
    /// </summary>
    /// <returns>包含随机模拟数据的测量结果</returns>
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
