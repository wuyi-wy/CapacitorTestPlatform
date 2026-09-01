using CapacitorTestPlatform.Core.Models;

namespace CapacitorTestPlatform.Core.Interfaces;

public interface IDeviceService
{
    Task<List<DeviceInfo>> GetAvailableDevicesAsync();
    Task<IDeviceDriver?> GetDriverAsync(string deviceModel);
    List<TestColumnDefinition> GetColumnsForTestItem(string testItem, string deviceModel);
}
