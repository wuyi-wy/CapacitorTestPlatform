namespace CapacitorTestPlatform.Core.Models;

public class DeviceInfo
{
    public string Model { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int DefaultBaudRate { get; set; }
    public string? PortName { get; set; }
    public bool IsConnected { get; set; }
}
