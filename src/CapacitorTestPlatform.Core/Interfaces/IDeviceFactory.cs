using CapacitorTestPlatform.Core.Models;

namespace CapacitorTestPlatform.Core.Interfaces;

/// <summary>
/// 设备工厂接口，根据设备型号创建对应的驱动实例。
/// </summary>
public interface IDeviceFactory
{
    /// <summary>根据设备型号创建驱动实例，型号不存在时返回 null</summary>
    IDeviceDriver? Create(string deviceModel);

    /// <summary>获取所有支持的设备列表</summary>
    List<DeviceInfo> GetAllDevices();

    /// <summary>按分类筛选设备（如 "LCR数字电桥"）</summary>
    List<DeviceInfo> GetDevicesByCategory(string category);
}
