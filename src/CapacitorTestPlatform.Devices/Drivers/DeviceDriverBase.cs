using CapacitorTestPlatform.Core.Interfaces;
using CapacitorTestPlatform.Core.Models;

namespace CapacitorTestPlatform.Devices.Drivers;

public abstract class DeviceDriverBase : IDeviceDriver
{
    protected readonly ISerialPortService SerialPortService;
    protected bool IsConnected;

    protected DeviceDriverBase(ISerialPortService serialPortService)
    {
        SerialPortService = serialPortService;
    }

    public abstract string ModelName { get; }
    public abstract string DisplayName { get; }
    public abstract string Category { get; }
    public abstract int DefaultBaudRate { get; }
    public abstract List<string> ConfigurableParameters { get; }

    public event EventHandler<string>? StatusChanged;

    public virtual async Task<bool> ConnectAsync(string portName, int? baudRate = null)
    {
        try
        {
            SerialPortService.Open(portName, baudRate ?? DefaultBaudRate);
            IsConnected = true;
            StatusChanged?.Invoke(this, $"已连接 {ModelName} ({portName})");
            return true;
        }
        catch (Exception ex)
        {
            StatusChanged?.Invoke(this, $"连接失败: {ex.Message}");
            return false;
        }
    }

    public virtual async Task DisconnectAsync()
    {
        await Task.Run(() =>
        {
            SerialPortService.Close();
            IsConnected = false;
            StatusChanged?.Invoke(this, $"已断开 {ModelName}");
        });
    }

    public abstract Task<bool> ConfigureAsync(Dictionary<string, string> parameters);
    public abstract Task<DeviceTestResult> MeasureAsync();

    public Task<bool> IsConnectedAsync() => Task.FromResult(IsConnected && SerialPortService.IsOpen);

    protected async Task<string> SendAsync(string command, int timeoutMs = 3000)
    {
        if (!SerialPortService.IsOpen)
            throw new InvalidOperationException("串口未打开");
        return await SerialPortService.SendCommandAsync(command, timeoutMs);
    }

    protected string Send(string command, int timeoutMs = 3000)
    {
        if (!SerialPortService.IsOpen)
            throw new InvalidOperationException("串口未打开");
        return SerialPortService.SendCommand(command, timeoutMs);
    }

    protected void Notify(string message) => StatusChanged?.Invoke(this, message);
}
