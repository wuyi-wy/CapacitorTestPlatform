using CapacitorTestPlatform.Core.Interfaces;
using System.IO.Ports;

namespace CapacitorTestPlatform.Devices;

public class SerialPortService : ISerialPortService, IDisposable
{
    private SerialPort? _serialPort;

    public bool IsOpen => _serialPort?.IsOpen ?? false;
    public string? PortName { get; private set; }
    public int BaudRate { get; private set; }

    public event EventHandler<string>? DataReceived;
    public event EventHandler<string>? ErrorOccurred;

    public void Open(string portName, int baudRate)
    {
        try
        {
            Close();
            _serialPort = new SerialPort(portName, baudRate, Parity.None, 8, StopBits.One)
            {
                ReadTimeout = 3000,
                WriteTimeout = 3000,
                NewLine = "\n"
            };
            _serialPort.DataReceived += OnDataReceived;
            _serialPort.Open();
            PortName = portName;
            BaudRate = baudRate;
        }
        catch (Exception ex)
        {
            ErrorOccurred?.Invoke(this, $"串口打开失败: {ex.Message}");
            throw;
        }
    }

    public void Close()
    {
        if (_serialPort != null)
        {
            _serialPort.DataReceived -= OnDataReceived;
            if (_serialPort.IsOpen)
                _serialPort.Close();
            _serialPort.Dispose();
            _serialPort = null;
        }
        PortName = null;
    }

    public string SendCommand(string command, int timeoutMs = 3000)
    {
        if (_serialPort == null || !_serialPort.IsOpen)
            throw new InvalidOperationException("串口未打开");

        _serialPort.ReadTimeout = timeoutMs;
        _serialPort.WriteTimeout = timeoutMs;
        _serialPort.DiscardInBuffer();
        _serialPort.WriteLine(command);

        try
        {
            var response = _serialPort.ReadLine().Trim();
            return response;
        }
        catch (TimeoutException)
        {
            throw new TimeoutException("设备响应超时");
        }
    }

    public async Task<string> SendCommandAsync(string command, int timeoutMs = 3000)
    {
        return await Task.Run(() => SendCommand(command, timeoutMs));
    }

    public string[] GetAvailablePorts()
    {
        return SerialPort.GetPortNames();
    }

    private void OnDataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        try
        {
            var data = _serialPort?.ReadExisting();
            if (!string.IsNullOrEmpty(data))
                DataReceived?.Invoke(this, data);
        }
        catch (Exception ex)
        {
            ErrorOccurred?.Invoke(this, $"数据接收错误: {ex.Message}");
        }
    }

    public void Dispose()
    {
        Close();
    }
}
