using CapacitorTestPlatform.Core.Interfaces;
using CapacitorTestPlatform.Core.Models;
using CapacitorTestPlatform.Devices.Drivers;

namespace CapacitorTestPlatform.Devices;

public class DeviceFactory : IDeviceFactory
{
    private readonly ISerialPortService _serialPortService;
    private readonly Dictionary<string, Func<IDeviceDriver>> _driverFactories;

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

    public IDeviceDriver? Create(string deviceModel)
    {
        return _driverFactories.TryGetValue(deviceModel, out var factory) ? factory() : null;
    }

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

    public List<DeviceInfo> GetDevicesByCategory(string category)
    {
        return GetAllDevices().Where(d => d.Category == category).ToList();
    }
}
