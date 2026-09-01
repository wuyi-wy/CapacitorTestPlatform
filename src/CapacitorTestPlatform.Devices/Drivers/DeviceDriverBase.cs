using CapacitorTestPlatform.Core.Interfaces;
using CapacitorTestPlatform.Core.Models;

namespace CapacitorTestPlatform.Devices.Drivers;

/// <summary>
/// 设备驱动基类，提供连接、断开、指令发送等通用功能，子类实现具体参数配置与测量逻辑。
/// </summary>
public abstract class DeviceDriverBase : IDeviceDriver
{
    /// <summary>串口通信服务</summary>
    protected readonly ISerialPortService SerialPortService;
    /// <summary>连接状态标记</summary>
    protected bool IsConnected;

    /// <summary>
    /// 初始化驱动基类。
    /// </summary>
    /// <param name="serialPortService">串口通信服务</param>
    protected DeviceDriverBase(ISerialPortService serialPortService)
    {
        SerialPortService = serialPortService;
    }

    /// <summary>设备型号标识（如 TH2689）</summary>
    public abstract string ModelName { get; }
    /// <summary>设备显示名称</summary>
    public abstract string DisplayName { get; }
    /// <summary>设备分类（如 LCR数字电桥、漏电流测试仪）</summary>
    public abstract string Category { get; }
    /// <summary>默认波特率</summary>
    public abstract int DefaultBaudRate { get; }
    /// <summary>可配置参数名称列表</summary>
    public abstract List<string> ConfigurableParameters { get; }

    /// <summary>状态变更事件</summary>
    public event EventHandler<string>? StatusChanged;

    /// <summary>
    /// 连接设备，打开串口通信。
    /// </summary>
    /// <param name="portName">端口名称</param>
    /// <param name="baudRate">波特率，为 null 时使用默认值</param>
    /// <returns>连接是否成功</returns>
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

    /// <summary>
    /// 断开设备连接，关闭串口。
    /// </summary>
    public virtual async Task DisconnectAsync()
    {
        await Task.Run(() =>
        {
            SerialPortService.Close();
            IsConnected = false;
            StatusChanged?.Invoke(this, $"已断开 {ModelName}");
        });
    }

    /// <summary>
    /// 配置设备参数（由子类实现具体协议）。
    /// </summary>
    /// <param name="parameters">参数名值对</param>
    /// <returns>配置是否成功</returns>
    public abstract Task<bool> ConfigureAsync(Dictionary<string, string> parameters);

    /// <summary>
    /// 执行一次测量并返回结果（由子类实现具体协议）。
    /// </summary>
    /// <returns>测量结果</returns>
    public abstract Task<DeviceTestResult> MeasureAsync();

    /// <summary>
    /// 查询设备是否处于连接状态。
    /// </summary>
    /// <returns>是否已连接</returns>
    public Task<bool> IsConnectedAsync() => Task.FromResult(IsConnected && SerialPortService.IsOpen);

    /// <summary>
    /// 异步发送 SCPI 指令。
    /// </summary>
    /// <param name="command">SCPI 指令</param>
    /// <param name="timeoutMs">超时毫秒数</param>
    /// <returns>设备响应</returns>
    protected async Task<string> SendAsync(string command, int timeoutMs = 3000)
    {
        if (!SerialPortService.IsOpen)
            throw new InvalidOperationException("串口未打开");
        return await SerialPortService.SendCommandAsync(command, timeoutMs);
    }

    /// <summary>
    /// 同步发送 SCPI 指令。
    /// </summary>
    /// <param name="command">SCPI 指令</param>
    /// <param name="timeoutMs">超时毫秒数</param>
    /// <returns>设备响应</returns>
    protected string Send(string command, int timeoutMs = 3000)
    {
        if (!SerialPortService.IsOpen)
            throw new InvalidOperationException("串口未打开");
        return SerialPortService.SendCommand(command, timeoutMs);
    }

    /// <summary>
    /// 触发状态变更通知。
    /// </summary>
    /// <param name="message">状态消息</param>
    protected void Notify(string message) => StatusChanged?.Invoke(this, message);
}
