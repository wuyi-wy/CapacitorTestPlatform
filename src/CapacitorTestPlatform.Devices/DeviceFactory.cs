using CapacitorTestPlatform.Core.Interfaces;
using CapacitorTestPlatform.Core.Models;
using CapacitorTestPlatform.Devices.Drivers;

namespace CapacitorTestPlatform.Devices;

/// <summary>
/// 设备工厂，根据设备型号创建对应的驱动实例，并提供设备列表查询。
/// </summary>
public class DeviceFactory : IDeviceFactory
{
    /// <summary>串口通信服务</summary>
    private readonly ISerialPortService _serialPortService;
    /// <summary>设备型号 → 驱动创建函数的映射字典</summary>
    private readonly Dictionary<string, Func<IDeviceDriver>> _driverFactories;

    /// <summary>
    /// 初始化设备工厂，注册所有支持的设备驱动。
    /// </summary>
    /// <param name="serialPortService">串口通信服务</param>
    public DeviceFactory(ISerialPortService serialPortService)
    {
        _serialPortService = serialPortService;
        _driverFactories = new Dictionary<string, Func<IDeviceDriver>>
        {
            ["TH2689"] = () => new TH2689Driver(_serialPortService),
            ["TH2683A"] = () => new TH2683ADriver(_serialPortService),
            ["TH2817A"] = () => new TH2817ADriver(_serialPortService),
            ["TH2832"] = () => new TH2832Driver(_serialPortService),
            ["TH9201"] = () => new TH9201Driver(_serialPortService),
            ["TH2810B"] = () => new TH2810BDriver(_serialPortService),
            ["MOCK"] = () => new MockDriver(_serialPortService)
        };
    }

    /// <summary>
    /// 根据设备型号创建对应的驱动实例。
    /// </summary>
    /// <param name="deviceModel">设备型号（如 TH2689、MOCK 等）</param>
    /// <returns>驱动实例，未匹配时返回 null</returns>
    public IDeviceDriver? Create(string deviceModel)
    {
        return _driverFactories.TryGetValue(deviceModel, out var factory) ? factory() : null;
    }

    /// <summary>
    /// 获取所有支持的设备信息列表。
    /// </summary>
    /// <returns>设备信息列表</returns>
    public List<DeviceInfo> GetAllDevices()
    {
        return new List<DeviceInfo>
        {
            new() { Model = "TH2817A", Name = "TH2817A LCR数字电桥", Category = "LCR数字电桥", DefaultBaudRate = 9600 },
            new() { Model = "TH2810B", Name = "TH2810B+ LCR数字电桥", Category = "LCR数字电桥", DefaultBaudRate = 19200 },
            new() { Model = "TH2689", Name = "TH2689 漏电流测试仪", Category = "漏电流测试仪", DefaultBaudRate = 19200 },
            new() { Model = "TH2683A", Name = "TH2683A 绝缘电阻测试仪", Category = "绝缘电阻测试仪", DefaultBaudRate = 9600 },
            new() { Model = "TH9201", Name = "TH9201 安规测试仪", Category = "极壳耐压", DefaultBaudRate = 19200 },
            new() { Model = "TH2832", Name = "TH2832 LCR数字电桥", Category = "LCR数字电桥", DefaultBaudRate = 115200 },
            new() { Model = "MOCK", Name = "模拟设备", Category = "模拟设备", DefaultBaudRate = 9600 }
        };
    }

    /// <summary>
    /// 按设备分类筛选设备列表。
    /// </summary>
    /// <param name="category">设备分类名称</param>
    /// <returns>匹配分类的设备信息列表</returns>
    public List<DeviceInfo> GetDevicesByCategory(string category)
    {
        return GetAllDevices().Where(d => d.Category == category).ToList();
    }
}
