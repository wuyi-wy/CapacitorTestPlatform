using CapacitorTestPlatform.Core.Models;

namespace CapacitorTestPlatform.Core.Interfaces;

/// <summary>
/// 设备驱动接口，定义所有测试仪器（TH2689/TH2683A/TH2817A 等）的统一操作契约。
/// 通过 SCPI 指令与设备通信，使用串口收发命令。
/// </summary>
public interface IDeviceDriver
{
    /// <summary>设备型号标识（如 "TH2689"）</summary>
    string ModelName { get; }

    /// <summary>设备显示名称（如 "绝缘测试仪"）</summary>
    string DisplayName { get; }

    /// <summary>设备分类（如 "绝缘电阻"、"漏电流"、"LCR数字电桥"）</summary>
    string Category { get; }

    /// <summary>默认波特率</summary>
    int DefaultBaudRate { get; }

    /// <summary>可配置参数列表</summary>
    List<string> ConfigurableParameters { get; }

    /// <summary>连接到指定串口</summary>
    Task<bool> ConnectAsync(string portName, int? baudRate = null);

    /// <summary>断开连接</summary>
    Task DisconnectAsync();

    /// <summary>向设备发送配置参数（如电压、速度、量程等）</summary>
    Task<bool> ConfigureAsync(Dictionary<string, string> parameters);

    /// <summary>执行一次测量，返回测量结果</summary>
    Task<DeviceTestResult> MeasureAsync();

    /// <summary>查询设备连接状态</summary>
    Task<bool> IsConnectedAsync();

    /// <summary>设备状态变更事件（连接/断开/错误）</summary>
    event EventHandler<string>? StatusChanged;
}
