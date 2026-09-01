using CapacitorTestPlatform.Core.Models;

namespace CapacitorTestPlatform.Core.Interfaces;

/// <summary>
/// 设备服务接口，提供设备查询和测试列定义。
/// </summary>
public interface IDeviceService
{
    /// <summary>获取所有可用设备信息</summary>
    Task<List<DeviceInfo>> GetAvailableDevicesAsync();

    /// <summary>根据设备型号获取驱动实例</summary>
    Task<IDeviceDriver?> GetDriverAsync(string deviceModel);

    /// <summary>根据测试项和设备型号获取对应的列定义（如电容量→C(μF)等列）</summary>
    List<TestColumnDefinition> GetColumnsForTestItem(string testItem, string deviceModel);
}
