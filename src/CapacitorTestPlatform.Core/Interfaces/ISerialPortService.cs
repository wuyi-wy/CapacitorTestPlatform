namespace CapacitorTestPlatform.Core.Interfaces;

/// <summary>
/// 串口通信服务接口，封装 System.IO.Ports.SerialPort 的读写操作。
/// 使用 SCPI 指令协议，换行符 \n 分隔命令。
/// </summary>
public interface ISerialPortService
{
    /// <summary>串口是否已打开</summary>
    bool IsOpen { get; }

    /// <summary>当前串口名（如 "COM3"）</summary>
    string? PortName { get; }

    /// <summary>当前波特率</summary>
    int BaudRate { get; }

    /// <summary>打开串口</summary>
    void Open(string portName, int baudRate);

    /// <summary>关闭串口</summary>
    void Close();

    /// <summary>同步发送 SCPI 指令并等待响应</summary>
    string SendCommand(string command, int timeoutMs = 3000);

    /// <summary>异步发送 SCPI 指令并等待响应</summary>
    Task<string> SendCommandAsync(string command, int timeoutMs = 3000);

    /// <summary>获取系统可用串口列表</summary>
    string[] GetAvailablePorts();

    /// <summary>收到设备数据事件</summary>
    event EventHandler<string>? DataReceived;

    /// <summary>通信错误事件</summary>
    event EventHandler<string>? ErrorOccurred;
}
