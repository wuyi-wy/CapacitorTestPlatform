using CapacitorTestPlatform.Core.Interfaces;
using CapacitorTestPlatform.Core.Models;

namespace CapacitorTestPlatform.Services;

public class DeviceService : IDeviceService
{
    private readonly IDeviceFactory _deviceFactory;

    public DeviceService(IDeviceFactory deviceFactory)
    {
        _deviceFactory = deviceFactory;
    }

    public async Task<List<DeviceInfo>> GetAvailableDevicesAsync()
    {
        return await Task.FromResult(_deviceFactory.GetAllDevices());
    }

    public async Task<IDeviceDriver?> GetDriverAsync(string deviceModel)
    {
        return await Task.FromResult(_deviceFactory.Create(deviceModel));
    }

    public List<TestColumnDefinition> GetColumnsForTestItem(string testItem, string deviceModel)
    {
        return testItem switch
        {
            "电容量" => new List<TestColumnDefinition>
            {
                new() { Header = "C", PropertyName = "Capacitance", Unit = "μF", Width = 150 },
                new() { Header = "频率", PropertyName = "Frequency", Unit = "Hz", Width = 120 }
            },
            "损耗角正切值(ESR)" or "损耗角正切" => new List<TestColumnDefinition>
            {
                new() { Header = "损耗", PropertyName = "Loss", Unit = "", Width = 150 },
                new() { Header = "频率", PropertyName = "Frequency", Unit = "Hz", Width = 120 }
            },
            "等效串联电阻(ESR)" or "ESR" => new List<TestColumnDefinition>
            {
                new() { Header = "ESR", PropertyName = "ESR", Unit = "MΩ", Width = 150 },
                new() { Header = "频率", PropertyName = "Frequency", Unit = "Hz", Width = 120 }
            },
            "阻抗" => new List<TestColumnDefinition>
            {
                new() { Header = "阻抗", PropertyName = "Impedance", Unit = "MΩ", Width = 150 },
                new() { Header = "频率", PropertyName = "Frequency", Unit = "Hz", Width = 120 }
            },
            "绝缘外套的绝缘电阻" => new List<TestColumnDefinition>
            {
                new() { Header = "绝缘电阻", PropertyName = "InsulationResistance", Unit = "MΩ", Width = 180 }
            },
            "漏电流" => new List<TestColumnDefinition>
            {
                new() { Header = "IL正向", PropertyName = "ILForward", Unit = "μA", Width = 120 },
                new() { Header = "HL正向", PropertyName = "HLForward", Unit = "S", Width = 100 },
                new() { Header = "IL反向", PropertyName = "ILReverse", Unit = "μA", Width = 120 },
                new() { Header = "HL反向", PropertyName = "HLReverse", Unit = "S", Width = 100 }
            },
            _ => new List<TestColumnDefinition>()
        };
    }
}
