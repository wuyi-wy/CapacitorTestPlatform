using CapacitorTestPlatform.Core.Models;

namespace CapacitorTestPlatform.Core.Interfaces;

public interface IDeviceFactory
{
    IDeviceDriver? Create(string deviceModel);
    List<DeviceInfo> GetAllDevices();
    List<DeviceInfo> GetDevicesByCategory(string category);
}
