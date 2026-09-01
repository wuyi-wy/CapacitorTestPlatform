using CapacitorTestPlatform.Core.Interfaces;
using System.IO.Ports;

namespace CapacitorTestPlatform.Devices;

/// <summary>
/// 串口通信服务，封装 System.IO.Ports 的打开、关闭、发送指令和接收数据。
/// </summary>
public class SerialPortService : ISerialPortService, IDisposable
{
    /// <summary>底层串口实例</summary>
    private SerialPort? _serialPort;

    /// <summary>串口是否已打开</summary>
    public bool IsOpen => _serialPort?.IsOpen ?? false;
    /// <summary>当前打开的端口名称</summary>
    public string? PortName { get; private set; }
    /// <summary>当前波特率</summary>
    public int BaudRate { get; private set; }

    /// <summary>数据接收事件</summary>
    public event EventHandler<string>? DataReceived;
    /// <summary>错误发生事件</summary>
    public event EventHandler<string>? ErrorOccurred;

    /// <summary>
    /// 打开指定串口。若已有连接，先关闭再重新打开。
    /// </summary>
    /// <param name="portName">端口名称（如 COM3）</param>
    /// <param name="baudRate">波特率</param>
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

    /// <summary>
    /// 关闭当前串口连接并释放资源。
    /// </summary>
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

    /// <summary>
    /// 向设备发送 SCPI 指令并同步等待响应。
    /// </summary>
    /// <param name="command">SCPI 指令字符串</param>
    /// <param name="timeoutMs">读写超时（毫秒），默认 3000</param>
    /// <returns>设备返回的响应字符串（已去除首尾空白）</returns>
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

    /// <summary>
    /// 向设备发送 SCPI 指令并异步等待响应。
    /// </summary>
    /// <param name="command">SCPI 指令字符串</param>
    /// <param name="timeoutMs">读写超时（毫秒），默认 3000</param>
    /// <returns>设备返回的响应字符串</returns>
    public async Task<string> SendCommandAsync(string command, int timeoutMs = 3000)
    {
        return await Task.Run(() => SendCommand(command, timeoutMs));
    }

    /// <summary>
    /// 获取当前可用的串口名称列表。
    /// </summary>
    /// <returns>可用端口名称数组</returns>
    public string[] GetAvailablePorts()
    {
        return SerialPort.GetPortNames();
    }

    /// <summary>
    /// 串口数据接收回调，触发 DataReceived 事件。
    /// </summary>
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

    /// <summary>
    /// 释放资源，关闭串口连接。
    /// </summary>
    public void Dispose()
    {
        Close();
    }
}
