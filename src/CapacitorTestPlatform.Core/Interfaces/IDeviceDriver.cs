using CapacitorTestPlatform.Core.Models;

namespace CapacitorTestPlatform.Core.Interfaces;

public interface IDeviceDriver
{
    string ModelName { get; }
    string DisplayName { get; }
    string Category { get; }
    int DefaultBaudRate { get; }
    List<string> ConfigurableParameters { get; }
    Task<bool> ConnectAsync(string portName, int? baudRate = null);
    Task DisconnectAsync();
    Task<bool> ConfigureAsync(Dictionary<string, string> parameters);
    Task<DeviceTestResult> MeasureAsync();
    Task<bool> IsConnectedAsync();
    event EventHandler<string>? StatusChanged;
}
