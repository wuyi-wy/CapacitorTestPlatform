namespace CapacitorTestPlatform.Core.Models;

/// <summary>
/// 设备信息，用于 UI 设备列表展示和设备选择。
/// </summary>
public class DeviceInfo
{
    /// <summary>设备型号标识（如 "TH2689"）</summary>
    public string Model { get; set; } = string.Empty;

    /// <summary>设备显示名称（如 "绝缘测试仪"）</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>设备分类（如 "绝缘电阻"、"漏电流"、"LCR数字电桥"）</summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>默认串口波特率</summary>
    public int DefaultBaudRate { get; set; }

    /// <summary>当前连接的串口名</summary>
    public string? PortName { get; set; }

    /// <summary>是否已连接</summary>
    public bool IsConnected { get; set; }
}
