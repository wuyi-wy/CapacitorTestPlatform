namespace CapacitorTestPlatform.Core.Interfaces;

public interface ISerialPortService
{
    bool IsOpen { get; }
    string? PortName { get; }
    int BaudRate { get; }
    void Open(string portName, int baudRate);
    void Close();
    string SendCommand(string command, int timeoutMs = 3000);
    Task<string> SendCommandAsync(string command, int timeoutMs = 3000);
    string[] GetAvailablePorts();
    event EventHandler<string>? DataReceived;
    event EventHandler<string>? ErrorOccurred;
}
